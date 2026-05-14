using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPReceiverService : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private int port = 5005;

    private UdpClient _udpClient;
    private Thread _receiveThread;
    private string _lastReceivedPacket = "";
    private bool _isAppRunning = true;

    public string LastData => _lastReceivedPacket;

    private void Start()
    {
        _isAppRunning = true;
        _receiveThread = new Thread(ReceiveData);
        _receiveThread.IsBackground = true; // Luồng tự hủy khi tắt game
        _receiveThread.Start();
        
        Debug.Log($"<color=cyan>[UDP] Trạm thu sóng đã mở tại cổng {port}</color>");
    }

    private void ReceiveData()
    {
        try
        {
            // ÉP LẮNG NGHE ĐÍCH DANH LOCALHOST (Tránh xung đột card mạng ảo)
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            _udpClient = new UdpClient(localEndPoint);
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (_isAppRunning)
            {
                try
                {
                    byte[] dataBytes = _udpClient.Receive(ref remoteEndPoint);
                    _lastReceivedPacket = Encoding.UTF8.GetString(dataBytes);
                }
                catch (SocketException)
                {
                    // Socket bị đóng khi tắt game, bỏ qua lỗi này
                }
            }
        }
        catch (Exception)
        {
            // Bắt mọi lỗi để tránh Crash Thread ngầm
        }
    }

    // CHẨN ĐOÁN Ở MAIN THREAD (An toàn 100%)
    private void Update()
    {
        // Khi nào bạn muốn test xem có nhận mạng không thì BỎ COMMENT dòng dưới đây:
        if (!string.IsNullOrEmpty(_lastReceivedPacket)) Debug.Log("<color=green>NHẬN ĐƯỢC:</color> " + _lastReceivedPacket);
    }

    private void OnDisable() => StopServer();
    private void OnApplicationQuit() => StopServer();

    private void StopServer()
    {
        _isAppRunning = false;
        if (_udpClient != null) _udpClient.Close();
        if (_receiveThread != null && _receiveThread.IsAlive) _receiveThread.Abort();
    }
}