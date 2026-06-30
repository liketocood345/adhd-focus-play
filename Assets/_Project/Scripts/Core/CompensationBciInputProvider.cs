using ADHDTraining.Core.Gaze;
using ADHDTraining.Core.MediaPipe;
using UnityEngine;

namespace ADHDTraining.Core
{
    /// <summary>
    /// USB 摄像头 + MediaPipe（EVTS 级面部 + 肩颈 + 举手）代偿输入。
    /// </summary>
    public class CompensationBciInputProvider : MonoBehaviour, IBciInputProvider
    {
        [Header("MediaPipe Bridge")]
        [SerializeField] private string pythonExe = MediaPipeBridgeLauncher.DefaultPython;
        [SerializeField] private bool showTrackerPreview = true;
        [SerializeField] private int poseComplexity = 1;

        [Header("Focus (manual via scroll wheel)")]
        [SerializeField] private float focus = 75f;
        [SerializeField] private float scrollSensitivity = 80f;

        [Header("Blink (EVTS eye openness: higher = more closed)")]
        [SerializeField] private float blinkOpenThreshold = 0.42f;
        [SerializeField] private float blinkClosedThreshold = 0.55f;

        [Header("Head gesture")]
        [SerializeField] private float nodPitchDelta = 8f;
        [SerializeField] private float shakeYawDelta = 10f;
        [SerializeField] private float turnYawThreshold = 12f;

        private readonly MediaPipeBridgeLauncher _launcher = new();
        private readonly MediaPipeMotionClient _client = new();
        private BciInputSnapshot _current;
        private MediaPipeMotionFrame _motion;
        private GazeSample _currentGaze = GazeSample.Invalid;
        private string _activeLaunchKey;
        private bool _trackingActive;
        private float _nextStartAllowedTime;

        private bool _wasBlinking;
        private bool _wasLeftHandRaised;
        private bool _wasRightHandRaised;
        private float _prevPitch;
        private float _prevYaw;
        private float _nodAccum;
        private float _shakeAccum;
        private HeadGesture _gestureThisFrame;
        private GazeScreenMapper _gazeMapper;

        public BciInputSnapshot Current => _current;
        public MediaPipeMotionFrame CurrentMotion => _motion;
        public GazeSample CurrentGaze => _currentGaze;
        public bool IsConnected => _client.IsReceiving;
        public string LastCameraListError { get; private set; }

        public void SetGazeMapper(GazeScreenMapper mapper) => _gazeMapper = mapper;

        public void ConfigurePaths(string unusedExe = null, string unusedModels = null, int cameraIndex = 0)
        {
            if (!string.IsNullOrWhiteSpace(unusedExe) && unusedExe.EndsWith("python.exe", System.StringComparison.OrdinalIgnoreCase))
                pythonExe = unusedExe;
            _launcher.SetPythonPath(pythonExe);
            ApplyStoredVideoSource();
        }

        public void ApplyStoredVideoSource()
        {
            CompensationVideoSourceStore.Load();
            CompensationTrackerCaptureSettings.Load();
        }

        public void ConfigureVideoSource(CompensationCaptureKind kind, int cameraIndex, string videoPath)
        {
            // 由 StartTracking 读取 Store
        }

        public string[] ListAvailableCameras(bool forceRefresh = false)
        {
            var cameras = MediaPipeBridgeLauncher.ListCameras(pythonExe, forceRefresh);
            if (cameras.Length > 0)
            {
                LastCameraListError = null;
                return cameras;
            }

            LastCameraListError = _launcher.LastError ?? "MediaPipe 摄像头枚举为空";
            return cameras;
        }

        public string ActiveSourceLabel => CompensationVideoSourceStore.Summary();

        public void StartTracking(bool forceRestart = false)
        {
            CompensationVideoSourceStore.Load();
            CompensationTrackerCaptureSettings.Load();
            var isCamera = CompensationVideoSourceStore.Kind == CompensationCaptureKind.Camera;
            var video = isCamera ? "" : CompensationVideoSourceStore.VideoPath;

            var args = new MediaPipeLaunchArgs
            {
                Port = MediaPipeMotionClient.DefaultPort,
                CameraIndex = CompensationVideoSourceStore.CameraIndex,
                Width = CompensationTrackerCaptureSettings.Width,
                Height = CompensationTrackerCaptureSettings.Height,
                Fps = CompensationTrackerCaptureSettings.Fps,
                Preview = showTrackerPreview,
                PoseComplexity = poseComplexity,
                VideoPath = video
            };

            var launchKey = BuildLaunchKey(args);
            if (!forceRestart && _trackingActive && _launcher.IsRunning && launchKey == _activeLaunchKey)
                return;

            if (Time.realtimeSinceStartup < _nextStartAllowedTime)
            {
                CancelInvoke(nameof(StartTrackingDeferred));
                Invoke(nameof(StartTrackingDeferred), _nextStartAllowedTime - Time.realtimeSinceStartup);
                return;
            }

            BeginTracking(args, launchKey);
        }

