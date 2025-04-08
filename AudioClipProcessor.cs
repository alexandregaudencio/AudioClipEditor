namespace AudioClipEditor
{
    using UnityEngine;

    public static class AudioClipProcessor
    {
        public static AudioClip TrimSilence(AudioClip clip, float trimStartThreshold = 0.01f, float trimEndThreshold = 0.01f)
        {
            if (clip == null) return null;

            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            int start = 0;
            int end = samples.Length - 1;

            while (start < samples.Length && Mathf.Abs(samples[start]) < trimStartThreshold) start++;
            while (end > start && Mathf.Abs(samples[end]) < trimEndThreshold) end--;

            int newLength = end - start + 1;
            if (newLength <= 0) return null;

            float[] trimmedSamples = new float[newLength];
            System.Array.Copy(samples, start, trimmedSamples, 0, newLength);

            AudioClip newClip = AudioClip.Create(clip.name + "_trimmed", newLength / clip.channels, clip.channels, clip.frequency, false);
            newClip.SetData(trimmedSamples, 0);
            return newClip;
        }

        public static AudioClip Normalize(AudioClip clip, float targetDb = -1f)
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