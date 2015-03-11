using System.Collections.Generic;
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
        private SerializedProperty drawMethodNames;

        private void OnEnable() {
            Script = target as APEventsReflection;

            InitializeSerializedProperties();
        }

        private void InitializeSerializedProperties() {
            apAnimator =
                serializedObject.FindProperty("apAnimator");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");
            skin =
                serializedObject.FindProperty("skin");
            settings =
                serializedObject.FindProperty("settings");
            nodeEvents = serializedObject.FindProperty("nodeEvents");
            drawMethodNames =
                serializedObject.FindProperty("drawMethodNames");

            SerializedPropertiesInitialized = true;
        }

        public bool SerializedPropertiesInitialized { get; set; }

        public override void OnInspectorGUI() {
            if (!Script.AssetsLoaded()) {
                DrawInfoLabel(
                    //"Asset files in extension folder were not found. "
                    "Required assets were not found.\n"
                    + "Reset component and if it does not help, restore extension "
                    + "folder content to its default state.");
                return;
            }
            if (!SerializedPropertiesInitialized) return;

            DrawAnimatorField();

            EditorGUILayout.Space();

            DisplayDrawMethodLabelsToggle();

            DrawReorderableEventList();

            DrawAdvancedSettingsFoldout();
            DrawAdvancedSettingsControls();
        }

        private void DisplayDrawMethodLabelsToggle() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(
                drawMethodNames,
                new GUIContent(
                    "Draw Labels",
                    ""));
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI() {
            if (!Script.AssetsLoaded()) return;

            HandleDrawingMethodNames();
        }

        private void HandleDrawingMethodNames() {
            if (!drawMethodNames.boolValue) return;

            var nodePositions = Script.GetNodePositions();
            var methodNames = Script.GetMethodNames();
            var style = Script.Skin.GetStyle("MethodNameLabel");

            DrawNodeLabels(
                nodePositions,
                methodNames,
                // TODO Get value from settings file.
                20,
                20,
                style);
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
                DrawSkinAssetField();
            }
        }

        private void DrawSkinAssetField() {

            serializedObject.Update();
            EditorGUILayout.PropertyField(skin);
            serializedObject.ApplyModifiedProperties();
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

        private void DrawNodeLabel(
            Vector3 nodePosition,
            string value,
            int offsetX,
            int offsetY,
            GUIStyle style) {

            // Translate node's 3d position into screen coordinates.
            var guiPoint = HandleUtility.WorldToGUIPoint(nodePosition);

            // Create rectangle for the label.
            var labelPosition = new Rect(
                guiPoint.x + offsetX,
                guiPoint.y + offsetY,
                // TODO Get default node width and height from settings file.
                100,
                30);

            Handles.BeginGUI();

            // Draw label.
            GUI.Label(
                labelPosition,
                value,
                style);

            Handles.EndGUI();
        }

        private void DrawNodeLabels(
            IList<Vector3> nodePositions,
            IList<string> textValues,
            int offsetX,
            int offsetY,
            GUIStyle style) {

            // Calculate difference between elements number in both collection.
            var elementsNoDelta =
                Mathf.Abs(nodePositions.Count - textValues.Count);
            // Find out which collection is bigger.
            var biggerCollection = (nodePositions.Count > textValues.Count)
                ? nodePositions.Count
                : textValues.Count;
            // Calculate biggest common index.
            var commonSize = biggerCollection - elementsNoDelta;

            for (var i = 0; i < commonSize; i++) {
                DrawNodeLabel(
                    nodePositions[i],
                    textValues[i],
                    offsetX,
                    offsetY,
                    style);
            }
        }

        private void DrawInfoLabel(string text) {
            EditorGUILayout.HelpBox(text, MessageType.Error);
        }

    }

}
