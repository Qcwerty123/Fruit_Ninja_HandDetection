using UnityEngine;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;

public class ScreenFlashService : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup _flashGroup;

    private void Start()
    {
        // Đảm bảo lúc mới vào game màn hình không bị trắng
        _flashGroup.alpha = 0f;
        _flashGroup.gameObject.SetActive(false);
    }

    /// <summary>
    /// Hiệu ứng màn hình chớp trắng xoá rồi giữ nguyên
    /// </summary>
    public async UniTask FadeToWhite(float duration = 0.5f)
    {
        _flashGroup.gameObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Bắt buộc dùng unscaled vì game đang bị HitStop
            _flashGroup.alpha = Mathf.Clamp01(elapsed / duration);
            await UniTask.Yield();
        }

        _flashGroup.alpha = 1f;
    }

    /// <summary>
    /// Xóa lớp sương trắng (Dùng khi người chơi bấm nút "Chơi lại")
    /// </summary>
    public void ResetFlash()
    {
        _flashGroup.alpha = 0f;
        _flashGroup.gameObject.SetActive(false);
    }

    /// <summary>
    /// Sáng lên rồi từ từ mờ đi (Tổng thời gian = fadeIn + fadeOut)
    /// </summary>
    public async UniTask Flashbang(float fadeInTime = 0.1f, float fadeOutTime = 0.4f)
    {
        _flashGroup.gameObject.SetActive(true);
        float elapsed = 0f;

        // 1. Sáng bừng lên rất nhanh
        while (elapsed < fadeInTime)
        {
            elapsed += Time.unscaledDeltaTime;
            _flashGroup.alpha = Mathf.Clamp01(elapsed / fadeInTime);
            await UniTask.Yield();
        }
        _flashGroup.alpha = 1f;

        elapsed = 0f;

        // 2. Mờ dần đi từ từ
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.unscaledDeltaTime;
            _flashGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOutTime);
            await UniTask.Yield();
        }
        _flashGroup.alpha = 0f;
    }
}