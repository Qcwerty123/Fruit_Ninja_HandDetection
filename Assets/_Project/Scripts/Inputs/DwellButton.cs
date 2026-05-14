using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Reflex.Attributes; 
using R3; // Bổ sung thư viện này để bắt sự kiện đổi Mode

[RequireComponent(typeof(RectTransform))]
public class DwellButton : MonoBehaviour
{
    [Header("Cài đặt Rê & Giữ")]
    public float dwellTime = 1.5f;
    public Image fillImage; 

    [Header("Sự kiện Kích hoạt")]
    public UnityEvent onDwellClick;

    [Inject] private readonly IInputService _inputService; 
    [Inject] private readonly GameModel _gameModel; 

    private RectTransform _rectTransform;
    private float _hoverTimer = 0f;
    private bool _isClicked = false;
    private Camera _uiCamera;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (fillImage != null) fillImage.fillAmount = 0f;
        
        var canvas = GetComponentInParent<Canvas>();
        _uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    }

    private void Start()
    {
        // Lắng nghe sự kiện đổi chế độ từ Settings
        if (_gameModel != null && _gameModel.CurrentInput != null)
        {
            _gameModel.CurrentInput
                .Subscribe(inputMethod => 
                {
                    if (fillImage != null)
                    {
                        // CHỈ BẬT hình ảnh vòng tròn khi đang xài CameraAI
                        fillImage.enabled = (inputMethod == InputMethod.CameraAI);
                    }

                    // Nếu người dùng đang rê tay dở mà bấm sang chuột, dọn dẹp sạch sẽ data thừa
                    if (inputMethod == InputMethod.Mouse)
                    {
                        CancelHover();
                    }
                })
                .RegisterTo(destroyCancellationToken);
        }
    }

    private void Update()
    {
        // CỬA BẢO VỆ: Nếu là chuột hoặc đã click rồi thì đứng im, không làm gì cả
        if (_inputService == null || _isClicked || _gameModel.CurrentInput.Value != InputMethod.CameraAI)
        {
            return;
        }

        Vector2 handPos = _inputService.GetCurrentPosition();

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
            CancelHover();
        }
    }

    private void CancelHover()
    {
        if (_hoverTimer > 0f)
        {
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

    public void ResetButton()
    {
        _isClicked = false;
        CancelHover();
    }
}