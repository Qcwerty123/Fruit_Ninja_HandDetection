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

    private float bombSpawnChance => _gameSettings.BombSpawnChance;

    // BỘ ĐỆM CACHE TRÁNH RÁC BỘ NHỚ (Garbage Collection)
    private List<FruitData> _bombsList = new List<FruitData>();
    private List<FruitData> _normalFruitsList = new List<FruitData>();

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        // TIỀN XỬ LÝ DỮ LIỆU: Phân loại 1 lần duy nhất lúc bật game
        PreloadFruitData();

        SpawnLoopAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private void PreloadFruitData()
    {
        var availableFruits = _gameSettings.AvailableFruits;
        if (availableFruits == null) return;

        // Quét bằng vòng lặp for truyền thống thay vì LINQ để tốc độ bàn thờ nhất
        for (int i = 0; i < availableFruits.Count; i++)
        {
            var fruitData = availableFruits[i];
            if (fruitData.isBomb)
            {
                _bombsList.Add(fruitData);
            }
            else
            {
                _normalFruitsList.Add(fruitData);
            }
        }
    }

    private async UniTaskVoid SpawnLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await UniTask.WaitUntil(() => _gameModel.State.Value == GameState.Playing, cancellationToken: ct);

                float delay = Random.Range(_gameSettings.MinDelay, _gameSettings.MaxDelay);
                await UniTask.WaitForSeconds(delay, cancellationToken: ct);

                if (_gameModel.State.Value != GameState.Playing) continue;

                SpawnAndLaunch();
            }
        }
        catch (System.OperationCanceledException) { }
    }

    private void SpawnAndLaunch()
    {
        // 1. Kiểm tra tỷ lệ có Bom thực tế không
        float actualBombChance = (_bombsList.Count > 0) ? bombSpawnChance : 0f;

        // 2. Quay số ngẫu nhiên
        bool isSpawningBomb = Random.value < actualBombChance;

        FruitData selectedData = null;

        // Lấy dữ liệu TRỰC TIẾP TỪ BỘ ĐỆM CACHE, không tính toán lại
        if (isSpawningBomb)
        {
            selectedData = _bombsList[Random.Range(0, _bombsList.Count)];
        }
        else if (_normalFruitsList.Count > 0)
        {
            selectedData = _normalFruitsList[Random.Range(0, _normalFruitsList.Count)];
        }

        if (selectedData == null) return;

        // 3. Tính toán vị trí và yêu cầu Pool
        Vector2 spawnPosition = GetDynamicSpawnPosition();
        FruitController fruit = _fruitPoolService.Spawn(selectedData, spawnPosition);

        if (fruit != null)
        {
            ApplyLaunchPhysics(fruit);
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