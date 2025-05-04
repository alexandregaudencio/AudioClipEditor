namespace AudioClipEditor
{
    using UnityEngine;

    public static class AudioClipProcessor
    {
        public static AudioClip TrimAudioClip(AudioClip clip, float startCut, float endCut)
        {
            int channels = clip.channels;
            int frequency = clip.frequency;
            int startSample = Mathf.FloorToInt(startCut * frequency);
            int endSample = Mathf.FloorToInt((clip.length - endCut) * frequency);
            int samples = endSample - startSample;

            if (samples <= 0)
                return null;
            float[] data = new float[clip.samples * channels];
            clip.GetData(data, 0);
            float[] trimmedData = new float[samples * channels];
            System.Array.Copy(data, startSample * channels, trimmedData, 0, trimmedData.Length);
            AudioClip trimmedAudioClip = AudioClip.Create(clip.name, samples, channels, frequency, false);
            trimmedAudioClip.SetData(trimmedData, 0);
            return trimmedAudioClip;
        }

        public static AudioClip ModifyGain(AudioClip clip, float targetDb = -3f)
        {
            if (clip == null) return null;

            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            float max = 0f;
            foreach (var s in samples)
                max = Mathf.Max(max, Mathf.Abs(s));
            if (max == 0f) return clip;

            float currentDb = 20f * Mathf.Log10(max);
            float gainDb = targetDb - currentDb;
            float gain = Mathf.Pow(10f, gainDb / 20f);

            for (int i = 0; i < samples.Length; i++)
                samples[i] *= gain;

            AudioClip newClip = AudioClip.Create(clip.name + "_normalized", clip.samples, clip.channels, clip.frequency, false);
            newClip.SetData(samples, 0);
            return newClip;
        }


    }
}