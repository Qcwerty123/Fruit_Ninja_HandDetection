using UnityEngine;
using UnityEngine.UI;
using Reflex.Attributes;
using R3; // Bổ sung thư viện Reactive

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class HandCursorUI : MonoBehaviour
{
    [Header("Cài đặt")]
    [Tooltip("Kéo Canvas gốc chứa con trỏ này vào đây")]
    [SerializeField] private Canvas _parentCanvas;
    
    [Tooltip("Tốc độ di chuyển của con trỏ (càng cao càng bám sát)")]
    [SerializeField] private float _smoothSpeed = 25f;

    // --- TIÊM THÊM GAMEMODEL ---
    [Inject] private readonly IInputService _inputService;
    [Inject] private readonly GameModel _gameModel; 

    private RectTransform _cursorRect;
    private RectTransform _canvasRect;
    private Camera _uiCamera;
    private Image _cursorImage; // Cache lại Image để bật/tắt

    private void Awake()
    {
        _cursorRect = GetComponent<RectTransform>();
        _cursorImage = GetComponent<Image>(); // Lấy component
        
        if (_parentCanvas != null)
        {
            _canvasRect = _parentCanvas.GetComponent<RectTransform>();
            _uiCamera = _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _parentCanvas.worldCamera;
        }
        else
        {
            Debug.LogError("<color=red>[UI]</color> Chưa gán Parent Canvas cho Hand Cursor!");
        }

        _cursorImage.raycastTarget = false;
    }

    private void Start()
    {
        // Theo dõi sự thay đổi của thiết bị đầu vào (Mouse / CameraAI)
        if (_gameModel != null && _gameModel.CurrentInput != null)
        {
            _gameModel.CurrentInput
                .Subscribe(inputMethod => 
                {
                    // Chỉ hiển thị hình ảnh con trỏ khi ở chế độ CameraAI
                    _cursorImage.enabled = (inputMethod == InputMethod.CameraAI);
                })
                .RegisterTo(destroyCancellationToken);
        }
    }

    private void Update()
    {
        // --- TỐI ƯU HIỆU NĂNG ---
        // Bỏ qua toàn bộ tính toán nội suy (Lerp) nếu đang dùng Chuột
        if (_inputService == null || _parentCanvas == null || _gameModel.CurrentInput.Value != InputMethod.CameraAI) 
            return;

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

        // 4. Hiệu ứng UX: Bóp nhỏ con trỏ lại khi đang "Chém" hoặc "Chụm tay"
        float targetScale = _inputService.IsSwiping() ? 0.7f : 1f;
        _cursorRect.localScale = Vector3.Lerp(_cursorRect.localScale, Vector3.one * targetScale, Time.unscaledDeltaTime * 15f);
    }
}