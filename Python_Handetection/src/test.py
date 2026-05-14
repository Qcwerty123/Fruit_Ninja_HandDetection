"""
Hand Detection Server - Ninja Fruit Unity
==========================================
Gesture mapping:
  - Ngón trỏ di chuyển  →  cursor
  - Chụm trỏ + cái      →  PINCH (click / giữ button)
  - Pinch + kéo nhanh   →  SLASH (chém quả)
  - Nhả ngón            →  RELEASE

Yêu cầu:
    pip install mediapipe==0.10.9 opencv-python numpy

Chạy:
    python hand_detection_server.py
"""

import cv2
import mediapipe as mp
import socket
import json
import numpy as np
import time
import sys
from collections import deque
from dataclasses import dataclass, asdict
from enum import Enum

# ─────────────────────────────────────────────
# CONFIG
# ─────────────────────────────────────────────
UNITY_IP   = "127.0.0.1"
UNITY_PORT = 5052

CAMERA_INDEX  = 0
FRAME_WIDTH   = 640
FRAME_HEIGHT  = 480
TARGET_FPS    = 60

# Pinch
PINCH_CLOSE_DIST  = 40.0   # px – khoảng cách trỏ+cái để tính là đang pinch
PINCH_OPEN_DIST   = 55.0   # px – hysteresis: phải mở rộng hơn để release
PINCH_HOLD_FRAMES = 2      # số frame liên tiếp để confirm pinch (chống jitter)

# Slash (chỉ kích hoạt khi đang pinch)
SLASH_SPEED_THRESHOLD = 38.0  # px/frame
SLASH_HISTORY_LEN     = 6     # sliding window

# Cursor smoothing (exponential moving average)
CURSOR_SMOOTH_ALPHA = 0.7    # 0=cứng, 1=không smooth

# Debug
SHOW_DEBUG    = True
TRAIL_LEN     = 24


# ─────────────────────────────────────────────
# GESTURE STATES
# ─────────────────────────────────────────────
class GestureState(str, Enum):
    IDLE    = "idle"
    HOVER   = "hover"
    PINCH   = "pinch"
    SLASH   = "slash"


# ─────────────────────────────────────────────
# PAYLOAD
# ─────────────────────────────────────────────
@dataclass
class HandPayload:
    state:          str   = GestureState.IDLE
    cursor_x:       float = 0.0
    cursor_y:       float = 0.0
    is_pinching:    bool  = False
    pinch_dist:     float = 0.0
    is_slashing:    bool  = False
    slash_speed:    float = 0.0
    slash_dir_x:    float = 0.0
    slash_dir_y:    float = 0.0
    wrist_x:        float = 0.0
    wrist_y:        float = 0.0
    thumb_x:        float = 0.0
    thumb_y:        float = 0.0
    fps:            float = 0.0
    timestamp:      float = 0.0


def to_json(payload: HandPayload) -> bytes:
    safe = {}
    for k, v in asdict(payload).items():
        if isinstance(v, (bool, np.bool_)):
            safe[k] = bool(v)
        elif isinstance(v, (int, float, np.integer, np.floating)):
            safe[k] = float(v)
        else:
            safe[k] = v
    return json.dumps(safe, separators=(',', ':')).encode('utf-8')


