using System;
using UnityEngine;

namespace ADHDTraining.Core
{
    [Serializable]
    public struct BciInputSnapshot
    {
        public float Focus;
        public bool Blink;
        public HeadGesture Head;
        public string RawDebug;

        /// <summary>眼球是否落在屏幕可视范围内（需先完成屏幕校准）。</summary>
        public bool GazeOnScreen;
        /// <summary>归一化屏幕坐标 (0~1)，左下为 (0,0)。</summary>
        public Vector2 GazeScreenPos;
        public bool GazeCalibrated;
        public bool LeftHandRaised;
        public bool RightHandRaised;
        public bool LeftHandRaiseEdge;
        public bool RightHandRaiseEdge;
        public int BodyLean;
        public bool IsLowFocus => Focus < 30f;
        public bool IsHighFocus => Focus > 70f;
        public bool IsMidFocus => Focus >= 40f && Focus <= 70f;
    }

    public enum HeadGesture
    {
        None,
        Nod,
        Shake,
        TurnLeft,
        TurnRight
    }
}
