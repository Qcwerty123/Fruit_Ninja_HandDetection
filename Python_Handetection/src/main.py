import cv2
from network import UDPSender
from detector import HandPinchDetector

def main():
    print("🚀 BẮT ĐẦU KHỞI ĐỘNG HỆ THỐNG AI SENSOR...")
    
    # Khởi tạo các Service (Giống khái niệm DI bên Unity)
    cap = cv2.VideoCapture(0)
    detector = HandPinchDetector(pinch_threshold=0.05)
    sender = UDPSender(ip="127.0.0.1", port=5005)

    print("✅ Hệ thống sẵn sàng! Nhấn 'q' trên cửa sổ Camera để thoát.")

    while True:
        success, img = cap.read()
        if not success:
            print("❌ Lỗi: Không thể đọc tín hiệu từ Webcam!")
            break

        # Lật ngược ảnh để soi gương
        img = cv2.flip(img, 1)

        # Xử lý hình ảnh qua AI
        img, data_string = detector.process_frame(img)

        # Nếu AI trả về dữ liệu (có bàn tay), gửi qua mạng
        if data_string:
            sender.send_data(data_string)
            # In lên màn hình để dễ Debug
            cv2.putText(img, f"Sending: {data_string}", (10, 50), 
                        cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 0), 2)

        # Hiển thị
        cv2.imshow("Fruit Ninja - AI Hand Tracker", img)

        # Kiểm tra lệnh thoát (Phím Q)
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

    # Dọn dẹp sạch sẽ khi kết thúc (Graceful Shutdown)
    print("🧹 Đang dọn dẹp hệ thống...")
    cap.release()
    cv2.destroyAllWindows()
    detector.release()
    sender.close()
    print("👋 Tạm biệt!")

# Chỉ chạy hàm main nếu file này được gọi trực tiếp
if __name__ == "__main__":
    main()