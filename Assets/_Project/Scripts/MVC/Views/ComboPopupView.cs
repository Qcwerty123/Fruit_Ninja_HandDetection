using UnityEngine;
using TMPro;
using Reflex.Attributes;
using R3;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Pool;

public class ComboPopupView : MonoBehaviour
{
    [Header("UI Setup")]
    [Tooltip("Kéo Prefab ComboText có gắn script ComboText vào đây")]
    [SerializeField] private ComboTexts comboTextPrefab; 
    [SerializeField] private Transform canvasTransform;

    [Header("Fonts (Juice)")]
    [SerializeField] private TMP_FontAsset[] comboFonts;

    [Inject] private readonly ComboService _comboService;

    // Đổi Object Pool để quản lý trực tiếp script ComboText
    private ObjectPool<ComboTexts> _comboPool;

    private void Awake()
    {
        _comboPool = new ObjectPool<ComboTexts>(
            createFunc: () => Instantiate(comboTextPrefab, canvasTransform),
            actionOnGet: (combo) => combo.gameObject.SetActive(true),
            actionOnRelease: (combo) => combo.gameObject.SetActive(false),
            actionOnDestroy: (combo) => Destroy(combo.gameObject),
            defaultCapacity: 5,
            maxSize: 15
        );
    }

    private void Start()
    {
        _comboService.ComboAchieved
            // 1. Chỉ nhận những tín hiệu KHÔNG PHẢI là Critical
            // 2. Và phải đảm bảo số lượng Combo >= 3 mới được hiện chữ
            .Where(info => !info.isCritical && info.comboCount >= 3) 
            .Subscribe(info => ShowComboPopup(info.comboCount, info.position))
            .RegisterTo(destroyCancellationToken);
    }

    private void ShowComboPopup(int count, Vector2 worldPosition)
    {
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        // Lấy Prefab từ Pool
        ComboTexts combo = _comboPool.Get();
        combo.transform.position = screenPosition;
        
        // Truyền dữ liệu vào script ComboText
        if (comboFonts != null && comboFonts.Length > 0)
        {
            int mathAmount = Mathf.Max(0, count - 3); 
            int index = mathAmount % comboFonts.Length;
            combo.Setup(count, comboFonts[index]);
        }

        AnimatePopupAsync(combo).Forget();
    }

    private async UniTaskVoid AnimatePopupAsync(ComboTexts combo)
    {
        CancellationToken ct = combo.GetCancellationTokenOnDestroy();
        
        try
        {
            combo.transform.localScale = Vector3.zero;
            combo.ResetAlpha(); // Dùng hàm tự viết bên ComboText để reset màu

            // 1. POP-UP NẢY LÊN (0.2s)
            float popDuration = 0.2f;
            float elapsed = 0f;
            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / popDuration;
                
                float c1 = 1.70158f;
                float c3 = c1 + 1f;
                float easeT = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                
                combo.transform.localScale = Vector3.one * Mathf.Max(0, easeT);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            combo.transform.localScale = Vector3.one;

            // 2. KHOE ĐIỂM (0.3s)
            await UniTask.WaitForSeconds(0.3f, cancellationToken: ct);

            // 3. BAY LÊN VÀ MỜ DẦN (0.7s)
            float fadeDuration = 0.7f;
            elapsed = 0f;
            Vector3 startPos = combo.transform.position;
            Vector3 endPos = startPos + Vector3.up * 100f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                
                combo.transform.position = Vector3.Lerp(startPos, endPos, t);
                combo.SetAlpha(Mathf.Lerp(1f, 0f, t)); // Fade mờ chữ đi

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            // 4. TRẢ VỀ POOL
            _comboPool.Release(combo);
        }
        catch (System.OperationCanceledException) 
        {
            if (combo != null && combo.gameObject.activeSelf)
            {
                _comboPool.Release(combo);
            }
        }
    }
}