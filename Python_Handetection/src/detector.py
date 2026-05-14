import cv2
import mediapipe as mp
import math

mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils

class HandTracker:
    def __init__(self, slash_threshold=1.5):
        self.slash_threshold = slash_threshold
        
        # Cấu hình tối ưu tốc độ cho 1 bàn tay
        self.hands = mp_hands.Hands(
            static_image_mode=False,
            max_num_hands=1,
            min_detection_confidence=0.7,
            min_tracking_confidence=0.7
        )
        print("👁️ [AI] Mô hình Slash Detection đã sẵn sàng.")

    def process_frame(self, img):
        img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        results = self.hands.process(img_rgb)

        data_to_send = None

        if results.multi_hand_landmarks:
            for hand_landmarks in results.multi_hand_landmarks:
                # 1. Lấy các điểm mốc sinh học
                wrist = hand_landmarks.landmark[0]          # Cổ tay
                index_mcp = hand_landmarks.landmark[5]      # Khớp ngón trỏ
                index_tip = hand_landmarks.landmark[8]      # Đầu ngón trỏ (Dùng làm tọa độ chém)

                # 2. Tính khoảng cách 2D bằng toán học tối ưu
                dist_wrist_mcp = math.hypot(index_mcp.x - wrist.x, index_mcp.y - wrist.y)
                dist_wrist_tip = math.hypot(index_tip.x - wrist.x, index_tip.y - wrist.y)

                # 3. Tính tỷ lệ (Tránh lỗi chia cho 0)
                ratio = dist_wrist_tip / (dist_wrist_mcp + 1e-6)

                # 4. Xác định trạng thái Xòe (Chém) hay Nắm (Nghỉ)
                is_slashing = 1 if ratio > self.slash_threshold else 0

                # 5. Đóng gói dữ liệu gửi qua UDP
                data_to_send = f"{index_tip.x:.3f}|{index_tip.y:.3f}|{is_slashing}"

                # --- VẼ ĐỒ HỌA TRỰC QUAN (DEBUG) ---
                h, w, _ = img.shape
                pixel_x, pixel_y = int(index_tip.x * w), int(index_tip.y * h)
                
                # Chém = Đỏ, Nghỉ = Xanh lá
                color = (0, 0, 255) if is_slashing else (0, 255, 0)
                
                mp_drawing.draw_landmarks(img, hand_landmarks, mp_hands.HAND_CONNECTIONS)
                cv2.circle(img, (pixel_x, pixel_y), 15, color, cv2.FILLED)
                cv2.putText(img, f"Ratio: {ratio:.2f}", (pixel_x + 20, pixel_y), 
                            cv2.FONT_HERSHEY_SIMPLEX, 0.6, color, 2)

        return img, data_to_send

    def release(self):
        self.hands.close()