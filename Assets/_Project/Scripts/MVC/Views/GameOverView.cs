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
    [SerializeField] private Button retryButton; // Kéo thả cục Button Retry vào đây

    [Inject] private readonly GameModel _gameModel;

    private void Start()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
        }
    }

    // Gọi TỰ ĐỘNG khi UIManager bật Panel này lên
    private void OnEnable()
    {
        UpdateScores();
        AnimatePanelAsync().Forget();
    }

    private void UpdateScores()
    {
        // Lấy điểm cuối cùng từ GameModel
        int finalScore = _gameModel.Score.Value;
        currentScoreText.text = finalScore.ToString();

        // Xử lý lưu và tải Kỷ lục (High Score)
        int highScore = PlayerPrefs.GetInt("FruitNinja_HighScore", 0);
        if (finalScore > highScore)
        {
            highScore = finalScore;
            PlayerPrefs.SetInt("FruitNinja_HighScore", highScore);
            PlayerPrefs.Save();
        }
        highScoreText.text = highScore.ToString();
    }

    private void OnRetryClicked()
    {
        // QUAN TRỌNG: Phải xả đông vật lý trước khi load lại Scene
        Time.timeScale = 1f; 
        _gameModel.StartGame(); // Reset lại trạng thái GameModel về Playing
    }

    private async UniTaskVoid AnimatePanelAsync()
    {
        panelContainer.localScale = Vector3.zero;
        float elapsed = 0f;
        float duration = 0.25f;

        while (elapsed < duration)
        {
            // Dùng unscaledDeltaTime để hiệu ứng luôn mượt dù timeScale có bằng 0 hay 1
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
            
            panelContainer.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, easeT);
            await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
        }
        panelContainer.localScale = Vector3.one;
    }
}