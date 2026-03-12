using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class BladeView : MonoBehaviour
{
    private TrailRenderer _trailRenderer;

    private void Awake()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
        _trailRenderer.emitting = false; 
    }

    public void UpdatePosition(Vector2 position)
    {
        transform.position = position;
    }

    public void StartSlicing()
    {
        _trailRenderer.Clear(); 
        _trailRenderer.emitting = true;
    }

    public void StopSlicing()
    {
        _trailRenderer.emitting = false;
        _trailRenderer.Clear(); // Xóa vệt cũ để không bị nối nét khi chém nhát mới
    }
}