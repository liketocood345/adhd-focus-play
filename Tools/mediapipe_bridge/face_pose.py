"""MediaPipe 面部特征提取（移植自 EasyVtuber / EasyVtuberStudio face_mesh + get_pose）。"""
from __future__ import annotations

import math
from typing import Any, Optional, Tuple

import numpy as np

# Face mesh landmark indices（与 EasyVtuber facial_points.py 一致）
MOUTH_TOP = 13
MOUTH_BOTTOM = 14
MOUTH_RIGHT = 78
MOUTH_LEFT1 = 409
MOUTH_LEFT2 = 375
IRIS_L_TOP = 386
IRIS_L_BOTTOM = 374
IRIS_L_LEFT = 263
IRIS_L_RIGHT = 382
IRIS_R_TOP = 159
IRIS_R_BOTTOM = 145
IRIS_R_LEFT = 155
IRIS_R_RIGHT = 33


def _dist(a, b) -> float:
    return math.sqrt((a.x - b.x) ** 2 + (a.y - b.y) ** 2 + (a.z - b.z) ** 2)


def extract_face_pose(landmarks) -> dict:
    """从 refine FaceMesh 关键点提取 EVTS 同级姿态量。"""
    iris_r = _iris_center(landmarks, "r")
    iris_l = _iris_center(landmarks, "l")

    mouth_h = landmarks[MOUTH_TOP].y - landmarks[MOUTH_BOTTOM].y
    mouth_w = landmarks[MOUTH_RIGHT].x - (landmarks[MOUTH_LEFT1].x + landmarks[MOUTH_LEFT2].x) / 2
    mouth_ratio = mouth_h / max(mouth_w, 1e-6)

    x_angle = math.atan2(landmarks[197].y - landmarks[9].y, landmarks[197].z - landmarks[9].z)
    y_angle = math.atan2(landmarks[IRIS_L_TOP].z - landmarks[IRIS_R_TOP].z,
                          landmarks[IRIS_L_TOP].x - landmarks[IRIS_R_TOP].x)
    z_angle = math.atan2(landmarks[9].y - landmarks[152].y, landmarks[9].x - landmarks[152].x)

    iris_rotation_l_h = _dist(landmarks[IRIS_L_TOP], landmarks[IRIS_L_BOTTOM])
    iris_rotation_l_w = _dist(landmarks[IRIS_L_LEFT], landmarks[IRIS_L_RIGHT])
    iris_rotation_r_h = _dist(landmarks[IRIS_R_TOP], landmarks[IRIS_R_BOTTOM])
    iris_rotation_r_w = _dist(landmarks[IRIS_R_LEFT], landmarks[IRIS_R_RIGHT])

    iris_l_h_temp = math.sqrt((iris_l.x - landmarks[IRIS_L_TOP].x) ** 2 + (iris_l.y - landmarks[IRIS_L_TOP].y) ** 2)
    iris_l_w_temp = math.sqrt((iris_l.x - landmarks[IRIS_L_RIGHT].x) ** 2 + (iris_l.y - landmarks[IRIS_L_RIGHT].y) ** 2)
    iris_r_h_temp = math.sqrt((iris_r.x - landmarks[IRIS_R_TOP].x) ** 2 + (iris_r.y - landmarks[IRIS_R_TOP].y) ** 2)
    iris_r_w_temp = math.sqrt((iris_r.x - landmarks[IRIS_R_RIGHT].x) ** 2 + (iris_r.y - landmarks[IRIS_R_RIGHT].y) ** 2)

    eye_x_ratio = ((iris_l_w_temp / max(iris_rotation_l_w, 1e-6) +
                    iris_r_w_temp / max(iris_rotation_r_w, 1e-6)) - 1) * 3
    eye_y_ratio = ((iris_l_h_temp / max(iris_rotation_l_h, 1e-6) +
                    iris_r_h_temp / max(iris_rotation_r_h, 1e-6)) - 1) * 3

    eye_l_h = 1 - 2 * (landmarks[IRIS_R_BOTTOM].y - landmarks[IRIS_R_TOP].y) / max(
        landmarks[IRIS_R_LEFT].x - landmarks[IRIS_R_RIGHT].x, 1e-6)
    eye_r_h = 1 - 2 * (landmarks[IRIS_L_BOTTOM].y - landmarks[IRIS_L_TOP].y) / max(
        landmarks[IRIS_L_LEFT].x - landmarks[IRIS_L_RIGHT].x, 1e-6)

    return {
        "eye_l": float(eye_l_h),
        "eye_r": float(eye_r_h),
        "mouth_ratio": float(mouth_ratio),
        "eye_y_ratio": float(eye_y_ratio),
        "eye_x_ratio": float(eye_x_ratio),
        "head_pitch": math.degrees(x_angle),
        "head_yaw": math.degrees(y_angle),
        "head_roll": math.degrees(z_angle),
        "gaze_yaw": float(eye_x_ratio * 8.0),
        "gaze_pitch": float(eye_y_ratio * 8.0),
    }


def _iris_center(landmarks, side: str):
    if side.lower() in ("r", "right"):
        idx = [473, 474, 475, 476, 477]
    else:
        idx = [468, 469, 470, 471, 472]
    t = np.zeros(3, dtype=np.float64)
    for i in idx:
        if i >= len(landmarks):
            continue
        t[0] += landmarks[i].x
        t[1] += landmarks[i].y
        t[2] += landmarks[i].z
    t /= max(len(idx), 1)

    class P:
        def __init__(self, x, y, z):
            self.x, self.y, self.z = x, y, z

    return P(t[0], t[1], t[2])
