// using UnityEngine;

// // Script này tự động sửa lỗi "mảnh vỡ nằm sai vị trí" khi dùng Object Pool
// public class PooledSlicedFruit : MonoBehaviour
// {
//     // Struct dùng để lưu trữ dữ liệu gốc của từng mảnh vỡ con (trái, phải)
//     // Dùng Struct (Value type) để tối ưu bộ nhớ, tránh tạo rác (GC)
//     private struct ChildData
//     {
//         public Transform Transform;
//         public Vector3 DefaultLocalPosition;
//         public Quaternion DefaultLocalRotation;
//         public Rigidbody2D Rb;
//     }

//     private ChildData[] _childrenData;

//     private void Awake()
//     {
//         // 1. Chỉ chạy 1 lần duy nhất khi Prefab được Instantiate lần đầu
//         int childCount = transform.childCount;
//         _childrenData = new ChildData[childCount];

//         for (int i = 0; i < childCount; i++)
//         {
//             Transform child = transform.GetChild(i);
            
//             // Bộ nhớ đệm (Cache) lại vị trí, góc xoay và Rigidbody của các mảnh con
//             _childrenData[i] = new ChildData
//             {
//                 Transform = child,
//                 DefaultLocalPosition = child.localPosition,
//                 DefaultLocalRotation = child.localRotation,
//                 Rb = child.GetComponent<Rigidbody2D>()
//             };
//         }
//     }

//     // Hàm này được Unity tự động gọi mỗi khi Pool bật Object này lên (SetActive(true))
//     private void OnEnable()
//     {
//         if (_childrenData == null) return;

//         // 2. Trả tất cả mảnh vỡ con về vị trí nguyên thủy
//         for (int i = 0; i < _childrenData.Length; i++)
//         {
//             ChildData data = _childrenData[i];
            
//             // Reset Transform
//             data.Transform.localPosition = data.DefaultLocalPosition;
//             data.Transform.localRotation = data.DefaultLocalRotation;

//             // Reset Vật lý (RẤT QUAN TRỌNG: Ngăn mảnh vỡ tiếp tục bay theo quán tính cũ)
//             if (data.Rb != null)
//             {
//                 data.Rb.velocity = Vector2.zero;
//                 data.Rb.angularVelocity = 0f;
//             }
//         }
//     }

//     // Hàm này giúp FruitController đẩy mảnh vỡ mà không cần GetComponentsInChildren
//     public void ApplySliceForce(Vector2 perpendicularDir, float force)
//     {
//         if (_childrenData.Length >= 2)
//         {
//             _childrenData[0].Rb.AddForce(perpendicularDir * force, ForceMode2D.Impulse);
//             _childrenData[1].Rb.AddForce(-perpendicularDir * force, ForceMode2D.Impulse);
//         }
//     }    
// }
using UnityEngine;

public class PooledSlicedFruit : MonoBehaviour
{
    private struct ChildData
    {
        public Transform Transform;
        public Vector3 DefaultLocalPosition;
        public Quaternion DefaultLocalRotation;
        public Rigidbody Rb;
    }

    private ChildData[] _childrenData;

    private void Awake()
    {
        int childCount = transform.childCount;
        _childrenData = new ChildData[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            _childrenData[i] = new ChildData
            {
                Transform = child,
                DefaultLocalPosition = child.localPosition,
                DefaultLocalRotation = child.localRotation,
                Rb = child.GetComponent<Rigidbody>()
            };
        }
    }

    private void OnEnable()
    {
        if (_childrenData == null) return;

        for (int i = 0; i < _childrenData.Length; i++)
        {
            ChildData data = _childrenData[i];
            
            data.Transform.localPosition = data.DefaultLocalPosition;
            data.Transform.localRotation = data.DefaultLocalRotation;

            if (data.Rb != null)
            {
                data.Rb.velocity = Vector3.zero;
                data.Rb.angularVelocity = Vector3.zero;
            }
        }
    }

    /// <summary>
    /// Áp dụng lực vật lý 3D khi bị chém, lấy cảm hứng từ logic của game gốc.
    /// </summary>
    /// <param name="cutDirection">Hướng di chuyển của lưỡi kiếm (Vector3)</param>
    /// <param name="velocity">Vận tốc của nhát chém</param>
    /// <param name="critical">Cờ đánh dấu chém chí mạng</param>
    public void ApplySliceForce(Vector3 cutDirection, float velocity, bool critical = false)
    {
        // Đảm bảo có đủ 2 mảnh vỡ để áp dụng lực
        if (_childrenData.Length < 2) return;

        Rigidbody partOne = _childrenData[0].Rb;
        Rigidbody partTwo = _childrenData[1].Rb;

        if (partOne == null || partTwo == null) return;

        // Tính toán 2 vector vuông góc với hướng chém để mảnh vỡ văng ra 2 bên
        // (Thay thế cho Utilities.GetVectorFromAngle)
        Vector3 forwardOne = new Vector3(-cutDirection.y, cutDirection.x, 0).normalized;
        Vector3 forwardTwo = -forwardOne;

        float multiplier;

        // 1. Áp dụng lực văng (AddForce)
        if (!critical)
        {
            multiplier = Mathf.Clamp(velocity / 100f, 1f, 2f);
            partOne.AddForce(-forwardOne * Random.Range(2f, 4f) * multiplier, ForceMode.Impulse);
            partTwo.AddForce(-forwardTwo * Random.Range(2f, 4f) * multiplier, ForceMode.Impulse);
        }
        else
        {
            multiplier = 35f;
            partOne.AddForce(-forwardOne * multiplier, ForceMode.Impulse);
            partTwo.AddForce(-forwardTwo * multiplier, ForceMode.Impulse);
        }

        // 2. Áp dụng lực lộn vòng 3D (AddTorque)
        // Lưu ý: Môi trường game vẫn là 2D URP, nhưng model 3D có thể lộn nhào theo trục X, Y, Z
        partOne.AddTorque(new Vector3(
            Random.Range(0.5f, 2f) * -Mathf.Sign(forwardOne.y),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ), ForceMode.Impulse);

        partTwo.AddTorque(new Vector3(
            Random.Range(0.5f, 2f) * -Mathf.Sign(forwardTwo.y),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ), ForceMode.Impulse);
    }
}