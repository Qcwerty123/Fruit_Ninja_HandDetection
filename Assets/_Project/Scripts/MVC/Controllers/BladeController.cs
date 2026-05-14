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
    [Tooltip("Cho 3-4 file tiếng vút khác nhau vào đây để bốc ngẫu nhiên")]
    [SerializeField] private AudioClip[] swooshSounds; 
    
    [Tooltip("Quãng đường rê chuột tối thiểu để phát ra 1 tiếng vút")]
    [SerializeField] private float distanceToPlaySwoosh = 2.0f; 

    // Biến cộng dồn quãng đường rê chuột
    private float _accumulatedDistance = 0f;

    [Inject] private readonly IInputService _inputService;
    [Inject] private readonly GameModel _gameModel;
    [Inject] private readonly AudioService _audioService; // Tiêm AudioService vào đây

    private readonly RaycastHit2D[] _hitsBuffer = new RaycastHit2D[10];
    
    private Vector2 _previousPosition;
    private bool _isSlicing;
    private bool _isLocked = false; 
    private Camera _mainCamera;

    // Biến theo dõi thời gian đếm ngược của âm thanh
    private float _currentSwooshTimer = 0f;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        if (_gameModel != null && _gameModel.State != null)
        {
            _gameModel.State
                .Where(state => state == GameState.Playing) 
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
        if (_gameModel.State.Value != GameState.Playing) return;
        if (_isLocked) return;

        // Đã xóa biến _currentSwooshTimer ở đây vì không dùng đếm giờ nữa
        
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
            
            _accumulatedDistance = 0f; // Reset lại khi bắt đầu chạm tay
            return;
        }

        view.UpdatePosition(currentWorldPosition);

        float distance = Vector2.Distance(currentWorldPosition, _previousPosition);
        
        if (distance > minSliceVelocity)
        {
            // --- LOGIC ÂM THANH MỚI: TÍNH THEO QUÃNG ĐƯỜNG ---
            _accumulatedDistance += distance;

            // Nếu đã vung đủ 1 đoạn dài -> Phát tiếng và reset lại biến đếm
            if (_accumulatedDistance >= distanceToPlaySwoosh)
            {
                if (swooshSounds != null && swooshSounds.Length > 0 && _audioService != null)
                {
                    // Bốc ngẫu nhiên 1 file âm thanh trong mảng
                    AudioClip randomSwoosh = swooshSounds[Random.Range(0, swooshSounds.Length)];
                    _audioService.PlaySFX(randomSwoosh, volume: 0.6f, randomizePitch: true);
                }
                
                // Trừ đi quãng đường đã dùng thay vì đưa về 0 để tránh mất phần dư
                _accumulatedDistance -= distanceToPlaySwoosh; 
            }
            // --- KẾT THÚC LOGIC ÂM THANH ---

            int hitCount = Physics2D.LinecastNonAlloc(_previousPosition, currentWorldPosition, _hitsBuffer, fruitLayer);
            
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hitCollider = _hitsBuffer[i].collider;
                
                if (hitCollider != null && hitCollider.enabled)
                {
                    hitCollider.enabled = false; 

                    if (hitCollider.TryGetComponent(out FruitController fruit))
                    {
                        Vector2 cutDirection = currentWorldPosition - _previousPosition;
                        
                        if (fruit.IsBomb())
                        {
                            LockBlade(); 
                            fruit.Slice(cutDirection, _previousPosition, currentWorldPosition).Forget(); 
                            break; 
                        }
                        else
                        {
                            fruit.Slice(cutDirection, _previousPosition, currentWorldPosition).Forget();
                        }
                    }
                }
            }
        }
        else
        {
            // Lưỡi kiếm di chuyển quá chậm (người chơi giữ im chuột) -> Reset quãng đường
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
        _accumulatedDistance = 0f; // Reset dọn dẹp khi nhấc tay lên
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
}