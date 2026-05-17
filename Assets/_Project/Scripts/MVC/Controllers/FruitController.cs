using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Rigidbody2D))]
public class FruitController : MonoBehaviour, ISliceable
{
    [Header("Audio (Âm thanh cục bộ)")]
    [Tooltip("Kéo AudioSource chứa tiếng xì xì của Bom vào đây (Nếu là trái cây thường thì để trống)")]
    [SerializeField] private AudioSource _loopingAudioSource;

    protected Rigidbody2D _rigidbody;
    protected Collider2D _collider; 

    protected FruitData _data;
    protected IObjectPool<FruitController> _pool;
    protected GameModel _gameModel;
    protected ComboService _comboService;
    protected VFXPoolService _vfxPoolService;
    protected FragmentPoolService _fragmentPoolService;
    protected AudioService _audioService;
    protected ScreenFlashService _screenFlashService;

    protected bool _isSliced = false; 

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
    }

    public void Setup(FruitData data, IObjectPool<FruitController> pool, GameModel gameModel, 
                      ComboService comboService, VFXPoolService vfxPoolService, 
                      AudioService audioService, ScreenFlashService screenFlashService,
                      FragmentPoolService fragmentPoolService)
    {
        _data = data;
        _pool = pool;
        _gameModel = gameModel;
        _comboService = comboService;
        _vfxPoolService = vfxPoolService;
        _audioService = audioService;
        _screenFlashService = screenFlashService;
        _fragmentPoolService = fragmentPoolService; 

        _rigidbody.velocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;
        _collider.enabled = true; 
        _isSliced = false; 

        if (_loopingAudioSource != null && _data.isBomb) 
        {
            _loopingAudioSource.Play();
        }
    }

    public void Launch(Vector2 force, float torque)
    {
        _rigidbody.AddForce(force, ForceMode2D.Impulse);
        _rigidbody.AddTorque(torque, ForceMode2D.Impulse);
    }

    // =========================================================
    // [CẬP NHẬT] Đã thêm velocity và isCritical vào chữ ký hàm
    // =========================================================
    public async UniTaskVoid Slice(Vector2 cutDirection, Vector2 cutStart, Vector2 cutEnd, float velocity, bool isCritical)
    {
        if (_isSliced) return;
        _isSliced = true;

        _collider.enabled = false;

        if (_loopingAudioSource != null && _loopingAudioSource.isPlaying)
        {
            _loopingAudioSource.Stop();
        }
        
        // Không truyền isCritical vào đây vì Juice Hit-Stop của Critical đã được xử lý trên BladeController
        ApplyJuiceEffects();

        if (_data != null && _audioService != null)
        {
            if (_data.impactSounds != null && _data.impactSounds.Length > 0)
            {
                AudioClip impact = _data.impactSounds[Random.Range(0, _data.impactSounds.Length)];
                _audioService.PlaySFX(impact, volume: 1f, randomizePitch: true);
            }

            if (_data.splatterSounds != null && _data.splatterSounds.Length > 0)
            {
                AudioClip splatter = _data.splatterSounds[Random.Range(0, _data.splatterSounds.Length)];
                _audioService.PlaySFX(splatter, volume: 0.85f, randomizePitch: true);
            }

            if (_data.detailSounds != null && _data.detailSounds.Length > 0)
            {
                AudioClip detail = _data.detailSounds[Random.Range(0, _data.detailSounds.Length)];
                _audioService.PlaySFX(detail, volume: 0.6f, randomizePitch: true);
            }
        }

        if (_data.isBomb)
        {
            await HandleBombLogic();
        }
        else 
        {
            // Truyền velocity và isCritical xuống dưới
            HandleFruitLogic(cutDirection, cutStart, cutEnd, velocity, isCritical);
        }

        Despawn();
    }

    private void ApplyJuiceEffects()
    {
        if (_data.isBomb)
        {
            GameJuice.HitStop(0.15f).Forget();
            GameJuice.ShakeCamera(Camera.main.transform, 0.5f, 0.4f).Forget();
            GameJuice.Vibrate(); 
        }
        else
        {
            // Lực chém thường
            GameJuice.HitStop(0.04f).Forget();
        }
    }

    private async UniTask HandleBombLogic()
    {
        if (_screenFlashService != null)
        {
            _screenFlashService.Flashbang(0.6f, 0.4f).Forget();
        }

        await UniTask.WaitForSeconds(0.6f, ignoreTimeScale: true);
        _gameModel?.TriggerGameOver();
    }

    // [CẬP NHẬT] Đã nhận thêm biến để truyền đi tiếp
    private void HandleFruitLogic(Vector2 cutDirection, Vector2 cutStart, Vector2 cutEnd, float velocity, bool isCritical)
    {
        int scoreToAdd = _data.scoreValue;
        
        if (isCritical)
        {
            // Lấy điểm bonus trực tiếp từ cấu hình của quả này
            scoreToAdd += _data.criticalBonusScore; 
        }

        _gameModel?.AddScore(scoreToAdd);
        
        // Spawn mảnh vỡ và truyền lực (velocity) + cờ bùng nổ (isCritical)
        SpawnSlicedPieces(cutDirection, cutStart, cutEnd, velocity, isCritical);

        SpawnLayeredJuiceVFX(cutDirection, isCritical); 
    }

    private void SpawnLayeredJuiceVFX(Vector2 cutDirection, bool isCritical)
    {
        if (_vfxPoolService == null || _data == null) return;

        float cutAngle = Mathf.Atan2(cutDirection.y, cutDirection.x) * Mathf.Rad2Deg;
        Quaternion cutRotation = Quaternion.Euler(0, 0, cutAngle);

        if (!_data.isBomb)
        {
            if (_data.impactVFX != null)
            {
                _vfxPoolService.SpawnParticleWithColorAndRotation(_data.impactVFX, transform.position, _data.splashColor, cutRotation);
            }

            if (_data.splatterVFX != null)
            {
                _vfxPoolService.SpawnParticleWithColorAndRotation(_data.splatterVFX, transform.position, _data.splashColor, cutRotation);
            }

            if (_data.pulpVFX != null)
            {
                _vfxPoolService.SpawnParticleWithColor(_data.pulpVFX, transform.position, _data.splashColor);
            }
            
            // [TÙY CHỌN] Nếu là Critical, bạn có thể gọi thêm một VFX đặc biệt (VD: nổ tia sáng vàng chói) ở đây
            if (isCritical)
            {
                // _vfxPoolService.SpawnParticle(criticalVFXPrefab, transform.position);
            }
        }
    }

    // [CẬP NHẬT] Interface cho class kế thừa văng mảnh vỡ
    protected virtual void SpawnSlicedPieces(Vector2 cutDirection, Vector2 cutStart, Vector2 cutEnd, float velocity, bool isCritical)
    {
        // Class con (như PooledSlicedFruit3D) sẽ override hàm này và nhận vận tốc chém.
    }

    public void Despawn()
    {
        if (gameObject.activeSelf)
        {
            _pool?.Release(this);
        }
    }

    public bool IsBomb() => _data != null && _data.isBomb;

    public FruitData Data => _data;
}