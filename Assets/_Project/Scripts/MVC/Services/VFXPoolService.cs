using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;
using System.Threading;

public class VFXPoolService
{
    private readonly Dictionary<GameObject, IObjectPool<GameObject>> _pools = new();
    private readonly Transform _rootTransform;

    public VFXPoolService()
    {
        GameObject root = new GameObject("[SYSTEM]_VFXPool");
        _rootTransform = root.transform;
    }

    // 1. HÀM CŨ GIỮ NGUYÊN: Dùng cho các hiệu ứng chung chung (Bom nổ, Khói...)
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, float delayDespawn = 3f)
    {
        if (prefab == null) return null;

        if (!_pools.TryGetValue(prefab, out var pool))
        {
            pool = CreatePool(prefab);
            _pools[prefab] = pool;
        }

        GameObject instance = pool.Get();
        instance.transform.SetPositionAndRotation(position, rotation);
        
        AutoDespawnAsync(instance, prefab, delayDespawn).Forget();

        return instance;
    }

    // 2. TÍNH NĂNG MỚI: Spawn hạt và tự động ĐỔI MÀU (Dành riêng cho Nước ép trái cây)
    public void SpawnParticleWithColor(GameObject particlePrefab, Vector3 position, Color color)
    {
        if (particlePrefab == null) return;

        if (!_pools.TryGetValue(particlePrefab, out var pool))
        {
            pool = CreatePool(particlePrefab);
            _pools[particlePrefab] = pool;
        }

        GameObject instance = pool.Get();
        instance.transform.position = position;

        // Lấy ParticleSystem và đổi màu
        if (instance.TryGetComponent<ParticleSystem>(out var ps))
        {
            var main = ps.main;
            main.startColor = color;
            ps.Play();

            // Tự động thu hồi dựa trên thời lượng (duration) của Particle đó + 0.1s cho an toàn
            AutoDespawnAsync(instance, particlePrefab, main.duration + 0.1f).Forget();
        }
        else
        {
            // Nếu lỡ truyền nhầm Prefab không có Particle, thu hồi sau 1 giây
            AutoDespawnAsync(instance, particlePrefab, 1f).Forget();
        }
    }

    // Hàm phụ trợ để code gọn hơn
    private IObjectPool<GameObject> CreatePool(GameObject prefab)
    {
        return new ObjectPool<GameObject>(
            createFunc: () => 
            {
                GameObject obj = Object.Instantiate(prefab, _rootTransform);
                obj.name = $"[VFX]_{prefab.name}"; 
                obj.SetActive(false);
                return obj;
            },
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => 
            {
                obj.SetActive(false);
                obj.transform.SetParent(_rootTransform); 
            },
            actionOnDestroy: Object.Destroy,
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 50
        );
    }

    private async UniTaskVoid AutoDespawnAsync(GameObject instance, GameObject prefabKey, float delay)
    {
        CancellationToken ct = instance.GetCancellationTokenOnDestroy();
        try
        {
            await UniTask.WaitForSeconds(delay, cancellationToken: ct);
            if (instance.activeInHierarchy && _pools.TryGetValue(prefabKey, out var pool))
            {
                pool.Release(instance);
            }
        }
        catch (System.OperationCanceledException) { }
    }

    public void SpawnSliceFlash(GameObject flashPrefab, Vector2 position, float angle)
    {
        if (flashPrefab == null) return;

        // Lấy từ pool (dùng hàm Spawn cũ của bạn)
        GameObject flash = Spawn(flashPrefab, position, Quaternion.Euler(0, 0, angle));
        
        // Thu hồi sau 0.2s vì vệt sáng thường rất ngắn
        AutoDespawnAsync(flash, flashPrefab, 0.2f).Forget();
    }
}