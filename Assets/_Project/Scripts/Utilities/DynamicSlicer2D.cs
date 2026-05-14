using UnityEngine;
using System.Collections.Generic;

public static class DynamicSlicer2D
{
    private static Sprite _squareMask;
    
    // TỐI ƯU RAM: Tái sử dụng List tĩnh (Zero-Allocation) để không xả rác mỗi lần chém
    private static readonly List<Vector2> _leftPoints = new List<Vector2>(32);
    private static readonly List<Vector2> _rightPoints = new List<Vector2>(32);

    public static void Slice(GameObject target, Vector2 cutStart, Vector2 cutEnd, Material sliceMaterial, PooledDynamicFragment leftHalf, PooledDynamicFragment rightHalf)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        PolygonCollider2D originalCol = target.GetComponent<PolygonCollider2D>();
        
        if (sr == null || sr.sprite == null || originalCol == null) return;

        Vector2 cutDir = (cutEnd - cutStart).normalized;
        float angle = Mathf.Atan2(cutDir.y, cutDir.x) * Mathf.Rad2Deg;

        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        Vector2 originalVelocity = rb != null ? rb.velocity : Vector2.zero;

        leftHalf.ResetState();
        rightHalf.ResetState();

        // --- SỬA LỖI LỆCH MASK (QUAN TRỌNG) ---
        // Không dùng cutStart. Ta phải "chiếu" tâm của trái cây vuông góc xuống đường cắt 
        // để đảm bảo Mask luôn luôn nằm CHÍNH GIỮA trái cây, bất kể đường chém dài bao nhiêu.
        Vector2 fruitPos = target.transform.position;
        Vector2 exactMaskPos = cutStart + Vector2.Dot(fruitPos - cutStart, cutDir) * cutDir;

        // 1. Cấu hình hình ảnh (Truyền exactMaskPos vào)
        ConfigVisual(leftHalf, target, exactMaskPos, angle, true, sr);
        ConfigVisual(rightHalf, target, exactMaskPos, angle, false, sr);

        // 2. Chia đôi khung Collider
        Vector2 localStart = target.transform.InverseTransformPoint(cutStart);
        Vector2 localEnd = target.transform.InverseTransformPoint(cutEnd);
        Vector2 localDir = (localEnd - localStart).normalized;

        SplitCollider(originalCol, leftHalf.polygonCollider, rightHalf.polygonCollider, localStart, localDir);

        // 3. Bật Vật lý
        ConfigPhysics(leftHalf.rigidBody, originalVelocity, cutDir, true);
        ConfigPhysics(rightHalf.rigidBody, originalVelocity, cutDir, false);
    }

    private static void SplitCollider(PolygonCollider2D originalCol, PolygonCollider2D leftCol, PolygonCollider2D rightCol, Vector2 localCutStart, Vector2 localCutDir)
    {
        // Dọn dẹp List tĩnh để xài lại
        _leftPoints.Clear();
        _rightPoints.Clear();

        Vector2[] points = originalCol.GetPath(0);

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 currentPoint = points[i];
            Vector2 nextPoint = points[(i + 1) % points.Length]; 

            bool currentIsLeft = IsLeftSide(localCutStart, localCutDir, currentPoint);
            bool nextIsLeft = IsLeftSide(localCutStart, localCutDir, nextPoint);

            if (currentIsLeft) _leftPoints.Add(currentPoint);
            else _rightPoints.Add(currentPoint);

            if (currentIsLeft != nextIsLeft)
            {
                Vector2 intersection = GetIntersection(currentPoint, nextPoint, localCutStart, localCutStart + localCutDir);
                _leftPoints.Add(intersection);
                _rightPoints.Add(intersection);
            }
        }

        if (_leftPoints.Count > 2)
        {
            leftCol.SetPath(0, _leftPoints.ToArray());
            leftCol.offset = originalCol.offset; 
        }
        if (_rightPoints.Count > 2)
        {
            rightCol.SetPath(0, _rightPoints.ToArray());
            rightCol.offset = originalCol.offset;
        }
    }

    private static void ConfigVisual(PooledDynamicFragment half, GameObject original, Vector2 maskPosition, float cutAngle, bool isLeft, SpriteRenderer originalSr)
    {
        half.transform.position = original.transform.position;
        half.transform.rotation = original.transform.rotation;
        half.transform.localScale = original.transform.localScale;

        half.spriteRenderer.sprite = originalSr.sprite;
        half.spriteRenderer.material = originalSr.material;
        half.spriteRenderer.sortingLayerID = originalSr.sortingLayerID;
        half.spriteRenderer.sortingOrder = originalSr.sortingOrder;
        half.spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        half.maskTransform.position = maskPosition; 
        float maskAngle = isLeft ? cutAngle : cutAngle + 180f;
        half.maskTransform.rotation = Quaternion.Euler(0, 0, maskAngle);
        
        // --- SỬA LỖI SCALE MASK ---
        // Triệt tiêu ảnh hưởng của scale từ Object cha để Mask không bị bóp méo
        float inverseScaleX = 1f / half.transform.localScale.x;
        float inverseScaleY = 1f / half.transform.localScale.y;

        float fruitMaxSize = Mathf.Max(originalSr.bounds.size.x, originalSr.bounds.size.y);
        float dynamicMaskSize = fruitMaxSize * 3f; // Tăng hệ số lên x3 cho an toàn tuyệt đối
        
        half.maskTransform.localScale = new Vector3(dynamicMaskSize * inverseScaleX, dynamicMaskSize * inverseScaleY, 1f);

        half.spriteMask.sprite = GetSquareMask();
    }

    private static void ConfigPhysics(Rigidbody2D rb, Vector2 originalVelocity, Vector2 cutDir, bool isLeft)
    {
        rb.velocity = originalVelocity;
        rb.mass = isLeft ? 0.8f : 1.2f;

        Vector2 pushDir = isLeft ? new Vector2(-cutDir.y, cutDir.x) : new Vector2(cutDir.y, -cutDir.x);
        rb.AddForce(pushDir * 4f, ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-150f, 150f)); 
    }

    private static bool IsLeftSide(Vector2 linePoint, Vector2 lineDir, Vector2 point) => (lineDir.x * (point.y - linePoint.y) - lineDir.y * (point.x - linePoint.x)) > 0;

    private static Vector2 GetIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float d = (p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x);
        if (d == 0) return p1; 
        float t = ((p1.x - p3.x) * (p3.y - p4.y) - (p1.y - p3.y) * (p3.x - p4.x)) / d;
        return new Vector2(p1.x + t * (p2.x - p1.x), p1.y + t * (p2.y - p1.y));
    }

    private static Sprite GetSquareMask()
    {
        if (_squareMask == null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _squareMask = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0f), 1f);
        }
        return _squareMask;
    }
}