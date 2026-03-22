using UnityEngine;
using Reflex.Attributes; // Thư viện DI

public class BladeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BladeView view;
    
    [Header("Settings")]
    [SerializeField] private LayerMask fruitLayer;
    [SerializeField] private float minSliceVelocity = 0.01f;

    // Reflex tự động tìm IInputService trong Container và bơm vào đây
    [Inject] private readonly IInputService _inputService;

    // Bộ đệm tĩnh Zero-Allocation: Tối đa chém trúng 10 object trong 1 frame
    private readonly RaycastHit2D[] _hitsBuffer = new RaycastHit2D[10];
    
    private Vector2 _previousPosition;
    private bool _isSlicing;

    private void Update()
    {
        // An toàn 100%: Dependency chắc chắn đã được Reflex bơm vào trước Update
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
            // Di chuyển đến vị trí mới TRƯỚC khi bật vẽ (chống lỗi dính nét)
            view.UpdatePosition(currentPosition);
            view.StartSlicing();
            _previousPosition = currentPosition;
            return;
        }

        view.UpdatePosition(currentPosition);

        float distance = Vector2.Distance(currentPosition, _previousPosition);
        if (distance > minSliceVelocity)
        {
            // Quét tia vật lý từ vị trí cũ đến vị trí mới
            int hitCount = Physics2D.LinecastNonAlloc(_previousPosition, currentPosition, _hitsBuffer, fruitLayer);
            
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hitCollider = _hitsBuffer[i].collider;
                
                // Kiểm tra va chạm hợp lệ
                if (hitCollider != null && hitCollider.enabled)
                {
                    // Tắt ngay va chạm để chống chém đúp
                    hitCollider.enabled = false; 

                    // Debug để kiểm tra    
                    Debug.Log($"<color=green>CHẶT ĐỨT: {hitCollider.gameObject.name}</color>");
                    
                    // Giao tiếp với trái cây
                    if (hitCollider.TryGetComponent(out FruitController fruit))
                    {
                        Vector2 cutDirection = currentPosition - _previousPosition;
                        fruit.Slice(cutDirection); // Hàm này ta đã viết ở Ngày 3
                    }
                }
            }
        }

        _previousPosition = currentPosition;
    }

    private void StopSlice()
    {
        _isSlicing = false;
        view.StopSlicing();
    }
}