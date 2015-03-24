using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools.AudioSynchronizerComponent {

    [CustomEditor(typeof (AudioSynchronizer))]
    public sealed class AudioSynchronizerEditor : Editor {

        private AudioSynchronizer Script { get; set; }

        private SerializedProperty audioSource;
        private SerializedProperty animator;

        private void OnEnable() {
            Script = (AudioSynchronizer) target;

            audioSource = serializedObject.FindProperty("audioSource");
            animator = serializedObject.FindProperty("animator");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                audioSource,
                new GUIContent(
                    "Audio Source",
                    ""));

            EditorGUILayout.PropertyField(
                animator,
                new GUIContent(
                    "Animator",
                    ""));

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
