using UnityEngine;

// Ép buộc dùng Circle Collider 2D để tối ưu hiệu năng tối đa cho máy yếu
[RequireComponent(typeof(CircleCollider2D))]
public class ClassicFruit : FruitController
{
    // [CẬP NHẬT] Thêm velocity và isCritical
    protected override void SpawnSlicedPieces(Vector2 cutDirection, Vector2 cutStart, Vector2 cutEnd, float velocity, bool isCritical)
    {
        if (_data.classicSlicedPrefab != null && _vfxPoolService != null)
        {
            GameObject slicedObj = _vfxPoolService.Spawn(_data.classicSlicedPrefab, transform.position, transform.rotation);
            
            // Tính toán hướng văng vuông góc với nhát chém
            Vector2 perpendicularDir = new Vector2(-cutDirection.y, cutDirection.x).normalized;
            
            if (slicedObj.TryGetComponent(out PooledSlicedFruit pooledVfx))
            {
                // ==========================================
                // LỰC VĂNG ĐỘNG (DYNAMIC FORCE)
                // ==========================================
                // Ngày xưa dùng 4f tĩnh. Giờ ta scale theo velocity. 
                // velocity của chuột/tay thường dao động từ 0.1 đến 1.5. Nhân với 20 sẽ ra lực tầm 2 -> 30
                float baseForce = velocity * 20f; 
                
                // Khóa giới hạn lực để tránh văng quá chậm (rớt bịch xuống đất) hoặc văng quá lố (biến mất khỏi camera)
                float appliedForce = Mathf.Clamp(baseForce, 3f, 15f);

                // Nếu là chí mạng, bạn có thể chủ động nhân đôi lực ở đây, hoặc để bên trong PooledSlicedFruit xử lý
                if (isCritical)
                {
                    appliedForce *= 1.5f; 
                }

                // Truyền toàn bộ hướng, lực và cờ chí mạng vào
                pooledVfx.ApplySliceForce(perpendicularDir, appliedForce, isCritical);
            }
        }
    }
}