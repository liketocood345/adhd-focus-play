using System.Collections.Generic;
using ADHDTraining.Core;
using ADHDTraining.Core.Gaze;
using UnityEngine;

namespace ADHDTraining.UI
{
    /// <summary>
    /// 左右屏心红点各 3 秒，取最后一秒眼球均值完成屏幕映射校准。
    /// </summary>
    public class GazeCalibrationController : MonoBehaviour
    {
        private const float PhaseDurationSec = 3f;
        private const float CollectTailSec = 1f;

        public GazeScreenMapper Mapper { get; private set; } = new();

        private CompensationBciInputProvider _compensation;
        private GazeCalibrationOverlay _overlay;
        private Phase _phase = Phase.None;
        private float _phaseStartUnscaled;
        private readonly List<GazeSample> _tailSamples = new();
        private GazeSample _leftAvg;

        public bool IsCalibrating => _phase != Phase.None;

        private enum Phase { None, Left, Right }

        private void Awake()
        {
            GazeCalibrationStore.LoadInto(Mapper);
        }

        public void BeginCalibration(CompensationBciInputProvider compensation)
        {
            if (compensation == null || IsCalibrating) return;

            if (!compensation.IsConnected && AppRoot.Instance?.Router?.ActiveMode != BciInputMode.Compensation)
            {
                AppRoot.Instance?.Router?.SetInputMode(BciInputMode.Compensation);
            }

            _compensation = compensation;
            _overlay = GazeCalibrationOverlay.Ensure();
            _overlay.Show();
            _tailSamples.Clear();
            BeginPhase(Phase.Left);
        }

        public void CancelCalibration()
        {
            _phase = Phase.None;
            _tailSamples.Clear();
            _overlay?.Hide();
        }

        private void BeginPhase(Phase phase)
        {
            _phase = phase;
            _phaseStartUnscaled = Time.unscaledTime;
            _tailSamples.Clear();
            UpdateOverlayUi();
        }

        private void Update()
        {
            if (_phase == Phase.None) return;

            var elapsed = Time.unscaledTime - _phaseStartUnscaled;
            if (elapsed >= PhaseDurationSec - CollectTailSec)
            {
                var gaze = _compensation?.CurrentGaze ?? GazeSample.Invalid;
                if (gaze.Valid) _tailSamples.Add(gaze);
            }

            UpdateOverlayUi(elapsed);

            if (elapsed < PhaseDurationSec) return;

            var avg = Average(_tailSamples);
            if (_phase == Phase.Left)
            {
                _leftAvg = avg;
                BeginPhase(Phase.Right);
                return;
            }

            if (_leftAvg.Valid && avg.Valid)
            {
                Mapper.ApplyCalibration(_leftAvg, avg);
                GazeCalibrationStore.Save(Mapper);
                Debug.Log($"[Gaze] 校准完成 center=({Mapper.GazeCenter.x:F2},{Mapper.GazeCenter.y:F2}) span=({Mapper.GazeSpan.x:F2},{Mapper.GazeSpan.y:F2})");
            }
            else
            {
                Debug.LogWarning("[Gaze] 校准失败：采样不足。请开启代偿模式并直视红点。");
            }

            _phase = Phase.None;
            _overlay?.Hide();
        }

        private void UpdateOverlayUi(float elapsed = -1f)
        {
            if (_overlay == null) return;
            if (elapsed < 0f) elapsed = Time.unscaledTime - _phaseStartUnscaled;

            var remain = Mathf.CeilToInt(Mathf.Max(0f, PhaseDurationSec - elapsed));
            if (_phase == Phase.Left)
            {
                _overlay.SetTarget(0.25f, 0.5f);
                _overlay.SetInstruction($"请盯着左侧红点\n保持 {remain} 秒…");
            }
            else
            {
                _overlay.SetTarget(0.75f, 0.5f);
                _overlay.SetInstruction($"请盯着右侧红点\n保持 {remain} 秒…");
            }
        }

        private static GazeSample Average(List<GazeSample> samples)
        {
            if (samples == null || samples.Count == 0) return GazeSample.Invalid;
            float yaw = 0f, pitch = 0f;
            foreach (var s in samples)
            {
                yaw += s.Yaw;
                pitch += s.Pitch;
            }
            var n = samples.Count;
            return new GazeSample { Yaw = yaw / n, Pitch = pitch / n, Valid = true };
        }
    }
}
