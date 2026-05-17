using UnityEngine;
using TMPro;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;
using System.Threading;

public class GameOverView : MonoBehaviour
{
    [Header("Cinematic Background")]
    [Tooltip("Tấm nền đen mờ bao phủ toàn màn hình")]
    [SerializeField] private CanvasGroup _backgroundDimmer; 

    [Header("UI Elements (Bảng điểm)")]
    [SerializeField] private Transform _panelContainer;
    [SerializeField] private TextMeshProUGUI _currentScoreText;
    [SerializeField] private TextMeshProUGUI _highScoreText;

    [Header("Quản lý Nút Chém (Physical Buttons)")]
    [Tooltip("Kéo GameObject chứa 2 nút chém Retry và Home ngoài Scene vào đây")]
    [SerializeField] private GameObject _buttonsContainer;
    [SerializeField] private SliceableButton _retrySliceButton; 
    [SerializeField] private SliceableButton _homeSliceButton; 

    [Header("Audio (Âm thanh Juice)")]
    [SerializeField] private AudioClip _scoreTickSound;   // Tiếng lách cách nhẹ khi số đang chạy
    [SerializeField] private AudioClip _scoreFinishSound; // Tiếng "Ting" khi chốt điểm hiện tại
    [SerializeField] private AudioClip _newRecordSound;   // Tiếng reo hò/kèn vinh danh khi phá kỷ lục

    [Inject] private readonly GameModel _gameModel;
    [Inject] private readonly AudioService _audioService; 

    private CancellationTokenSource _cts;

    private void Start()
    {
        // Đăng ký sự kiện nét chém thay cho Click chuột
        if (_retrySliceButton != null) _retrySliceButton.OnSlicedEvent.AddListener(OnRetrySliced);
        if (_homeSliceButton != null) _homeSliceButton.OnSlicedEvent.AddListener(ReturnToMainMenuSliced);
    }

    private void OnEnable()
    {
        // BẮT BUỘC: Mở khóa thời gian để các mảnh vỡ nút bấm có thể rơi tự do khi bị chém
        Time.timeScale = 1f;

        if (_buttonsContainer != null) _buttonsContainer.SetActive(true);

        _cts = new CancellationTokenSource();
        StartGameOverSequenceAsync(_cts.Token).Forget();
    }

    private void OnDisable()
    {
        // Hủy các luồng hiệu ứng đang chạy dở nếu Panel bị tắt đột ngột
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        if (_buttonsContainer != null) _buttonsContainer.SetActive(false);
    }

    private async UniTaskVoid StartGameOverSequenceAsync(CancellationToken token)
    {
        // 1. Reset trạng thái ban đầu
        _currentScoreText.text = "0";
        int oldHighScore = PlayerPrefs.GetInt("FruitNinja_HighScore", 0);
        _highScoreText.text = "BEST: " + oldHighScore.ToString();
        
        if (_backgroundDimmer != null) _backgroundDimmer.alpha = 0f;
        _panelContainer.localScale = Vector3.zero;

        // 2. Chạy hiệu ứng xuất hiện (Mờ nền + Bật Bảng)
        await AnimateIntroAsync(token);

        // 3. Chạy hiệu ứng đếm số điểm
        await AnimateScoreTallyAsync(_gameModel.Score.Value, oldHighScore, token);
    }

    private async UniTask AnimateIntroAsync(CancellationToken token)
    {
        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // Nền đen từ từ hiện rõ (Alpha lên 0.7 để vẫn nhìn thấy mờ mờ cảnh game đằng sau)
            if (_backgroundDimmer != null)
                _backgroundDimmer.alpha = Mathf.Lerp(0f, 0.7f, t);

            // Hiệu ứng Back-Ease-Out (Phóng to lố ra ngoài 1 tí rồi giật lại kích thước chuẩn)
            float easeT = Mathf.Clamp01(t);
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float bounceScale = 1f + c3 * Mathf.Pow(easeT - 1f, 3f) + c1 * Mathf.Pow(easeT - 1f, 2f);

            _panelContainer.localScale = Vector3.one * Mathf.Max(0, bounceScale);

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        
        if (_backgroundDimmer != null) _backgroundDimmer.alpha = 0.7f;
        _panelContainer.localScale = Vector3.one;
    }

    private async UniTask AnimateScoreTallyAsync(int finalScore, int oldHighScore, CancellationToken token)
    {
        // Đợi 0.2s cho người chơi định hình bảng điểm rồi mới đếm số
        await UniTask.WaitForSeconds(0.2f, ignoreTimeScale: true, cancellationToken: token);

        float duration = 1.2f; 
        float elapsed = 0f;
        int lastTickValue = -1;

        // --- BƯỚC 1: CHẠY SỐ TỪNG BƯỚC ---
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f); // Chạy nhanh lúc đầu, rề rề lúc sau
            
            int currentVal = Mathf.RoundToInt(Mathf.Lerp(0, finalScore, easeT));
            _currentScoreText.text = currentVal.ToString();

            // Phát tiếng lách cách mỗi khi số nhảy sang đơn vị mới
            if (currentVal != lastTickValue && currentVal > 0)
            {
                lastTickValue = currentVal;
                if (_audioService != null && _scoreTickSound != null)
                {
                    _audioService.PlaySFX(_scoreTickSound, volume: 0.5f, randomizePitch: true);
                }
            }
            
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        
        _currentScoreText.text = finalScore.ToString();

        // Chốt điểm: Phát âm thanh Ting và nảy số
        if (_audioService != null && _scoreFinishSound != null)
            _audioService.PlaySFX(_scoreFinishSound, volume: 1f, randomizePitch: false);
        await PlayBounceAnimation(_currentScoreText.transform, 1.4f, token);

        // --- BƯỚC 2: KIỂM TRA PHÁ KỶ LỤC ---
        if (finalScore > oldHighScore)
        {
            await UniTask.WaitForSeconds(0.4f, ignoreTimeScale: true, cancellationToken: token);

            PlayerPrefs.SetInt("FruitNinja_HighScore", finalScore);
            PlayerPrefs.Save();

            _highScoreText.text = "NEW RECORD: " + finalScore.ToString();
            _highScoreText.color = Color.yellow; // Đổi màu chữ sang vàng rực rỡ

            if (_audioService != null && _newRecordSound != null)
                _audioService.PlaySFX(_newRecordSound, volume: 1f, randomizePitch: false);

            await PlayBounceAnimation(_highScoreText.transform, 1.6f, token);
        }
    }

    private async UniTask PlayBounceAnimation(Transform target, float maxScale, CancellationToken token)
    {
        float bounceDuration = 0.15f;
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = Vector3.one * maxScale; 

        float elapsed = 0;
        // Boom lên
        while (elapsed < bounceDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / bounceDuration);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        elapsed = 0;
        // Thu về
        while (elapsed < bounceDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / bounceDuration);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        target.localScale = originalScale;
    }

    // --- HÀM GỌI KHI NÚT BỊ CHÉM ---
    private void OnRetrySliced()
    {
        _gameModel.StartGame(); 
    }

    private void ReturnToMainMenuSliced()
    {
        _gameModel.State.Value = GameState.MainMenu;
    }
}