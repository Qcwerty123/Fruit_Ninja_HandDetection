using UnityEngine;
using Reflex.Attributes;
using R3;

public class UIManager : MonoBehaviour
{
    [Inject] private readonly GameModel _gameModel;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    private void Start()
    {
        // Lắng nghe sự thay đổi State từ GameModel và chuyển giao diện tương ứng
        _gameModel.State.Subscribe(HandleStateChanged).RegisterTo(destroyCancellationToken);
    }

    private void HandleStateChanged(GameState currentState)
    {
        // 1. Tắt toàn bộ Panel trước để dọn dẹp màn hình
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // 2. Chỉ bật Panel ứng với State hiện tại
        switch (currentState)
        {
            case GameState.MainMenu:
                if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                break;
                
            case GameState.Playing:
                if (hudPanel != null) hudPanel.SetActive(true);
                break;
                
            case GameState.Paused:
                if (hudPanel != null) hudPanel.SetActive(true); // Vẫn hiện HUD ở dưới nền
                if (pausePanel != null) pausePanel.SetActive(true);
                break;
                
            case GameState.GameOver:
                if (gameOverPanel != null) gameOverPanel.SetActive(true);
                break;
        }
    }
}