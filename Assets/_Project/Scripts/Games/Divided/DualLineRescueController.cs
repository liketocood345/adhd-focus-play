using UnityEngine;

namespace ADHDTraining.Games.Divided
{
    /// <summary>
    /// 双线救援：左侧眨眼救动物，右侧点头救无人机。
    /// 参考：track-of-thought 多目标并行注意力。
    /// </summary>
    public class DualLineRescueController : MonoBehaviour
    {
        public enum Side { Left, Right }

        [SerializeField] private Core.IBciInputProvider input;
        [SerializeField] private float minEventInterval = 1.5f;
        [SerializeField] private float maxEventInterval = 4f;

        private float _nextEventAt;
        private Side _pendingSide;
        private int _score;

        private void Update()
        {
            if (input == null) return;

            if (Time.time >= _nextEventAt)
                SpawnEvent();

            if (_pendingSide == Side.Left && input.Current.Blink)
                Resolve(true);
            else if (_pendingSide == Side.Right && input.Current.Head == Core.HeadGesture.Nod)
                Resolve(true);
        }

        private void SpawnEvent()
        {
            _pendingSide = Random.value > 0.5f ? Side.Left : Side.Right;
            var focus = input.Current.Focus;
            var interval = Mathf.Lerp(maxEventInterval, minEventInterval, focus / 100f);
            _nextEventAt = Time.time + interval;
        }

        private void Resolve(bool correct)
        {
            if (correct) _score += 10;
            _nextEventAt = 0f;
        }
    }
}
