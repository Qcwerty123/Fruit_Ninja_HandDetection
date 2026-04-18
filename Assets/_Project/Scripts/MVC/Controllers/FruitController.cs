using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks; // Đảm bảo đã cài UniTask

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FruitController : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    private Collider2D _collider;

    private FruitData _data;
    private IObjectPool<FruitController> _pool;
    private GameModel _gameModel;
    private ComboService _comboService;
    private VFXPoolService _vfxPoolService;
    private AudioService _audioService;
    private ScreenFlashService _screenFlashService;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
    }

    public void Setup(FruitData data, IObjectPool<FruitController> pool, GameModel gameModel, 
                     ComboService comboService, VFXPoolService vfxPoolService, AudioService audioService, ScreenFlashService screenFlashService)
    {
        _data = data;
        _pool = pool;
        _gameModel = gameModel;
        _comboService = comboService;
        _vfxPoolService = vfxPoolService;
        _audioService = audioService;
        _screenFlashService = screenFlashService;

        _rigidbody.velocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;
        _collider.enabled = true; 
    }

    public void Launch(Vector2 force, float torque)
    {
        _rigidbody.AddForce(force, ForceMode2D.Impulse);
        _rigidbody.AddTorque(torque, ForceMode2D.Impulse);
    }

    public async UniTaskVoid Slice(Vector2 cutDirection)
    {
        _collider.enabled = false;

        // 1. Kích hoạt hiệu ứng ngay lập tức (Rung, Khựng, m thanh)
        // Chúng ta không await ở đây vì muốn các hiệu ứng này chạy song song
        ApplyJuiceEffects();

        if (_audioService != null && _data.sliceSound != null)
        {
            _audioService.PlaySFX(_data.sliceSound, volume: 1f);
        }

        if (_data.isBomb)
        {
            // Đợi vụ nổ diễn ra xong rồi mới báo GameOver
            await HandleBombLogic();
        }
        else 
        {
            HandleFruitLogic(cutDirection);
        }

        Despawn();
    }

    private void ApplyJuiceEffects()
    {
        if (_data.isBomb)
        {
            // Hiệu ứng cực mạnh cho Bom
            GameJuice.HitStop(0.15f).Forget();
            GameJuice.ShakeCamera(Camera.main.transform, 0.5f, 0.4f).Forget();
            GameJuice.Vibrate(); // Rung điện thoại
        }
        else
        {
            // Hiệu ứng nhẹ nhàng cho Trái cây
            GameJuice.HitStop(0.04f).Forget();
            //GameJuice.ShakeCamera(Camera.main.transform, 0.08f, 0.12f).Forget();
        }
    }

    private async UniTask HandleBombLogic()
    {
        // 1. Phun VFX nổ
        if (_data.slicedPrefab != null && _vfxPoolService != null)
        {
            _vfxPoolService.Spawn(_data.slicedPrefab, transform.position, Quaternion.identity);
        }

        // 2. GỌI HIỆU ỨNG CHỚP TRẮNG TỪ TỪ (Mất 0.6s để trắng xoá toàn màn hình)
        if (_screenFlashService != null)
        {
            _screenFlashService.Flashbang(0.6f, 0.4f).Forget();
        }

        // 3. Khoảnh khắc "Nín thở" chờ màn hình trắng hẳn
        await UniTask.WaitForSeconds(0.6f, ignoreTimeScale: true);

        // 4. Lúc này màn hình đã trắng xoá, hiện UI Game Over lên sẽ cực kỳ mượt!
        _gameModel?.TriggerGameOver();
    }

    private void HandleFruitLogic(Vector2 cutDirection)
    {
        _gameModel?.AddScore(_data.scoreValue);
        _comboService?.RecordSlice(transform.position);

        // 1. Sinh Mảnh vỡ với lực văng vuông góc
        if (_data.slicedPrefab != null && _vfxPoolService != null)
        {
            GameObject slicedObj = _vfxPoolService.Spawn(_data.slicedPrefab, transform.position, transform.rotation);
            Vector2 perpendicularDir = new Vector2(-cutDirection.y, cutDirection.x).normalized;
            
            if (slicedObj.TryGetComponent(out PooledSlicedFruit pooledVfx))
            {
                // Truyền thêm vận tốc cũ để mảnh vỡ bay tự nhiên hơn
                pooledVfx.ApplySliceForce(perpendicularDir, 4f);
            }
        }

        // 2. Sinh Nước ép (Splash)
        if (_vfxPoolService != null)
        {
            if (_data.specialJuiceParticle != null)
            {
                _vfxPoolService.Spawn(_data.specialJuiceParticle, transform.position, Quaternion.identity);
            }
            else if (_data.baseJuiceParticle != null)
            {
                _vfxPoolService.SpawnParticleWithColor(_data.baseJuiceParticle, transform.position, _data.splashColor);
            }
        }
    }

    public void Despawn()
    {
        if (gameObject.activeSelf)
        {
            _pool?.Release(this);
        }
    }

    public bool IsBomb()
    {
        if(_data != null)
        {
            return _data.isBomb;
        }
        return false;
    }
}