        public void RestartTracking()
        {
            StopTracking();
            _nextStartAllowedTime = Time.realtimeSinceStartup + 0.45f;
            StartTracking(forceRestart: true);
        }

        private void StartTrackingDeferred() => StartTracking(forceRestart: true);

        private void BeginTracking(MediaPipeLaunchArgs args, string launchKey)
        {
            _client.Start(MediaPipeMotionClient.DefaultPort);

            if (!_launcher.Start(args))
            {
                _trackingActive = false;
                Debug.LogWarning($"[代偿] MediaPipe 未启动: {_launcher.LastError}");
                return;
            }

            _activeLaunchKey = launchKey;
            _trackingActive = true;
            _nextStartAllowedTime = Time.realtimeSinceStartup + 0.35f;
        }

        private static string BuildLaunchKey(MediaPipeLaunchArgs args) =>
            $"{args.CameraIndex}|{args.Width}x{args.Height}@{args.Fps}|p{args.PoseComplexity}|" +
            $"{(args.Preview ? 1 : 0)}|{args.VideoPath ?? ""}";

        public void StopTracking()
        {
            CancelInvoke(nameof(StartTrackingDeferred));
            _launcher.Stop();
            _client.Stop();
            _trackingActive = false;
            _activeLaunchKey = null;
            _wasBlinking = false;
        }

        private void OnDestroy() => StopTracking();

        private void Update()
        {
            focus = FocusScrollController.ApplyScroll(focus, scrollSensitivity);
            _gestureThisFrame = HeadGesture.None;
            _motion = _client.Latest;

            var blink = false;
            var leftHandEdge = false;
            var rightHandEdge = false;

            if (_motion.IsTracking)
            {
                blink = DetectBlink(_motion);
                DetectHeadGestures(_motion);
                leftHandEdge = _motion.LeftHandRaised && !_wasLeftHandRaised;
                rightHandEdge = _motion.RightHandRaised && !_wasRightHandRaised;
                _wasLeftHandRaised = _motion.LeftHandRaised;
                _wasRightHandRaised = _motion.RightHandRaised;

                if (_motion.FaceValid)
                {
                    _currentGaze = new GazeSample
                    {
                        Yaw = _motion.GazeYaw,
                        Pitch = _motion.GazePitch,
                        Valid = true
                    };
                }
            }
            else
            {
                _wasLeftHandRaised = _wasRightHandRaised = false;
            }

            var calibrated = _gazeMapper != null && _gazeMapper.IsCalibrated;
            var onScreen = false;
            var screenPos = new Vector2(0.5f, 0.5f);
            if (calibrated && _currentGaze.Valid)
                _gazeMapper.TryMapToScreen(_currentGaze, out screenPos, out onScreen);

            _current = new BciInputSnapshot
            {
                Focus = focus,
                Blink = blink,
                Head = _gestureThisFrame,
                LeftHandRaised = _motion.LeftHandRaised,
                RightHandRaised = _motion.RightHandRaised,
                LeftHandRaiseEdge = leftHandEdge,
                RightHandRaiseEdge = rightHandEdge,
                BodyLean = _motion.BodyLean,
                GazeCalibrated = calibrated,
                GazeOnScreen = onScreen,
                GazeScreenPos = screenPos,
                RawDebug = BuildRawDebug(screenPos, onScreen)
            };
        }

        private string BuildRawDebug(Vector2 screenPos, bool onScreen)
        {
            if (!_motion.IsTracking) return "no track";
            return $"MP fps:{_motion.Fps:F0} face:{_motion.FaceValid} pose:{_motion.PoseValid} " +
                   $"eyeL:{_motion.EyeL:F2} eyeR:{_motion.EyeR:F2} " +
                   $"Lhand:{_motion.LeftHandRaised} Rhand:{_motion.RightHandRaised} lean:{_motion.BodyLean} " +
                   $"scr({screenPos.x:F2},{screenPos.y:F2}) on:{onScreen}";
        }

        private bool DetectBlink(MediaPipeMotionFrame frame)
        {
            // EVTS get_pose: eye_l/eye_r 越大表示眼睑越闭合（与 OpenSee 1=睁开相反）
            var eye = Mathf.Max(frame.EyeL, frame.EyeR);
            var closed = eye > blinkClosedThreshold;
            var blink = !_wasBlinking && closed;
            if (eye < blinkOpenThreshold)
                _wasBlinking = false;
            else if (closed)
                _wasBlinking = true;
            return blink;
        }

        private void DetectHeadGestures(MediaPipeMotionFrame frame)
        {
            var pitch = frame.HeadPitch;
            var yaw = frame.HeadYaw;
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
