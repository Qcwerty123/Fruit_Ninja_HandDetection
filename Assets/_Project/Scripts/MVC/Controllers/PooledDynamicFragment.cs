using UnityEngine;
using UnityEngine.Rendering;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Pool;

public class PooledDynamicFragment : MonoBehaviour
{
    [Header("Core Components")]
    public SpriteRenderer spriteRenderer;
    public PolygonCollider2D polygonCollider;
    public Rigidbody2D rigidBody;
    public SortingGroup sortingGroup;

    [Header("Mask Components")]
    public Transform maskTransform;
    public SpriteMask spriteMask;

    private IObjectPool<PooledDynamicFragment> _parentPool;
    private CancellationTokenSource _despawnCts;

    public void Setup(IObjectPool<PooledDynamicFragment> pool)
    {
        _parentPool = pool;
        
        // Reset và tạo CancellationToken mới cho mỗi lần tái sử dụng
        _despawnCts?.Cancel();
        _despawnCts?.Dispose();
        _despawnCts = new CancellationTokenSource();

        // Phương án dự phòng: Sau 7 giây mà chưa chạm Kill Zone (VD: bay tít lên trời) thì tự hủy
        AutoDespawnFallbackAsync(_despawnCts.Token).Forget();
    }

    private async UniTaskVoid AutoDespawnFallbackAsync(CancellationToken ct)
    {
        // Chờ 7 giây. Nếu CancellationToken bị hủy (do chạm Kill Zone), nó sẽ ngắt ngang ngay lập tức
        bool isCanceled = await UniTask.WaitForSeconds(7f, cancellationToken: ct).SuppressCancellationThrow();
        
        if (!isCanceled && gameObject.activeSelf && _parentPool != null)
        {
            _parentPool.Release(this);
        }
    }

    // ==========================================
    // LOGIC CHẠM KILL ZONE
    // ==========================================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem có chạm vào Object có Tag là "KillZone" không
        if (collision.CompareTag("KillZone"))
        {
            Despawn();
        }
    }

    private void Despawn()
    {
        if (gameObject.activeSelf && _parentPool != null)
        {
            // Ngay khi chạm Kill Zone, hủy cái đếm ngược 7 giây dự phòng đi để nhẹ máy
            _despawnCts?.Cancel(); 
            _parentPool.Release(this);
        }
    }

    public void ResetState()
    {
        rigidBody.velocity = Vector2.zero;
        rigidBody.angularVelocity = 0f;
        polygonCollider.pathCount = 0; 
    }

    private void OnDestroy()
    {
        // Dọn rác khi Scene đóng hoặc game tắt
        _despawnCts?.Cancel();
        _despawnCts?.Dispose();
    }
}