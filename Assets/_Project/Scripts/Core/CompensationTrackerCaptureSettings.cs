using UnityEngine;

namespace ADHDTraining.Core
{
    /// <summary>
    /// OpenSeeFace 摄像头采集分辨率与帧率（PlayerPrefs 记忆）。
    /// </summary>
    public static class CompensationTrackerCaptureSettings
    {
        /// <summary>低于此采集帧率时，眨眼边沿检测可能漏检（建议 ≥18）。</summary>
        public const int MinReliableBlinkFps = 18;

        private const string WidthKey = "adhd_comp_cap_width";
        private const string HeightKey = "adhd_comp_cap_height";
        private const string FpsKey = "adhd_comp_cap_fps";
        private const string PresetKey = "adhd_comp_cap_preset";

        public static int Width { get; private set; } = 640;
        public static int Height { get; private set; } = 360;
        public static int Fps { get; private set; } = 24;
        public static int PresetIndex { get; private set; } = 1;

        public readonly struct CapturePreset
        {
            public readonly string Label;
            public readonly int W;
            public readonly int H;
            public readonly int FrameRate;

            public CapturePreset(string label, int w, int h, int frameRate)
            {
                Label = label;
                W = w;
                H = h;
                FrameRate = frameRate;
            }
        }

        public static readonly CapturePreset[] Presets =
        {
            new("省资源", 640, 360, 15),
            new("推荐", 640, 360, 24),
            new("标清", 640, 480, 24),
            new("流畅", 640, 480, 30),
            new("高清", 1280, 720, 24),
        };

        public static readonly int[] FpsSteps = { 12, 15, 18, 24, 30 };

        public static readonly (int w, int h, string label)[] ResolutionSteps =
        {
            (424, 240, "424×240"),
            (640, 360, "640×360"),
            (640, 480, "640×480"),
            (848, 480, "848×480"),
            (1280, 720, "1280×720"),
        };

        public static void Load()
        {
            Width = PlayerPrefs.GetInt(WidthKey, 640);
            Height = PlayerPrefs.GetInt(HeightKey, 360);
            Fps = PlayerPrefs.GetInt(FpsKey, 24);
            PresetIndex = PlayerPrefs.GetInt(PresetKey, 1);
            ClampValues();
        }

        public static void Save()
        {
            ClampValues();
            PlayerPrefs.SetInt(WidthKey, Width);
            PlayerPrefs.SetInt(HeightKey, Height);
            PlayerPrefs.SetInt(FpsKey, Fps);
            PlayerPrefs.SetInt(PresetKey, PresetIndex);
            PlayerPrefs.Save();
        }

        public static void ApplyPreset(int index)
        {
            if (Presets.Length == 0) return;
            PresetIndex = (index % Presets.Length + Presets.Length) % Presets.Length;
            var p = Presets[PresetIndex];
            Width = p.W;
            Height = p.H;
            Fps = p.FrameRate;
            Save();
        }

        public static void CyclePreset(int delta) => ApplyPreset(PresetIndex + delta);

        public static void CycleFps(int delta)
        {
            Load();
            var idx = IndexOfStep(FpsSteps, Fps);
            idx = (idx + delta + FpsSteps.Length) % FpsSteps.Length;
            Fps = FpsSteps[idx];
            PresetIndex = -1;
            Save();
        }

        public static void CycleResolution(int delta)
        {
            Load();
            var idx = IndexOfResolution(Width, Height);
            idx = (idx + delta + ResolutionSteps.Length) % ResolutionSteps.Length;
            Width = ResolutionSteps[idx].w;
            Height = ResolutionSteps[idx].h;
            PresetIndex = -1;
            Save();
        }

        public static string CaptureLine()
        {
            Load();
            var preset = PresetIndex >= 0 && PresetIndex < Presets.Length
                ? Presets[PresetIndex].Label
                : "自定义";
            return $"{preset} {Width}×{Height}@{Fps}fps";
        }

        public static bool IsBlinkFpsRisky => Fps < MinReliableBlinkFps;

        public static string BlinkFpsHint()
        {
            Load();
            if (!IsBlinkFpsRisky)
                return $"采集 {Width}×{Height} @ {Fps}fps（眨眼建议 ≥{MinReliableBlinkFps}fps）";
            return $"⚠ {Fps}fps 低于 {MinReliableBlinkFps}fps，眨眼可能漏检";
        }

        private static void ClampValues()
        {
            Width = Mathf.Clamp(Width, 320, 1920);
            Height = Mathf.Clamp(Height, 240, 1080);
            Fps = Mathf.Clamp(Fps, 10, 60);
        }

        private static int IndexOfStep(int[] steps, int value)
        {
            for (var i = 0; i < steps.Length; i++)
                if (steps[i] == value) return i;
            var nearest = 0;
            var best = int.MaxValue;
            for (var i = 0; i < steps.Length; i++)
            {
                var d = Mathf.Abs(steps[i] - value);
                if (d < best) { best = d; nearest = i; }
            }
            return nearest;
        }

        private static int IndexOfResolution(int w, int h)
        {
            for (var i = 0; i < ResolutionSteps.Length; i++)
                if (ResolutionSteps[i].w == w && ResolutionSteps[i].h == h) return i;
            return 1;
        }
    }
}
