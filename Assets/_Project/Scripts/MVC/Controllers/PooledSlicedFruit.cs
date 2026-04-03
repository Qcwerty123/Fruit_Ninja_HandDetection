using UnityEngine;

// Script này tự động sửa lỗi "mảnh vỡ nằm sai vị trí" khi dùng Object Pool
public class PooledSlicedFruit : MonoBehaviour
{
    // Struct dùng để lưu trữ dữ liệu gốc của từng mảnh vỡ con (trái, phải)
    // Dùng Struct (Value type) để tối ưu bộ nhớ, tránh tạo rác (GC)
    private struct ChildData
    {
        public Transform Transform;
        public Vector3 DefaultLocalPosition;
        public Quaternion DefaultLocalRotation;
        public Rigidbody2D Rb;
    }

    private ChildData[] _childrenData;

    private void Awake()
    {
        // 1. Chỉ chạy 1 lần duy nhất khi Prefab được Instantiate lần đầu
        int childCount = transform.childCount;
        _childrenData = new ChildData[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            
            // Bộ nhớ đệm (Cache) lại vị trí, góc xoay và Rigidbody của các mảnh con
            _childrenData[i] = new ChildData
            {
                Transform = child,
                DefaultLocalPosition = child.localPosition,
                DefaultLocalRotation = child.localRotation,
                Rb = child.GetComponent<Rigidbody2D>()
            };
        }
    }

    // Hàm này được Unity tự động gọi mỗi khi Pool bật Object này lên (SetActive(true))
    private void OnEnable()
    {
        if (_childrenData == null) return;

        // 2. Trả tất cả mảnh vỡ con về vị trí nguyên thủy
        for (int i = 0; i < _childrenData.Length; i++)
        {
            ChildData data = _childrenData[i];
            
            // Reset Transform
            data.Transform.localPosition = data.DefaultLocalPosition;
            data.Transform.localRotation = data.DefaultLocalRotation;

            // Reset Vật lý (RẤT QUAN TRỌNG: Ngăn mảnh vỡ tiếp tục bay theo quán tính cũ)
            if (data.Rb != null)
            {
                data.Rb.velocity = Vector2.zero;
                data.Rb.angularVelocity = 0f;
            }
        }
    }

    // Hàm này giúp FruitController đẩy mảnh vỡ mà không cần GetComponentsInChildren
    public void ApplySliceForce(Vector2 perpendicularDir, float force)
    {
        if (_childrenData.Length >= 2)
        {
            _childrenData[0].Rb.AddForce(perpendicularDir * force, ForceMode2D.Impulse);
            _childrenData[1].Rb.AddForce(-perpendicularDir * force, ForceMode2D.Impulse);
        }
    }    
}