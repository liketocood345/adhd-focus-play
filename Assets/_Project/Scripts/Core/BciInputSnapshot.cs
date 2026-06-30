using System;

namespace ADHDTraining.Core
{
    /// <summary>
    /// HybridBCI 平台输出的实时信号快照。
    /// </summary>
    [Serializable]
    public struct BciInputSnapshot
    {
        public float Focus;          // 0-100
        public bool Blink;
        public HeadGesture Head;

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
