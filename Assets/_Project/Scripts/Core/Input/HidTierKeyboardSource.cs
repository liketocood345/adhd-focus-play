using UnityEngine;

namespace ADHDTraining.Core.Input
{
    /// <summary>
    /// 磁轴 HID 档位协议占位（F1–F24 行程档位）。见 Docs/AnalogKeyboardIntegration.md。
    /// </summary>
    public class HidTierKeyboardSource : IAnalogKeyboardSource
    {
        public bool IsAvailable => false;
        public string SourceName => "hid_tier";

        public void Poll() { }

        public float ReadKeyTravel(KeyBinding binding) => 0f;

        public bool WasPressedThisFrame(KeyBinding binding) => false;
    }
}
