using UnityEngine;
using Reflex.Attributes;
using R3; // Thêm R3 để theo dõi trạng thái Settings
using Cysharp.Threading.Tasks; // Thêm UniTask làm hiệu ứng
using System.Threading;

public class MainMenuView : MonoBehaviour
{
    [Inject] private readonly GameModel _gameModel;
    [Inject] private readonly AudioService _audioService;

    [Header("Title Animation (Hiệu ứng Logo)")]
    [Tooltip("Kéo Logo/Title của game vào đây")]
    [SerializeField] private Transform _titleTransform;
    [Tooltip("Tốc độ bay lơ lửng")]
    [SerializeField] private float _titleFloatSpeed = 2f;
    [Tooltip("Độ cao nhấp nhô")]
    [SerializeField] private float _titleFloatHeight = 15f;

    [Header("Audio (Âm thanh)")]
    [SerializeField] private AudioClip _menuMusic;
    [SerializeField] private AudioClip _uiClickSound;

    [Header("Quản lý vật thể chém ngoài Scene")]
    [Tooltip("Kéo GameObject MenuButtonsContainer ngoài Hierarchy vào đây")]
    [SerializeField] private GameObject _menuButtonsContainer; 

    [Header("Chế độ chơi: CLASSIC")]
    [SerializeField] private SliceableButton _classicSliceButton;

    [Header("Chế độ chơi: DYNAMIC")]
    [SerializeField] private SliceableButton _dynamicSliceButton;

    [Header("Cài đặt: SETTINGS")]
    [SerializeField] private SliceableButton _settingsSliceButton;

    // Các biến lưu trữ nội bộ
    private CancellationTokenSource _cts;
    private Vector3 _initialTitlePos;
    private Vector3 _initialTitleScale;

    private void Awake()
    {
        // Lưu lại vị trí và kích thước gốc của Title để làm mốc cho hiệu ứng
        if (_titleTransform != null)
        {
            _initialTitlePos = _titleTransform.localPosition;
            _initialTitleScale = _titleTransform.localScale;
        }
    }

    private void Start()
    {
        // Đăng ký sự kiện chém nút
        if (_classicSliceButton != null) _classicSliceButton.OnSlicedEvent.AddListener(StartClassicMode);
        if (_dynamicSliceButton != null) _dynamicSliceButton.OnSlicedEvent.AddListener(StartDynamicMode);
        if (_settingsSliceButton != null) _settingsSliceButton.OnSlicedEvent.AddListener(OpenSettings);

        // [TÍNH NĂNG MỚI]: Khóa nút chém khi đang mở Settings
        _gameModel.IsSettingsOpen.Subscribe(isSettingsOpen =>
        {
            // Chỉ can thiệp khi ta đang thực sự ở MainMenu
            if (_gameModel.State.Value == GameState.MainMenu)
            {
                // Nếu mở Settings -> Tắt cục Container chứa trái cây
                // Nếu đóng Settings -> Bật lại cục Container
                if (_menuButtonsContainer != null) 
                {
                    _menuButtonsContainer.SetActive(!isSettingsOpen);
                }
            }
        }).AddTo(destroyCancellationToken);
    }

    private void OnEnable()
    {
        if (_audioService != null && _menuMusic != null)
        {
            _audioService.PlayMusic(_menuMusic); 
        }

        // Đảm bảo Container bật/tắt đúng theo trạng thái hiện tại của Settings
        if (_menuButtonsContainer != null) 
        {
            _menuButtonsContainer.SetActive(!_gameModel.IsSettingsOpen.Value);
        }

        // Khởi động chuỗi hiệu ứng cho Title
        _cts = new CancellationTokenSource();
        if (_titleTransform != null)
        {
            AnimateTitleAsync(_cts.Token).Forget();
        }
    }

    private void OnDisable()
    {
        if (_audioService != null)
        {
            _audioService.StopMusic();
        }

        if (_menuButtonsContainer != null) 
        {
            _menuButtonsContainer.SetActive(false);
        }

        // Dọn dẹp tiến trình UniTask để tránh rò rỉ bộ nhớ
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    // --- HỆ THỐNG HIỆU ỨNG TITLE ---
    private async UniTaskVoid AnimateTitleAsync(CancellationToken token)
    {
        // 1. Hiệu ứng Intro: Phóng to và nảy (Pop-up Bounce)
        float duration = 0.6f;
        float elapsed = 0f;
        _titleTransform.localScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // Công thức toán học Back-Ease-Out tạo cảm giác nảy "Juicy"
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float easeT = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

            _titleTransform.localScale = Vector3.LerpUnclamped(Vector3.zero, _initialTitleScale, easeT);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        
        _titleTransform.localScale = _initialTitleScale; // Chốt lại kích thước chuẩn

        // 2. Hiệu ứng Idle: Bay lơ lửng nhấp nhô và "thở" (Breathing Scale)
        while (!token.IsCancellationRequested)
        {
            // Dùng hàm Sine tạo sóng dao động mượt mà
            float sinTime = Mathf.Sin(Time.realtimeSinceStartup * _titleFloatSpeed);

            // Cập nhật vị trí lên/xuống
            _titleTransform.localPosition = _initialTitlePos + new Vector3(0, sinTime * _titleFloatHeight, 0);
            
            // Cập nhật phình ra/xẹp vào cực nhẹ (3%) để có cảm giác sống động
            _titleTransform.localScale = _initialTitleScale * (1f + (sinTime * 0.03f));

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    // --- CÁC HÀM XỬ LÝ SỰ KIỆN ---
    private void StartClassicMode()
    {
        PlayClickSound();
        _gameModel.CurrentMode.Value = GameMode.Classic;
        _gameModel.StartGame();
    }

    public void StartDynamicMode()
    {
        PlayClickSound();
        _gameModel.CurrentMode.Value = GameMode.Dynamic;
        _gameModel.StartGame();
    }

    private void OpenSettings()
    {
        PlayClickSound();
        _gameModel.IsSettingsOpen.Value = true;
    }

    private void PlayClickSound()
    {
        if (_audioService != null && _uiClickSound != null)
        {
            _audioService.PlaySFX(_uiClickSound, volume: 1f, randomizePitch: false);
        }
    }
}