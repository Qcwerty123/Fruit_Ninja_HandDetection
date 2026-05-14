using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class FruitPoolService
{
    // Dictionary quản lý nhiều Pool cùng lúc dựa trên Prefab gốc
    private readonly Dictionary<GameObject, IObjectPool<FruitController>> _pools = new();
    
    private readonly GameModel _gameModel;
    private readonly ComboService _comboService;
    private readonly VFXPoolService _vfxPoolService;
    private readonly FragmentPoolService _fragmentPoolService;
    private readonly AudioService _audioService;
    private readonly ScreenFlashService _screenFlashService;
    private readonly Transform _poolRoot;

    public FruitPoolService(GameModel gameModel, ComboService comboService, 
                            VFXPoolService vfxPoolService, AudioService audioService, 
                            ScreenFlashService screenFlashService,
                            FragmentPoolService fragmentPoolService)
    {
        _gameModel = gameModel;
        _comboService = comboService;
        _vfxPoolService = vfxPoolService;
        _audioService = audioService;
        _screenFlashService = screenFlashService;
        _fragmentPoolService = fragmentPoolService;

        _poolRoot = new GameObject("[SYSTEM]_FruitPool").transform;
    }

    public FruitController Spawn(FruitData data, Vector3 position)
    {
        if (data == null) return null;

        // --- CẬP NHẬT QUAN TRỌNG: LẤY ĐÚNG PREFAB THEO MODE ---
        // Hỏi GameModel xem đang chơi Mode nào để bốc đúng Prefab tương ứng
        GameObject targetPrefab = _gameModel.CurrentMode.Value == GameMode.Dynamic 
            ? data.dynamicPrefab 
            : data.classicPrefab;

        if (targetPrefab == null)
        {
            Debug.LogWarning($"<color=yellow>[FruitPool]</color> Thiếu Prefab cho chế độ {_gameModel.CurrentMode.Value} ở trái cây: {data.fruitName}");
            return null;
        }

        // Kiểm tra xem đã có Pool cho Prefab này chưa, nếu chưa thì tạo mới
        // Dùng targetPrefab làm Key (Khóa) để không bao giờ bị lẫn lộn giữa Classic và Dynamic
        if (!_pools.TryGetValue(targetPrefab, out var pool))
        {
            pool = CreatePoolForPrefab(targetPrefab);
            _pools[targetPrefab] = pool;
        }

        FruitController fruit = pool.Get();
        fruit.transform.position = position;
        
        // Truyền chính cái 'pool' quản lý nó vào hàm Setup để nó biết đường 'về nhà'
        fruit.Setup(data, pool, _gameModel, _comboService, _vfxPoolService, _audioService, _screenFlashService, _fragmentPoolService);

        return fruit;
    }

    private IObjectPool<FruitController> CreatePoolForPrefab(GameObject prefab)
    {
        return new ObjectPool<FruitController>(
            createFunc: () => {
                GameObject obj = Object.Instantiate(prefab, _poolRoot);
                return obj.GetComponent<FruitController>(); // Tự động nhận diện ClassicFruit hoặc DynamicFruit
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