using System.Collections.Generic;
using ATP.AnimationPathAnimator.ReorderableList;
using ATP.AnimationPathAnimator.APAnimatorComponent;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathAnimator.APEventsComponent {

    [CustomEditor(typeof (APEvents))]
    public class APEventsEditor : Editor {

        #region FIELDS
        #endregion
        #region PROPERTIES
        public bool SerializedPropertiesInitialized { get; set; }

        private APEvents Script { get; set; }

        private APEventsSettings Settings { get; set; }
        #endregion
        #region SERIALIZED PROPERTIES
        private SerializedProperty advancedSettingsFoldout;

        private SerializedProperty animator;
        private SerializedProperty drawMethodNames;
        private SerializedProperty nodeEvents;
        private SerializedProperty settings;
        private SerializedProperty skin;
        #endregion

        #region UNITY MESSAGES
        public override void OnInspectorGUI() {
            if (!AssetsLoaded()) {
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

        private void OnEnable() {
            Script = target as APEvents;

            if (!AssetsLoaded()) return;

            Settings = Script.Settings;

            InitializeSerializedProperties();
        }
        private void OnSceneGUI() {
        if (!AssetsLoaded()) return;

            HandleDrawingMethodNames();
        }

        private bool AssetsLoaded() {
            return (bool) Utilities.InvokeMethodWithReflection(
                Script,
                "AssetsLoaded",
                null);
        }

        #endregion
        #region INSPECTOR
        private void DisplayDrawMethodLabelsToggle() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(
                drawMethodNames,
                new GUIContent(
                    "Draw Labels",
                    ""));
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAdvancedSettingsControls() {
            if (advancedSettingsFoldout.boolValue) {
                DrawSettingsAssetField();
                DrawSkinAssetField();
            }
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

        private void DrawAnimatorField() {

            EditorGUILayout.PropertyField(
                animator,
                new GUIContent(
                    "APAnimator",
                    ""));
        }

        private void DrawInfoLabel(string text) {
            EditorGUILayout.HelpBox(text, MessageType.Error);
        }

        private void DrawReorderableEventList() {

            serializedObject.Update();

            ReorderableListGUI.Title("Events");
            ReorderableListGUI.ListField(nodeEvents);

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

        private void DrawSkinAssetField() {

            serializedObject.Update();
            EditorGUILayout.PropertyField(skin);
            serializedObject.ApplyModifiedProperties();
        }
        #endregion
        #region METHODS
        private void HandleDrawingMethodNames() {
            if (!drawMethodNames.boolValue) return;
            // Return if path data does not exist.
            if (Script.Animator.PathData == null) return;

            var methodNames = (string[])Utilities.InvokeMethodWithReflection(
                Script,
                "GetMethodNames",
                null);

            var nodePositions = (Vector3[])Utilities.InvokeMethodWithReflection(
                Script,
                "GetNodePositions",
                new object[] {methodNames.Length});

            var style = Script.Skin.GetStyle("MethodNameLabel");

            SceneHandles.DrawNodeLabels(
                nodePositions,
                methodNames,
                Settings.MethodNameLabelOffsetX,
                Settings.MethodNameLabelOffsetY,
                Settings.DefaultNodeLabelWidth,
                Settings.DefaultNodeLabelHeight,
                style);
        }

        private void InitializeSerializedProperties() {
            animator =
                serializedObject.FindProperty("animator");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");
            skin =
                serializedObject.FindProperty("skin");
            settings =
                serializedObject.FindProperty("settings");
            nodeEvents = serializedObject.FindProperty("nodeEventSlots");
            drawMethodNames =
                serializedObject.FindProperty("drawMethodNames");

            SerializedPropertiesInitialized = true;
        }
        #endregion
        //private void DrawNodeLabel(
        //    Vector3 nodePosition,
        //    string value,
        //    int offsetX,
        //    int offsetY,
        //    GUIStyle style) {

        //    // Translate node's 3d position into screen coordinates.
        //    var guiPoint = HandleUtility.WorldToGUIPoint(nodePosition);

        //    // Create rectangle for the label.
        //    var labelPosition = new Rect(
        //        guiPoint.x + offsetX,
        //        guiPoint.y + offsetY,
        //        SettingsAsset.DefaultNodeLabelWidth,
        //        SettingsAsset.DefaultNodeLabelHeight);

        //    Handles.BeginGUI();

        //    // Draw label.
        //    GUI.Label(
        //        labelPosition,
        //        value,
        //        style);

        //    Handles.EndGUI();
        //}

        //private void DrawNodeLabels(
        //    IList<Vector3> nodePositions,
        //    IList<string> textValues,
        //    int offsetX,
        //    int offsetY,
        //    GUIStyle style) {

        //    // Calculate difference between elements number in both collection.
        //    var elementsNoDelta =
        //        Mathf.Abs(nodePositions.Count - textValues.Count);
        //    // Find out which collection is bigger.
        //    var biggerCollection = (nodePositions.Count > textValues.Count)
        //        ? nodePositions.Count
        //        : textValues.Count;
        //    // Calculate biggest common index.
        //    var commonSize = biggerCollection - elementsNoDelta;

        //    for (var i = 0; i < commonSize; i++) {
        //        DrawNodeLabel(
        //            nodePositions[i],
        //            textValues[i],
        //            offsetX,
        //            offsetY,
        //            style);
        //    }
        //}
    }

}
