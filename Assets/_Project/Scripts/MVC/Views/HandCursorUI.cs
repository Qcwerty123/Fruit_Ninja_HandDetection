using UnityEngine;
using UnityEngine.UI;
using Reflex.Attributes;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class HandCursorUI : MonoBehaviour
{
    [Header("Cài đặt")]
    [Tooltip("Kéo Canvas gốc chứa con trỏ này vào đây")]
    [SerializeField] private Canvas _parentCanvas;
    
    [Tooltip("Tốc độ di chuyển của con trỏ (càng cao càng bám sát)")]
    [SerializeField] private float _smoothSpeed = 25f;

    [Inject] private readonly IInputService _inputService;

    private RectTransform _cursorRect;
    private RectTransform _canvasRect;
    private Camera _uiCamera;

    private void Awake()
    {
        _cursorRect = GetComponent<RectTransform>();
        
        if (_parentCanvas != null)
        {
            _canvasRect = _parentCanvas.GetComponent<RectTransform>();
            // Tự động nhận diện loại Canvas để dùng đúng Camera
            _uiCamera = _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _parentCanvas.worldCamera;
        }
        else
        {
            Debug.LogError("<color=red>[UI]</color> Chưa gán Parent Canvas cho Hand Cursor!");
        }

        // Tự động tắt Raycast Target bằng code để phòng trường hợp bạn quên
        GetComponent<Image>().raycastTarget = false;
    }

    private void Update()
    {
        if (_inputService == null || _parentCanvas == null) return;

        // 1. Lấy tọa độ pixel từ mạng UDP
        Vector2 screenPos = _inputService.GetCurrentPosition();

        // 2. Chuyển đổi tọa độ Màn hình -> Tọa độ cục bộ của Canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, 
            screenPos, 
            _uiCamera, 
            out Vector2 localPoint);

        // 3. Di chuyển con trỏ mượt mà (Lerp)
        _cursorRect.anchoredPosition = Vector2.Lerp(_cursorRect.anchoredPosition, localPoint, Time.unscaledDeltaTime * _smoothSpeed);

        

        // 4. (Tùy chọn) Hiệu ứng UX: Bóp nhỏ con trỏ lại khi đang "Chém" hoặc "Chụm tay"
        float targetScale = _inputService.IsSwiping() ? 0.7f : 1f;
        _cursorRect.localScale = Vector3.Lerp(_cursorRect.localScale, Vector3.one * targetScale, Time.unscaledDeltaTime * 15f);
    }
}