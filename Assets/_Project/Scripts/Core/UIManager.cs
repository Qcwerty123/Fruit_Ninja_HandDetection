using UnityEngine;
using Reflex.Attributes;
using R3;
using Cysharp.Threading.Tasks;

public class UIManager : MonoBehaviour
{
    [Inject] private readonly GameModel _gameModel;
    [Inject] private readonly AudioService _audioService;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Overlays")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio")]
    [SerializeField] private AudioClip gameplayMusic;

    private HUDView _hudView;

    private void Start()
    {
        if (hudPanel != null)
        {
            _hudView = hudPanel.GetComponent<HUDView>();
        }

        _gameModel.State.Subscribe(state => HandleStateChanged(state).Forget()).RegisterTo(destroyCancellationToken);

        _gameModel.IsSettingsOpen.Subscribe(isOpen => 
        {
            if (settingsPanel != null) settingsPanel.SetActive(isOpen);
        }).RegisterTo(destroyCancellationToken);
    }

    private async UniTaskVoid HandleStateChanged(GameState currentState)
    {
        _gameModel.IsSettingsOpen.Value = false;

        // ==========================================
        // XỬ LÝ CHỜ HIỆU ỨNG GAME OVER TỪ HUDVIEW
        // ==========================================
        if (currentState == GameState.GameOver)
        {
            if (_hudView != null)
            {
                await _hudView.PlayGameOverSequenceAsync(); 
            }
        }

        // ==========================================
        // [ĐÃ SỬA LỖI] ĐIỀU PHỐI PANEL THÔNG MINH
        // ==========================================
        bool isMainMenu = currentState == GameState.MainMenu;
        bool isPlaying = currentState == GameState.Playing;
        bool isPaused = currentState == GameState.Paused;
        bool isGameOver = currentState == GameState.GameOver;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(isMainMenu);
        
        // Cốt lõi của việc sửa lỗi: HUD luôn giữ trạng thái Active khi đang chơi VÀ khi đang Pause
        if (hudPanel != null) hudPanel.SetActive(isPlaying || isPaused); 
        
        if (pausePanel != null) pausePanel.SetActive(isPaused);
        if (gameOverPanel != null) gameOverPanel.SetActive(isGameOver);

        // Xử lý các tác vụ riêng lẻ (như phát nhạc nền)
        switch (currentState)
        {
            case GameState.Playing:
                if (_audioService != null && gameplayMusic != null)
                {
                    _audioService.PlayMusic(gameplayMusic);
                }
                break;
        }
    }
}