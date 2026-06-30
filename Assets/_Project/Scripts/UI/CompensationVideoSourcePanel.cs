using System.Collections.Generic;
using System.IO;
using ADHDTraining.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ADHDTraining.UI
{
    /// <summary>
    /// 主菜单：代偿模式视频输入源（摄像头 / 本地视频文件）。
    /// </summary>
    public class CompensationVideoSourcePanel : MonoBehaviour
    {
        private CompensationBciInputProvider _compensation;
        private BciInputRouter _router;
        private Text _sourceLabel;
        private Text _cameraLabel;
        private Text _statusLabel;
        private Text _presetLabel;
        private Text _fpsLabel;
        private Text _resLabel;
        private readonly List<string> _cameras = new();
        private int _selectedCameraIndex;
        private Button _cameraModeBtn;
        private Button _videoModeBtn;

        private static readonly Color ModeActive = new(0.22f, 0.42f, 0.62f, 0.95f);
        private static readonly Color ModeIdle = new(0.15f, 0.2f, 0.35f, 0.92f);

        public static CompensationVideoSourcePanel Create(
            Transform parent, BciInputRouter router, CompensationBciInputProvider compensation)
        {
            var root = new GameObject("CompensationVideoSourcePanel");
            root.transform.SetParent(parent, false);
            var panel = root.AddComponent<CompensationVideoSourcePanel>();
            panel._router = router;
            panel._compensation = compensation;
            panel.Build();
            return panel;
        }

        private void Build()
        {
            CompensationVideoSourceStore.Load();

            var rt = gameObject.AddComponent<RectTransform>();
            MainMenuRightDock.ApplyChildLayout(gameObject, MainMenuHudLayout.VideoSourceHeight);

            var bg = gameObject.AddComponent<Image>();
            UiSprites.Apply(bg, new Color(0.05f, 0.08f, 0.12f, 0.88f));

            var title = AppHudController.CreateText(transform, "Title", "代偿视频源", 15, TextAnchor.MiddleLeft);
            var titleRt = title.rectTransform;
            titleRt.anchorMin = new Vector2(0.04f, 0.93f);
            titleRt.anchorMax = new Vector2(0.96f, 0.99f);
            titleRt.offsetMin = titleRt.offsetMax = Vector2.zero;

            _sourceLabel = AppHudController.CreateText(transform, "Summary", "", 11, TextAnchor.MiddleLeft);
            var summaryRt = _sourceLabel.rectTransform;
            summaryRt.anchorMin = new Vector2(0.04f, 0.85f);
            summaryRt.anchorMax = new Vector2(0.96f, 0.93f);
            summaryRt.offsetMin = summaryRt.offsetMax = Vector2.zero;
            _sourceLabel.color = new Color(0.75f, 0.85f, 1f);

            _statusLabel = AppHudController.CreateText(transform, "Status", "", 10, TextAnchor.MiddleLeft);
            var statusRt = _statusLabel.rectTransform;
            statusRt.anchorMin = new Vector2(0.04f, 0.77f);
            statusRt.anchorMax = new Vector2(0.96f, 0.85f);
            statusRt.offsetMin = statusRt.offsetMax = Vector2.zero;
            _statusLabel.color = new Color(1f, 0.75f, 0.55f);

            AppHudController.CreateButton(transform, "◀", new Vector2(0.02f, 0.68f), new Vector2(0.10f, 0.76f))
                .onClick.AddListener(() => ChangePreset(-1));
            _presetLabel = AppHudController.CreateText(transform, "PresetLabel", "推荐 640×360@24", 10, TextAnchor.MiddleCenter);
            AnchorRect(_presetLabel.rectTransform, new Vector2(0.11f, 0.68f), new Vector2(0.89f, 0.76f));
            AppHudController.CreateButton(transform, "▶", new Vector2(0.90f, 0.68f), new Vector2(0.98f, 0.76f))
                .onClick.AddListener(() => ChangePreset(1));

            AppHudController.CreateButton(transform, "F◀", new Vector2(0.02f, 0.58f), new Vector2(0.10f, 0.66f))
                .onClick.AddListener(() => ChangeFps(-1));
            _fpsLabel = AppHudController.CreateText(transform, "FpsLabel", "24fps", 10, TextAnchor.MiddleCenter);
            AnchorRect(_fpsLabel.rectTransform, new Vector2(0.11f, 0.58f), new Vector2(0.30f, 0.66f));
            AppHudController.CreateButton(transform, "F▶", new Vector2(0.31f, 0.58f), new Vector2(0.39f, 0.66f))
                .onClick.AddListener(() => ChangeFps(1));

            AppHudController.CreateButton(transform, "R◀", new Vector2(0.41f, 0.58f), new Vector2(0.49f, 0.66f))
                .onClick.AddListener(() => ChangeResolution(-1));
            _resLabel = AppHudController.CreateText(transform, "ResLabel", "640×360", 10, TextAnchor.MiddleCenter);
            AnchorRect(_resLabel.rectTransform, new Vector2(0.50f, 0.58f), new Vector2(0.89f, 0.66f));
            AppHudController.CreateButton(transform, "R▶", new Vector2(0.90f, 0.58f), new Vector2(0.98f, 0.66f))
                .onClick.AddListener(() => ChangeResolution(1));

            _cameraModeBtn = CreateModeButton("摄像头", CompensationCaptureKind.Camera, new Vector2(0.02f, 0.48f), new Vector2(0.48f, 0.56f));
            _videoModeBtn = CreateModeButton("视频文件", CompensationCaptureKind.VideoFile, new Vector2(0.52f, 0.48f), new Vector2(0.98f, 0.56f));

            AppHudController.CreateButton(transform, "◀", new Vector2(0.02f, 0.36f), new Vector2(0.11f, 0.46f))
                .onClick.AddListener(PrevCamera);
            _cameraLabel = AppHudController.CreateText(transform, "CameraName", "摄像头 0", 11, TextAnchor.MiddleCenter);
            var camRt = _cameraLabel.rectTransform;
            camRt.anchorMin = new Vector2(0.12f, 0.36f);
            camRt.anchorMax = new Vector2(0.70f, 0.46f);
            camRt.offsetMin = camRt.offsetMax = Vector2.zero;
            AppHudController.CreateButton(transform, "▶", new Vector2(0.72f, 0.36f), new Vector2(0.81f, 0.46f))
                .onClick.AddListener(NextCamera);
            AppHudController.CreateButton(transform, "刷新", new Vector2(0.83f, 0.36f), new Vector2(0.98f, 0.46f))
                .onClick.AddListener(() =>
                {
                    RefreshCameras();
                    RefreshUi();
                    if (_router != null && _router.ActiveMode == BciInputMode.Compensation)
                        _compensation?.StartTracking();
                });

            AppHudController.CreateButton(transform, "选择视频…", new Vector2(0.02f, 0.22f), new Vector2(0.98f, 0.34f))
                .onClick.AddListener(PickVideoFile);

            AppHudController.CreateButton(transform, "屏幕校准", new Vector2(0.02f, 0.08f), new Vector2(0.98f, 0.20f))
                .onClick.AddListener(StartScreenCalibration);

            RefreshCameras();
            ApplyStoredSource();
            RefreshUi();

            if (_router != null && _router.ActiveMode == BciInputMode.Compensation)
                _compensation?.StartTracking();
        }

        private void ApplyStoredSource()
        {
            _compensation?.ApplyStoredVideoSource();
        }

        private Button CreateModeButton(string label, CompensationCaptureKind kind, Vector2 min, Vector2 max)
        {
            var btn = AppHudController.CreateButton(transform, label, min, max);
            btn.onClick.AddListener(() =>
            {
                if (kind == CompensationCaptureKind.Camera)
                {
                    if (!IsPlaceholderCamera())
                        CompensationVideoSourceStore.SetCamera(_selectedCameraIndex, _cameras[_selectedCameraIndex]);
                    else
                        CompensationVideoSourceStore.SetCamera(_selectedCameraIndex);
                }
                else if (!string.IsNullOrEmpty(CompensationVideoSourceStore.VideoPath))
                {
                    CompensationVideoSourceStore.SetVideoFile(CompensationVideoSourceStore.VideoPath);
                }
                else
                {
                    CompensationVideoSourceStore.SelectKind(CompensationCaptureKind.VideoFile);
                }

                ApplyAndMaybeRestart();
                RefreshUi();
            });
            return btn;
        }

        private void RefreshCameras()
        {
            _compensation?.StopTracking();
            _cameras.Clear();
            if (_compensation != null)
                _cameras.AddRange(_compensation.ListAvailableCameras(forceRefresh: true));

            if (_cameras.Count == 0)
            {
                var err = _compensation?.LastCameraListError ?? "未知错误";
                _cameras.Add("（未检测到摄像头）");
                if (_statusLabel != null)
                    _statusLabel.text = err.Length > 48 ? err.Substring(0, 46) + "…" : err;
            }

            CompensationVideoSourceStore.Load();
            _selectedCameraIndex = CompensationVideoSourceStore.ResolveCameraIndex(_cameras);
            if (!IsPlaceholderCamera())
                CompensationVideoSourceStore.UpdateCameraSelection(_selectedCameraIndex, _cameras[_selectedCameraIndex]);
            UpdateCameraLabel();
        }

        private void PrevCamera()
        {
            if (_cameras.Count == 0 || IsPlaceholderCamera()) return;
            _selectedCameraIndex = (_selectedCameraIndex - 1 + _cameras.Count) % _cameras.Count;
            CompensationVideoSourceStore.SetCamera(_selectedCameraIndex, _cameras[_selectedCameraIndex]);
            ApplyAndMaybeRestart();
            RefreshUi();
        }

        private void NextCamera()
        {
            if (_cameras.Count == 0 || IsPlaceholderCamera()) return;
            _selectedCameraIndex = (_selectedCameraIndex + 1) % _cameras.Count;
            CompensationVideoSourceStore.SetCamera(_selectedCameraIndex, _cameras[_selectedCameraIndex]);
            ApplyAndMaybeRestart();
            RefreshUi();
        }

        private void StartScreenCalibration()
        {
            if (_compensation == null) return;
            AppRoot.Ensure().GazeCalibration?.BeginCalibration(_compensation);
        }

        private void PickVideoFile()
        {
            if (!VideoFilePicker.TryPickVideo(out var path)) return;
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[代偿] 视频文件不存在: {path}");
                return;
            }

            CompensationVideoSourceStore.SetVideoFile(path);
            ApplyAndMaybeRestart();
            RefreshUi();
        }

        private void ChangePreset(int delta)
        {
            CompensationTrackerCaptureSettings.CyclePreset(delta);
            ApplyAndMaybeRestart();
            RefreshUi();
        }

        private void ChangeFps(int delta)
        {
            CompensationTrackerCaptureSettings.CycleFps(delta);
            ApplyAndMaybeRestart();
            RefreshUi();
        }

        private void ChangeResolution(int delta)
        {
            CompensationTrackerCaptureSettings.CycleResolution(delta);
            ApplyAndMaybeRestart();
            RefreshUi();
        }

        private void ApplyAndMaybeRestart()
        {
            _compensation?.ApplyStoredVideoSource();
            if (_router != null && _router.ActiveMode == BciInputMode.Compensation)
                _compensation?.RestartTracking();
        }

        private void RefreshUi()
        {
            CompensationVideoSourceStore.Load();
            CompensationTrackerCaptureSettings.Load();
            _selectedCameraIndex = CompensationVideoSourceStore.ResolveCameraIndex(_cameras);
            UpdateCameraLabel();
            _sourceLabel.text = CompensationVideoSourceStore.Summary();
            HighlightModeButtons();
            UpdateCaptureControls();
        }

        private void UpdateCaptureControls()
        {
            if (_presetLabel != null)
                _presetLabel.text = CompensationTrackerCaptureSettings.CaptureLine();
            if (_fpsLabel != null)
                _fpsLabel.text = $"{CompensationTrackerCaptureSettings.Fps}fps";
            if (_resLabel != null)
                _resLabel.text = $"{CompensationTrackerCaptureSettings.Width}×{CompensationTrackerCaptureSettings.Height}";

            if (_statusLabel == null) return;
            if (_cameras.Count == 0 || IsPlaceholderCamera())
                return;

            if (CompensationVideoSourceStore.Kind == CompensationCaptureKind.VideoFile)
            {
                _statusLabel.text =
                    $"视频模式：分辨率随源文件；眨眼建议源 ≥{CompensationTrackerCaptureSettings.MinReliableBlinkFps}fps";
                _statusLabel.color = new Color(0.75f, 0.85f, 1f);
                return;
            }

            _statusLabel.text = CompensationTrackerCaptureSettings.BlinkFpsHint();
            _statusLabel.color = CompensationTrackerCaptureSettings.IsBlinkFpsRisky
                ? new Color(1f, 0.5f, 0.45f)
                : new Color(0.65f, 0.9f, 0.7f);
        }

        private static void AnchorRect(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private void HighlightModeButtons()
        {
            var isCamera = CompensationVideoSourceStore.Kind == CompensationCaptureKind.Camera;
            SetModeButtonState(_cameraModeBtn, isCamera);
            SetModeButtonState(_videoModeBtn, !isCamera);
        }

        private static void SetModeButtonState(Button btn, bool active)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
                UiSprites.Apply(img, active ? ModeActive : ModeIdle);
        }

        private void UpdateCameraLabel()
        {
            if (_cameraLabel == null) return;
            if (IsPlaceholderCamera())
            {
                _cameraLabel.text = _cameras[0];
                return;
            }

            var name = _cameras[_selectedCameraIndex];
            if (name.Length > 22) name = name.Substring(0, 20) + "…";
            _cameraLabel.text = $"#{_selectedCameraIndex} {name}";
        }

        private bool IsPlaceholderCamera() =>
            _cameras.Count == 1 && _cameras[0].StartsWith("（未检测到");
    }
}
