using System.Collections.Generic;
using ATP.SimplePathAnimator.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace ATP.SimplePathAnimator.Events {

    [CustomEditor(typeof (PathEvents))]
    public class PathEventsEditor : Editor {
        #region FIELDS


        #endregion

        #region PROPERTIES

        private SerializedObject EventsDataSerObj { get; set; }

        private PathEvents Script { get; set; }

        private PathEventsSettings PathEventsSettings;

        #endregion

        #region SERIALIZED PROPERTIES

        private SerializedProperty pathAnimator;

        private SerializedProperty drawMethodNames;

        private SerializedProperty nodeEvents;

        private SerializedProperty skin;
        private SerializedProperty settings;
        #endregion

        #region UNITY MESSAGES
        public override void OnInspectorGUI() {
            InitializeNodeEvents();

            serializedObject.Update();

            DrawEventsDataAssetField();

            EditorGUILayout.BeginHorizontal();

            DrawCreateEventsDataAssetButton();
            DrawResetEventsDataInspectorButton();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(pathAnimator);
            DrawSettingsAssetField();
            EditorGUILayout.PropertyField(skin);

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            DrawEventList();

        }

        private void InitializeNodeEvents() {
            if (Event.current.type != EventType.Layout) return;

            if (Script.EventsData != null && EventsDataSerObj == null) {
                // Initialize SerializedObject EventsDataSerObj.
                EventsDataSerObj = new SerializedObject(Script.EventsData);
                // Iniatilize SerializedProperty nodeEvents.
                nodeEvents = EventsDataSerObj.FindProperty("nodeEvents");
            }
        }

        private void DrawEventList() {
            if (nodeEvents == null) return;

            EventsDataSerObj.Update();

            ReorderableListGUI.Title("Events");
            ReorderableListGUI.ListField(nodeEvents);

            EventsDataSerObj.ApplyModifiedProperties();
        }

        private void OnEnable() {
            Script = (PathEvents) target;
            PathEventsSettings = Script.Settings;

            if (EventsDataSerObj != null) {
                nodeEvents = EventsDataSerObj.FindProperty("nodeEvents");
            }

            pathAnimator = serializedObject.FindProperty("pathAnimator");
            drawMethodNames = serializedObject.FindProperty("drawMethodNames");
            skin = serializedObject.FindProperty("skin");
            settings = serializedObject.FindProperty("settings");
        }

        private void OnSceneGUI() {
            // Return if path data is not assigned to the PathAnimator component.
            if (Script.PathAnimator.PathData == null) return;
            if (Script.EventsData == null) return;

            // TODO Guard against null Skin.
            HandleDrawingMethodNames();
        }

        #endregion
        #region INSPECTOR
        private void DrawSettingsAssetField() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                settings,
                new GUIContent(
                    "Settings Asset",
                    ""));

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region METHODS

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
                PathEventsSettings.DefaultNodeLabelWidth,
                PathEventsSettings.DefaultNodeLabelHeight);

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

            for (var i = 0; i < textValues.Count; i++) {
                DrawNodeLabel(
                    nodePositions[i],
                    textValues[i],
                    offsetX,
                    offsetY,
                    style);
            }
        }

        private void HandleDrawingMethodNames() {
            if (!drawMethodNames.boolValue) return;

            var nodePositions = Script.GetNodePositions();
            var methodNames = Script.GetMethodNames();
            var style = Script.Skin.GetStyle("MethodNameLabel");

            DrawNodeLabels(
                nodePositions,
                methodNames,
                PathEventsSettings.MethodNameLabelOffsetX,
                PathEventsSettings.MethodNameLabelOffsetY,
                style);
        }

        protected virtual void DrawEventsDataAssetField() {
            //serializedObject.Update();

            //EditorGUILayout.PropertyField(
            //    eventsData,
            //    new GUIContent(
            //        "Events Data",
            //        ""));

            //serializedObject.ApplyModifiedProperties();

            Script.EventsData = (PathEventsData) EditorGUILayout.ObjectField(
                new GUIContent(
                    "Events Asset",
                    ""),
                Script.EventsData,
                typeof(PathEventsData),
                false);
        }

        private void DrawCreateEventsDataAssetButton() {
            if (GUILayout.Button(
                new GUIContent(
                    "New Events List",
                    ""))) {

                // Display save panel.
                var savePath = EditorUtility.SaveFilePanelInProject(
                    "Save Events List Asset File",
                    // TODO Make it a property.
                    "EventsList",
                    "asset",
                    "");

                // Path cannot be empty.
                if (savePath == "") return;

                // Create new path asset.
                var asset =
                    ScriptableObjectUtility.CreateAsset<PathEventsData>(
                    savePath);

                // Assign asset as the current path.
                Script.EventsData = asset;
            }
        }

        private void DrawResetEventsDataInspectorButton() {
            if (GUILayout.Button(
                new GUIContent(
                    "Reset Events",
                    "Reset path to default."))) {

                if (Script.EventsData == null) return;

                // Allow undo this operation.
                Undo.RecordObject(Script.EventsData, "Change events data.");

                // Reset curves to its default state.
                Script.EventsData.ResetEvents();
            }
        }

        #endregion
    }

}