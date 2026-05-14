using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class FragmentPoolService
{
    private readonly Dictionary<GameObject, IObjectPool<PooledDynamicFragment>> _pools = new();
    private readonly Transform _poolRoot;

    public FragmentPoolService()
    {
        _poolRoot = new GameObject("[SYSTEM]_FragmentPool").transform;
    }

    public PooledDynamicFragment Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        if (!_pools.TryGetValue(prefab, out var pool))
        {
            pool = CreatePoolForPrefab(prefab);
            _pools[prefab] = pool;
        }

        PooledDynamicFragment fragment = pool.Get();
        fragment.transform.position = position;
        fragment.transform.rotation = rotation;
        
        // Truyền thẳng cái Pool quản lý vào để mảnh vỡ biết đường "tự sát"
        fragment.Setup(pool);

        return fragment;
    }

    private IObjectPool<PooledDynamicFragment> CreatePoolForPrefab(GameObject prefab)
    {
        return new ObjectPool<PooledDynamicFragment>(
            createFunc: () => {
                GameObject obj = Object.Instantiate(prefab, _poolRoot);
                return obj.GetComponent<PooledDynamicFragment>();
            },
            actionOnGet: f => f.gameObject.SetActive(true),
            actionOnRelease: f => f.gameObject.SetActive(false),
            actionOnDestroy: f => Object.Destroy(f.gameObject),
            collectionCheck: false,
            defaultCapacity: 10,  // Chém 1 phát ra 2 mảnh, 10 là đủ gánh 5 trái cây cùng lúc
            maxSize: 50           // Tối đa gánh 25 trái cây bị chém nát trên màn hình cùng lúc
        );
    }
}