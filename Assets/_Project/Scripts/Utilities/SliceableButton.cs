using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

public class SliceableButton : MonoBehaviour, ISliceable
{
    [Header("Sự kiện Menu (Giống OnClick)")]
    public UnityEvent OnSlicedEvent;

    [Header("Hiển thị 2D ban đầu")]
    [SerializeField] private GameObject _wholeSprite; 

    [Header("Mảnh vỡ 3D và Hiệu ứng")]
    [SerializeField] private PooledSlicedFruit _slicedPrefab3D; 
    [SerializeField] private ParticleSystem _splashParticle; 

    private Collider2D _collider; 
    private bool _isSliced = false;

    public bool IsBomb() => false;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>(); 
    }

    private void OnEnable()
    {
        ResetButton();
    }

    // =========================================================================
    // [CẬP NHẬT] Thay đổi chữ ký hàm để khớp hoàn toàn với Interface ISliceable mới
    // =========================================================================
    public async UniTaskVoid Slice(Vector2 cutDirection, Vector2 cutStart, Vector2 cutEnd, float velocity, bool isCritical)
    {
        if (_isSliced) return;
        _isSliced = true;

        // 1. TẮT COLLIDER NGAY LẬP TỨC để ngăn chém trúng lần 2 trong lúc chờ chuyển cảnh
        if (_collider != null) _collider.enabled = false;

        // 2. Tắt hiển thị Sprite 2D
        if (_wholeSprite != null) _wholeSprite.SetActive(false);

        // 3. Chạy Particle nước ép
        if (_splashParticle != null) _splashParticle.Play();

        // 4. Sinh mảnh vỡ 3D văng ra
        if (_slicedPrefab3D != null)
        {
            Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, 0);
            PooledSlicedFruit slicedObj = Instantiate(_slicedPrefab3D, spawnPos, Quaternion.identity);
            
            // Thay vì dùng cự ly hướng chém giả lập, sử dụng luôn vận tốc vuốt thực tế từ Blade truyền vào
            float appliedVelocity = velocity * 150f;
            
            // Truyền cờ isCritical vào hàm ép lực văng nếu bạn muốn nút bấm Menu cũng bùng nổ khi chém trúng tâm
            slicedObj.ApplySliceForce(cutDirection, appliedVelocity, isCritical);
        }

        // 5. Kích hoạt sự kiện UI chuyển màn hình
        OnSlicedEvent?.Invoke();

        // 6. Dự phòng: Nếu người chơi cứ đứng im ở Menu không bấm gì, sau 1.5s quả sẽ tự hồi phục
        await UniTask.WaitForSeconds(1.5f, ignoreTimeScale: true);
        ResetButton();
    }

    public void ResetButton()
    {
        _isSliced = false;

        if (_collider != null) 
        {
            _collider.enabled = true; 
        }

        if (_wholeSprite != null) 
        {
            _wholeSprite.SetActive(true);
        }
    }
}