using UnityEngine;

namespace ADHDTraining.Games.Inhibition
{
    /// <summary>
    /// 红灯停绿灯行：晶石眨眼收集，陨石抑制眨眼。
    /// 参考开源：response-inhibition-game-unity-project / DoggoNogo
    /// </summary>
    public class FallingObjectInhibitionController : MonoBehaviour
    {
        public enum FallingType { Crystal, Meteor }

        [SerializeField] private Core.IBciInputProvider input;
        [SerializeField] private float baseFallSpeed = 3f;

        private int _score;
        private int _laserUsesLeft = 3;

        public void OnObjectReachedPlayer(FallingType type)
        {
            if (input == null) return;

            if (type == FallingType.Crystal)
            {
                if (input.Current.Blink) _score += 10;
                // else: 漏掉，播放加油提示
            }
            else if (type == FallingType.Meteor && input.Current.Blink)
            {
                _score -= 5;
            }
        }

        public float GetFallSpeed()
        {
            if (input == null) return baseFallSpeed;
            return baseFallSpeed * Mathf.Lerp(0.5f, 1.5f, input.Current.Focus / 100f);
        }

        public void TryLaserClear()
        {
            if (input == null || _laserUsesLeft <= 0) return;
            if (input.Current.Head == Core.HeadGesture.Shake)
                _laserUsesLeft--;
        }
    }
}
