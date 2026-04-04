using UnityEngine;
using UnityEngine.UI;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;

public class PauseView : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform panelContainer; 
    [SerializeField] private Button resumeButton; // Kéo thả cục Button Resume vào đây

    [Inject] private readonly GameModel _gameModel;

    private void Start()
    {
        // Gán sự kiện cho nút (đảm bảo nút đã được kéo vào Inspector)
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(() => _gameModel.TogglePause());
        }
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