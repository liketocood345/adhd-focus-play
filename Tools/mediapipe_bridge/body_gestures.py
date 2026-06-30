"""肩颈与举手检测（MediaPipe Pose + Hands）。"""
from __future__ import annotations

from typing import Any, Dict, Optional

# BlazePose 索引
_NOSE = 0
_L_SHOULDER = 11
_R_SHOULDER = 12
_L_ELBOW = 13
_R_ELBOW = 14
_L_WRIST = 15
_R_WRIST = 16
_L_HIP = 23
_R_HIP = 24


def _lm(landmarks, idx):
    p = landmarks[idx]
    return p.x, p.y, p.z, getattr(p, "visibility", 1.0)


def extract_body(pose_landmarks, pose_world) -> Dict[str, Any]:
    if pose_landmarks is None:
        return {"pose_valid": False}

    ls = _lm(pose_landmarks, _L_SHOULDER)
    rs = _lm(pose_landmarks, _R_SHOULDER)
    lw = _lm(pose_landmarks, _L_WRIST)
    rw = _lm(pose_landmarks, _R_WRIST)
    le = _lm(pose_landmarks, _L_ELBOW)
    re = _lm(pose_landmarks, _R_ELBOW)
    nose = _lm(pose_landmarks, _NOSE)

    neck_x = (ls[0] + rs[0]) * 0.5
    neck_y = (ls[1] + rs[1]) * 0.5
    neck_z = (ls[2] + rs[2]) * 0.5

    left_raised = _hand_raised(lw, le, ls)
    right_raised = _hand_raised(rw, re, rs)

    shoulder_mid_x = neck_x
    body_lean = 0
    if hasattr(extract_body, "_baseline_x"):
        dx = shoulder_mid_x - extract_body._baseline_x
        if dx > 0.04:
            body_lean = 1
        elif dx < -0.04:
            body_lean = -1
    else:
        extract_body._baseline_x = shoulder_mid_x

    # 缓慢更新基线
    extract_body._baseline_x = extract_body._baseline_x * 0.995 + shoulder_mid_x * 0.005

    return {
        "pose_valid": True,
        "neck_x": neck_x,
        "neck_y": neck_y,
        "neck_z": neck_z,
        "ls_x": ls[0], "ls_y": ls[1], "ls_z": ls[2],
        "rs_x": rs[0], "rs_y": rs[1], "rs_z": rs[2],
        "lw_x": lw[0], "lw_y": lw[1], "lw_z": lw[2],
        "rw_x": rw[0], "rw_y": rw[1], "rw_z": rw[2],
        "left_hand_raised": left_raised,
        "right_hand_raised": right_raised,
        "body_lean": body_lean,
        "nose_y": nose[1],
    }


def _hand_raised(wrist, elbow, shoulder, margin: float = 0.06) -> bool:
    wx, wy, wz, wv = wrist
    ex, ey, ez, ev = elbow
    sx, sy, sz, sv = shoulder
    if min(wv, ev, sv) < 0.5:
        return False
    # 图像坐标 y 向下：手腕高于肩 = wy < sy - margin
    if wy >= sy - margin:
        return False
    # 肘腕伸展：手腕高于肘
    return wy < ey - margin * 0.5


extract_body._baseline_x = 0.5
