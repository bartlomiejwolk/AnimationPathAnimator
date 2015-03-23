using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools.AudioSourceControllerComponent {

    [CustomEditor(typeof (AudioSourceController))]
    public sealed class AudioSourceControllerEditor : Editor {

        private SerializedProperty audioSource;

        private void OnEnable() {
            audioSource = serializedObject.FindProperty("audioSource");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                audioSource,
                new GUIContent(
                    "Audio Source",
                    ""));

            serializedObject.ApplyModifiedProperties();
        }

    }

}
