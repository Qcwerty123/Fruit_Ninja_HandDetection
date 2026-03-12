using UnityEngine;

public class ScreenInputService : MonoBehaviour, IInputService
{
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    public bool IsSwiping()
    {
        // Nhận diện chuột trái hoặc vuốt màn hình cảm ứng
        return Input.GetMouseButton(0) || 
              (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved);
    }

    public Vector2 GetCurrentPosition()
    {
        Vector3 screenPos = Input.GetMouseButton(0) ? Input.mousePosition : (Vector3)Input.GetTouch(0).position;
        screenPos.z = 10f; // Set Z để không bị trùng mặt phẳng với Camera
        return _mainCamera.ScreenToWorldPoint(screenPos);
    }

    /* PHIÊN BẢN NÂNG CAO HƠN, CHUẨN PRODUCTION HƠN, GIẢM BUG VỀ TÍNH TOÁN VỊ TRÍ CHÍNH XÁC 100% CHO CÁC TRÒ CHƠI 2D
    public Vector2 GetCurrentPosition()
    {
        Vector3 screenPos = Input.GetMouseButton(0) ? Input.mousePosition : (Vector3)Input.GetTouch(0).position;
        
        // CHUẨN PRODUCTION: Tự động tính khoảng cách từ Camera tới mặt phẳng 2D (Z=0)
        screenPos.z = Mathf.Abs(_mainCamera.transform.position.z); 
        
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
        
        // Trả về exacly Vector2, gọt bỏ hoàn toàn trục Z để tính toán vật lý 2D chính xác 100%
        return new Vector2(worldPos.x, worldPos.y); 
    }
    */
}