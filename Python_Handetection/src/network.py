import socket

class UDPSender:
    def __init__(self, ip="127.0.0.1", port=5005):
        """Khởi tạo trạm phát sóng UDP"""
        self.ip = ip
        self.port = port
        self.address = (self.ip, self.port)
        # Sử dụng IPv4 (AF_INET) và giao thức UDP (SOCK_DGRAM)
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        print(f"🌐 [NETWORK] Đã mở cổng UDP tại {self.ip}:{self.port}")

    def send_data(self, data_string):
        """Gửi chuỗi dữ liệu qua mạng"""
        try:
            # Phải encode chuỗi thành byte trước khi gửi
            self.sock.sendto(data_string.encode('utf-8'), self.address)
        except Exception as e:
            print(f"⚠️ [NETWORK ERROR] Lỗi gửi dữ liệu: {e}")

    def close(self):
        """Đóng kết nối an toàn"""
        self.sock.close()
        print("🌐 [NETWORK] Đã đóng cổng UDP.")