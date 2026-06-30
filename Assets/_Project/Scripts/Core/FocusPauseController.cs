using System;
using UnityEngine;

namespace ADHDTraining.Core
{
    /// <summary>
    /// 报告通用规则：专注力持续低于 30 超过阈值秒数则暂停。
    /// </summary>
    public class FocusPauseController : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour inputProviderBehaviour;

        private IBciInputProvider InputProvider =>
            inputProviderBehaviour as IBciInputProvider;
        [SerializeField] private float lowFocusThreshold = 30f;
        [SerializeField] private float pauseAfterSeconds = 3f;

        public event Action Paused;
        public event Action Resumed;

        public bool IsPaused { get; private set; }

        private float _lowFocusTimer;

        private void Update()
        {
            if (InputProvider == null) return;

            if (InputProvider.Current.Focus < lowFocusThreshold)
                _lowFocusTimer += Time.deltaTime;
            else
                _lowFocusTimer = 0f;

            if (!IsPaused && _lowFocusTimer >= pauseAfterSeconds)
            {
                IsPaused = true;
                Paused?.Invoke();
            }
            else if (IsPaused && InputProvider.Current.Focus >= lowFocusThreshold)
            {
                IsPaused = false;
                Resumed?.Invoke();
            }
        }
    }
}
