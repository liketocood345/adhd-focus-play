#!/usr/bin/env python3
"""
MediaPipe 动作追踪桥接 → Unity UDP JSON（代偿模式）。

- FaceMesh refine_landmarks（EasyVtuberStudio 同级面部）
- Pose 肩颈 + 举手
- USB 摄像头（Windows DirectShow）
"""
from __future__ import annotations

import argparse
import json
import socket
import sys
import time
from typing import List, Optional

import cv2
import mediapipe as mp

from body_gestures import extract_body
from face_pose import extract_face_pose

DEFAULT_PORT = 9878


def list_cameras(max_probe: int = 10) -> List[str]:
    names = []
    for i in range(max_probe):
        cap = cv2.VideoCapture(i, cv2.CAP_DSHOW)
        if not cap.isOpened():
            cap.release()
            time.sleep(0.05)
            continue
        names.append(f"Camera {i}")
        cap.release()
        time.sleep(0.12)
    return names


def open_capture(camera_index: int, video_path: Optional[str], width: int, height: int, fps: int):
    if video_path:
        cap = cv2.VideoCapture(video_path)
        if cap.isOpened():
            return cap
        cap.release()
        return cap

    last_err = None
    for attempt in range(4):
        cap = cv2.VideoCapture(camera_index, cv2.CAP_DSHOW)
        if not cap.isOpened():
            cap.release()
            last_err = f"open failed attempt {attempt + 1}"
            time.sleep(0.35 * (attempt + 1))
            continue
        cap.set(cv2.CAP_PROP_FRAME_WIDTH, width)
        cap.set(cv2.CAP_PROP_FRAME_HEIGHT, height)
        cap.set(cv2.CAP_PROP_FPS, fps)
        warmed = 0
        for _ in range(8):
            ok, _ = cap.read()
            if ok:
                warmed += 1
        if warmed >= 2:
            return cap
        cap.release()
        last_err = f"warmup failed attempt {attempt + 1}"
        time.sleep(0.35 * (attempt + 1))
    print(f"ERROR: cannot open camera {camera_index}: {last_err}", file=sys.stderr)
    return cv2.VideoCapture()


def draw_overlay(frame, face_lm, pose_lm, packet: dict):
    h, w = frame.shape[:2]
    mp_draw = mp.solutions.drawing_utils
    mp_styles = mp.solutions.drawing_styles
    if face_lm is not None:
        mp_draw.draw_landmarks(
            frame, face_lm, mp.solutions.face_mesh.FACEMESH_CONTOURS,
            None, mp_styles.get_default_face_mesh_contours_style())
    if pose_lm is not None:
        mp_draw.draw_landmarks(
            frame, pose_lm, mp.solutions.pose.POSE_CONNECTIONS,
            mp_styles.get_default_pose_landmarks_style())
    cv2.putText(frame, f"MP {packet.get('fps', 0):.0f}fps", (8, 22),
                cv2.FONT_HERSHEY_SIMPLEX, 0.55, (0, 255, 128), 1)
    if packet.get("left_hand_raised"):
        cv2.putText(frame, "L HAND UP", (8, 44), cv2.FONT_HERSHEY_SIMPLEX, 0.55, (255, 200, 0), 2)
    if packet.get("right_hand_raised"):
        cv2.putText(frame, "R HAND UP", (8, 66), cv2.FONT_HERSHEY_SIMPLEX, 0.55, (255, 200, 0), 2)
    if packet.get("face_valid"):
        el = packet.get("eye_l", 0)
        er = packet.get("eye_r", 0)
        cv2.putText(frame, f"eye {el:.2f}/{er:.2f}", (8, 88),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, (200, 220, 255), 1)


def run(args):
    if args.list_cameras:
        for i, name in enumerate(list_cameras()):
            print(f"{i}: {name}")
        return 0

    cap = open_capture(args.camera, args.video or None, args.width, args.height, args.fps)
    if not cap.isOpened():
        print("ERROR: cannot open video source", file=sys.stderr)
        return 1

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    target = (args.host, args.port)

    face_mesh = mp.solutions.face_mesh.FaceMesh(
        static_image_mode=False,
        refine_landmarks=True,
        max_num_faces=1,
        min_detection_confidence=0.5,
        min_tracking_confidence=0.5,
    )
    pose = mp.solutions.pose.Pose(
        static_image_mode=False,
        model_complexity=args.pose_complexity,
        smooth_landmarks=True,
        min_detection_confidence=0.5,
        min_tracking_confidence=0.5,
    )

    seq = 0
    t_fps = time.perf_counter()
    frames = 0
    inst_fps = 0.0

    while True:
        ok, frame = cap.read()
        if not ok:
            if args.video:
                cap.set(cv2.CAP_PROP_POS_FRAMES, 0)
                continue
            break

        frames += 1
        now = time.perf_counter()
        if now - t_fps >= 1.0:
            inst_fps = frames / (now - t_fps)
            frames = 0
            t_fps = now

        rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        face_res = face_mesh.process(rgb)
        pose_res = pose.process(rgb)

        packet = {
            "seq": seq,
            "face_valid": False,
            "pose_valid": False,
            "fps": inst_fps,
        }
        seq += 1

        face_lm = None
        pose_lm = None

        if face_res.multi_face_landmarks:
            face_lm = face_res.multi_face_landmarks[0]
            fp = extract_face_pose(face_lm.landmark)
            packet.update(fp)
            packet["face_valid"] = True

        if pose_res.pose_landmarks:
            pose_lm = pose_res.pose_landmarks
            body = extract_body(pose_lm.landmark, pose_res.pose_world_landmarks)
            packet.update(body)

        sock.sendto(json.dumps(packet, separators=(",", ":")).encode("utf-8"), target)

        if args.preview:
            draw_overlay(frame, face_lm, pose_lm, packet)
            cv2.imshow("MediaPipe Tracker", cv2.flip(frame, 1))
            if cv2.waitKey(1) & 0xFF == ord("q"):
                break

    cap.release()
    face_mesh.close()
    pose.close()
    cv2.destroyAllWindows()
    return 0


def main():
    p = argparse.ArgumentParser(description="MediaPipe → Unity UDP bridge")
    p.add_argument("--host", default="127.0.0.1")
    p.add_argument("--port", type=int, default=DEFAULT_PORT)
    p.add_argument("--camera", type=int, default=0)
    p.add_argument("--video", default="")
    p.add_argument("--width", type=int, default=640)
    p.add_argument("--height", type=int, default=360)
    p.add_argument("--fps", type=int, default=24)
    p.add_argument("--preview", type=int, default=1)
    p.add_argument("--pose-complexity", type=int, default=1, choices=[0, 1, 2])
    p.add_argument("--list-cameras", action="store_true")
    args = p.parse_args()
    sys.exit(run(args))


if __name__ == "__main__":
    main()
