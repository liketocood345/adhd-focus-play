using UnityEngine;

namespace ADHDTraining.Core
{
    /// <summary>
    /// HybridBCI 正式设备占位。接入 SDK 后在此读取蓝牙数据。
    /// </summary>
    public class HybridBciInputProvider : MonoBehaviour, IBciInputProvider
    {
        [SerializeField] private float focus = 50f;
        [SerializeField] private float scrollSensitivity = 80f;
        [SerializeField] private bool allowScrollFallback = true;

        private BciInputSnapshot _current;

        public BciInputSnapshot Current => _current;
        public bool IsConnected => false;

        private void Update()
        {
            if (allowScrollFallback)
                focus = FocusScrollController.ApplyScroll(focus, scrollSensitivity);

            // TODO: 接入 HybridBCI SDK — 读取专注力、眨眼、头动
            _current = new BciInputSnapshot
            {
                Focus = focus,
                Blink = false,
                Head = HeadGesture.None
            };
        }
    }
}
