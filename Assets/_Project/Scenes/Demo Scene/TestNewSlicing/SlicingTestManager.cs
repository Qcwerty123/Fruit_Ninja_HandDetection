using UnityEngine;

public class SlicingTestManager : MonoBehaviour
{
    [SerializeField] private Material sliceMaterial; // Vật liệu cho 2 mảnh vỡ
    [SerializeField] private LayerMask fruitLayer;   // Layer của trái cây test

    private Vector2 _startPos;
    private Camera _cam;

    private void Awake() => _cam = Camera.main;

    private void Update()
    {
        // 1. Nhấn chuột trái để đặt điểm bắt đầu
        if (Input.GetMouseButtonDown(0))
        {
            _startPos = _cam.ScreenToWorldPoint(Input.mousePosition);
        }

        // 2. Thả chuột trái để thực hiện nhát chém
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 endPos = _cam.ScreenToWorldPoint(Input.mousePosition);
            ExecuteTestSlice(_startPos, endPos);
        }
    }

    private void ExecuteTestSlice(Vector2 start, Vector2 end)
    {
        // Vẽ đường chém để debug trong Scene view
        Debug.DrawLine(start, end, Color.red, 2f);

        // Quét tia để tìm trái cây
        RaycastHit2D hit = Physics2D.Linecast(start, end, fruitLayer);

        if (hit.collider != null)
        {
            GameObject target = hit.collider.gameObject;
            
            // Gọi thuật toán cắt động (Đã viết ở bước trước)
            GameObject[] halves = DynamicSlicer2D.Slice(target, start, end, sliceMaterial);

            if (halves != null)
            {
                target.SetActive(false); // Tắt quả cũ
                Debug.Log("<color=green>[Test]</color> Cắt thành công!");
            }
        }
    }
}