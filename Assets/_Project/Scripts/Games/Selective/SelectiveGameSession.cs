using ADHDTraining.Core;
using ADHDTraining.Core.Session;
using ADHDTraining.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ADHDTraining.Games.Selective
{
    public class SelectiveGameSession : GameSessionBase
    {
        private AudioSource _targetSource;
        private AudioSource _distractorSource;
        private Text _hudText;
        private float _nextTargetAt;
        private bool _targetWindowActive;
        private int _coins;

        protected override void Awake()
        {
            gameId = GameIds.Selective;
            sessionDurationSec = 45f;
            base.Awake();
            BuildAudio();
            BuildHud();
        }

        protected override void OnSessionStarted()
        {
            _coins = 0;
            ScheduleTarget();
            _targetSource.loop = true;
            _distractorSource.loop = true;
            _targetSource.Play();
            _distractorSource.Play();
        }

        protected override void OnSessionTick()
        {
            ApplyMix(Input.Current.Focus);

            if (Time.time >= _nextTargetAt && !_targetWindowActive)
            {
                _targetWindowActive = true;
                _nextTargetAt = Time.time + 1.2f;
            }
            else if (_targetWindowActive && Time.time >= _nextTargetAt)
            {
                _targetWindowActive = false;
                ScheduleTarget();
            }

            if (Input.Current.Blink)
            {
                if (_targetWindowActive)
                {
                    _coins += 5;
                    AddScore(5, true);
                }
                else
                {
                    AddScore(-2, false);
                }
                _targetWindowActive = false;
                ScheduleTarget();
            }

            _hudText.text = $"听音寻宝\n专注: {Input.Current.Focus:F0}\n金币: {_coins}\n剩余: {Remaining:F0}s\n" +
                            (_targetWindowActive ? ">>> 目标声出现！眨眼 <<<" : "等待目标声…");
        }

        protected override void OnSessionEnded() { }

        protected override string BuildExtraJson() => $"{{\"coins\":{_coins}}}";

        private void ScheduleTarget() => _nextTargetAt = Time.time + Random.Range(2.5f, 5f);

        private void ApplyMix(float focus)
        {
            if (focus > 70f) { _targetSource.volume = 1f; _distractorSource.volume = 0.2f; }
            else if (focus >= 40f) { _targetSource.volume = 0.6f; _distractorSource.volume = 0.6f; }
            else { _targetSource.volume = 0.15f; _distractorSource.volume = 1f; }
        }

        private void BuildAudio()
        {
            var root = new GameObject("Audio");
            _targetSource = root.AddComponent<AudioSource>();
            _distractorSource = root.AddComponent<AudioSource>();
            _targetSource.clip = ProceduralToneUtility.CreateTone("target", 880f, 2f);
            _distractorSource.clip = ProceduralToneUtility.CreateTone("distractor", 220f, 2f);
            _targetSource.loop = _distractorSource.loop = true;
        }

        private void BuildHud()
        {
            var canvas = UiCanvasFactory.CreateOverlay("GameHud", 150);
            _hudText = AppHudController.CreateText(canvas.transform, "Hud", "", 22, TextAnchor.UpperCenter);
            var rt = _hudText.rectTransform;
            rt.anchorMin = new Vector2(0.2f, 0.55f);
            rt.anchorMax = new Vector2(0.8f, 0.88f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
