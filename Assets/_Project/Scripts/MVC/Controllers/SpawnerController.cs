using UnityEngine;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using R3;

public class SpawnerController : MonoBehaviour
{
    private Camera _mainCamera;

    // --- DEPENDENCIES ---
    [Inject] private readonly FruitPoolService _fruitPoolService;
    [Inject] private readonly GameModel _gameModel;
    [Inject] private readonly GameSettings _gameSettings;
    [Inject] private readonly AudioService _audioService; // [CẬP NHẬT] Tiêm AudioService để phát tiếng ném

    [Header("Audio (Âm thanh)")]
    [Tooltip("Tiếng 'Vút' khi trái cây được bắn lên")]
    [SerializeField] private AudioClip _fruitTossSound; 

    private float bombSpawnChance => _gameSettings.BombSpawnChance;

    private List<FruitData> _bombsList = new List<FruitData>();
    private List<FruitData> _normalFruitsList = new List<FruitData>();

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        _gameModel.CurrentMode
            .Subscribe(mode => PreloadFruitData(mode))
            .RegisterTo(destroyCancellationToken);

        SpawnLoopAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private void PreloadFruitData(GameMode mode)
    {
        _bombsList.Clear();
        _normalFruitsList.Clear();

        var availableFruits = _gameSettings.GetAvailableFruits(mode);
        if (availableFruits == null) return;

        for (int i = 0; i < availableFruits.Count; i++)
        {
            var fruitData = availableFruits[i];
            if (fruitData.isBomb) _bombsList.Add(fruitData);
            else _normalFruitsList.Add(fruitData);
        }
        
        Debug.Log($"<color=green>[Spawner]</color> Đã nạp {availableFruits.Count} vật phẩm cho chế độ {mode}");
    }

    private async UniTaskVoid SpawnLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                // ==========================================
                // [CẬP NHẬT 1] KIỂM TRA CỜ IS_SPAWNING
                // Đợi cho đến khi Game ở trạng thái Playing VÀ HUD đếm ngược xong (IsSpawning = true)
                // ==========================================
                await UniTask.WaitUntil(() => _gameModel.State.Value == GameState.Playing && _gameModel.IsSpawning.Value, cancellationToken: ct);

                float delay = Random.Range(_gameSettings.MinDelay, _gameSettings.MaxDelay);
                await UniTask.WaitForSeconds(delay, cancellationToken: ct);

                // [CẬP NHẬT 2] Kiểm tra lại cờ một lần nữa sau khi delay, đề phòng người chơi vừa bấm Pause
                if (_gameModel.State.Value != GameState.Playing || !_gameModel.IsSpawning.Value) continue;

                SpawnAndLaunch();
            }
        }
        catch (System.OperationCanceledException) { }
    }

    private void SpawnAndLaunch()
    {
        float actualBombChance = (_bombsList.Count > 0) ? bombSpawnChance : 0f;
        bool isSpawningBomb = Random.value < actualBombChance;

        FruitData selectedData = null;

        if (isSpawningBomb)
        {
            selectedData = _bombsList[Random.Range(0, _bombsList.Count)];
        }
        else if (_normalFruitsList.Count > 0)
        {
            selectedData = _normalFruitsList[Random.Range(0, _normalFruitsList.Count)];
        }

        if (selectedData == null) return;

        Vector2 spawnPosition = GetDynamicSpawnPosition();
        
        FruitController fruit = _fruitPoolService.Spawn(selectedData, spawnPosition);

        if (fruit != null)
        {
            ApplyLaunchPhysics(fruit);
            
            // ==========================================
            // [CẬP NHẬT 3] PHÁT ÂM THANH NÉM
            // ==========================================
            if (_audioService != null && _fruitTossSound != null)
            {
                // Cho volume hơi nhỏ (0.4f) để làm nền, không lấn át tiếng chém (Impact)
                _audioService.PlaySFX(_fruitTossSound, volume: 0.4f, randomizePitch: true);
            }
        }
    }

    private void ApplyLaunchPhysics(FruitController fruit)
    {
        float randomAngle = Random.Range(-_gameSettings.MaxAngle, _gameSettings.MaxAngle);
        Quaternion rotation = Quaternion.Euler(0, 0, randomAngle);
        Vector2 forceDirection = rotation * Vector2.up;
        
        float forceMagnitude = Random.Range(_gameSettings.MinForce, _gameSettings.MaxForce);
        Vector2 finalForce = forceDirection * forceMagnitude;
        float randomTorque = Random.Range(_gameSettings.MinTorque, _gameSettings.MaxTorque);

        fruit.Launch(finalForce, randomTorque);
    }

    private Vector2 GetDynamicSpawnPosition()
    {
        float randomX = Random.Range(0.1f, 0.9f);
        Vector3 viewportPos = new Vector3(randomX, -0.1f, Mathf.Abs(_mainCamera.transform.position.z));
        return _mainCamera.ViewportToWorldPoint(viewportPos);
    }
}