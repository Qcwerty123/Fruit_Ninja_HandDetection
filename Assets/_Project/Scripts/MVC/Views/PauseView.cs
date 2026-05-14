using UnityEngine;
using UnityEngine.UI;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;

public class PauseView : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform panelContainer; 

    [Header("Nút Resume (Tiếp tục)")]
    [SerializeField] private Button resumeButton; 
    [SerializeField] private DwellButton dwellResumeButton; 

    [Header("Nút Settings (Cài đặt)")]
    [SerializeField] private Button settingsButton; 
    [SerializeField] private DwellButton dwellSettingsButton; 

    [Header("Nút Home (Về Main Menu)")]
    [SerializeField] private Button homeButton; 
    [SerializeField] private DwellButton dwellHomeButton; 

    [Inject] private readonly GameModel _gameModel;

    private void Start()
    {
        // --- 1. SỰ KIỆN RESUME ---
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (dwellResumeButton != null)
            dwellResumeButton.onDwellClick.AddListener(ResumeGame);

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

    private void ResumeGame()
    {
        if (dwellResumeButton != null) dwellResumeButton.ResetButton();
        _gameModel.TogglePause();
    }

    private void OpenSettings()
    {
        if (dwellSettingsButton != null) dwellSettingsButton.ResetButton();
        
        // Ra lệnh mở bảng Settings đè lên trên (UIManager sẽ tự lo việc hiển thị)
        _gameModel.IsSettingsOpen.Value = true;
    }

    private void ReturnToMainMenu()
    {
        if (dwellHomeButton != null) dwellHomeButton.ResetButton();
        
        // QUAN TRỌNG: Phải xả nén thời gian (TimeScale = 1) trước khi thoát
        // Nếu không, game sẽ bị kẹt ở trạng thái đóng băng khi về Main Menu
        Time.timeScale = 1f; 
        
        // Đổi State về Main Menu. UIManager sẽ tự động tắt HUD, tắt Pause và bật màn hình chính.
        _gameModel.State.Value = GameState.MainMenu;
        
        Debug.Log("<color=cyan>[Pause Panel]</color> Đã quay về Main Menu");
    }

    // OnEnable được gọi TỰ ĐỘNG ngay khoảnh khắc UIManager gọi pausePanel.SetActive(true)
    private void OnEnable()
    {
        AnimatePanelAsync().Forget();
    }

    private async UniTaskVoid AnimatePanelAsync()
    {
        panelContainer.localScale = Vector3.zero;
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            // BẮT BUỘC dùng unscaledDeltaTime vì lúc này GameModel đã ép timeScale = 0
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f); // Ease Out Cubic
            
            panelContainer.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, easeT);
            await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
        }
        panelContainer.localScale = Vector3.one;
    }
}