using System.IO;
using UnityEditor;
using UnityEngine;

public class AudioClipEditorWindow : EditorWindow
{
    private AudioClip targetClip;
    private float silenceThreshold = 0.01f;
    private bool normalizeVolume = true;
    private AudioClip previewClip;
    private AudioSource previewSource;

    private Texture2D originalWaveformTex;
    private Texture2D previewWaveformTex;
    private const int waveformWidth = 512;
    private const int waveformHeight = 100;


    [MenuItem("Tools/AudioClip Processor")]
    public static void ShowWindow()
    {
        GetWindow<AudioClipEditorWindow>("AudioClip Processor");
    }

    void OnGUI()
    {
        GUILayout.Label("AudioClip Processor", EditorStyles.boldLabel);

        targetClip = (AudioClip)EditorGUILayout.ObjectField("AudioClip", targetClip, typeof(AudioClip), false);
        silenceThreshold = EditorGUILayout.Slider("Silence Threshold", silenceThreshold, 0f, 0.1f);
        normalizeVolume = EditorGUILayout.Toggle("Normalize Volume", normalizeVolume);

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Play Preview", GUILayout.ExpandWidth(true)))
            if (targetClip != null) PlayProcessedClip();
        if (GUILayout.Button("Stop Preview", GUILayout.ExpandWidth(true)))
            StopPreview();
        GUILayout.EndHorizontal();


        GUILayout.Space(10);
        if (GUILayout.Button("Save Processed Clip") && targetClip != null)
            SaveProcessedClip();


        PreviewClip();
        originalWaveformTex = AudioWaveformGenerator.GenerateWaveform(targetClip, waveformWidth, waveformHeight, Color.green);
        previewWaveformTex = AudioWaveformGenerator.GenerateWaveform(previewClip, waveformWidth, waveformHeight, Color.yellow);

        if (originalWaveformTex != null && previewWaveformTex != null)
        {
            GUILayout.Space(10);
            DrawWaveformPreview(originalWaveformTex, "Original");

            GUILayout.Space(4);
            DrawWaveformPreview(previewWaveformTex, "Processed");
        }


    }
    void DrawWaveformPreview(Texture2D tex, string label)
    {
        GUILayout.Label(label, EditorStyles.boldLabel);
        Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(waveformHeight));
        EditorGUI.DrawRect(r, new Color(0.15f, 0.15f, 0.15f));
        GUI.DrawTexture(r, tex, ScaleMode.StretchToFill);
    }




    void PreviewClip()
    {
        Debug.Log("preview updated");
        previewClip = AudioClipProcessor.TrimSilence(targetClip, silenceThreshold);
        if (normalizeVolume && previewClip != null)
            previewClip = AudioClipProcessor.Normalize(previewClip);

        if (previewClip == null)
        {
            Debug.LogWarning("Nada pra reproduzir.");
            return;
        }





    }

    void PlayProcessedClip()
    {
        if (previewSource == null)
        {
            GameObject go = new GameObject("PreviewAudioSource");
            previewSource = go.AddComponent<AudioSource>();
            go.hideFlags = HideFlags.HideAndDontSave;
        }
        previewSource.clip = previewClip;

        previewSource.Play();
    }

    void StopPreview()
    {
        if (previewSource != null && previewSource.isPlaying)
            previewSource.Stop();
    }

    void SaveProcessedClip()
    {
        string path = EditorUtility.SaveFilePanel("Save Processed Clip (.ogg)", Application.dataPath, targetClip.name + "_processed", "ogg");
        if (string.IsNullOrEmpty(path)) return;

        AudioClip processed = AudioClipProcessor.TrimSilence(targetClip, silenceThreshold);
        if (normalizeVolume && processed != null)
            processed = AudioClipProcessor.Normalize(processed);

        if (processed == null)
        {
            Debug.LogWarning("Audio inválido.");
            return;
        }

        // Salva WAV temporário
        string tempWav = Path.Combine(Application.temporaryCachePath, "temp_audio.wav");
        byte[] wavData = WavUtility.FromAudioClip(processed, out _, false);
        File.WriteAllBytes(tempWav, wavData);

        // Converte para .ogg
        string ffmpegArgs = $"-y -i \"{tempWav}\" -c:a libvorbis \"{path}\"";
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = ffmpegArgs,
            CreateNoWindow = true,
            UseShellExecute = false
        });

        Debug.Log("Salvo como .ogg: " + path);
    }

}
