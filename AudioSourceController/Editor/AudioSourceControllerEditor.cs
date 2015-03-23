using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools.AudioSourceControllerComponent {

    [CustomEditor(typeof (AudioSourceController))]
    public sealed class AudioSourceControllerEditor : Editor {

        private SerializedProperty audioSource;
        private SerializedProperty animator;

        private void OnEnable() {
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

    }

}
