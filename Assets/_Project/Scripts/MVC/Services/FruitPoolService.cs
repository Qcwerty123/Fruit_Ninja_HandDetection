using UnityEngine;
using UnityEngine.Pool;

public class FruitPoolService
{
    private readonly IObjectPool<FruitController> _pool;
    private readonly GameModel _gameModel;
    private readonly FruitData _defaultData;

    public FruitPoolService(FruitController prefab, FruitData defaultData, GameModel gameModel)
    {
        _gameModel = gameModel;
        _defaultData = defaultData;

        _pool = new ObjectPool<FruitController>(
            createFunc: () => {
                FruitController instance = Object.Instantiate(prefab);
                instance.gameObject.SetActive(false);
                return instance;
            },
            actionOnGet: fruit => fruit.gameObject.SetActive(true),
            actionOnRelease: fruit => fruit.gameObject.SetActive(false),
            actionOnDestroy: fruit => Object.Destroy(fruit.gameObject),
            collectionCheck: false, // Tắt check an toàn để tối đa tốc độ
            defaultCapacity: 20,
            maxSize: 50
        );
    }

    public FruitController Spawn(Vector2 position, Vector2 force, float torque)
    {
        FruitController fruit = _pool.Get();
        fruit.Setup(_defaultData, _pool, _gameModel);
        fruit.Launch(position, force, torque);
        return fruit;
    }
}