using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools.AudioSynchronizerComponent {

    [CustomEditor(typeof (AudioSynchronizer))]
    public sealed class AudioSynchronizerEditor : Editor {

        private AudioSynchronizer Script { get; set; }

        private SerializedProperty audioSource;
        private SerializedProperty animator;
        private SerializedProperty autoPlay;
        private SerializedProperty autoPlayDelay;

        private void OnEnable() {
            Script = (AudioSynchronizer) target;

            audioSource = serializedObject.FindProperty("audioSource");
            animator = serializedObject.FindProperty("animator");
            autoPlay = serializedObject.FindProperty("autoPlay");
            autoPlayDelay = serializedObject.FindProperty("autoPlayDelay");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                audioSource,
                new GUIContent(
                    "Audio Source",
                    "AudioSource component reference."));

            EditorGUILayout.PropertyField(
                animator,
                new GUIContent(
                    "Animator",
                    "Animator component reference."));

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(
                autoPlay,
                new GUIContent(
                    "Auto Play",
                    "Play on enter game mode."));

            EditorGUIUtility.labelWidth = 50;

            EditorGUILayout.PropertyField(
                autoPlayDelay,
                new GUIContent(
                    "Delay",
                    "Auto play delay."));

            EditorGUIUtility.labelWidth = 00;

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI() {
            HandlePlayPauseShortcut();
        }

        private void HandlePlayPauseShortcut() {
            if (Event.current.type == EventType.KeyDown
                && Event.current.keyCode == AudioSynchronizer.PlayPauseKey) {

                Utilities.InvokeMethodWithReflection(
                    Script,
                    "HandlePlayPause",
                    null);
            }
        }

    }

}
