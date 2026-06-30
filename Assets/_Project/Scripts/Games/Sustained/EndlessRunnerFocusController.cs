using ADHDTraining.Core;
using UnityEngine;

namespace ADHDTraining.Games.Sustained
{
    /// <summary>
    /// 无尽跑酷者：专注力控制速度与得分倍率，头动变道。
    /// </summary>
    public class EndlessRunnerFocusController : MonoBehaviour
    {
        [SerializeField] private BciInputRouter input;
        [SerializeField] private Transform player;
        [SerializeField] private float baseSpeed = 5f;
        [SerializeField] private float laneWidth = 2f;

        private int _lane; // -1, 0, 1
        private float _highFocusStreak;

        private void Update()
        {
            if (input == null || player == null) return;

            var focus = input.Current.Focus;
            var speedMultiplier = Mathf.Lerp(0.4f, 1.5f, focus / 100f);
            player.position += Vector3.forward * (baseSpeed * speedMultiplier * Time.deltaTime);

            if (focus > 70f) _highFocusStreak += Time.deltaTime;
            else _highFocusStreak = 0f;

            if (input.Current.Head == HeadGesture.TurnLeft && _lane > -1) _lane--;
            if (input.Current.Head == HeadGesture.TurnRight && _lane < 1) _lane++;

            var targetX = _lane * laneWidth;
            var pos = player.position;
            pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * 8f);
            player.position = pos;
        }
    }
}
