using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FruitController : MonoBehaviour
{
    // --- COMPONENT CỦA BẢN THÂN (Cache lại để tăng tốc độ truy xuất) ---
    private Rigidbody2D _rigidbody;
    private Collider2D _collider;

    // --- DEPENDENCIES (Được bơm từ FruitPoolService) ---
    private FruitData _data;
    private IObjectPool<FruitController> _pool;
    
    // Các Service hệ thống
    private GameModel _gameModel;
    private ComboService _comboService;
    private VFXPoolService _vfxPoolService;
    private AudioService _audioService;

    // Chuẩn U6PHA: Awake chỉ dùng để tự khởi tạo (Self-initialization).
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
    }

    // Hàm Setup nhận các Dependency Injection bằng tay từ Pool
    public void Setup(
        FruitData data, 
        IObjectPool<FruitController> pool, 
        GameModel gameModel, 
        ComboService comboService, 
        VFXPoolService vfxPoolService, 
        AudioService audioService)
    {
        _data = data;
        _pool = pool;
        _gameModel = gameModel;
        _comboService = comboService;
        _vfxPoolService = vfxPoolService;
        _audioService = audioService;

        // Reset trạng thái vật lý (Vì nó là Object xài lại từ Pool)
        _rigidbody.velocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;
        
        // Mở lại va chạm chuẩn bị cho lần ném mới
        _collider.enabled = true; 
    }

    // Spawner sẽ gọi hàm này để ném quả trái cây lên
    public void Launch(Vector2 force, float torque)
    {
        _rigidbody.AddForce(force, ForceMode2D.Impulse);
        _rigidbody.AddTorque(torque, ForceMode2D.Impulse);
    }

    // Lưỡi kiếm (Blade) sẽ gọi hàm này khi quét trúng
    public void Slice(Vector2 cutDirection)
    {
        // 1. Tắt Collider ngay lập tức để tránh bug 1 nhát chém trúng 2 lần
        _collider.enabled = false;

        // 2. Phát âm thanh "Chém" hoặc "Nổ" qua AudioPool
        if (_audioService != null && _data.sliceSound != null)
        {
            // Thay đổi cao độ ngẫu nhiên để âm thanh nghe tự nhiên hơn
            _audioService.PlaySound(_data.sliceSound, volume: 1f, pitch: Random.Range(0.9f, 1.15f));
        }

        // 3. Phân loại logic Game (Bom vs Trái cây)
        if (_data.isBomb)
        {
            // Trúng bom -> Thua game
            _gameModel?.TriggerGameOver();
            
            // Sinh hiệu ứng nổ
            if (_data.slicedPrefab != null && _vfxPoolService != null)
            {
                _vfxPoolService.Spawn(_data.slicedPrefab, transform.position, Quaternion.identity);
            }
        }
        else 
        {
            // Trúng trái cây -> Cộng điểm & Tính Combo
            _gameModel?.AddScore(_data.scoreValue);
            _comboService?.RecordSlice(transform.position);

            // Sinh Mảnh vỡ (Sử dụng script PooledSlicedFruit siêu tối ưu)
            if (_data.slicedPrefab != null && _vfxPoolService != null)
            {
                GameObject slicedObj = _vfxPoolService.Spawn(_data.slicedPrefab, transform.position, transform.rotation);
                
                // Tính toán hướng văng vuông góc với đường kiếm chém
                Vector2 perpendicularDir = new Vector2(-cutDirection.y, cutDirection.x).normalized;
                
                // Yêu cầu mảnh vỡ tự văng ra, không dùng GetComponentsInChildren nữa
                if (slicedObj.TryGetComponent(out PooledSlicedFruit pooledVfx))
                {
                    pooledVfx.ApplySliceForce(perpendicularDir, 4f);
                }
            }

            // Sinh Vệt nước ép (Juice Particle)
            if (_data.juiceParticle != null && _vfxPoolService != null)
            {
                GameObject juice = _vfxPoolService.Spawn(_data.juiceParticle, transform.position, Quaternion.identity);
                if (juice.TryGetComponent(out ParticleSystem ps)) 
                {
                    ps.Play();
                }
            }
        }

        // 4. Trả cái "xác nguyên vẹn" này về Pool để Spawner dùng cho lần sau
        Despawn();
    }

    // Nếu trái cây rơi xuống đáy màn hình mà không bị chém, Zone kết liễu sẽ gọi hàm này
    public void Despawn()
    {
        if (gameObject.activeSelf)
        {
            _pool?.Release(this);
        }
    }

    public bool IsBomb()
    {
        if (_data != null)
        {
            return _data.isBomb;
        }
        return false;
    }
}