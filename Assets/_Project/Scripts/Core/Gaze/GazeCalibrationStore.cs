using UnityEngine;

namespace ADHDTraining.Core.Gaze
{
    public static class GazeCalibrationStore
    {
        private const string CalibratedKey = "adhd_gaze_calibrated";
        private const string CenterXKey = "adhd_gaze_center_x";
        private const string CenterYKey = "adhd_gaze_center_y";
        private const string SpanXKey = "adhd_gaze_span_x";
        private const string SpanYKey = "adhd_gaze_span_y";

        public static void Save(GazeScreenMapper mapper)
        {
            if (mapper == null || !mapper.IsCalibrated) return;
            PlayerPrefs.SetInt(CalibratedKey, 1);
            PlayerPrefs.SetFloat(CenterXKey, mapper.GazeCenter.x);
            PlayerPrefs.SetFloat(CenterYKey, mapper.GazeCenter.y);
            PlayerPrefs.SetFloat(SpanXKey, mapper.GazeSpan.x);
            PlayerPrefs.SetFloat(SpanYKey, mapper.GazeSpan.y);
            PlayerPrefs.Save();
        }

        public static void LoadInto(GazeScreenMapper mapper)
        {
            if (mapper == null) return;
            if (PlayerPrefs.GetInt(CalibratedKey, 0) != 1)
            {
                mapper.Load(Vector2.zero, Vector2.one, false);
                return;
            }

            mapper.Load(
                new Vector2(PlayerPrefs.GetFloat(CenterXKey), PlayerPrefs.GetFloat(CenterYKey)),
                new Vector2(PlayerPrefs.GetFloat(SpanXKey), PlayerPrefs.GetFloat(SpanYKey)),
                true);
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(CalibratedKey);
            PlayerPrefs.DeleteKey(CenterXKey);
            PlayerPrefs.DeleteKey(CenterYKey);
            PlayerPrefs.DeleteKey(SpanXKey);
            PlayerPrefs.DeleteKey(SpanYKey);
            PlayerPrefs.Save();
        }
    }
}
