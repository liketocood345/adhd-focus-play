using System;
using ADHDTraining.Core.Session;
using ADHDTraining.Games;
using UnityEngine;

namespace ADHDTraining.Core
{
    /// <summary>
    /// 游戏会话基类：注入 BCI、低专注暂停、CSV 记录、返回主菜单。
    /// </summary>
    public abstract class GameSessionBase : MonoBehaviour, IGameSessionReporter
    {
        [SerializeField] protected string gameId;
        [SerializeField] protected float sessionDurationSec = 60f;
        [SerializeField] protected bool autoStart = true;

        protected IBciInputProvider Input { get; private set; }
        protected FocusPauseController PauseController { get; private set; }

        protected int Score { get; set; }
        protected int CorrectCount { get; set; }
        protected int WrongCount { get; set; }
        protected int PauseCount { get; private set; }

        private float _elapsed;
        private float _focusSum;
        private int _focusSamples;
        private bool _running;
        private bool _ended;

        public string GameId => gameId;
        public int LiveScore => Score;
        public int LiveCorrectCount => CorrectCount;
        public int LiveWrongCount => WrongCount;
        public float Elapsed => _elapsed;
        public float Remaining => Mathf.Max(0f, sessionDurationSec - _elapsed);
        public bool IsRunning => _running;

        public event Action<int, int, int> ScoreChanged;

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(gameId))
                gameId = GuessGameIdFromType(GetType());

            var root = AppRoot.Ensure();
            Input = root.Router;
            PauseController = root.GetComponent<FocusPauseController>() ?? root.gameObject.AddComponent<FocusPauseController>();
            PauseController.Bind(Input);
            PauseController.Paused += OnPaused;
            PauseController.Resumed += OnResumed;
        }

        protected virtual void Start()
        {
            if (autoStart) StartSession();
        }

        protected virtual void Update()
        {
            if (!_running || PauseController.IsPaused) return;

            _elapsed += Time.deltaTime;
            var focus = Input.Current.Focus;
            _focusSum += focus;
            _focusSamples++;

            OnSessionTick();

            ScoreChanged?.Invoke(Score, CorrectCount, WrongCount);

            if (_elapsed >= sessionDurationSec)
                EndSession();
        }

        public void StartSession()
        {
            _running = true;
            _ended = false;
            _elapsed = 0f;
            _focusSum = 0f;
            _focusSamples = 0;
            Score = 0;
            CorrectCount = 0;
            WrongCount = 0;
            PauseCount = 0;
            OnSessionStarted();
        }

        public void EndSession()
        {
            if (_ended) return;
            _ended = true;
            _running = false;

            var avgFocus = _focusSamples > 0 ? _focusSum / _focusSamples : 0f;
            var mode = AppRoot.Ensure().Router.ActiveMode;
            var record = SessionRecordService.CreateRecord(
                gameId, mode, _elapsed, Score, CorrectCount, WrongCount, avgFocus, PauseCount, BuildExtraJson());
            AppRoot.Ensure().Records.Append(record);
            AppRoot.Ensure().Hud?.RefreshScoreboard();
            OnSessionEnded();
        }

        public void ReturnToMainMenu()
        {
            EndSession();
            SceneLoader.LoadMainMenu();
        }

        protected void AddScore(int delta, bool correct)
        {
            Score += delta;
            if (correct) CorrectCount++;
            else WrongCount++;
            GameAudioLibrary.PlayOneShot(transform, correct ? GameAudioLibrary.Correct : GameAudioLibrary.Wrong);
        }

        protected abstract void OnSessionStarted();
        protected abstract void OnSessionTick();
        protected abstract void OnSessionEnded();
        protected virtual string BuildExtraJson() => "{}";

        private void OnPaused() => PauseCount++;
        private void OnResumed() { }

        private static string GuessGameIdFromType(System.Type type)
        {
            var name = type.Name;
            if (name.Contains("Selective")) return GameIds.Selective;
            if (name.Contains("Sustained") || name.Contains("Runner")) return GameIds.Sustained;
            if (name.Contains("Shifting") || name.Contains("Reverse")) return GameIds.Shifting;
            if (name.Contains("Divided") || name.Contains("Dual")) return GameIds.Divided;
            if (name.Contains("Inhibition") || name.Contains("Falling")) return GameIds.Inhibition;
            return GameIds.Selective;
        }
    }
}
