using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Rigidbody2D))]
public class FruitController : MonoBehaviour
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

    // [CẬP NHẬT] Thêm biến cờ để ngăn chém 2 lần vào cùng 1 quả
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

        // Reset trạng thái vật lý và cờ chém khi lấy từ Pool ra
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

    public async UniTaskVoid Slice(Vector2 cutDirection, Vector2 cutStart, Vector2 cutEnd)
    {
        // Chặn ngay nếu quả này đã bị chém rồi (Tránh lỗi văng rác Pool)
        if (_isSliced) return;
        _isSliced = true;

        _collider.enabled = false;

        // Tắt tiếng xì xì ngay lập tức nếu là Bom
        if (_loopingAudioSource != null && _loopingAudioSource.isPlaying)
        {
            _loopingAudioSource.Stop();
        }
        
        ApplyJuiceEffects();

        // =========================================================
        // [CẬP NHẬT] HỆ THỐNG AUDIO LAYERING (TRỘN ÂM THANH)
        // =========================================================
        if (_data != null && _audioService != null)
        {
            // 1. Lớp va đập (Khô - Bắt buộc)
            if (_data.impactSounds != null && _data.impactSounds.Length > 0)
            {
                AudioClip impact = _data.impactSounds[Random.Range(0, _data.impactSounds.Length)];
                _audioService.PlaySFX(impact, volume: 1f, randomizePitch: true);
            }

            // 2. Lớp xịt nước (Ướt - Nếu có)
            if (_data.splatterSounds != null && _data.splatterSounds.Length > 0)
            {
                AudioClip splatter = _data.splatterSounds[Random.Range(0, _data.splatterSounds.Length)];
                _audioService.PlaySFX(splatter, volume: 0.85f, randomizePitch: true);
            }

            // 3. Lớp chi tiết (Nhỏ giọt - Nếu có)
            if (_data.detailSounds != null && _data.detailSounds.Length > 0)
            {
                AudioClip detail = _data.detailSounds[Random.Range(0, _data.detailSounds.Length)];
                _audioService.PlaySFX(detail, volume: 0.6f, randomizePitch: true);
            }
        }
        // =========================================================

        if (_data.isBomb)
        {
            await HandleBombLogic();
        }
        else 
        {
            HandleFruitLogic(cutDirection, cutStart, cutEnd);
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

    // private void HandleFruitLogic(Vector2 cutDirection, Vector2 cutStart, Vector2 cutEnd)
    // {
    //     _gameModel?.AddScore(_data.scoreValue);
    //     _comboService?.RecordSlice(transform.position);

    //     SpawnSlicedPieces(cutDirection, cutStart, cutEnd);

    //     if (_vfxPoolService != null)
    //     {
    //         if (_data.specialJuiceParticle != null)
    //         {
    //             _vfxPoolService.Spawn(_data.specialJuiceParticle, transform.position, Quaternion.identity);
    //         }
    //         else if (_data.baseJuiceParticle != null)
    //         {
    //             _vfxPoolService.SpawnParticleWithColor(_data.baseJuiceParticle, transform.position, _data.splashColor);
    //         }
    //     }
    // }

    private void HandleFruitLogic(Vector2 cutDirection, Vector2 cutStart, Vector2 cutEnd)
    {
        _gameModel?.AddScore(_data.scoreValue);
        _comboService?.RecordSlice(transform.position);

        SpawnSlicedPieces(cutDirection, cutStart, cutEnd);

        // [CẬP NHẬT] Gọi hệ thống VFX phân lớp và truyền hướng chém vào
        SpawnLayeredJuiceVFX(cutDirection); 
    }

    // ==========================================
    // TẠO VFX PHÂN LỚP THEO HƯỚNG CHÉM (CẬP NHẬT)
    // ==========================================
    private void SpawnLayeredJuiceVFX(Vector2 cutDirection)
    {
        if (_vfxPoolService == null || _data == null) return;

        // Tính toán góc của nhát chém
        float cutAngle = Mathf.Atan2(cutDirection.y, cutDirection.x) * Mathf.Rad2Deg;
        Quaternion cutRotation = Quaternion.Euler(0, 0, cutAngle);

        if (!_data.isBomb)
        {
            // 1. Lớp Impact (ĐÃ SỬA: Ép xoay theo hướng chém)
            if (_data.impactVFX != null)
            {
                _vfxPoolService.SpawnParticleWithColorAndRotation(_data.impactVFX, transform.position, _data.splashColor, cutRotation);
            }

            // 2. Lớp Splatter (Tia nước xoay theo hướng chém)
            if (_data.splatterVFX != null)
            {
                _vfxPoolService.SpawnParticleWithColorAndRotation(_data.splatterVFX, transform.position, _data.splashColor, cutRotation);
            }

            // 3. Lớp Pulp (Rơi tự do hoặc dính tường, tỏa tròn đều nên không cần xoay)
            if (_data.pulpVFX != null)
            {
                _vfxPoolService.SpawnParticleWithColor(_data.pulpVFX, transform.position, _data.splashColor);
            }
        }
    }

    protected virtual void SpawnSlicedPieces(Vector2 cutDirection, Vector2 cutStart, Vector2 cutEnd)
    {
        // Lớp cha không làm gì cả
    }

    public void Despawn()
    {
        if (gameObject.activeSelf)
        {
            _pool?.Release(this);
        }
    }

    public bool IsBomb() => _data != null && _data.isBomb;
}