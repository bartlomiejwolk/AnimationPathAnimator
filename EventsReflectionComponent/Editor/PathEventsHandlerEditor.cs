using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathAnimator.PathEventsHandlerComponent {

    [CustomEditor(typeof (PathEventsHandler))]
    public class PathEventsHandlerEditor : Editor {

        private PathEventsHandler Script { get; set; }

        private SerializedProperty pathAnimator;
        private SerializedProperty advancedSettingsFoldout;
        private SerializedProperty skin;
        private SerializedProperty settings;

        private void OnEnable() {
            Script = target as PathEventsHandler;

            pathAnimator =
                serializedObject.FindProperty("pathAnimator");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");
            skin =
                serializedObject.FindProperty("skin");
            settings =
                serializedObject.FindProperty("settings");
        }

        public override void OnInspectorGUI() {
            // TODO Extract method.
            EditorGUILayout.PropertyField(
                pathAnimator,
                new GUIContent(
                    "Animator",
                    ""));

            DrawAdvancedSettingsFoldout();
            DrawAdvancedSettingsControls();
        }

        private void DrawAdvancedSettingsFoldout() {
            serializedObject.Update();

            advancedSettingsFoldout.boolValue = EditorGUILayout.Foldout(
                advancedSettingsFoldout.boolValue,
                new GUIContent(
                    "Advanced Settings",
                    ""));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAdvancedSettingsControls() {
            if (advancedSettingsFoldout.boolValue) {
                DrawSettingsAssetField();
                EditorGUILayout.PropertyField(skin);
            }
        }

        private void DrawSettingsAssetField() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                settings,
                new GUIContent(
                    "Settings Asset",
                    ""));

            serializedObject.ApplyModifiedProperties();
        }

    }

}
