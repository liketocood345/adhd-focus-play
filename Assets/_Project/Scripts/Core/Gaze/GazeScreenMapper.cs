using System;
using UnityEngine;

namespace ADHDTraining.Core.Gaze
{
    /// <summary>
    /// 由左右校准点（屏幕 25%/75% 水平中心）推算屏幕中心与可视范围。
    /// </summary>
    [Serializable]
    public class GazeScreenMapper
    {
        private const float LeftScreenX = 0.25f;
        private const float RightScreenX = 0.75f;
        private const float CenterScreenY = 0.5f;

        public bool IsCalibrated { get; private set; }
        public Vector2 GazeCenter { get; private set; }
        public Vector2 GazeSpan { get; private set; } = Vector2.one;

        public void ApplyCalibration(GazeSample leftTarget, GazeSample rightTarget)
        {
            if (!leftTarget.Valid || !rightTarget.Valid)
            {
                IsCalibrated = false;
                return;
            }

            var gL = leftTarget.ToVector2();
            var gR = rightTarget.ToVector2();
            GazeCenter = (gL + gR) * 0.5f;

            var spanX = Mathf.Abs(gR.x - gL.x);
            if (spanX < 0.01f)
                spanX = 0.01f;

            // 0.25~0.75 屏宽对应 spanX 的 gaze 差值
            GazeSpan = new Vector2(spanX, spanX);
            IsCalibrated = true;
        }

        public bool TryMapToScreen(GazeSample gaze, out Vector2 screenPos, out bool onScreen)
        {
            screenPos = new Vector2(0.5f, CenterScreenY);
            onScreen = false;
            if (!IsCalibrated || !gaze.Valid) return false;

            var g = gaze.ToVector2();
            var nx = 0.5f + (g.x - GazeCenter.x) / GazeSpan.x;
            var ny = CenterScreenY + (g.y - GazeCenter.y) / GazeSpan.y;
            screenPos = new Vector2(nx, ny);
            onScreen = nx >= 0f && nx <= 1f && ny >= 0f && ny <= 1f;
            return true;
        }

        public void Load(Vector2 center, Vector2 span, bool calibrated)
        {
            GazeCenter = center;
            GazeSpan = span.sqrMagnitude < 1e-6f ? Vector2.one : span;
            IsCalibrated = calibrated;
        }
    }
}
