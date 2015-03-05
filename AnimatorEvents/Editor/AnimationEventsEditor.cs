﻿using System.Collections.Generic;
using ATP.SimplePathAnimator.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace ATP.SimplePathAnimator.PathEvents {

    [CustomEditor(typeof (AnimatorEvents))]
    public class AnimationEventsEditor : Editor {
        #region FIELDS


        #endregion

        #region PROPERTIES

        public float DefaultNodeLabelHeight {
            get { return 30; }
        }

        public float DefaultNodeLabelWidth {
            get { return 100; }
        }

        public int MethodNameLabelOffsetX {
            get { return 30; }
        }

        public int MethodNameLabelOffsetY {
            get { return -20; }
        }

        private AnimatorEvents Script { get; set; }

        #endregion

        #region SERIALIZED PROPERTIES

        private SerializedProperty pathAnimator;

        private SerializedProperty drawMethodNames;

        private SerializedProperty nodeEvents;

        private SerializedProperty skin;
        private SerializedProperty eventsData;
        #endregion

        #region UNITY MESSAGES
        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawEventsDataAssetField();

            EditorGUILayout.BeginHorizontal();

            DrawCreateEventsDataAssetButton();
            DrawResetEventsDataInspectorButton();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(pathAnimator);
            EditorGUILayout.PropertyField(skin);

            ReorderableListGUI.Title("Events");
            ReorderableListGUI.ListField(nodeEvents);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable() {
            Script = (AnimatorEvents) target;

            nodeEvents = serializedObject.FindProperty("nodeEvents");
            pathAnimator = serializedObject.FindProperty("pathAnimator");
            drawMethodNames = serializedObject.FindProperty("drawMethodNames");
            skin = serializedObject.FindProperty("skin");
            eventsData = serializedObject.FindProperty("eventsData");
        }

        private void OnSceneGUI() {
            if (Script.EventsData == null) return;

            // TODO Guard against null Skin.
            HandleDrawingMethodNames();
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
                DefaultNodeLabelWidth,
                DefaultNodeLabelHeight);

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
                MethodNameLabelOffsetX,
                MethodNameLabelOffsetY,
                style);
        }

        protected virtual void DrawEventsDataAssetField() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                eventsData,
                new GUIContent(
                    "Events Data",
                    ""));

            serializedObject.ApplyModifiedProperties();
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
                    ScriptableObjectUtility.CreateAsset<AnimatorEventsData>(
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