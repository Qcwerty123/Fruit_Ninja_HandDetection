using UnityEngine;
using Reflex.Attributes;
using R3;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Pool;

public class CriticalPopupView : MonoBehaviour
{
    [Header("UI Setup")]
    [Tooltip("Kéo Prefab có gắn script CriticalText vào đây")]
    [SerializeField] private CriticalTexts _criticalTextPrefab; 
    [SerializeField] private Transform _canvasTransform;

    [Inject] private readonly ComboService _comboService;

    private ObjectPool<CriticalTexts> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<CriticalTexts>(
            createFunc: () => Instantiate(_criticalTextPrefab, _canvasTransform),
            actionOnGet: (obj) => obj.gameObject.SetActive(true),
            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj.gameObject),
            defaultCapacity: 3,
            maxSize: 10
        );
    }

    private void Start()
    {
        _comboService.ComboAchieved
            .Where(info => info.isCritical) 
            // 2. Truyền thẳng info xuống
            .Subscribe(info => ShowCriticalPopup(info.position, info.criticalBonus))
            .RegisterTo(destroyCancellationToken);
    }

    // 3. Cập nhật hàm Show
    private void ShowCriticalPopup(Vector2 worldPosition, int bonusScore)
    {
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        
        CriticalTexts popup = _pool.Get();
        popup.transform.position = screenPosition;
        
        // Nạp đúng số điểm đọc được từ quả trái cây
        popup.Setup(bonusScore);

        AnimatePopupAsync(popup).Forget();
    }

    private async UniTaskVoid AnimatePopupAsync(CriticalTexts popup)
    {
        CancellationToken ct = popup.GetCancellationTokenOnDestroy();
        
        try
        {
            popup.transform.localScale = Vector3.zero;
            popup.SetAlpha(1f);

            // ==========================================
            // GIAI ĐOẠN 1: PHÓNG TO NẢY MẠNH (Giữ cấu trúc code cũ của bạn)
            // LƯU Ý: Phải dùng unscaledDeltaTime vì game đang bị Hit-Stop
            // ==========================================
            float popDuration = 0.15f;
            float elapsed = 0f;
            while (elapsed < popDuration)
            {
                elapsed += Time.unscaledDeltaTime; 
                float t = elapsed / popDuration;
                float easeT = NảyMạnh_BackEaseOut(t);
                
                popup.transform.localScale = Vector3.one * Mathf.Max(0, easeT);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            popup.transform.localScale = Vector3.one;

            // ==========================================
            // GIAI ĐOẠN 2: CHỜ 0.5s (Như trong FunctionTimer cũ của bạn)
            // ==========================================
            await UniTask.WaitForSeconds(0.5f, ignoreTimeScale: true, cancellationToken: ct);

            // ==========================================
            // GIAI ĐOẠN 3: THU NHỎ VỀ 0 VÀ BIẾN MẤT
            // ==========================================
            float fadeDuration = 0.15f;
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeDuration;
                
                // Thu nhỏ dần về 0
                popup.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
                // Làm mờ dần
                popup.SetAlpha(Mathf.Lerp(1f, 0f, t));
                
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            // Trả về kho
            _pool.Release(popup);
        }
        catch (System.OperationCanceledException) 
        {
            if (popup != null && popup.gameObject.activeSelf)
            {
                _pool.Release(popup);
            }
        }
    }

    // Công thức toán học tạo độ nảy
    private float NảyMạnh_BackEaseOut(float t)
    {
        float c1 = 1.70158f * 1.5f; 
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}