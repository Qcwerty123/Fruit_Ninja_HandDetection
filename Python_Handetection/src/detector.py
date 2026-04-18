import cv2
import mediapipe as mp
import math

class HandPinchDetector:
    def __init__(self, pinch_threshold=0.05):
        """Khởi tạo mô hình AI nhận diện tay"""
        self.pinch_threshold = pinch_threshold
        self.mp_hands = mp.solutions.hands
        # Cấu hình tối ưu: Chỉ 1 tay, độ tự tin 70% để chống nhiễu
        self.hands = self.mp_hands.Hands(
            static_image_mode=False,
            max_num_hands=1,
            min_detection_confidence=0.7,
            min_tracking_confidence=0.7
        )
        self.mp_draw = mp.solutions.drawing_utils
        print("👁️ [AI] Đã tải mô hình MediaPipe Hands thành công.")

    def process_frame(self, img):
        """Phân tích khung hình, trả về (Ảnh đã vẽ, Chuỗi dữ liệu)"""
        # 1. Chuyển đổi hệ màu cho MediaPipe
        img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        results = self.hands.process(img_rgb)

        data_to_send = None

        # 2. Nếu thấy bàn tay
        if results.multi_hand_landmarks:
            for hand_landmarks in results.multi_hand_landmarks:
                # Lấy ngón cái (4) và ngón trỏ (8)
                thumb = hand_landmarks.landmark[4]
                index = hand_landmarks.landmark[8]

                # Tính khoảng cách và trạng thái Pinch
                distance = math.sqrt((index.x - thumb.x)**2 + (index.y - thumb.y)**2)
                is_pinching = 1 if distance < self.pinch_threshold else 0

                # Lấy trung điểm
                center_x = (thumb.x + index.x) / 2
                center_y = (thumb.y + index.y) / 2

                # Đóng gói dữ liệu chuẩn
                data_to_send = f"{center_x:.3f}|{center_y:.3f}|{is_pinching}"

                # --- Vẽ đồ họa UI trực quan ---
                h, w, c = img.shape
                pixel_x, pixel_y = int(center_x * w), int(center_y * h)
                color = (0, 0, 255) if is_pinching else (0, 255, 0) # Đỏ nếu chụm, Xanh nếu mở
                
                # Vẽ khung xương và chấm tròn
                self.mp_draw.draw_landmarks(img, hand_landmarks, self.mp_hands.HAND_CONNECTIONS)
                cv2.circle(img, (pixel_x, pixel_y), 15, color, cv2.FILLED)

        return img, data_to_send

    def release(self):
        """Giải phóng RAM của mô hình AI"""
        self.hands.close()