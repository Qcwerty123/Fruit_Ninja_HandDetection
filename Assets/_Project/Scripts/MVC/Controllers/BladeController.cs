using UnityEngine;
using Reflex.Attributes; 
using Cysharp.Threading.Tasks; 
using R3;

public class BladeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BladeView view;
    
    [Header("Settings")]
    [SerializeField] private LayerMask fruitLayer;
    [SerializeField] private float minSliceVelocity = 0.01f;

    [Header("Audio (Âm thanh)")]
    [SerializeField] private AudioClip[] swooshSounds; 
    [SerializeField] private float distanceToPlaySwoosh = 2.0f; 
    
    // --- [MỚI] THIẾT LẬP CRITICAL ---
    [Header("Critical Hit (Kỹ năng)")]
    [Tooltip("Khoảng cách tối đa từ nét chém đến tâm quả để được tính là Critical (ví dụ: 0.15)")]
    [SerializeField] private float _criticalPrecisionThreshold = 0.15f; 
    [Tooltip("Tiếng chém chí mạng cực mạnh (Bass boost)")]
    [SerializeField] private AudioClip _criticalHitSound;

    private float _accumulatedDistance = 0f;

    [Inject] private readonly IInputService _inputService;
    [Inject] private readonly GameModel _gameModel;
    [Inject] private readonly AudioService _audioService;
    // --- [MỚI] TIÊM COMBO SERVICE ---
    [Inject] private readonly ComboService _comboService; 

    private readonly RaycastHit2D[] _hitsBuffer = new RaycastHit2D[10];
    
    private Vector2 _previousPosition;
    private bool _isSlicing;
    private bool _isLocked = false; 
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        if (_gameModel != null && _gameModel.State != null)
        {
            _gameModel.State
                .Subscribe(_ => 
                {
                    UnlockBlade(); 
                    StopSlice();   
                })
                .RegisterTo(destroyCancellationToken);
        }
    }

    private void Update()
    {
        if (_isLocked) return;
        
        if (_inputService.IsSwiping())
        {
            ContinueSlice();
        }
        else if (_isSlicing)
        {
            StopSlice();
        }
    }

    private void ContinueSlice()
    {
        Vector2 screenPosition = _inputService.GetCurrentPosition();
        Vector3 worldPos3D = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0f));
        Vector2 currentWorldPosition = new Vector2(worldPos3D.x, worldPos3D.y);

        if (!_isSlicing)
        {
            _isSlicing = true;
            view.UpdatePosition(currentWorldPosition);
            view.StartSlicing();
            _previousPosition = currentWorldPosition;
            _accumulatedDistance = 0f;
            return;
        }

        view.UpdatePosition(currentWorldPosition);
        float distance = Vector2.Distance(currentWorldPosition, _previousPosition);
        
        if (distance > minSliceVelocity)
        {
            // Âm thanh vung kiếm
            _accumulatedDistance += distance;
            if (_accumulatedDistance >= distanceToPlaySwoosh)
            {
                if (swooshSounds != null && swooshSounds.Length > 0 && _audioService != null)
                {
                    AudioClip randomSwoosh = swooshSounds[Random.Range(0, swooshSounds.Length)];
                    _audioService.PlaySFX(randomSwoosh, volume: 0.6f, randomizePitch: true);
                }
                _accumulatedDistance -= distanceToPlaySwoosh; 
            }

            // Quét va chạm
            int hitCount = Physics2D.LinecastNonAlloc(_previousPosition, currentWorldPosition, _hitsBuffer, fruitLayer);
            
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hitCollider = _hitsBuffer[i].collider;
                
                if (hitCollider != null && hitCollider.enabled)
                {
                    hitCollider.enabled = false; 

                    if (hitCollider.TryGetComponent(out ISliceable sliceableTarget))
                    {
                        Vector2 cutDirection = currentWorldPosition - _previousPosition;
                        
                        // Nếu chém trúng BOM
                        if (sliceableTarget.IsBomb())
                        {
                            LockBlade(); 
                            // Truyền isCritical = false cho bom
                            sliceableTarget.Slice(cutDirection, _previousPosition, currentWorldPosition, distance, false).Forget(); 
                            break; 
                        }
                        else
                        {
                            // --- [MỚI] XỬ LÝ CRITICAL CHO TRÁI CÂY ---
                            
                            // 1. Tính độ chính xác của nhát chém (Khoảng cách từ tâm quả đến đoạn thẳng cắt)
                            float precision = GetDistanceToSegment(hitCollider.transform.position, _previousPosition, currentWorldPosition);
                            bool isCritical = precision <= _criticalPrecisionThreshold;

                            if (isCritical)
                            {
                                // Kích hoạt Hit-Stop (Khựng thời gian)
                                ApplyHitStopAsync().Forget();

                                // Phát âm thanh đặc biệt
                                if (_audioService != null && _criticalHitSound != null)
                                {
                                    _audioService.PlaySFX(_criticalHitSound, volume: 1.2f, randomizePitch: false);
                                }
                            }

                            // 2. Chém vật thể (truyền cờ isCritical xuống để tạo lực văng mạnh hơn nếu muốn)
                            sliceableTarget.Slice(cutDirection, _previousPosition, currentWorldPosition, distance, isCritical).Forget();

                            // 3. Thông báo cho hệ thống Combo (Đã nhận diện Critical)
                            // Lưu ý: ComboService của bạn cần có hàm AddFruitToCombo nhận FruitController hoặc Transform
                            if (hitCollider.TryGetComponent(out FruitController fruitController))
                            {
                                _comboService.AddFruitToCombo(fruitController, isCritical);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            _accumulatedDistance = 0f;
        }

        if (!_isLocked)
        {
            _previousPosition = currentWorldPosition;
        }
    }

    private void StopSlice()
    {
        _isSlicing = false;
        _accumulatedDistance = 0f; 
        view.StopSlicing();
    }

    public void LockBlade()
    {
        _isLocked = true;
    }

    public void UnlockBlade()
    {
        _isLocked = false;
    }

    // --- [MỚI] HÀM TOÁN HỌC TÍNH KHOẢNG CÁCH TỪ ĐIỂM ĐẾN ĐOẠN THẲNG ---
    private float GetDistanceToSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        float l2 = (lineStart - lineEnd).sqrMagnitude;
        if (l2 == 0) return Vector2.Distance(point, lineStart); // Nếu điểm đầu và cuối trùng nhau

        // Tính hình chiếu của điểm lên đoạn thẳng
        float t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(point - lineStart, lineEnd - lineStart) / l2));
        Vector2 projection = lineStart + t * (lineEnd - lineStart); 
        
        return Vector2.Distance(point, projection);
    }

    // --- [MỚI] HIỆU ỨNG HIT-STOP ---
    private async UniTaskVoid ApplyHitStopAsync()
    {
        // Khựng game (10% tốc độ thực)
        Time.timeScale = 0.1f; 

        // Đợi 0.05s thực tế
        await UniTask.WaitForSeconds(0.05f, ignoreTimeScale: true);

        // Chỉ nhả thời gian nếu game vẫn đang trong quá trình chơi (không bị Pause)
        if (_gameModel.State.Value == GameState.Playing)
        {
            Time.timeScale = 1f;
        }
    }
}