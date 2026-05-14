using UnityEngine;
using UnityEngine.UI;
using Reflex.Attributes;
using R3;
using Cysharp.Threading.Tasks;

public class SettingsView : MonoBehaviour
{
    [Inject] private readonly GameModel _gameModel;
    [Inject] private readonly AudioService _audioService; 

    [Header("UI - Input Settings (Thiết bị chém)")]
    [Tooltip("Nút chọn chém bằng Chuột")]
    [SerializeField] private Button _btnMouseInput;
    [Tooltip("Nút chọn chém bằng Camera AI")]
    [SerializeField] private Button _btnCameraInput;
    
    [Header("UI - Audio Settings")]
    [SerializeField] private Slider _sliderBGM; 
    [SerializeField] private Slider _sliderSFX; 

    [Header("UI - General")]
    [SerializeField] private Button _btnClose;  
    [SerializeField] private Transform panelContainer; 

    [Header("Visual Feedback")]
    [SerializeField] private Color _activeColor = Color.green; 
    [SerializeField] private Color _inactiveColor = Color.white; 

    private void Start()
    {
        // ==========================================
        // 1. LIÊN KẾT LOGIC CHUYỂN THIẾT BỊ ĐẦU VÀO
        // ==========================================
        _btnMouseInput.onClick.AddListener(() => 
        {
            _gameModel.CurrentInput.Value = InputMethod.Mouse;
        });

        _btnCameraInput.onClick.AddListener(() => 
        {
            _gameModel.CurrentInput.Value = InputMethod.CameraAI;
        });

        // Tự động đổi màu nút khi thay đổi Input
        _gameModel.CurrentInput
            .Subscribe(UpdateInputVisuals)
            .RegisterTo(destroyCancellationToken);

        // ==========================================
        // 2. LIÊN KẾT LOGIC ÂM THANH
        // ==========================================
        if (_audioService != null)
        {
            _sliderBGM.value = _audioService.GetBGMVolume(); 
            _sliderSFX.value = _audioService.GetSFXVolume();

            _sliderBGM.onValueChanged.AddListener(val => _audioService.SetBGMVolume(val));
            _sliderSFX.onValueChanged.AddListener(val => _audioService.SetSFXVolume(val));
        }

        // ==========================================
        // 3. LOGIC ĐÓNG BẢNG
        // ==========================================
        if (_btnClose != null)
        {
            _btnClose.onClick.AddListener(() => _gameModel.IsSettingsOpen.Value = false);
        }
    }

    private void UpdateInputVisuals(InputMethod currentInput)
    {
        // Đổi màu để nhận biết đang xài Chuột hay Cam
        _btnMouseInput.image.color = currentInput == InputMethod.Mouse ? _activeColor : _inactiveColor;
        _btnCameraInput.image.color = currentInput == InputMethod.CameraAI ? _activeColor : _inactiveColor;
    }

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