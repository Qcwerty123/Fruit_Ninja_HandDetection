import cv2
import time
from network import UDPSender
from detector import HandTracker

def main():
    print("🚀 BẮT ĐẦU KHỞI ĐỘNG HỆ THỐNG AI SENSOR...")
    
    cap = cv2.VideoCapture(0)
    # Tùy chỉnh độ phân giải camera thấp xuống để tăng FPS nếu máy yếu (VD: 640x480)
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

    detector = HandTracker(slash_threshold=1.5)
    sender = UDPSender(ip="127.0.0.1", port=5005)

    pTime = 0 # Thời gian frame trước

    print("✅ Hệ thống sẵn sàng! Nhấn 'q' trên cửa sổ Camera để thoát.")

    while True:
        success, img = cap.read()
        if not success:
            print("❌ Lỗi: Không thể đọc tín hiệu từ Webcam!")
            break

        img = cv2.flip(img, 1)

        # Đoạn code cốt lõi
        img, data_string = detector.process_frame(img)

        if data_string:
            sender.send_data(data_string)
            cv2.putText(img, f"Data: {data_string}", (10, 80), 
                        cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 0), 2)

        # Tính toán và hiển thị FPS
        cTime = time.time()
        fps = 1 / (cTime - pTime) if (cTime - pTime) > 0 else 0
        pTime = cTime
        cv2.putText(img, f"FPS: {int(fps)}", (10, 40), 
                    cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 255), 2)

        cv2.imshow("AI Hand Tracker", img)

        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

    print("🧹 Đang dọn dẹp hệ thống...")
    cap.release()
    cv2.destroyAllWindows()
    detector.release()
    sender.close()
    print("👋 Tạm biệt!")

if __name__ == "__main__":
    main()