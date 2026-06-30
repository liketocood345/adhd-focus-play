using ADHDTraining.Core;
using ADHDTraining.Core.Session;
using ADHDTraining.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ADHDTraining.Games.Shifting
{
    public class ShiftingGameSession : GameSessionBase
    {
        private Text _commandText;
        private Text _hudText;
        private float _nextCommandAt;
        private CommandKind _shown;
        private bool _awaitingResponse;

        private enum CommandKind { RaiseLeft, RaiseRight, StepLeft, StepRight }

        protected override void Awake()
        {
            gameId = GameIds.Shifting;
            sessionDurationSec = 60f;
            base.Awake();
            BuildHud();
        }

        protected override void OnSessionStarted()
        {
            _awaitingResponse = false;
            _nextCommandAt = Time.time + 1f;
        }

        protected override void OnSessionTick()
        {
            if (Time.time >= _nextCommandAt && !_awaitingResponse)
                SpawnCommand();

            if (_awaitingResponse)
            {
                if (TryDetectOppositeAction())
                    ResolveGesture(true);
                else if (Input.Current.Head == HeadGesture.Shake)
                    ResolveGesture(false);
            }

            _hudText.text = $"指令反转\n得分: {Score}  剩余: {Remaining:F0}s\n" +
                            "举对侧手 / 对侧踏步 · 摇头跳过";
        }

        protected override void OnSessionEnded() { }

        private bool TryDetectOppositeAction()
        {
            var snap = Input.Current;
            return _shown switch
            {
                CommandKind.RaiseLeft => snap.RightHandRaiseEdge,
                CommandKind.RaiseRight => snap.LeftHandRaiseEdge,
                CommandKind.StepLeft => snap.BodyLean > 0,
                CommandKind.StepRight => snap.BodyLean < 0,
                _ => false
            };
        }

        private void SpawnCommand()
        {
            _shown = (CommandKind)Random.Range(0, 4);
            _commandText.text = "请做相反动作：\n" + (_shown switch
            {
                CommandKind.RaiseLeft => "举右手",
                CommandKind.RaiseRight => "举左手",
                CommandKind.StepLeft => "向右一步",
                CommandKind.StepRight => "向左一步",
                _ => ""
            });
            _awaitingResponse = true;
            var interval = Mathf.Lerp(5f, 2f, Input.Current.Focus / 100f);
            _nextCommandAt = Time.time + interval;
        }

        private void ResolveGesture(bool confirmed)
        {
            if (!confirmed)
            {
                _awaitingResponse = false;
                return;
            }

            var correct = TryDetectOppositeAction();
            AddScore(correct ? 10 : -5, correct);
            _awaitingResponse = false;
            _nextCommandAt = Time.time + 0.5f;
        }

        private void BuildHud()
        {
            var canvas = UiCanvasFactory.CreateOverlay("GameHud", 150);
            _commandText = AppHudController.CreateText(canvas.transform, "Command", "准备…", 32, TextAnchor.MiddleCenter);
            _commandText.rectTransform.anchorMin = new Vector2(0.15f, 0.35f);
            _commandText.rectTransform.anchorMax = new Vector2(0.85f, 0.55f);
            _commandText.rectTransform.offsetMin = _commandText.rectTransform.offsetMax = Vector2.zero;
            _hudText = AppHudController.CreateText(canvas.transform, "Hud", "", 20, TextAnchor.UpperCenter);
            var rt = _hudText.rectTransform;
            rt.anchorMin = new Vector2(0.2f, 0.58f);
            rt.anchorMax = new Vector2(0.8f, 0.88f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
