using UnityEngine;
using TMPro;
using Reflex.Attributes;
using R3;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ComboPopupView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI comboTextPrefab; 
    [SerializeField] private Transform canvasTransform;

    [Inject] private readonly ComboService _comboService;

    private void Start()
    {
        // Lắng nghe sự kiện (Dùng RegisterTo cho an toàn bộ nhớ)
        _comboService.ComboAchieved
            .Subscribe(info => ShowComboPopup(info.comboCount, info.position))
            .RegisterTo(destroyCancellationToken);
    }

    private void ShowComboPopup(int count, Vector2 worldPosition)
    {
        // Chuyển tọa độ thế giới (Vật lý 2D) sang tọa độ màn hình (UI Canvas)
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        TextMeshProUGUI popup = Instantiate(comboTextPrefab, canvasTransform);
        popup.transform.position = screenPosition;
        popup.text = $"COMBO {count}!";
        
        AnimatePopupAsync(popup).Forget();
    }

    private async UniTaskVoid AnimatePopupAsync(TextMeshProUGUI popup)
    {
        // Gắn token sinh tử vào chữ Text này
        CancellationToken ct = popup.GetCancellationTokenOnDestroy();
        
        try
        {
            float duration = 1f;
            float elapsed = 0f;
            Vector3 startPos = popup.transform.position;
            Vector3 endPos = startPos + Vector3.up * 150f; // Bay lên trên 150 pixel

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                
                // Nội suy vị trí (Bay lên)
                popup.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                
                // Nội suy Alpha (Mờ dần)
                Color c = popup.color;
                c.a = Mathf.Lerp(1f, 0f, elapsed / duration);
                popup.color = c;

                await UniTask.Yield(PlayerLoopTiming.Update, ct); // Chờ sang frame kế tiếp
            }

            Destroy(popup.gameObject);
        }
        catch (System.OperationCanceledException) { }
    }
}