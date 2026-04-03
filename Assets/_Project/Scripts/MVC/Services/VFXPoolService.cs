using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;
using System.Threading;

public class VFXPoolService
{
    private readonly Dictionary<GameObject, IObjectPool<GameObject>> _pools = new();
    
    // Biến lưu trữ thư mục gốc
    private readonly Transform _rootTransform;

    public VFXPoolService()
    {
        // Tự động tạo một Empty GameObject trên Scene khi Service được khởi tạo
        GameObject root = new GameObject("[SYSTEM]_VFXPool");
        _rootTransform = root.transform;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        if (!_pools.TryGetValue(prefab, out var pool))
        {
            pool = new ObjectPool<GameObject>(
                createFunc: () => 
                {
                    // Ép nó làm con của thư mục gốc ngay khi vừa Instantiate
                    GameObject obj = Object.Instantiate(prefab, _rootTransform);
                    obj.name = $"[VFX]_{prefab.name}"; 
                    obj.SetActive(false);
                    return obj;
                },
                actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj => 
                {
                    obj.SetActive(false);
                    // Đưa nó về lại thư mục gốc (đề phòng trường hợp trong lúc chơi nó bị đổi cha)
                    obj.transform.SetParent(_rootTransform); 
                },
                actionOnDestroy: Object.Destroy,
                collectionCheck: false,
                defaultCapacity: 10,
                maxSize: 50
            );
            
            _pools[prefab] = pool;
        }

        GameObject instance = pool.Get();
        instance.transform.SetPositionAndRotation(position, rotation);
        
        AutoDespawnAsync(instance, prefab, 3f).Forget();

        return instance;
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
}