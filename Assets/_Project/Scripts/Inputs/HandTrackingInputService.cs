using UnityEngine;

public class HandTrackingInputService : IInputService
{
    private readonly UDPReceiverService _udpReceiver;
    private Vector2 _currentPosition;
    
    // Thuật toán Ghost Tracking (Bù đắp frame bị mất)
    private bool _isSlashing;
    private float _lastSlashingTime;
    private const float GracePeriod = 0.15f; // Thời gian "châm chước" nếu tay bị nhòe

    // Smoothing (Khử nhiễu)
    private const float SmoothTime = 0.15f; 
    private Vector2 _currentVelocity;

    public HandTrackingInputService(UDPReceiverService udpReceiver)
    {
        _udpReceiver = udpReceiver;
    }

    // public void Update()
    // {
    //     string data = _udpReceiver.LastData;
    //     if (string.IsNullOrEmpty(data)) return;

    //     try
    //     {
    //         string[] parts = data.Split('|');
    //         if (parts.Length == 3)
    //         {
    //             float rawX = float.Parse(parts[0]);
    //             float rawY = float.Parse(parts[1]);
    //             bool isSlashingNow = parts[2] == "1";

    //             Vector2 targetPos = new Vector2(rawX * Screen.width, (1f - rawY) * Screen.height);
    //             _currentPosition = Vector2.SmoothDamp(_currentPosition, targetPos, ref _currentVelocity, SmoothTime);

    //             // --- LOGIC GRACE PERIOD ---
    //             if (isSlashingNow)
    //             {
    //                 _isSlashing = true;
    //                 _lastSlashingTime = Time.time; // Cập nhật mốc thời gian vung tay cuối cùng
    //             }
    //             else
    //             {
    //                 // Nếu nhận số 0, đừng tắt kiếm ngay. Hãy đợi quá 0.15s xem có phải do camera mất nét không.
    //                 if (Time.time - _lastSlashingTime > GracePeriod)
    //                 {
    //                     _isSlashing = false;
    //                 }
    //             }
    //         }
    //     }
    //     catch (System.Exception)
    //     {
    //         // Bỏ qua nếu gói tin bị lỗi định dạng
    //     }
    // }

    public void Update()
    {
        string data = _udpReceiver.LastData;
        if (string.IsNullOrEmpty(data)) return;

        try
        {
            string[] parts = data.Split('|');
            if (parts.Length == 3)
            {
                float rawX = float.Parse(parts[0]);
                float rawY = float.Parse(parts[1]);
                bool isSlashingNow = parts[2] == "1";

                Vector2 targetPos = new Vector2(rawX * Screen.width, (1f - rawY) * Screen.height);
                
                // 1. ÉP DÙNG UNSCALED DELTA TIME Ở ĐÂY
                _currentPosition = Vector2.SmoothDamp(
                    _currentPosition, 
                    targetPos, 
                    ref _currentVelocity, 
                    SmoothTime, 
                    Mathf.Infinity, 
                    Time.unscaledDeltaTime // Tham số bí mật để thoát khỏi TimeScale = 0
                );

                // --- LOGIC GRACE PERIOD ---
                if (isSlashingNow)
                {
                    _isSlashing = true;
                    // 2. DÙNG UNSCALED TIME THAY VÌ TIME.TIME
                    _lastSlashingTime = Time.unscaledTime; 
                }
                else
                {
                    // 3. DÙNG UNSCALED TIME ĐỂ ĐO THỜI GIAN
                    if (Time.unscaledTime - _lastSlashingTime > GracePeriod)
                    {
                        _isSlashing = false;
                    }
                }
            }
        }
        catch (System.Exception)
        {
            // Bỏ qua nếu gói tin bị lỗi định dạng
        }
    }    

    public Vector2 GetCurrentPosition() => _currentPosition;

    public bool IsSwiping() => _isSlashing;
}