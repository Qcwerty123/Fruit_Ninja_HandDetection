using UnityEngine;
using TMPro;
using Reflex.Attributes;
using R3;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;

public class HUDView : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    
    // [CẬP NHẬT 1] Thêm biến Text để hiển thị Best Score
    [SerializeField] private TextMeshProUGUI bestScoreText; 
    
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private Button pauseButton;
    [SerializeField] private DwellButton dwellPauseButton;

    [Header("Start Sequence (Hiệu ứng bắt đầu)")]
    [SerializeField] private TextMeshProUGUI startText; 
    [SerializeField] private AudioClip startSound; 

    [Header("GameOver Sequence")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private AudioClip gameOverSound;
    
    [Inject] private readonly GameModel _gameModel;
    [Inject] private readonly AudioService _audioService;
    
    private CancellationTokenSource _scoreAnimCts;
    
    // [CẬP NHẬT 2] Biến lưu kỷ lục tạm thời trong ván chơi
    private int _currentBestScore = 0;

    private void OnEnable()
    {
        // [CẬP NHẬT 3] Mỗi lần bật HUD lên (bắt đầu ván mới), lấy Kỷ lục mới nhất từ bộ nhớ
        _currentBestScore = PlayerPrefs.GetInt("FruitNinja_HighScore", 0);
        if (bestScoreText != null)
        {
            bestScoreText.text = $"BEST: {_currentBestScore}";
        }

        if (startText != null)
        {
            PlayStartSequenceAsync().Forget();
        }
    }

    private void Start()
    {
        if (pauseButton == null) pauseButton = GetComponentInChildren<Button>();
        if (dwellPauseButton == null) dwellPauseButton = GetComponentInChildren<DwellButton>();
        
        if (pauseButton != null) pauseButton.onClick.AddListener(() => _gameModel.TogglePause());
        if (dwellPauseButton != null) dwellPauseButton.onDwellClick.AddListener(() => _gameModel.TogglePause());

        _gameModel.Score.Subscribe(UpdateScoreDisplay).RegisterTo(destroyCancellationToken);

        _gameModel.Lives.Subscribe(lives => {
            livesText.text = $"LIVES: {Mathf.Max(0, lives)}";
        }).RegisterTo(destroyCancellationToken);

        // Tắt chữ GameOver lúc đầu
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
    }

    // ==========================================
    // HIỆU ỨNG READY... GO!!
    // ==========================================
    private async UniTaskVoid PlayStartSequenceAsync()
    {
        startText.gameObject.SetActive(true);
        CancellationToken ct = this.GetCancellationTokenOnDestroy();

        try
        {
            // --- NHỊP 1: READY ---
            startText.text = "READY";
            startText.color = Color.yellow; 
            
            if (_audioService != null && startSound != null) 
                _audioService.PlaySFX(startSound, volume: 0.8f);

            await ScaleTextAsync(Vector3.zero, Vector3.one, 0.3f, ct); 
            await UniTask.WaitForSeconds(0.6f, cancellationToken: ct); 
            await ScaleTextAsync(Vector3.one, Vector3.zero, 0.15f, ct); 

            // --- NHỊP 2: GO!! ---
            startText.text = "GO!!";
            startText.color = Color.green;

            await ScaleTextAsync(Vector3.zero, Vector3.one * 1.3f, 0.3f, ct); 
            await UniTask.WaitForSeconds(0.5f, cancellationToken: ct); 
            await ScaleTextAsync(Vector3.one * 1.3f, Vector3.zero, 0.2f, ct); 

            startText.gameObject.SetActive(false);

            // Báo hiệu cho Spawner biết là đã đếm ngược xong
            _gameModel.IsSpawning.Value = true; 
        }
        catch (System.OperationCanceledException) { }
    }

    private async UniTask ScaleTextAsync(Vector3 from, Vector3 to, float duration, CancellationToken ct)
    {
        float elapsed = 0;
        bool isBouncing = from.sqrMagnitude < to.sqrMagnitude; 

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            if (isBouncing)
            {
                float c1 = 1.70158f;
                float c3 = c1 + 1f;
                float easeT = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                startText.transform.localScale = Vector3.LerpUnclamped(from, to, easeT);
            }
            else
            {
                startText.transform.localScale = Vector3.Lerp(from, to, t);
            }
            
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
        startText.transform.localScale = to;
    }

    // ==========================================
    // HIỆU ỨNG GAME OVER
    // ==========================================    

    public async UniTask PlayGameOverSequenceAsync()
    {
        if (gameOverText == null) return;

        gameOverText.gameObject.SetActive(true);
        gameOverText.text = "GAME OVER";
        gameOverText.transform.localScale = Vector3.zero;

        if (_audioService != null && gameOverSound != null)
            _audioService.PlaySFX(gameOverSound, volume: 1f);

        CancellationToken ct = this.GetCancellationTokenOnDestroy();

        // 1. Phóng to
        float duration = 0.5f;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float easeT = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
            
            gameOverText.transform.localScale = Vector3.one * easeT;
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
        gameOverText.transform.localScale = Vector3.one;

        // 2. Dừng hình 1.5 giây để người xem tiếc nuối
        await UniTask.WaitForSeconds(1.5f, cancellationToken: ct);

        // 3. Tắt chữ
        gameOverText.gameObject.SetActive(false);
    }

    // ==========================================
    // HIỆU ỨNG ĐIỂM SỐ
    // ==========================================
    private void UpdateScoreDisplay(int newScore)
    {
        scoreText.text = newScore.ToString();
        if (newScore > 0) PlayScoreBounce().Forget();

        // [CẬP NHẬT 4] Cập nhật Best Score trực tiếp nếu phá kỷ lục
        if (newScore > _currentBestScore)
        {
            _currentBestScore = newScore;
            if (bestScoreText != null)
            {
                bestScoreText.text = $"BEST: {_currentBestScore}";
            }
        }
    }

    private async UniTaskVoid PlayScoreBounce()
    {
        _scoreAnimCts?.Cancel();
        _scoreAnimCts = new CancellationTokenSource();
        var ct = _scoreAnimCts.Token;

        try
        {
            float duration = 0.1f;
            Vector3 originalScale = Vector3.one;
            Vector3 targetScale = Vector3.one * 1.3f;

            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                scoreText.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                scoreText.transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            
            scoreText.transform.localScale = originalScale;
        }
        catch (System.OperationCanceledException) { }
    }

    private void OnDestroy()
    {
        _scoreAnimCts?.Cancel();
        _scoreAnimCts?.Dispose();
    }
}