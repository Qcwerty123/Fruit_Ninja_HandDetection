using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public static class GameJuice
{
    private static CancellationTokenSource _shakeCts;
    private static bool _isHitStopping = false;

    // ================== NGƯNG ĐỌNG THỜI GIAN ==================

    public static async UniTaskVoid HitStop(float duration = 0.05f)
    {
        if (_isHitStopping) return; 

        _isHitStopping = true;
        float originalScale = Time.timeScale;
        Time.timeScale = 0f; 

        try
        {
            await UniTask.WaitForSeconds(duration, ignoreTimeScale: true);
        }
        finally
        {
            Time.timeScale = originalScale;
            _isHitStopping = false;
        }
    }

    // ================== CAMERA SHAKE (PERLIN NOISE) ==================

    public static async UniTaskVoid ShakeCamera(Transform camTransform, float intensity, float duration, float frequency = 50f)
    {
        _shakeCts?.Cancel();
        _shakeCts?.Dispose();
        _shakeCts = new CancellationTokenSource();
        CancellationToken ct = _shakeCts.Token;

        float zPos = camTransform.localPosition.z;
        float elapsed = 0f;
        
        // Random một điểm bắt đầu (Seed) để mỗi lần chém rung một kiểu khác nhau
        float seedX = Random.Range(0f, 100f);
        float seedY = Random.Range(0f, 100f);

        try
        {
            while (elapsed < duration)
            {
                // Sử dụng Perlin Noise để rung mượt mà
                // Noise trả về giá trị từ 0 đến 1. Trừ đi 0.5 để nó dao động từ -0.5 đến 0.5
                float x = (Mathf.PerlinNoise(seedX + elapsed * frequency, 0f) - 0.5f) * 2f * intensity;
                float y = (Mathf.PerlinNoise(0f, seedY + elapsed * frequency) - 0.5f) * 2f * intensity;
                
                // Mẹo xịn: Làm cường độ rung giảm dần về 0 khi sắp hết thời gian (Fade out)
                float dampening = 1f - (elapsed / duration);
                
                camTransform.localPosition = new Vector3(x * dampening, y * dampening, zPos);
                
                elapsed += Time.unscaledDeltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        }
        catch (System.OperationCanceledException) { }
        finally
        {
            if (camTransform != null)
            {
                camTransform.localPosition = new Vector3(0, 0, zPos);
            }
        }
    }

    // ================== HAPTIC FEEDBACK (RUNG THIẾT BỊ) ==================

    /// <summary>
    /// Rung điện thoại. (Lưu ý: Unity mặc định chỉ có 1 kiểu rung Handheld.Vibrate. 
    /// Nếu muốn rung nhẹ/mạnh trên iOS/Android, cần cài thêm package Haptics sau).
    /// </summary>
    public static void Vibrate()
    {
        // Chỉ rung nếu người chơi không tắt trong Cài đặt (Ví dụ bạn có biến PlayerPrefs)
        if (PlayerPrefs.GetInt("Settings_Haptics", 1) == 1)
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}