# ─────────────────────────────────────────────
# GESTURE ANALYZER
# ─────────────────────────────────────────────
class GestureAnalyzer:
    def __init__(self):
        self.state = GestureState.IDLE
        self._smooth_x = 0.5
        self._smooth_y = 0.5
        self._pinch_confirm = 0
        self._is_pinching   = False
        self._tip_history: deque[tuple[float, float]] = deque(maxlen=SLASH_HISTORY_LEN)

    def update(self, lm) -> HandPayload:
        ix, iy = 1.0 - float(lm[8].x),  float(lm[8].y)
        tx, ty = 1.0 - float(lm[4].x),  float(lm[4].y)
        wx, wy = float(lm[0].x),  float(lm[0].y)

        # Cursor smooth
        self._smooth_x += CURSOR_SMOOTH_ALPHA * (ix - self._smooth_x)
        self._smooth_y += CURSOR_SMOOTH_ALPHA * (iy - self._smooth_y)

        # Pinch distance
        pinch_dist = float(np.hypot(
            (ix - tx) * FRAME_WIDTH,
            (iy - ty) * FRAME_HEIGHT
        ))

        # Pinch với hysteresis
        if pinch_dist < PINCH_CLOSE_DIST:
            self._pinch_confirm = min(self._pinch_confirm + 1, PINCH_HOLD_FRAMES + 1)
        elif pinch_dist > PINCH_OPEN_DIST:
            self._pinch_confirm = 0
        self._is_pinching = self._pinch_confirm >= PINCH_HOLD_FRAMES

        # Slash velocity (chỉ khi pinch)
        is_slashing = False
        speed = dir_x = dir_y = 0.0

        if self._is_pinching:
            self._tip_history.append((ix * FRAME_WIDTH, iy * FRAME_HEIGHT))
            if len(self._tip_history) >= 3:
                dx = float(self._tip_history[-1][0] - self._tip_history[0][0])
                dy = float(self._tip_history[-1][1] - self._tip_history[0][1])
                speed = float(np.hypot(dx, dy))
                if speed > SLASH_SPEED_THRESHOLD:
                    is_slashing = True
                    dir_x = float(dx / speed)
                    dir_y = float(dy / speed)
        else:
            self._tip_history.clear()

        # State machine
        if is_slashing:
            self.state = GestureState.SLASH
        elif self._is_pinching:
            self.state = GestureState.PINCH
        else:
            self.state = GestureState.HOVER

        return HandPayload(
            state       = str(self.state),
            cursor_x    = float(self._smooth_x),
            cursor_y    = float(self._smooth_y),
            is_pinching = bool(self._is_pinching),
            pinch_dist  = float(pinch_dist),
            is_slashing = bool(is_slashing),
            slash_speed = float(speed),
            slash_dir_x = float(dir_x),
            slash_dir_y = float(dir_y),
            wrist_x     = float(wx),
            wrist_y     = float(wy),
            thumb_x     = float(tx),
            thumb_y     = float(ty),
        )

    def reset(self) -> HandPayload:
        self.state          = GestureState.IDLE
        self._pinch_confirm = 0
        self._is_pinching   = False
        self._tip_history.clear()
        return HandPayload(state=str(GestureState.IDLE))


# ─────────────────────────────────────────────
# DEBUG OVERLAY
# ─────────────────────────────────────────────
class DebugOverlay:
    STATE_COLOR = {
        GestureState.IDLE:  (120, 120, 120),
        GestureState.HOVER: (50,  220, 255),
        GestureState.PINCH: (50,  200, 50),
        GestureState.SLASH: (50,  50,  255),
    }

    def __init__(self):
        self._trail      = deque(maxlen=TRAIL_LEN)
        self._mp_drawing = mp.solutions.drawing_utils
        self._mp_styles  = mp.solutions.drawing_styles
        self._mp_hands   = mp.solutions.hands

    def draw(self, frame: np.ndarray, payload: HandPayload, mp_results) -> np.ndarray:
        h, w  = frame.shape[:2]
        state = GestureState(payload.state)
        color = self.STATE_COLOR.get(state, (180, 180, 180))

        # Skeleton tay
        if mp_results.multi_hand_landmarks:
            for hand_lm in mp_results.multi_hand_landmarks:
                self._mp_drawing.draw_landmarks(
                    frame, hand_lm,
                    self._mp_hands.HAND_CONNECTIONS,
                    self._mp_styles.get_default_hand_landmarks_style(),
                    self._mp_styles.get_default_hand_connections_style()
                )

        if state == GestureState.IDLE:
            self._trail.clear()
            cv2.putText(frame, "IDLE - Dua tay vao camera",
                        (20, 40), cv2.FONT_HERSHEY_SIMPLEX, 0.65, color, 2)
            return frame

        cx = int((1.0 - payload.cursor_x) * w)
        cy = int(payload.cursor_y * h)

        # Trail
        self._trail.append((cx, cy))
        for i in range(1, len(self._trail)):
            a = i / len(self._trail)
            c = tuple(int(v * a) for v in color)
            cv2.line(frame, self._trail[i-1], self._trail[i], c, max(1, int(a * 5)))

        # Cursor
        radius = 7 if state == GestureState.PINCH else 12
        cv2.circle(frame, (cx, cy), radius, color, -1)
        cv2.circle(frame, (cx, cy), radius + 3, color, 1)

        # Thumb
        tx = int((1.0 - payload.thumb_x) * w)
        ty = int(payload.thumb_y * h)
        cv2.circle(frame, (tx, ty), 8, (200, 200, 200), 2)

        # Đường nối khi gần pinch
        if payload.pinch_dist < PINCH_OPEN_DIST * 1.2:
            pct    = max(0.0, 1.0 - payload.pinch_dist / PINCH_OPEN_DIST)
            lcolor = (int(50 * pct), int(220 * pct), int(50 * pct))
            cv2.line(frame, (cx, cy), (tx, ty), lcolor, 2)

        # Slash arrow
        if state == GestureState.SLASH:
            ex = cx + int(payload.slash_dir_x * 60)
            ey = cy + int(payload.slash_dir_y * 60)
            cv2.arrowedLine(frame, (cx, cy), (ex, ey), color, 3, tipLength=0.35)

        # HUD
        lines = [
            f"State : {state.value.upper()}",
            f"Cursor: ({payload.cursor_x:.2f}, {payload.cursor_y:.2f})",
            f"Pinch : {payload.pinch_dist:.1f}px  {'[ON]' if payload.is_pinching else ''}",
            f"Speed : {payload.slash_speed:.1f}   {'[SLASH!]' if payload.is_slashing else ''}",
            f"FPS   : {payload.fps:.0f}",
        ]
        for i, txt in enumerate(lines):
            cv2.putText(frame, txt, (12, 30 + i * 22),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.55, color, 1)

        return frame


