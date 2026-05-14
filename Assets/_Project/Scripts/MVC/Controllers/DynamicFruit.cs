using UnityEngine;
using Reflex.Attributes; // Thêm thư viện DI

[RequireComponent(typeof(PolygonCollider2D))]
public class DynamicFruit : FruitController
{
    [Header("Dynamic Slicing Settings")]
    [SerializeField] private Material _sliceMaterial;
    [SerializeField] private GameObject _blankFragmentPrefab;

    protected override void SpawnSlicedPieces(Vector2 cutDirection, Vector2 cutStart, Vector2 cutEnd)
    {
        if (_fragmentPoolService == null || _blankFragmentPrefab == null) return;

        // Xin trực tiếp 2 mảnh vỡ đã được cast chuẩn kiểu PooledDynamicFragment
        PooledDynamicFragment leftHalf = _fragmentPoolService.Spawn(_blankFragmentPrefab, transform.position, transform.rotation);
        PooledDynamicFragment rightHalf = _fragmentPoolService.Spawn(_blankFragmentPrefab, transform.position, transform.rotation);

        if (leftHalf != null && rightHalf != null)
        {
            DynamicSlicer2D.Slice(gameObject, cutStart, cutEnd, _sliceMaterial, leftHalf, rightHalf);
        }
    }
}