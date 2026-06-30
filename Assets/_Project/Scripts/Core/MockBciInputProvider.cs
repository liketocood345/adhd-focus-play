using ADHDTraining.Core.BciTransport;
using ADHDTraining.Core.Input;
using UnityEngine;

namespace ADHDTraining.Core
{
    /// <summary>
    /// 无摄像头时的键盘模拟输入，支持压感键盘分层（Wooting/HID/标准键鼠）。
    /// </summary>
    public class MockBciInputProvider : MonoBehaviour, IBciInputProvider
    {
        [SerializeField] private float focus = 75f;
        [SerializeField] private float scrollSensitivity = 80f;

        private IAnalogKeyboardSource _keyboard;
        private BciInputSnapshot _current;
        private float _prevBlinkTravel;
        private bool _prevLeftHand;
        private bool _prevRightHand;

        public BciInputSnapshot Current => _current;
        public bool IsConnected => _keyboard?.IsAvailable ?? true;
        public IAnalogKeyboardSource KeyboardSource => _keyboard;

        private void Awake()
        {
            _keyboard = AnalogKeyboardSourcePicker.CreateBest();
        }

        private void Update()
        {
            _keyboard?.Poll();
            focus = FocusScrollController.ApplyScroll(focus, scrollSensitivity);

            var travelFocus = _keyboard?.ReadKeyTravel(KeyBinding.FocusPrimary) ?? 0f;
            if (travelFocus > 0.01f)
                focus = Mathf.Lerp(focus, travelFocus * 100f, Time.deltaTime * 6f);

            var head = HeadGesture.None;
            if (_keyboard != null)
            {
                if (_keyboard.WasPressedThisFrame(KeyBinding.Nod)) head = HeadGesture.Nod;
                else if (_keyboard.WasPressedThisFrame(KeyBinding.Shake)) head = HeadGesture.Shake;
                else if (_keyboard.WasPressedThisFrame(KeyBinding.TurnLeft)) head = HeadGesture.TurnLeft;
                else if (_keyboard.WasPressedThisFrame(KeyBinding.TurnRight)) head = HeadGesture.TurnRight;
            }

            var blinkTravel = _keyboard?.ReadKeyTravel(KeyBinding.Blink) ?? 0f;
            var blink = _keyboard != null
                ? _keyboard.WasPressedThisFrame(KeyBinding.Blink) || (_prevBlinkTravel < 0.5f && blinkTravel >= 0.85f)
                : BciLegacyInput.GetKeyDown(KeyCode.Space);
            _prevBlinkTravel = blinkTravel;

            var leftHand = BciLegacyInput.GetKey(KeyCode.Q);
            var rightHand = BciLegacyInput.GetKey(KeyCode.E);

            _current = new BciInputSnapshot
            {
                Focus = focus,
                Blink = blink,
                Head = head,
                LeftHandRaised = leftHand,
                RightHandRaised = rightHand,
                LeftHandRaiseEdge = leftHand && !_prevLeftHand,
                RightHandRaiseEdge = rightHand && !_prevRightHand,
                RawDebug = _keyboard?.SourceName ?? "mock"
            };
            _prevLeftHand = leftHand;
            _prevRightHand = rightHand;
        }
    }
}
