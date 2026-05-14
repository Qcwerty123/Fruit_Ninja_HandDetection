using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Reflex.Attributes;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class GameOverView : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform panelContainer;
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    [Header("Audio (Âm thanh)")]
    [Tooltip("Âm thanh chốt điểm (VD: Tiếng Ting, Bell hoặc Cash Register)")]
    [SerializeField] private AudioClip scoreFinishSound;

    [Header("Nút Retry (Chơi lại)")]
    [SerializeField] private Button retryButton; 
    [SerializeField] private DwellButton dwellRetryButton; 

    [Header("Nút Settings (Cài đặt)")]
    [SerializeField] private Button settingsButton; 
    [SerializeField] private DwellButton dwellSettingsButton; 

    [Header("Nút Home (Về Main Menu)")]
    [SerializeField] private Button homeButton; 
    [SerializeField] private DwellButton dwellHomeButton; 

    [Inject] private readonly GameModel _gameModel;
    [Inject] private readonly AudioService _audioService; // Tiêm AudioService để phát tiếng

    private void Start()
    {
        // --- 1. SỰ KIỆN RETRY ---
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);
        if (dwellRetryButton != null)
            dwellRetryButton.onDwellClick.AddListener(OnRetryClicked);

        // --- 2. SỰ KIỆN SETTINGS ---
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
        if (dwellSettingsButton != null)
            dwellSettingsButton.onDwellClick.AddListener(OpenSettings);

        // --- 3. SỰ KIỆN MAIN MENU ---
        if (homeButton != null)
            homeButton.onClick.AddListener(ReturnToMainMenu);
        if (dwellHomeButton != null)
            dwellHomeButton.onDwellClick.AddListener(ReturnToMainMenu);
    }

    // Gọi TỰ ĐỘNG khi UIManager bật Panel này lên
    private void OnEnable()
    {
        // Khởi chạy chuỗi hiệu ứng Game Over
        StartGameOverSequenceAsync().Forget();
    }

    private async UniTaskVoid StartGameOverSequenceAsync()
    {
        // 1. Reset text về 0 và lấy kỷ lục cũ
        currentScoreText.text = "0";
        int oldHighScore = PlayerPrefs.GetInt("FruitNinja_HighScore", 0);
        highScoreText.text = "Best: " + oldHighScore.ToString();

        // 2. Chờ hiệu ứng bật Panel mở ra xong (0.25s)
        await AnimatePanelAsync();

        // 3. Bắt đầu chạy hiệu ứng số điểm
        await AnimateScoreTallyAsync(_gameModel.Score.Value, oldHighScore);
    }

    private async UniTask AnimateScoreTallyAsync(int finalScore, int oldHighScore)
    {
        float duration = 1.0f; // Thời gian chạy số (1 giây)
        float elapsed = 0f;

        // --- BƯỚC 1: CHẠY SỐ ĐIỂM HIỆN TẠI ---
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f); // Ease-Out Cubic (Chạy nhanh lúc đầu, chậm dần về cuối)
            
            int currentVal = Mathf.RoundToInt(Mathf.Lerp(0, finalScore, easeT));
            currentScoreText.text = currentVal.ToString();
            
            await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
        }
        // Đảm bảo số cuối cùng chính xác tuyệt đối
        currentScoreText.text = finalScore.ToString();

        // Phát âm thanh khi chốt xong điểm
        if (_audioService != null && scoreFinishSound != null)
        {
            await PlayBounceAnimation(currentScoreText.transform);
            _audioService.PlaySFX(scoreFinishSound, volume: 1f, randomizePitch: false);
        }

        // --- BƯỚC 2: KIỂM TRA PHÁ KỶ LỤC ---
        if (finalScore > oldHighScore)
        {
            // Nghỉ 0.5 giây để người chơi nhận ra mình vừa vượt kỷ lục
            await UniTask.WaitForSeconds(0.5f, ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());

            // Lưu kỷ lục mới vào hệ thống
            PlayerPrefs.SetInt("FruitNinja_HighScore", finalScore);
            PlayerPrefs.Save();

            // Đổi chữ sang kỷ lục mới
            highScoreText.text = "Best: " + finalScore.ToString();

            // Phát chung file âm thanh nhưng cho Pitch cao hơn một chút để nghe phấn khích hơn
            if (_audioService != null && scoreFinishSound != null)
            {
                _audioService.PlaySFX(scoreFinishSound, volume: 1f, randomizePitch: true); 
            }

            // Chạy hiệu ứng nảy to chữ Best Score lên
            await PlayBounceAnimation(highScoreText.transform);
        }
    }

    // Hàm hỗ trợ tạo hiệu ứng nảy (Scale Bounce) cho bất kỳ Transform nào
    private async UniTask PlayBounceAnimation(Transform target)
    {
        float bounceDuration = 0.15f;
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = Vector3.one * 1.4f; // Phóng to 1.4 lần

        float elapsed = 0;
        // Phóng to
        while (elapsed < bounceDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / bounceDuration);
            await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
        }

        elapsed = 0;
        // Thu nhỏ lại bình thường
        while (elapsed < bounceDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / bounceDuration);
            await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
        }
        target.localScale = originalScale;
    }

    private void OnRetryClicked()
    {
        Time.timeScale = 1f; 
        if (dwellRetryButton != null) dwellRetryButton.ResetButton(); 
        
        _gameModel.StartGame(); 
    }

    private void OpenSettings()
    {
        if (dwellSettingsButton != null) dwellSettingsButton.ResetButton();
        _gameModel.IsSettingsOpen.Value = true;
    }

    private void ReturnToMainMenu()
    {
        if (dwellHomeButton != null) dwellHomeButton.ResetButton();
        
        Time.timeScale = 1f; 
        _gameModel.State.Value = GameState.MainMenu;
    }

    private async UniTask AnimatePanelAsync()
    {
        panelContainer.localScale = Vector3.zero;
        float elapsed = 0f;
        float duration = 0.25f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
            
            panelContainer.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, easeT);
            await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
        }
        panelContainer.localScale = Vector3.one;
    }
}