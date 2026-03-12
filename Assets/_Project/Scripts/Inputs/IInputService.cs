using UnityEngine;

public interface IInputService
{
    // Kiểm tra xem người chơi có đang thực hiện thao tác chém không
    bool IsSwiping(); 
    
    // Lấy tọa độ chém hiện tại (đã convert sang World Space)
    Vector2 GetCurrentPosition(); 
}