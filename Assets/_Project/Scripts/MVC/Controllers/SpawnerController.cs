using UnityEngine;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using R3;

public class SpawnerController : MonoBehaviour
{
    private Camera _mainCamera;

    [Inject] private readonly FruitPoolService _fruitPoolService;
    [Inject] private readonly GameModel _gameModel;
    [Inject] private readonly GameSettings _gameSettings;
    [Inject] private readonly AudioService _audioService;

    [Header("Audio (Âm thanh)")]
    [SerializeField] private AudioClip _fruitTossSound; 

    [Header("Thiết lập Độ khó (Difficulty Curve)")]
    [Tooltip("Thời gian (giây) để game đạt đến độ khó tối đa")]
    [SerializeField] private float _timeToMaxDifficulty = 90f; 
    [Tooltip("Khoảng thời gian nghỉ CỘNG THÊM lúc mới vào game (giúp nhịp độ chậm lại)")]
    [SerializeField] private float _initialExtraDelay = 2.0f;
    [Tooltip("Số giây an toàn đầu game (Không ra bom)")]
    [SerializeField] private float _safeTimeNoBombs = 10f;

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

        foreach (var fruitData in availableFruits)
        {
            if (fruitData.isBomb) _bombsList.Add(fruitData);
            else _normalFruitsList.Add(fruitData);
        }
    }

    private async UniTaskVoid SpawnLoopAsync(CancellationToken ct)
    {
        try
        {
            float currentPlayTime = 0f; // Bộ đếm thời gian của ván chơi hiện tại

            while (!ct.IsCancellationRequested)
            {
                await UniTask.WaitUntil(() => _gameModel.State.Value == GameState.Playing && _gameModel.IsSpawning.Value, cancellationToken: ct);

                // Nếu vừa vào ván mới (hoặc chơi lại), reset bộ đếm
                if (_gameModel.Score.Value == 0 && currentPlayTime > 0)
                {
                    currentPlayTime = 0f;
                }

                // ==========================================
                // 1. TÍNH TOÁN HỆ SỐ ĐỘ KHÓ (0.0 -> 1.0)
                // ==========================================
                float difficultyFactor = Mathf.Clamp01(currentPlayTime / _timeToMaxDifficulty);

                // ==========================================
                // 2. NHỊP ĐỘ CHẬM -> NHANH (PACING)
                // ==========================================
                // Lúc mới chơi: Delay = Delay gốc + ExtraDelay (rất chậm)
                // Về sau: ExtraDelay giảm dần về 0 (nhịp độ nhanh dần về chuẩn)
                float currentExtraDelay = Mathf.Lerp(_initialExtraDelay, 0f, difficultyFactor);
                
                float waveDelay = Random.Range(_gameSettings.MinDelay, _gameSettings.MaxDelay) + currentExtraDelay;
                
                await UniTask.WaitForSeconds(waveDelay, cancellationToken: ct);
                
                // Cộng dồn thời gian chơi
                currentPlayTime += waveDelay;

                if (_gameModel.State.Value != GameState.Playing || !_gameModel.IsSpawning.Value) continue;

                // ==========================================
                // 3. TỶ LỆ BOM (BOMB CHANCE TĂNG DẦN)
                // ==========================================
                float currentBombChance = 0f;
                if (currentPlayTime > _safeTimeNoBombs) // Hết thời gian an toàn mới bắt đầu có bom
                {
                    // Tỷ lệ bom tăng dần từ 0 lên mức Max cấu hình trong GameSettings
                    float bombFactor = Mathf.Clamp01((currentPlayTime - _safeTimeNoBombs) / (_timeToMaxDifficulty - _safeTimeNoBombs));
                    currentBombChance = Mathf.Lerp(0f, _gameSettings.BombSpawnChance, bombFactor);
                }

                // ==========================================
                // 4. MẪU NÉM ĐA DẠNG DẦN (WAVE PATTERNS)
                // ==========================================
                // Ban đầu: 100% ném 1 quả. Về sau: giảm dần xuống chỉ còn 35%, nhường chỗ cho Combo
                float singleChance = Mathf.Lerp(1.0f, 0.35f, difficultyFactor);
                
                float patternRoll = Random.value;
                int fruitCount = Random.Range(2, 5); 

                if (patternRoll < singleChance) 
                {
                    SpawnAndLaunch(currentBombChance);
                }
                else if (patternRoll < singleChance + ((1f - singleChance) / 2f)) // Chia đều % còn lại cho Đồng thời và Liên thanh
                {
                    for (int i = 0; i < fruitCount; i++)
                    {
                        float viewportX = Mathf.Lerp(0.1f, 0.9f, (float)i / (fruitCount - 1));
                        SpawnAndLaunch(currentBombChance, viewportX);
                    }
                }
                else 
                {
                    for (int i = 0; i < fruitCount; i++)
                    {
                        SpawnAndLaunch(currentBombChance);
                        float seqDelay = Random.Range(0.15f, 0.25f);
                        await UniTask.WaitForSeconds(seqDelay, cancellationToken: ct);
                        currentPlayTime += seqDelay; // Cộng dồn cả thời gian ném liên thanh
                    }
                }
            }
        }
        catch (System.OperationCanceledException) { }
    }

    /// <summary>
    /// Nhận thêm tham số currentBombChance được truyền từ hệ thống cân bằng độ khó
    /// </summary>
    private void SpawnAndLaunch(float currentBombChance, float? forcedViewportX = null)
    {
        float actualBombChance = (_bombsList.Count > 0) ? currentBombChance : 0f;
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

        float viewportX = forcedViewportX ?? Random.Range(0.1f, 0.9f);
        Vector3 viewportPos = new Vector3(viewportX, -0.1f, Mathf.Abs(_mainCamera.transform.position.z));
        Vector2 spawnPosition = _mainCamera.ViewportToWorldPoint(viewportPos);
        
        FruitController fruit = _fruitPoolService.Spawn(selectedData, spawnPosition);

        if (fruit != null)
        {
            ApplyLaunchPhysics(fruit, viewportX); 
            
            if (_audioService != null && _fruitTossSound != null)
            {
                _audioService.PlaySFX(_fruitTossSound, volume: 0.4f, randomizePitch: true);
            }
        }
    }

    private void ApplyLaunchPhysics(FruitController fruit, float viewportX)
    {
        float targetAngle = Mathf.Lerp(-_gameSettings.MaxAngle, _gameSettings.MaxAngle, (viewportX - 0.1f) / 0.8f);
        float finalAngle = targetAngle + Random.Range(-5f, 5f);
        
        Quaternion rotation = Quaternion.Euler(0, 0, finalAngle);
        Vector2 forceDirection = rotation * Vector2.up;
        
        float forceMagnitude = Random.Range(_gameSettings.MinForce, _gameSettings.MaxForce);
        Vector2 finalForce = forceDirection * forceMagnitude;
        
        float randomTorque = Random.Range(_gameSettings.MinTorque, _gameSettings.MaxTorque);
        int torqueSign = Random.value > 0.5f ? 1 : -1;
        float finalTorque = randomTorque * torqueSign * 0.3f; 

        fruit.Launch(finalForce, finalTorque);
    }
}