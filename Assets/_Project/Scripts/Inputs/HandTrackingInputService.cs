using UnityEngine;

public class HandTrackingInputService : IInputService
{
    private readonly UDPReceiverService _udpReceiver;
    private Vector2 _currentPosition;
    private bool _isPinching;

    // Smoothing (Khử nhiễu): Giá trị càng nhỏ càng mượt nhưng sẽ có độ trễ nhẹ
    private const float SmoothTime = 0.15f; 
    private Vector2 _currentVelocity;

    public HandTrackingInputService(UDPReceiverService udpReceiver)
    {
        _udpReceiver = udpReceiver;
    }

    public void Update()
    {
        string data = _udpReceiver.LastData;
        if (string.IsNullOrEmpty(data)) return;

        try
        {
            // Parse chuỗi "X|Y|Pinch"
            string[] parts = data.Split('|');
            if (parts.Length == 3)
            {
                // Tọa độ từ Python là 0->1
                float rawX = float.Parse(parts[0]);
                float rawY = float.Parse(parts[1]);
                _isPinching = parts[2] == "1";

                // Quy đổi sang tọa độ màn hình (Screen Space)
                // Lưu ý: Đảo ngược Y vì MediaPipe (0 là trên) khác Unity (0 là dưới)
                Vector2 targetPos = new Vector2(rawX * Screen.width, (1f - rawY) * Screen.height);

                // Thuật toán SmoothDamp giúp vệt kiếm không bị rung giật (Jitter)
                _currentPosition = Vector2.SmoothDamp(_currentPosition, targetPos, ref _currentVelocity, SmoothTime);
            }
        }
        catch (System.Exception)
        {
            // Bỏ qua nếu gói tin bị lỗi định dạng
        }
    }

    public Vector2 GetCurrentPosition() => _currentPosition;

    public bool IsSwiping() => _isPinching;
}