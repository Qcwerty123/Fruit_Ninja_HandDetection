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
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private Button pauseButton;
    [SerializeField] private DwellButton dwellPauseButton;

    [Inject] private readonly GameModel _gameModel;
    
    private CancellationTokenSource _scoreAnimCts;

    private void Start()
    {
        // 1. Tự động gán sự kiện cho nút Pause (nếu chưa kéo tay trong Inspector)
        if (pauseButton == null) pauseButton = GetComponentInChildren<Button>();
        if (dwellPauseButton == null) dwellPauseButton = GetComponentInChildren<DwellButton>();
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(() => _gameModel.TogglePause());
        }

        if (dwellPauseButton != null)
        {
            dwellPauseButton.onDwellClick.AddListener(() => _gameModel.TogglePause());
        }

        // 2. Lắng nghe Điểm số: Mỗi khi Score thay đổi, cập nhật Text và chạy hiệu ứng
        _gameModel.Score
            .Subscribe(UpdateScoreDisplay)
            .RegisterTo(destroyCancellationToken);

        // 3. Lắng nghe Mạng sống: Cập nhật Text tương ứng
        _gameModel.Lives
            .Subscribe(lives => {
                livesText.text = $"LIVES: {Mathf.Max(0, lives)}";
            })
            .RegisterTo(destroyCancellationToken);
    }

    private void UpdateScoreDisplay(int newScore)
    {
        scoreText.text = newScore.ToString();
        
        // Chạy hiệu ứng phóng to thu nhỏ nhẹ khi tăng điểm
        if (newScore > 0) PlayScoreBounce().Forget();
    }

    private async UniTaskVoid PlayScoreBounce()
    {
        // Hủy hiệu ứng cũ nếu đang chạy để tránh chồng chéo
        _scoreAnimCts?.Cancel();
        _scoreAnimCts = new CancellationTokenSource();
        var ct = _scoreAnimCts.Token;

        try
        {
            float duration = 0.1f;
            Vector3 originalScale = Vector3.one;
            Vector3 targetScale = Vector3.one * 1.3f;

            // Phóng to
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                scoreText.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            // Thu nhỏ về cũ
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