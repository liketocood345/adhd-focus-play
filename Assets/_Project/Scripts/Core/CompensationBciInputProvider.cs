using OpenSee;
using UnityEngine;

namespace ADHDTraining.Core
{
    /// <summary>
    /// USB 摄像头 + OpenSeeFace 代偿：眨眼与头动来自面部追踪，专注力由滚轮手动设置。
    /// </summary>
    public class CompensationBciInputProvider : MonoBehaviour, IBciInputProvider
    {
        [Header("OpenSeeFace")]
        [SerializeField] private OpenSee openSee;
        [SerializeField] private OpenSeeLauncher launcher;

        [Header("Focus (manual via scroll wheel)")]
        [SerializeField] private float focus = 75f;
        [SerializeField] private float scrollSensitivity = 80f;

        [Header("Blink")]
        [SerializeField] private float blinkThreshold = -0.35f;
        [SerializeField] private float blinkReleaseThreshold = -0.15f;

        [Header("Head gesture")]
        [SerializeField] private float nodPitchDelta = 8f;
        [SerializeField] private float shakeYawDelta = 10f;
        [SerializeField] private float turnYawThreshold = 12f;

        private BciInputSnapshot _current;
        private bool _wasBlinking;
        private float _prevPitch;
        private float _prevYaw;
        private float _nodAccum;
        private float _shakeAccum;
        private HeadGesture _gestureThisFrame;

        public BciInputSnapshot Current => _current;
        public bool IsConnected => openSee != null && openSee.receivedPackets > 0;

        public void Bind(OpenSee.OpenSee see, OpenSeeLauncher seeLauncher)
        {
            openSee = see;
            launcher = seeLauncher;
        }

        public void ConfigurePaths(string exePath, string modelPath, int cameraIndex = 0)
        {
            if (launcher == null) return;
            launcher.exePath = exePath;
            launcher.modelPath = modelPath;
            launcher.cameraIndex = cameraIndex;
            launcher.autoStart = false;
            if (openSee != null)
                launcher.openSeeTarget = openSee;
        }

        public void StartTracking()
        {
            launcher?.StartTracker();
        }

        public void StopTracking()
        {
            launcher?.StopTracker();
        }

        private void Update()
        {
            focus = FocusScrollController.ApplyScroll(focus, scrollSensitivity);
            _gestureThisFrame = HeadGesture.None;

            var blink = false;
            if (openSee != null && openSee.trackingData != null && openSee.trackingData.Length > 0)
            {
                var data = openSee.trackingData[0];
                blink = DetectBlink(data);
                DetectHeadGestures(data);
            }

            _current = new BciInputSnapshot
            {
                Focus = focus,
                Blink = blink,
                Head = _gestureThisFrame
            };
        }

        private bool DetectBlink(OpenSee.OpenSeeData data)
        {
            var eye = Mathf.Min(data.features.EyeLeft, data.features.EyeRight);
            var closed = eye < blinkThreshold;
            var blink = !_wasBlinking && closed;
            if (eye > blinkReleaseThreshold)
                _wasBlinking = false;
            else if (closed)
                _wasBlinking = true;
            return blink;
        }

        private void DetectHeadGestures(OpenSee.OpenSeeData data)
        {
            var pitch = data.rotation.x;
            var yaw = data.rotation.y;
            var dPitch = pitch - _prevPitch;
            var dYaw = yaw - _prevYaw;

            if (dPitch < -nodPitchDelta)
                _nodAccum += Mathf.Abs(dPitch);
            if (Mathf.Abs(dYaw) > shakeYawDelta)
                _shakeAccum += Mathf.Abs(dYaw);

            if (_nodAccum > nodPitchDelta * 2f)
            {
                _gestureThisFrame = HeadGesture.Nod;
                _nodAccum = 0f;
            }
            else if (_shakeAccum > shakeYawDelta * 2.5f)
            {
                _gestureThisFrame = HeadGesture.Shake;
                _shakeAccum = 0f;
            }
            else if (yaw < -turnYawThreshold)
                _gestureThisFrame = HeadGesture.TurnLeft;
            else if (yaw > turnYawThreshold)
                _gestureThisFrame = HeadGesture.TurnRight;

            _prevPitch = pitch;
            _prevYaw = yaw;
            _nodAccum = Mathf.Max(0f, _nodAccum - Time.deltaTime * 30f);
            _shakeAccum = Mathf.Max(0f, _shakeAccum - Time.deltaTime * 30f);
        }
    }
}
