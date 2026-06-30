using ADHDTraining.Core;
using ADHDTraining.Core.Session;
using ADHDTraining.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ADHDTraining.Games.Divided
{
    public class DividedGameSession : GameSessionBase
    {
        private Text _leftPanel;
        private Text _rightPanel;
        private Text _hudText;
        private Side _pending = Side.None;
        private float _nextEventAt;

        private enum Side { None, Left, Right }

        protected override void Awake()
        {
            gameId = GameIds.Divided;
            sessionDurationSec = 60f;
            base.Awake();
            BuildHud();
        }

        protected override void OnSessionStarted()
        {
            _pending = Side.None;
            _nextEventAt = Time.time + 1f;
        }

        protected override void OnSessionTick()
        {
            if (Time.time >= _nextEventAt && _pending == Side.None)
                SpawnEvent();

            if (_pending == Side.Left && Input.Current.Blink)
                Resolve(true);
            else if (_pending == Side.Right && Input.Current.Head == HeadGesture.Nod)
                Resolve(true);

            _leftPanel.text = _pending == Side.Left ? "左：动物求救！\n眨眼救援" : "左侧待命";
            _rightPanel.text = _pending == Side.Right ? "右：无人机求救！\n点头救援" : "右侧待命";
            _hudText.text = $"双线救援\n得分: {Score}  剩余: {Remaining:F0}s";
        }

        protected override void OnSessionEnded() { }

        private void SpawnEvent()
        {
            _pending = Random.value > 0.5f ? Side.Left : Side.Right;
            var interval = Mathf.Lerp(4f, 1.5f, Input.Current.Focus / 100f);
            _nextEventAt = Time.time + interval + 3f;
        }

        private void Resolve(bool success)
        {
            if (success) AddScore(10, true);
            else AddScore(-3, false);
            _pending = Side.None;
            _nextEventAt = Time.time + 0.4f;
        }

        private void BuildHud()
        {
            var canvas = UiCanvasFactory.CreateOverlay("GameHud", 150);
            _leftPanel = AppHudController.CreateText(canvas.transform, "Left", "", 24, TextAnchor.MiddleCenter);
            _leftPanel.rectTransform.anchorMin = new Vector2(0.02f, 0.25f);
            _leftPanel.rectTransform.anchorMax = new Vector2(0.48f, 0.75f);
            _leftPanel.color = new Color(0.7f, 0.9f, 1f);
            _leftPanel.rectTransform.offsetMin = _leftPanel.rectTransform.offsetMax = Vector2.zero;

            _rightPanel = AppHudController.CreateText(canvas.transform, "Right", "", 24, TextAnchor.MiddleCenter);
            _rightPanel.rectTransform.anchorMin = new Vector2(0.52f, 0.25f);
            _rightPanel.rectTransform.anchorMax = new Vector2(0.98f, 0.75f);
            _rightPanel.color = new Color(1f, 0.85f, 0.7f);
            _rightPanel.rectTransform.offsetMin = _rightPanel.rectTransform.offsetMax = Vector2.zero;

            _hudText = AppHudController.CreateText(canvas.transform, "Hud", "", 20, TextAnchor.UpperCenter);
            var rt = _hudText.rectTransform;
            rt.anchorMin = new Vector2(0.2f, 0.78f);
            rt.anchorMax = new Vector2(0.8f, 0.92f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
