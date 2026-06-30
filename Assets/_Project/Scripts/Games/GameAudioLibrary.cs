using UnityEngine;

namespace ADHDTraining.Games
{
    /// <summary>
    /// 运行时程序化音效；开源仓库音频需先 Import Open Source Art 导入。
    /// </summary>
    public static class GameAudioLibrary
    {
        private static AudioClip _correct;
        private static AudioClip _wrong;
        private static AudioClip _tick;
        private static AudioClip _success;

        public static AudioClip Correct => _correct ??= ProceduralToneUtility.CreateChime("sfx_correct", 880f, 0.18f);
        public static AudioClip Wrong => _wrong ??= ProceduralToneUtility.CreateChime("sfx_wrong", 180f, 0.28f);
        public static AudioClip Tick => _tick ??= ProceduralToneUtility.CreateTone("sfx_tick", 660f, 0.06f);
        public static AudioClip Success => _success ??= ProceduralToneUtility.CreateChime("sfx_success", 523f, 0.35f);

        public static AudioSource Ensure2D(Transform parent, string name = "GameAudio")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            return src;
        }

        public static void PlayOneShot(Transform parent, AudioClip clip, float volume = 0.55f)
        {
            if (clip == null || parent == null) return;
            var src = Ensure2D(parent);
            src.volume = volume;
            src.PlayOneShot(clip);
            Object.Destroy(src.gameObject, clip.length + 0.1f);
        }
    }
}
