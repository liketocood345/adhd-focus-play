using UnityEngine;

namespace ADHDTraining.Games
{
    public static class ProceduralToneUtility
    {
        public static AudioClip CreateTone(string name, float frequency, float durationSec = 1f, int sampleRate = 44100)
        {
            var samples = Mathf.CeilToInt(sampleRate * durationSec);
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)sampleRate;
                var env = Mathf.Clamp01(1f - t / durationSec);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.35f * env;
            }
            var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateChime(string name, float frequency, float durationSec = 0.25f, int sampleRate = 44100)
        {
            var samples = Mathf.CeilToInt(sampleRate * durationSec);
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)sampleRate;
                var env = Mathf.Clamp01(1f - t / durationSec);
                var tone = Mathf.Sin(2f * Mathf.PI * frequency * t)
                           + 0.35f * Mathf.Sin(2f * Mathf.PI * frequency * 2f * t);
                data[i] = tone * 0.22f * env * env;
            }
            var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
