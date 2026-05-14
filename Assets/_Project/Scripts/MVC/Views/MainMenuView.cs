using UnityEngine;
using UnityEngine.UI;
using Reflex.Attributes;

public class MainMenuView : MonoBehaviour
{
    [Inject] private readonly GameModel _gameModel;
    [Inject] private readonly AudioService _audioService; // Tiêm AudioService vào đây

    [Header("Audio (Âm thanh)")]
    [SerializeField] private AudioClip _menuMusic; // Kéo file nhạc nền Menu vào đây
    [SerializeField] private AudioClip _uiClickSound; // Kéo file tiếng chém UI vào đây

    [Header("Chế độ chơi: CLASSIC")]
    [SerializeField] private Button classicPlayButton;
    [SerializeField] private DwellButton classicDwellButton;

    [Header("Chế độ chơi: DYNAMIC")]
    [SerializeField] private Button dynamicPlayButton;
    [SerializeField] private DwellButton dynamicDwellButton;

    [Header("Cài đặt: SETTINGS")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private DwellButton settingsDwellButton;

    // Tự động phát nhạc nền khi Panel Main Menu được bật lên
    private void OnEnable()
    {
        if (_audioService != null && _menuMusic != null)
        {
            // Phát nhạc nền, loop = true (mặc định trong AudioService)
            _audioService.PlayMusic(_menuMusic); 
        }
    }
    private void OnDisable()
    {
        // Khi Panel bị tắt (do chuyển sang Playing), dừng ngay nhạc Menu
        if (_audioService != null)
        {
            _audioService.StopMusic();
        }
    }

    private void Start()
    {
        if (classicPlayButton != null) classicPlayButton.onClick.AddListener(StartClassicMode);
        if (classicDwellButton != null) classicDwellButton.onDwellClick.AddListener(StartClassicMode);

        if (dynamicPlayButton != null) dynamicPlayButton.onClick.AddListener(StartDynamicMode);
        if (dynamicDwellButton != null) dynamicDwellButton.onDwellClick.AddListener(StartDynamicMode);

        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (settingsDwellButton != null) settingsDwellButton.onDwellClick.AddListener(OpenSettings);
    }

    private void StartClassicMode()
    {
        PlayClickSound();
        
        _gameModel.CurrentMode.Value = GameMode.Classic;
        if (classicDwellButton != null) classicDwellButton.ResetButton();
        _gameModel.StartGame();
    }

    private void StartDynamicMode()
    {
        PlayClickSound();
        
        _gameModel.CurrentMode.Value = GameMode.Dynamic;
        if (dynamicDwellButton != null) dynamicDwellButton.ResetButton();
        _gameModel.StartGame();
    }

    private void OpenSettings()
    {
        PlayClickSound();
        
        _gameModel.IsSettingsOpen.Value = true;
        if (settingsDwellButton != null) settingsDwellButton.ResetButton();
        Debug.Log("<color=yellow>[Main Menu]</color> Yêu cầu hệ thống mở Cài đặt");
    }

    // Hàm tiện ích để gọi tiếng Click tránh lặp code
    private void PlayClickSound()
    {
        if (_audioService != null && _uiClickSound != null)
        {
            _audioService.PlaySFX(_uiClickSound, volume: 1f, randomizePitch: false); // Tiếng UI thường không cần random pitch
        }
    }
}