using UnityEngine;

namespace ADHDTraining.Games.Selective
{
    /// <summary>
    /// 听音寻宝：专注力调节目标声与干扰声音量。
    /// 报告阈值：>70 清晰，40-70 相当，&lt;40 淹没。
    /// </summary>
    public class SoundTreasureHuntController : MonoBehaviour
    {
        [SerializeField] private Core.IBciInputProvider input;
        [SerializeField] private AudioSource targetSource;
        [SerializeField] private AudioSource distractorSource;
        [SerializeField] private float roundDuration = 45f;

        private int _coins;
        private float _timer;

        private void Update()
        {
            if (input == null) return;

            var focus = input.Current.Focus;
            ApplyMix(focus);

            if (input.Current.Blink)
                OnPlayerBlink();

            _timer += Time.deltaTime;
        }

        private void ApplyMix(float focus)
        {
            if (targetSource == null || distractorSource == null) return;

            if (focus > 70f)
            {
                targetSource.volume = 1f;
                distractorSource.volume = 0.2f;
            }
            else if (focus >= 40f)
            {
                targetSource.volume = 0.6f;
                distractorSource.volume = 0.6f;
            }
            else
            {
                targetSource.volume = 0.15f;
                distractorSource.volume = 1f;
            }
        }

        private void OnPlayerBlink()
        {
            // TODO: 判定当前是否为目标声时刻
            _coins++;
        }
    }
}
