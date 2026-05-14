using UnityEngine;

// Ép buộc dùng Circle Collider 2D để tối ưu hiệu năng tối đa cho máy yếu
[RequireComponent(typeof(CircleCollider2D))]
public class ClassicFruit : FruitController
{
    protected override void SpawnSlicedPieces(Vector2 cutDirection, Vector2 cutStart, Vector2 cutEnd)
    {
        // Logic hoán đổi Prefab cũ của bạn
        if (_data.classicSlicedPrefab != null && _vfxPoolService != null)
        {
            GameObject slicedObj = _vfxPoolService.Spawn(_data.classicSlicedPrefab, transform.position, transform.rotation);
            Vector2 perpendicularDir = new Vector2(-cutDirection.y, cutDirection.x).normalized;
            
            if (slicedObj.TryGetComponent(out PooledSlicedFruit pooledVfx))
            {
                pooledVfx.ApplySliceForce(perpendicularDir, 4f);
            }
        }
    }
}