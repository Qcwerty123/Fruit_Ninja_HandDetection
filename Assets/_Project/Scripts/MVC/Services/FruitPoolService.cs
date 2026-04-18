using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

public class FruitPoolService
{
    // Dictionary quản lý nhiều Pool cùng lúc dựa trên Prefab gốc
    private readonly Dictionary<GameObject, IObjectPool<FruitController>> _pools = new();
    
    private readonly GameModel _gameModel;
    private readonly ComboService _comboService;
    private readonly VFXPoolService _vfxPoolService;
    private readonly AudioService _audioService;
    private readonly ScreenFlashService _screenFlashService;
    private readonly Transform _poolRoot;

    public FruitPoolService(GameModel gameModel, ComboService comboService, VFXPoolService vfxPoolService, AudioService audioService, ScreenFlashService screenFlashService)
    {
        _gameModel = gameModel;
        _comboService = comboService;
        _vfxPoolService = vfxPoolService;
        _audioService = audioService;
        _screenFlashService = screenFlashService;

        _poolRoot = new GameObject("[SYSTEM]_FruitPool").transform;
    }

    public FruitController Spawn(FruitData data, Vector3 position)
    {
        if (data.fruitPrefab == null) return null;

        // Kiểm tra xem đã có Pool cho Prefab này chưa, nếu chưa thì tạo mới
        if (!_pools.TryGetValue(data.fruitPrefab, out var pool))
        {
            pool = CreatePoolForPrefab(data.fruitPrefab);
            _pools[data.fruitPrefab] = pool;
        }

        FruitController fruit = pool.Get();
        fruit.transform.position = position;
        
        // Truyền chính cái 'pool' quản lý nó vào hàm Setup để nó biết đường 'về nhà'
        fruit.Setup(data, pool, _gameModel, _comboService, _vfxPoolService, _audioService, _screenFlashService);

        return fruit;
    }

    private IObjectPool<FruitController> CreatePoolForPrefab(GameObject prefab)
    {
        return new ObjectPool<FruitController>(
            createFunc: () => {
                GameObject obj = Object.Instantiate(prefab, _poolRoot);
                return obj.GetComponent<FruitController>();
            },
            actionOnGet: f => f.gameObject.SetActive(true),
            actionOnRelease: f => f.gameObject.SetActive(false),
            actionOnDestroy: f => Object.Destroy(f.gameObject),
            collectionCheck: false,
            defaultCapacity: 5,
            maxSize: 20
        );
    }
}