class GestureState(str, Enum):
    IDLE    = "idle"
    HOVER   = "hover"
    PINCH   = "pinch"
    SLASH   = "slash"

    def __str__(self):
        return self.value   

# ─────────────────────────────────────────────
# MAIN
# ─────────────────────────────────────────────
def main():
    mp_hands_module = mp.solutions.hands
    hands = mp_hands_module.Hands(
        static_image_mode=False,
        max_num_hands=1,
        model_complexity=0,
        min_detection_confidence=0.6,
        min_tracking_confidence=0.5
    )

    cap = cv2.VideoCapture(CAMERA_INDEX)
    if not cap.isOpened():
        print(f"[ERROR] Khong mo duoc camera index={CAMERA_INDEX}")
        sys.exit(1)
    cap.set(cv2.CAP_PROP_FRAME_WIDTH,  FRAME_WIDTH)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, FRAME_HEIGHT)
    cap.set(cv2.CAP_PROP_FPS,          TARGET_FPS)
    cap.set(cv2.CAP_PROP_BUFFERSIZE,   1)

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    print(f"[INFO] Gui UDP -> {UNITY_IP}:{UNITY_PORT}")
    print("[INFO] Phim: Q=thoat | D=bat/tat debug")

    gesture    = GestureAnalyzer()
    overlay    = DebugOverlay()
    show_debug = SHOW_DEBUG
    fps_hist   = deque(maxlen=30)
    prev_time  = time.perf_counter()

    try:
        while True:
            ret, frame = cap.read()
            if not ret:
                continue

            now = time.perf_counter()
            fps_hist.append(1.0 / max(now - prev_time, 1e-9))
            prev_time = now

            frame = cv2.flip(frame, 1)
            rgb   = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            rgb.flags.writeable = False
            results = hands.process(rgb)
            rgb.flags.writeable = True

            if results.multi_hand_landmarks:
                payload = gesture.update(results.multi_hand_landmarks[0].landmark)
            else:
                payload = gesture.reset()

            payload.fps       = round(float(np.mean(fps_hist)), 1)
            payload.timestamp = float(now)

            # Trong vòng lặp while, sau khi có payload
            if payload.state != GestureState.IDLE:
                print(f"cursor=({payload.cursor_x:.3f}, {payload.cursor_y:.3f})  state={payload.state}")

            try:
                sock.sendto(to_json(payload), (UNITY_IP, UNITY_PORT))
            except Exception as e:
                print(f"[WARN] UDP: {e}")

            if show_debug:
                frame = overlay.draw(frame, payload, results)
                cv2.imshow("Hand Detection - Ninja Fruit", frame)

            key = cv2.waitKey(1) & 0xFF
            if key == ord('q'):
                break
            elif key == ord('d'):
                show_debug = not show_debug
                if not show_debug:
                    cv2.destroyAllWindows()

    except KeyboardInterrupt:
        print("\n[INFO] Dung server.")
    finally:
        hands.close()
        cap.release()
        sock.close()
        cv2.destroyAllWindows()
        print("[INFO] Cleanup xong.")


if __name__ == "__main__":
    main()