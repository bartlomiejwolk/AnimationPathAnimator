using ATP.AnimationPathAnimator.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathAnimator.APEventsReflectionComponent {

    [CustomEditor(typeof (APEventsReflection))]
    public class APEventsReflectionEditor : Editor {

        private APEventsReflection Script { get; set; }

        private SerializedProperty apAnimator;
        private SerializedProperty advancedSettingsFoldout;
        private SerializedProperty skin;
        private SerializedProperty settings;
        private SerializedProperty nodeEvents;

        private void OnEnable() {
            Script = target as APEventsReflection;

            apAnimator =
                serializedObject.FindProperty("apAnimator");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");
            skin =
                serializedObject.FindProperty("skin");
            settings =
                serializedObject.FindProperty("settings");
            nodeEvents = serializedObject.FindProperty("nodeEvents");
        }

        public override void OnInspectorGUI() {
            DrawAnimatorField();

            DrawReorderableEventList();

            DrawAdvancedSettingsFoldout();
            DrawAdvancedSettingsControls();
        }

        private void DrawAnimatorField() {

            EditorGUILayout.PropertyField(
                apAnimator,
                new GUIContent(
                    "APAnimator",
                    ""));
        }

        private void DrawReorderableEventList() {

            serializedObject.Update();

            ReorderableListGUI.Title("Events");
            ReorderableListGUI.ListField(nodeEvents);

            serializedObject.ApplyModifiedProperties();
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
