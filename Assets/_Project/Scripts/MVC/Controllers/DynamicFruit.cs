using UnityEngine;
using Reflex.Attributes;

[RequireComponent(typeof(PolygonCollider2D))]
public class DynamicFruit : FruitController
{
    [Header("Dynamic Slicing Settings")]
    [SerializeField] private Material _sliceMaterial;
    [SerializeField] private GameObject _blankFragmentPrefab;

    // [CẬP NHẬT] Thêm velocity và isCritical vào chữ ký hàm
    protected override void SpawnSlicedPieces(Vector2 cutDirection, Vector2 cutStart, Vector2 cutEnd, float velocity, bool isCritical)
    {
        if (_fragmentPoolService == null || _blankFragmentPrefab == null) return;

        // Xin trực tiếp 2 mảnh vỡ đã được cast chuẩn kiểu PooledDynamicFragment
        PooledDynamicFragment leftHalf = _fragmentPoolService.Spawn(_blankFragmentPrefab, transform.position, transform.rotation);
        PooledDynamicFragment rightHalf = _fragmentPoolService.Spawn(_blankFragmentPrefab, transform.position, transform.rotation);

        if (leftHalf != null && rightHalf != null)
        {
            // Cắt Mesh 2D
            DynamicSlicer2D.Slice(gameObject, cutStart, cutEnd, _sliceMaterial, leftHalf, rightHalf);

            // Tùy chọn (Juice): Nếu class PooledDynamicFragment của bạn có hàm ép lực (AddForce), 
            // bạn có thể lấy velocity và isCritical truyền vào 2 mảnh leftHalf và rightHalf ở đây
            // để chúng văng mạnh sang 2 bên khi chém chí mạng.
        }
    }
}