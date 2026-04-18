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
        
        // 1. Nếu lưỡi kiếm bị khóa (do chém trúng bom), dừng cập nhật vị trí mới
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
        Vector2 currentPosition = _inputService.GetCurrentPosition();

        if (!_isSlicing)
        {
            _isSlicing = true;
            view.UpdatePosition(currentPosition);
            view.StartSlicing();
            _previousPosition = currentPosition;
            return;
        }

        view.UpdatePosition(currentPosition);

        float distance = Vector2.Distance(currentPosition, _previousPosition);
        if (distance > minSliceVelocity)
        {
            int hitCount = Physics2D.LinecastNonAlloc(_previousPosition, currentPosition, _hitsBuffer, fruitLayer);
            
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hitCollider = _hitsBuffer[i].collider;
                
                if (hitCollider != null && hitCollider.enabled)
                {
                    hitCollider.enabled = false; 

                    // Debug.Log($"<color=green>CHẶT ĐỨT: {hitCollider.gameObject.name}</color>");
                    
                    if (hitCollider.TryGetComponent(out FruitController fruit))
                    {
                        Vector2 cutDirection = currentPosition - _previousPosition;
                        
                        // 2. TÁCH BIỆT LOGIC BOM VÀ TRÁI CÂY TẠI ĐÂY
                        if (fruit.IsBomb())
                        {
                            LockBlade(); // Đóng băng Trail ngay tại tọa độ va chạm
                            fruit.Slice(cutDirection).Forget(); // Gọi Forget() chuẩn convention UniTask
                            
                            // QUAN TRỌNG: Ngắt vòng lặp ngay lập tức!
                            // Nếu quét 1 đường trúng quả Bom và 1 quả Táo phía sau,
                            // game sẽ chỉ ghi nhận nổ Bom, quả Táo được an toàn.
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
            _previousPosition = currentPosition;
        }
    }

    private void StopSlice()
    {
        _isSlicing = false;
        view.StopSlicing();
    }

    /// <summary>
    /// Đóng băng cập nhật tọa độ của lưỡi kiếm
    /// </summary>
    public void LockBlade()
    {
        _isLocked = true;
        // Tùy chọn: Bạn có thể gọi view.StopSlicing() ở đây nếu muốn vệt kiếm 
        // mờ dần ngay lúc trúng bom thay vì đóng băng trên màn hình.
    }

    /// <summary>
    /// Gọi hàm này khi bắt đầu ván mới (Vd: Nhấn nút Chơi Lại)
    /// </summary>
    public void UnlockBlade()
    {
        _isLocked = false;
    }
}