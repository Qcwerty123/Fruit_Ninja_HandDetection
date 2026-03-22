using UnityEngine;
using Reflex.Attributes;

public class SpawnerController : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private Collider2D spawnArea;

    [Inject] private readonly FruitPoolService _fruitPoolService;
    [Inject] private readonly GameModel _gameModel;

    private float _spawnTimer; 

    private void Start()
    {
        _spawnTimer = 1f; // Delay lượt ném đầu tiên
    }

    private void Update()
    {
        // Khóa Spawner nếu game chưa bắt đầu hoặc đã Game Over
        if (_gameModel == null || !_gameModel.IsPlaying.Value || _gameModel.IsGameOver.Value) return;

        _spawnTimer -= Time.deltaTime;

        if (_spawnTimer <= 0f)
        {
            SpawnFruit();
            _spawnTimer = Random.Range(gameSettings.minSpawnDelay, gameSettings.maxSpawnDelay);
        }
    }

    private void SpawnFruit()
    {
        Bounds bounds = spawnArea.bounds;
        Vector2 spawnPos = new Vector2(Random.Range(bounds.min.x, bounds.max.x), bounds.min.y);
        
        float screenCenterX = 0f;
        float directionX = (screenCenterX - spawnPos.x) * Random.Range(0.5f, 1.5f);
        Vector2 force = new Vector2(directionX, Random.Range(gameSettings.minSpawnForce, gameSettings.maxSpawnForce));
        float torque = Random.Range(-5f, 5f);

        _fruitPoolService.Spawn(spawnPos, force, torque);
    }
}