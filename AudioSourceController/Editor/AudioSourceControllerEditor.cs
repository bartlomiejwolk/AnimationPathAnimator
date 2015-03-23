using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools.AudioSourceControllerComponent {

    [CustomEditor(typeof (AudioSourceController))]
    public sealed class AudioSourceControllerEditor : Editor {

        private AudioSourceController Script { get; set; }

        private SerializedProperty audioSource;
        private SerializedProperty animator;

        private void OnEnable() {
            Script = (AudioSourceController) target;

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
                && Event.current.keyCode == AudioSourceController.PlayPauseKey) {

                Utilities.InvokeMethodWithReflection(
                    Script,
                    "HandlePlayPause",
                    null);
            }
        }

    }

}
