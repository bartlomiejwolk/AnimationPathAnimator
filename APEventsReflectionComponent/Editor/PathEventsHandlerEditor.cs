using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathAnimator.PathEventsHandlerComponent {

    [CustomEditor(typeof (APEventsReflection))]
    public class PathEventsHandlerEditor : Editor {

        private APEventsReflection Script { get; set; }

        private SerializedProperty pathAnimator;
        private SerializedProperty advancedSettingsFoldout;
        private SerializedProperty skin;
        private SerializedProperty settings;

        private void OnEnable() {
            Script = target as APEventsReflection;

            pathAnimator =
                serializedObject.FindProperty("APAnimator");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");
            skin =
                serializedObject.FindProperty("skin");
            settings =
                serializedObject.FindProperty("messageSettings");
        }

        public override void OnInspectorGUI() {
            // TODO Extract method.
            EditorGUILayout.PropertyField(
                pathAnimator,
                new GUIContent(
                    "APAnimator",
                    ""));

            DrawAdvancedSettingsFoldout();
            DrawAdvancedSettingsControls();
        }

        private void DrawAdvancedSettingsFoldout() {
            serializedObject.Update();

            advancedSettingsFoldout.boolValue = EditorGUILayout.Foldout(
                advancedSettingsFoldout.boolValue,
                new GUIContent(
                    "Advanced messageSettings",
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
                    "messageSettings Asset",
                    ""));

            serializedObject.ApplyModifiedProperties();
        }

    }

}
