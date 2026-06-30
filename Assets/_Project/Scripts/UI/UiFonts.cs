using UnityEngine;

namespace ADHDTraining.UI
{
    public static class UiFonts
    {
        private static Font _cached;

        public static Font Default
        {
            get
            {
                if (_cached != null) return _cached;

                _cached = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_cached == null)
                    _cached = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (_cached == null)
                {
                    _cached = Font.CreateDynamicFontFromOSFont(
                        new[] { "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Arial" }, 32);
                }

                if (_cached == null)
                    Debug.LogWarning("[UI] No font available — UGUI text will be invisible.");

                return _cached;
            }
        }
    }
}
