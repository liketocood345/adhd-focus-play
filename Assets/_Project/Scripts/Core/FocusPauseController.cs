using UnityEngine;

namespace ADHDTraining.Core
{
    public class FocusPauseController : MonoBehaviour
    {
        private IBciInputProvider _input;
        [SerializeField] private float lowFocusThreshold = 30f;
        [SerializeField] private float pauseAfterSeconds = 3f;

        public event System.Action Paused;
        public event System.Action Resumed;
        public bool IsPaused { get; private set; }

        private float _lowFocusTimer;

        public void Bind(IBciInputProvider input) => _input = input;

        private void Update()
        {
            if (_input == null) return;

            if (_input.Current.Focus < lowFocusThreshold)
                _lowFocusTimer += Time.deltaTime;
            else
                _lowFocusTimer = 0f;

            if (!IsPaused && _lowFocusTimer >= pauseAfterSeconds)
            {
                IsPaused = true;
                Paused?.Invoke();
            }
            else if (IsPaused && _input.Current.Focus >= lowFocusThreshold)
            {
                IsPaused = false;
                Resumed?.Invoke();
            }
        }
    }
}
