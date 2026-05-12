using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Reflex.Attributes; // Dùng DI của Reflex

[RequireComponent(typeof(RectTransform))]
public class DwellButton : MonoBehaviour
{
    [Header("Cài đặt Rê & Giữ")]
    public float dwellTime = 1.5f;
    public Image fillImage; // Vòng tròn Progress Bar

    [Header("Sự kiện Kích hoạt")]
    public UnityEvent onDwellClick;

    // Bơm InputService từ Reflex vào để lấy tọa độ tay
    [Inject] private IInputService _inputService; 

    private RectTransform _rectTransform;
    private float _hoverTimer = 0f;
    private bool _isClicked = false;
    private Camera _uiCamera;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        
        // Nếu Canvas của bạn là Screen Space - Overlay, uiCamera là null. 
        // Nếu là Screen Space - Camera (URP), hãy gán main camera vào.
        var canvas = GetComponentInParent<Canvas>();
        _uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    }

    private void Update()
    {
        if (_inputService == null || _isClicked) return;

        Vector2 handPos = _inputService.GetCurrentPosition();

        // Kiểm tra xem điểm ngón tay ảo có nằm trong phạm vi của Nút không
        bool isHovering = RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, handPos, _uiCamera);

        if (isHovering)
        {
            _hoverTimer += Time.unscaledDeltaTime;
            if (fillImage != null) fillImage.fillAmount = _hoverTimer / dwellTime;

            if (_hoverTimer >= dwellTime)
            {
                ExecuteClick();
            }
        }
        else
        {
            // Nếu tay trượt ra ngoài, reset tiến trình lập tức
            _hoverTimer = 0f;
            if (fillImage != null) fillImage.fillAmount = 0f;
        }
    }

    private void ExecuteClick()
    {
        _isClicked = true;
        if (fillImage != null) fillImage.fillAmount = 1f;
        
        onDwellClick.Invoke();
        Debug.Log($"<color=orange>[UI]</color> Đã kích hoạt Dwell Click bằng AI Sensor!");
    }

    // Cung cấp hàm Reset để tái sử dụng nút sau khi Pause/Resume
    public void ResetButton()
    {
        _isClicked = false;
        _hoverTimer = 0f;
        if (fillImage != null) fillImage.fillAmount = 0f;
    }
}