using UnityEngine;

namespace ADHDTraining.Core.Input
{
    /// <summary>
    /// Wooting Analog SDK 占位：检测不到原生库时 IsAvailable=false，由上层回退标准键鼠。
    /// 接入时在此 P/Invoke wooting_analog_read_full_buffer。
    /// </summary>
    public class WootingAnalogKeyboardSource : IAnalogKeyboardSource
    {
        public bool IsAvailable => false;
        public string SourceName => "wooting_analog";

        public void Poll() { }

        public float ReadKeyTravel(KeyBinding binding) => 0f;

        public bool WasPressedThisFrame(KeyBinding binding) => false;
    }
}
