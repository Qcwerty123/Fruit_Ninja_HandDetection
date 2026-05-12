using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public static class DynamicSlicer2D
{
    private static Sprite _squareMask;

    public static GameObject[] Slice(GameObject target, Vector2 cutStart, Vector2 cutEnd, Material sliceMaterial)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        PolygonCollider2D originalCol = target.GetComponent<PolygonCollider2D>();
        
        if (sr == null || sr.sprite == null || originalCol == null) 
        {
            Debug.LogWarning("<color=red>[Slicer]</color> Trái cây cần có PolygonCollider2D để cắt!");
            return null;
        }

        Vector2 cutDir = (cutEnd - cutStart).normalized;
        float angle = Mathf.Atan2(cutDir.y, cutDir.x) * Mathf.Rad2Deg;

        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        Vector2 originalVelocity = rb != null ? rb.velocity : Vector2.zero;

        // 1. Tạo 2 mảnh ghép (Xử lý phần Nhìn - Visual)
        GameObject leftHalf = CreateHalf("LeftPiece", target, cutStart, angle, true);
        GameObject rightHalf = CreateHalf("RightPiece", target, cutStart, angle, false);

        // 2. Chuyển đổi tọa độ nhát chém sang Không gian cục bộ của trái cây
        Vector2 localStart = target.transform.InverseTransformPoint(cutStart);
        Vector2 localEnd = target.transform.InverseTransformPoint(cutEnd);
        Vector2 localDir = (localEnd - localStart).normalized;

        // 3. Xử lý phần Chạm (Physics) - Chia đôi khung Collider
        SplitCollider(originalCol, leftHalf, rightHalf, localStart, localDir);

        // 4. Bật Vật lý để chúng văng ra (Tắt IgnoreCollision vì giờ khung va chạm đã hoàn hảo)
        AddPhysics(leftHalf, originalVelocity, cutDir, true);
        AddPhysics(rightHalf, originalVelocity, cutDir, false);

        return new GameObject[] { leftHalf, rightHalf };
    }

    // ==========================================
    // LOGIC TOÁN HỌC: CẮT KHUNG VA CHẠM (COLLIDER)
    // ==========================================
    private static void SplitCollider(PolygonCollider2D originalCol, GameObject leftHalf, GameObject rightHalf, Vector2 localCutStart, Vector2 localCutDir)
    {
        Vector2[] points = originalCol.GetPath(0);
        List<Vector2> leftPoints = new List<Vector2>();
        List<Vector2> rightPoints = new List<Vector2>();

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 currentPoint = points[i];
            Vector2 nextPoint = points[(i + 1) % points.Length]; 

            bool currentIsLeft = IsLeftSide(localCutStart, localCutDir, currentPoint);
            bool nextIsLeft = IsLeftSide(localCutStart, localCutDir, nextPoint);

            if (currentIsLeft) leftPoints.Add(currentPoint);
            else rightPoints.Add(currentPoint);

            if (currentIsLeft != nextIsLeft)
            {
                Vector2 intersection = GetIntersection(currentPoint, nextPoint, localCutStart, localCutStart + localCutDir);
                leftPoints.Add(intersection);
                rightPoints.Add(intersection);
            }
        }

        // [ĐÃ SỬA] Gán lại chuẩn xác Offset từ trái cây gốc sang mảnh vỡ
        if (leftPoints.Count > 2)
        {
            PolygonCollider2D col = leftHalf.AddComponent<PolygonCollider2D>();
            col.SetPath(0, leftPoints.ToArray());
            col.offset = originalCol.offset; 
        }
        if (rightPoints.Count > 2)
        {
            PolygonCollider2D col = rightHalf.AddComponent<PolygonCollider2D>();
            col.SetPath(0, rightPoints.ToArray());
            col.offset = originalCol.offset;
        }
    }

    // Tính toán Tích có hướng (Cross Product) để phân loại Trái/Phải
    private static bool IsLeftSide(Vector2 linePoint, Vector2 lineDir, Vector2 point)
    {
        return (lineDir.x * (point.y - linePoint.y) - lineDir.y * (point.x - linePoint.x)) > 0;
    }

    // Thuật toán tìm điểm giao nhau của 2 đoạn thẳng
    private static Vector2 GetIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float d = (p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x);
        if (d == 0) return p1; // Tránh lỗi chia cho 0 nếu song song
        
        float t = ((p1.x - p3.x) * (p3.y - p4.y) - (p1.y - p3.y) * (p3.x - p4.x)) / d;
        return new Vector2(p1.x + t * (p2.x - p1.x), p1.y + t * (p2.y - p1.y));
    }

    // ==========================================
    // LOGIC HIỂN THỊ VÀ VẬT LÝ
    // ==========================================
    private static GameObject CreateHalf(string name, GameObject original, Vector2 cutStart, float cutAngle, bool isLeft)
    {
        GameObject half = new GameObject(name);
        half.transform.position = original.transform.position;
        half.transform.rotation = original.transform.rotation;
        half.transform.localScale = original.transform.localScale;

        half.AddComponent<SortingGroup>();

        SpriteRenderer originalSr = original.GetComponent<SpriteRenderer>();
        SpriteRenderer sr = half.AddComponent<SpriteRenderer>();
        sr.sprite = originalSr.sprite;
        sr.material = originalSr.material;
        sr.sortingLayerID = originalSr.sortingLayerID;
        sr.sortingOrder = originalSr.sortingOrder;
        sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        GameObject maskObj = new GameObject("Mask");
        maskObj.transform.SetParent(half.transform);
        maskObj.transform.position = cutStart; 
        
        // [ĐÃ SỬA] Công thức xoay chuẩn xác: 
        // Bên trái thì xoay đúng bằng góc chém. Bên phải thì xoay ngược lại 180 độ.
        float maskAngle = isLeft ? cutAngle : cutAngle + 180f;
        maskObj.transform.rotation = Quaternion.Euler(0, 0, maskAngle);
        
        // Phóng to mặt nạ lên mức tối đa để không bao giờ bị hụt khi chém từ xa
        maskObj.transform.localScale = new Vector3(10000f, 10000f, 1f);

        SpriteMask mask = maskObj.AddComponent<SpriteMask>();
        mask.sprite = GetSquareMask();

        return half;
    }

    private static void AddPhysics(GameObject half, Vector2 originalVelocity, Vector2 cutDir, bool isLeft)
    {
        // Collider đã được thêm ở hàm SplitCollider, giờ chỉ thêm Rigidbody
        Rigidbody2D rb = half.AddComponent<Rigidbody2D>();
        rb.velocity = originalVelocity;

        // Trọng lượng thay đổi tùy mảnh to hay nhỏ (Tùy chọn)
        rb.mass = isLeft ? 0.8f : 1.2f;

        Vector2 pushDir = isLeft ? Vector2.Perpendicular(cutDir) : -Vector2.Perpendicular(cutDir);
        rb.AddForce(pushDir * 4f, ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-150f, 150f)); 
    }

    private static Sprite GetSquareMask()
    {
        if (_squareMask == null)
        {
            Texture2D tex = new Texture2D(2, 2);
            Color[] colors = new Color[] { Color.white, Color.white, Color.white, Color.white };
            tex.SetPixels(colors);
            tex.Apply();
            _squareMask = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0f), 100f);
        }
        return _squareMask;
    }
}