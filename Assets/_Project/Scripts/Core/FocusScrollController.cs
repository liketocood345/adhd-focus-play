using UnityEngine;

namespace ADHDTraining.Core
{
    /// <summary>
    /// 鼠标滚轮调节专注力（0-100），代偿与模拟模式共用。
    /// </summary>
    public static class FocusScrollController
    {
        public const float MinFocus = 0f;
        public const float MaxFocus = 100f;

        public static float ApplyScroll(float current, float scrollSensitivity = 80f)
        {
            var scroll = BciLegacyInput.MouseScrollWheel;
            if (Mathf.Abs(scroll) < 0.0001f)
                return current;

            return Mathf.Clamp(current + scroll * scrollSensitivity, MinFocus, MaxFocus);
        }
    }
}
