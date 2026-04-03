using UnityEngine;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;
using System.Threading;

public class SpawnerController : MonoBehaviour
{
    private Camera _mainCamera;

    // --- DEPENDENCIES (Được Reflex tự động tiêm vào) ---
    [Inject] private readonly FruitPoolService _fruitPoolService;
    [Inject] private readonly GameModel _gameModel;
    [Inject] private readonly GameSettings _gameSettings; 

    private void Awake()
    {
        // Cache Camera để tối ưu hiệu năng
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        // Chạy vòng lặp sinh trái cây vô tận gắn với vòng đời của Object này
        SpawnLoopAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid SpawnLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                // Chỉ sinh trái cây khi Game đang trong trạng thái Playing
                await UniTask.WaitUntil(() => _gameModel.IsPlaying.Value, cancellationToken: ct);

                // Lấy thời gian chờ ngẫu nhiên từ GameSettings
                float delay = Random.Range(_gameSettings.MinDelay, _gameSettings.MaxDelay);
                await UniTask.WaitForSeconds(delay, cancellationToken: ct);

                // Kiểm tra lại lần nữa sau khi chờ (phòng trường hợp vừa thua game trong lúc đợi)
                if (!_gameModel.IsPlaying.Value) continue;

                SpawnAndLaunch();
            }
        }
        catch (System.OperationCanceledException) 
        {
            // Tự động dừng khi chuyển Scene hoặc hủy Object
        }
    }

    private void SpawnAndLaunch()
    {
        // 1. Lấy danh sách trái cây có sẵn từ Settings
        var availableFruits = _gameSettings.AvailableFruits;
        if (availableFruits == null || availableFruits.Count == 0) return;

        // 2. Bốc ngẫu nhiên một loại dữ liệu (Táo, Cam, Bom...)
        FruitData randomData = availableFruits[Random.Range(0, availableFruits.Count)];

        // 3. Tính toán vị trí sinh ra ngẫu nhiên dưới mép màn hình (Dynamic Viewport)
        Vector2 spawnPosition = GetDynamicSpawnPosition();

        // 4. Yêu cầu Pool lấy ra "xác" tương ứng với loại quả này
        // (Lưu ý: FruitPoolService bản Multi-Prefab sẽ dùng 'randomData' để tìm Pool)
        FruitController fruit = _fruitPoolService.Spawn(randomData, spawnPosition);

        if (fruit != null)
        {
            // 5. Tính toán lực ném và góc xoay dựa trên thông số trong GameSettings
            ApplyLaunchPhysics(fruit);
        }
    }

    private void ApplyLaunchPhysics(FruitController fruit)
    {
        // Tạo góc ném ngẫu nhiên nghiêng về phía giữa màn hình
        float randomAngle = Random.Range(-_gameSettings.MaxAngle, _gameSettings.MaxAngle);
        Quaternion rotation = Quaternion.Euler(0, 0, randomAngle);
        Vector2 forceDirection = rotation * Vector2.up;
        
        // Tính toán độ lớn của lực và mô-men xoắn (torque)
        float forceMagnitude = Random.Range(_gameSettings.MinForce, _gameSettings.MaxForce);
        Vector2 finalForce = forceDirection * forceMagnitude;
        float randomTorque = Random.Range(_gameSettings.MinTorque, _gameSettings.MaxTorque);

        // Kích hoạt ném
        fruit.Launch(finalForce, randomTorque);
    }

    private Vector2 GetDynamicSpawnPosition()
    {
        // X chạy từ 10% đến 90% chiều rộng màn hình để tránh sát mép quá
        float randomX = Random.Range(0.1f, 0.9f);
        
        // Y = -0.1f để nằm dưới mép màn hình 10% chiều cao
        Vector3 viewportPos = new Vector3(randomX, -0.1f, Mathf.Abs(_mainCamera.transform.position.z));
        
        return _mainCamera.ViewportToWorldPoint(viewportPos);
    }
}