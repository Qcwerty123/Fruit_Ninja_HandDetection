// using UnityEngine;

// public class ScreenInputService : IInputService
// {
//     private readonly Camera _mainCamera;

//     public ScreenInputService()
//     {
//         _mainCamera = Camera.main;
//     }

//     public bool IsSwiping()
//     {
//         return Input.GetMouseButton(0) || 
//               (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved);
//     }

//     public Vector2 GetCurrentPosition()
//     {
//         Vector3 screenPos = Input.GetMouseButton(0) ? Input.mousePosition : (Vector3)Input.GetTouch(0).position;
        
//         // Tính Z động dựa trên khoảng cách Camera chuẩn Production
//         screenPos.z = Mathf.Abs(_mainCamera.transform.position.z); 
//         Vector3 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
        
//         return new Vector2(worldPos.x, worldPos.y);
//     }
// }
using UnityEngine;

public class ScreenInputService : IInputService
{
    public bool IsSwiping()
    {
        return Input.GetMouseButton(0) || 
              (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved);
    }

    public Vector2 GetCurrentPosition()
    {
        // Trả về thẳng Tọa độ màn hình (Screen Space) 
        // để đồng bộ với HandTrackingInputService và khớp với BladeController
        if (Input.touchCount > 0)
        {
            return Input.GetTouch(0).position;
        }
        return Input.mousePosition;
    }
}