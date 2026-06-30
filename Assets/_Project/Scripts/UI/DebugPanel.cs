using ADHDTraining.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ADHDTraining.UI
{
    public class DebugPanel : MonoBehaviour
    {
        private BciInputRouter _router;
        private GameObject _window;
        private Text _statsText;
        private InputMotionAvatarView _avatar;

        public static DebugPanel Create(Transform parent, BciInputRouter router)
        {
            var root = new GameObject("DebugPanel");
            root.transform.SetParent(parent, false);
            var panel = root.AddComponent<DebugPanel>();
            panel._router = router;
            panel.Build(parent);
            return panel;
        }

        public void Refresh(BciInputRouter router)
        {
            _router = router;
            if (_window == null || !_window.activeSelf) return;
            var snap = router.Current;
            var motion = router.Compensation?.CurrentMotion ?? default;
            _statsText.text =
                $"模式: {router.ActiveMode}\n" +
                $"连接: {router.IsConnected}\n" +
                $"专注: {snap.Focus:F1}\n" +
                $"眨眼: {snap.Blink}\n" +
                $"头动: {snap.Head}\n" +
                $"注视校准: {(snap.GazeCalibrated ? "是" : "否")}\n" +
                $"在屏内: {snap.GazeOnScreen}\n" +
                $"屏坐标: ({snap.GazeScreenPos.x:F2}, {snap.GazeScreenPos.y:F2})\n" +
                $"举手 L:{snap.LeftHandRaised} R:{snap.RightHandRaised} lean:{snap.BodyLean}\n" +
                $"眼睑 L/R: {motion.EyeL:F2}/{motion.EyeR:F2}\n" +
                $"Raw: {snap.RawDebug}";
            _avatar?.Apply(snap, router.Compensation?.CurrentMotion ?? default);
        }

        private void Build(Transform canvasParent)
        {
            var toggleRt = gameObject.AddComponent<RectTransform>();
            toggleRt.anchorMin = new Vector2(0f, 1f);
            toggleRt.anchorMax = new Vector2(0f, 1f);
            toggleRt.pivot = new Vector2(0f, 1f);
            toggleRt.sizeDelta = new Vector2(100, 36);
            toggleRt.anchoredPosition = new Vector2(12, -12);

            var toggleBtn = AppHudController.CreateButton(transform, "调试", Vector2.zero, Vector2.one);
            toggleBtn.onClick.AddListener(ToggleWindow);

            _window = new GameObject("DebugWindow");
            _window.transform.SetParent(canvasParent, false);
            var wrt = _window.AddComponent<RectTransform>();
            wrt.anchorMin = new Vector2(0f, 1f);
            wrt.anchorMax = new Vector2(0f, 1f);
            wrt.pivot = new Vector2(0f, 1f);
            wrt.sizeDelta = new Vector2(340, 260);
            wrt.anchoredPosition = new Vector2(12, -56);
            UiSprites.Apply(_window.AddComponent<Image>(), new Color(0.08f, 0.1f, 0.14f, 0.95f));
            _window.SetActive(false);

            var avatarGo = new GameObject("AvatarView");
            avatarGo.transform.SetParent(_window.transform, false);
            var art = avatarGo.AddComponent<RectTransform>();
            art.anchorMin = new Vector2(0.02f, 0.35f);
            art.anchorMax = new Vector2(0.48f, 0.98f);
            art.offsetMin = art.offsetMax = Vector2.zero;
            _avatar = avatarGo.AddComponent<InputMotionAvatarView>();
            _avatar.Initialize();

            var statsGo = new GameObject("Stats");
            statsGo.transform.SetParent(_window.transform, false);
            var srt = statsGo.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0.1f);
            srt.anchorMax = new Vector2(0.98f, 0.98f);
            srt.offsetMin = srt.offsetMax = Vector2.zero;
            _statsText = AppHudController.CreateText(statsGo.transform, "Stats", "", 14, TextAnchor.UpperLeft);
        }

        private void ToggleWindow()
        {
            if (_window == null) return;
            _window.SetActive(!_window.activeSelf);
            if (_window.activeSelf)
            {
                _avatar?.ResetCalibration();
                Refresh(_router);
            }
        }
    }
}
