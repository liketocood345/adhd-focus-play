using UnityEngine;

namespace ADHDTraining.Games.Shifting
{
    /// <summary>
    /// 指令反转：显示指令后做相反动作，点头确认/摇头放弃。
    /// 专注力控制指令间隔。参考：StroopApp 任务切换范式。
    /// </summary>
    public class ReverseCommandController : MonoBehaviour
    {
        [SerializeField] private Core.IBciInputProvider input;
        [SerializeField] private float minInterval = 2f;
        [SerializeField] private float maxInterval = 5f;

        private float _nextCommandAt;
        private int _score;

        private void Update()
        {
            if (input == null || Time.time < _nextCommandAt) return;

            SpawnCommand();
            ScheduleNext();
        }

        private void SpawnCommand()
        {
            // TODO: 随机指令 + UI/语音
        }

        private void ScheduleNext()
        {
            var focus = input.Current.Focus;
            var t = Mathf.Lerp(maxInterval, minInterval, focus / 100f);
            _nextCommandAt = Time.time + t;
        }

        public void OnGestureConfirmed(bool correct)
        {
            if (correct) _score += 10;
        }
    }
}
