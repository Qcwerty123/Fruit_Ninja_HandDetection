using UnityEngine;

public class SlicingTestManager : MonoBehaviour
{
    [Header("Cài đặt Chém (Slicing)")]
    [SerializeField] private Material sliceMaterial; 
    [SerializeField] private LayerMask fruitLayer;   
    [SerializeField] private GameObject blankFragmentPrefab;

    [Header("Cài đặt Stress Test (Spawner)")]
    [Tooltip("Kéo Prefab trái cây test (đã có Rigidbody2D và PolygonCollider2D) vào đây")]
    [SerializeField] private GameObject fruitPrefab;
    
    [Tooltip("Thời gian sinh ra 1 quả mới (giây). Càng nhỏ càng lag!")]
    [Range(0.05f, 2f)]
    [SerializeField] private float spawnInterval = 0.3f;
    
    [Tooltip("Độ rộng của khu vực bắn trái cây")]
    [SerializeField] private float spawnWidth = 6f;

    private Vector2 _previousPos;
    private Camera _cam;
    private bool _isSlicing;
    private float _spawnTimer;

    private void Awake() => _cam = Camera.main;

    private void Update()
    {
        // --- LOGIC 1: STRESS TEST SPAWNER ---
        if (fruitPrefab != null)
        {
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= spawnInterval)
            {
                _spawnTimer = 0f;
                SpawnTestFruit();
            }
        }

        // --- LOGIC 2: CONTINUOUS SLICING ---
        if (Input.GetMouseButtonDown(0))
        {
            _previousPos = _cam.ScreenToWorldPoint(Input.mousePosition);
            _isSlicing = true;
        }

        if (Input.GetMouseButton(0) && _isSlicing)
        {
            Vector2 currentPos = _cam.ScreenToWorldPoint(Input.mousePosition);

            if (Vector2.Distance(currentPos, _previousPos) > 0.05f)
            {
                ExecuteTestSlice(_previousPos, currentPos);
                _previousPos = currentPos; 
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            _isSlicing = false;
        }
    }

    private void SpawnTestFruit()
    {
        // Random vị trí xuất phát ở cạnh dưới màn hình
        float randomX = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
        Vector2 spawnPos = new Vector2(randomX, -5f); // Tọa độ Y = -5 (Dưới đáy cam)

        GameObject fruit = Instantiate(fruitPrefab, spawnPos, Quaternion.identity);

        // Bắn trái cây bay lên trên với một góc ngẫu nhiên
        Rigidbody2D rb = fruit.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float forceX = Random.Range(-3f, 3f);
            float forceY = Random.Range(12f, 18f); // Lực đẩy lên (Impulse)
            
            rb.AddForce(new Vector2(forceX, forceY), ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-200f, 200f)); // Xoay vòng vòng
        }

        // Tự hủy quả nguyên vẹn nếu 5 giây không ai chém trúng (Tránh rác bộ nhớ)
        Destroy(fruit, 5f);
    }

    private void ExecuteTestSlice(Vector2 start, Vector2 end)
    {
        Debug.DrawLine(start, end, Color.red, 0.5f);

        RaycastHit2D hit = Physics2D.Linecast(start, end, fruitLayer);

        if (hit.collider != null)
        {
            GameObject target = hit.collider.gameObject;
            
            // 1. Sinh ra 2 vỏ rỗng ngay tại vị trí trái cây (Dùng Instantiate cho Scene Test)
            GameObject leftObj = Instantiate(blankFragmentPrefab, target.transform.position, target.transform.rotation);
            GameObject rightObj = Instantiate(blankFragmentPrefab, target.transform.position, target.transform.rotation);

            // 2. Lấy component và nạp vào thuật toán
            if (leftObj.TryGetComponent(out PooledDynamicFragment leftHalf) && 
                rightObj.TryGetComponent(out PooledDynamicFragment rightHalf))
            {
                // Setup với tham số null vì Test Scene không dùng Pool, không cần tự động trả về kho
                leftHalf.Setup(null);
                rightHalf.Setup(null);

                // Thuật toán cắt động Zero-Allocation
                DynamicSlicer2D.Slice(target, start, end, sliceMaterial, leftHalf, rightHalf);

                target.SetActive(false); 

                // Tự hủy thủ công sau 3 giây để dọn rác (Do không dùng Pool)
                Destroy(leftObj, 3f);
                Destroy(rightObj, 3f);
            }
        }
    }
}