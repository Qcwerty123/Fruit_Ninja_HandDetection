using UnityEngine;

public class BladeController : MonoBehaviour
{
    [SerializeField] private BladeView view;
    [SerializeField] private LayerMask fruitLayer;
    [SerializeField] private float minSliceVelocity = 0.01f;

    private IInputService _inputService;
    private Vector2 _previousPosition;
    private bool _isSlicing;

    // Hàm nhận Dependency từ bên ngoài bơm vào
    public void Construct(IInputService inputService)
    {
        _inputService = inputService;
    }

    private void Update()
    {
        // Chặn logic nếu chưa có Input Service
        if (_inputService == null) return;

        if (_inputService.IsSwiping())
        {
            ContinueSlice();
        }
        else if (_isSlicing)
        {
            StopSlice();
        }
    }

    private void ContinueSlice()
    {
        Vector2 currentPosition = _inputService.GetCurrentPosition();

        if (!_isSlicing)
        {
            _isSlicing = true;
            
            view.UpdatePosition(currentPosition);

            view.StartSlicing();
            
            _previousPosition = currentPosition;
            return;
        }

        view.UpdatePosition(currentPosition);

        float distance = Vector2.Distance(currentPosition, _previousPosition);
        if (distance > minSliceVelocity)
        {
            RaycastHit2D hit = Physics2D.Linecast(_previousPosition, currentPosition, fruitLayer);
            
            if (hit.collider != null)
            {
                Debug.Log($"<color=green>CHẶT ĐỨT: {hit.collider.gameObject.name}</color>");
            }
        }

        _previousPosition = currentPosition;
    }

    private void StopSlice()
    {
        _isSlicing = false;
        view.StopSlicing();
    }
}