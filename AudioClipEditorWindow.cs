using System.IO;
using UnityEditor;
using UnityEngine;
namespace AudioClipEditor
{
    public class AudioClipEditorWindow : EditorWindow
    {
        private AudioClip targetClip;
        private float trimStart = 0.01f;
        private bool normalizeVolume = true;
        private AudioClip previewClip;
        private AudioSource previewSource;

        private Texture2D originalWaveformTex;
        private Texture2D previewWaveformTex;
        private const int waveformWidth = 512;
        private const int waveformHeight = 60;


        [MenuItem("Tools/AudioClip Processor")]
        public static void ShowWindow()
        {
            GetWindow<AudioClipEditorWindow>("AudioClip Processor");
        }

        void OnGUI()
        {
            GUILayout.Space(10);
            var newTargetClip = (AudioClip)EditorGUILayout.ObjectField("AudioClip", targetClip, typeof(AudioClip), false);
            if (newTargetClip != targetClip)
            {
                targetClip = newTargetClip;
                trimStart = 0;
            }
            normalizeVolume = EditorGUILayout.Toggle("Normalize", normalizeVolume);

            GUILayout.Space(10);
            GUILayout.Label("Trim", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            trimStart = EditorGUILayout.Slider("start", trimStart, 0f, 0.99f);
            GUILayout.Label("%");
            GUILayout.EndHorizontal();

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

            GUILayout.Space(10);
            EditorGUI.BeginDisabledGroup(targetClip == null);
            {
                GUILayout.BeginHorizontal();

                GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f); // verde claro
                if (GUILayout.Button("Play", GUILayout.Height(25)))
                    PlayProcessedClip();

                GUI.backgroundColor = new Color(1f, 0.3f, 0.3f); // vermelho claro
                if (GUILayout.Button("Stop", GUILayout.Height(25)))
                    StopPreview();

                GUI.backgroundColor = Color.white;

                GUILayout.EndHorizontal();
            }
            EditorGUI.EndDisabledGroup();


            GUILayout.Space(10);
            EditorGUI.BeginDisabledGroup(targetClip == null);
            {
                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = originalColor * 1.2f;
                GUIStyle boldButton = new GUIStyle(GUI.skin.button);
                boldButton.fontStyle = FontStyle.Bold;
                if (GUILayout.Button(" Save Edited clip", boldButton, GUILayout.Height(30)))
                {
                    SaveProcessedClipOverriding();
                    trimStart = 0;
                }

                GUI.backgroundColor = originalColor;
            }
            EditorGUI.EndDisabledGroup();



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
            if (targetClip == null) return;
            previewClip = AudioClipProcessor.TrimStart(targetClip, trimStart);
            if (normalizeVolume)
                previewClip = AudioClipProcessor.Normalize(previewClip);
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


        //Unity does not provide a way to export audioClip as .pm3 or .ogg, just .wav
        void SaveProcessedClip()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Processed Clip", targetClip.name, "wav", "Save processed clip");
            if (string.IsNullOrEmpty(path)) return;


            byte[] wavData = WavUtility.FromAudioClip(previewClip, out _, false);
            File.WriteAllBytes(Path.Combine(Application.dataPath, path.Replace("Assets/", "")), wavData);
            AssetDatabase.Refresh();

        }

        void SaveProcessedClipOverriding()
        {
            string assetPath = AssetDatabase.GetAssetPath(targetClip);
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);

            byte[] wavData = WavUtility.FromAudioClip(previewClip, out _, false);
            File.WriteAllBytes(fullPath, wavData);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.Default);
            AssetDatabase.Refresh();
        }

    }
}
