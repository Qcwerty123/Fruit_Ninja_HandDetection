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

    [Inject] private readonly IInputService _inputService;
    [Inject] private readonly GameModel _gameModel;

    // Bộ đệm tĩnh Zero-Allocation: Tối đa chém trúng 10 object trong 1 frame
    private readonly RaycastHit2D[] _hitsBuffer = new RaycastHit2D[10];
    
    private Vector2 _previousPosition;
    private bool _isSlicing;
    
    // Cờ trạng thái đóng băng lưỡi kiếm
    private bool _isLocked = false; 
    
    // Cache Camera để tránh tốn GC khi gọi Camera.main liên tục
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        // Đăng ký lắng nghe sự thay đổi của GameState
        if (_gameModel != null && _gameModel.State != null)
        {
            _gameModel.State
                .Where(state => state == GameState.Playing) // Chỉ quan tâm khi chuyển sang trạng thái Playing
                .Subscribe(_ => 
                {
                    UnlockBlade(); // Tự động mở khóa lưỡi kiếm
                    StopSlice();   // Reset luôn cả vệt kiếm để không bị dính nét từ ván cũ
                })
                .RegisterTo(destroyCancellationToken);
        }
    }

    private void Update()
    {
        if (_gameModel.State.Value != GameState.Playing) return;
        
        // Nếu lưỡi kiếm bị khóa (do chém trúng bom), dừng cập nhật vị trí mới
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
        // Lấy tọa độ Pixel trên màn hình (Từ Hand Tracking hoặc Chuột)
        Vector2 screenPosition = _inputService.GetCurrentPosition();

        // 2. QUY ĐỔI SANG TỌA ĐỘ THẾ GIỚI 2D (World Space)
        Vector3 worldPos3D = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0f));
        Vector2 currentWorldPosition = new Vector2(worldPos3D.x, worldPos3D.y);

        if (!_isSlicing)
        {
            _isSlicing = true;
            view.UpdatePosition(currentWorldPosition);
            view.StartSlicing();
            _previousPosition = currentWorldPosition;
            return;
        }

        view.UpdatePosition(currentWorldPosition);

        float distance = Vector2.Distance(currentWorldPosition, _previousPosition);
        if (distance > minSliceVelocity)
        {
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
                            fruit.Slice(cutDirection).Forget(); 
                            break; 
                        }
                        else
                        {
                            fruit.Slice(cutDirection).Forget();
                        }
                    }
                }
            }
        }

        // Chỉ cập nhật previousPosition nếu không bị khóa
        if (!_isLocked)
        {
            _previousPosition = currentWorldPosition;
        }
    }

    private void StopSlice()
    {
        _isSlicing = false;
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