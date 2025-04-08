using UnityEngine;

public static class AudioWaveformGenerator
{
    public static Texture2D GenerateWaveform(AudioClip clip, int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.SetPixels(new Color[width * height]); // clear

        if (clip == null) return tex;

        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        int step = Mathf.Max(1, samples.Length / width);
        for (int x = 0; x < width; x++)
        {
            int start = x * step;
            float max = 0f;
            for (int i = 0; i < step && (start + i) < samples.Length; i++)
                max = Mathf.Max(max, Mathf.Abs(samples[start + i]));

            int y = Mathf.RoundToInt(max * (height / 2));
            for (int j = 0; j < y; j++)
            {
                tex.SetPixel(x, (height / 2) + j, color);
                tex.SetPixel(x, (height / 2) - j, color);
            }
        }

        tex.Apply();
        return tex;
    }
}
