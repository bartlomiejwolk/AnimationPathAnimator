#define DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using ATP.LoggingTools;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools.AnimatorComponent {

    /// <summary>
    ///     Editor class responsible for drawing inspector and on-scene handles. All editor related functionality is defined
    ///     here.
    /// </summary>
    [CustomEditor(typeof (AnimationPathAnimator))]
    public sealed class AnimationPathAnimatorEditor : Editor {
        #region EXPORTER

        public void DrawExportControls() {
            EditorGUILayout.BeginHorizontal();

            Script.ExportSamplingFrequency =
                EditorGUILayout.IntField(
                    new GUIContent(
                        "Export Sampling",
                        "Number of points to export for 1 m of the animation "
                        + "path. If set to 0, it'll export only keys defined in "
                        + "the path."),
                    Script.ExportSamplingFrequency);

            // Limit value.
            if (Script.ExportSamplingFrequency < 1) {
                Script.ExportSamplingFrequency = 1;
            }

            if (GUILayout.Button("Export")) {
                Script.ExportNodes(Script.ExportSamplingFrequency);
            }

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        ///     Reference to target script.
        /// </summary>
        private AnimationPathAnimator Script { get; set; }

        /// <summary>
        ///     Is true when serialized properties are initialized.
        /// </summary>
        private bool SerializedPropertiesInitialized { get; set; }

        /// <summary>
        ///     <c>SerializedObject</c> for <c>AnimatorSettings</c> asset.
        /// </summary>
        //private SerializedObject serializedObject { get; set; }

        #endregion

        #region SERIALIZED PROPERTIES
        private SerializedProperty advancedSettingsFoldout;

        private SerializedProperty animatedGO;
        private SerializedProperty animationTime;
        private SerializedProperty autoPlayDelay;
        private SerializedProperty enableControlsInPlayMode;
        private SerializedProperty gizmoCurveColor;
        private SerializedProperty longJumpValue;
        private SerializedProperty positionHandle;
        private SerializedProperty rotationCurveColor;
        private SerializedProperty settings;
        private SerializedProperty shortJumpValue;
        private SerializedProperty skin;
        private SerializedProperty targetGO;

        #endregion SERIALIZED PROPERTIES

        #region UNITY MESSAGES

        public override void OnInspectorGUI() {
            // Check for required assets.
            if (!RequiredAssetsLoaded()) {
                DrawInfoLabel(
                    "Required assets were not found.\n"
                    + "Reload scene and if it does not help, restore extension "
                    + "folder content to its default state.");

                return;
            }

            // Check if serialized properties are initialized.
            if (!SerializedPropertiesInitialized) return;

            HandleUndoEvent();
            DrawInspector();

            if (GUI.changed) {
                // Save settings asset.
                EditorUtility.SetDirty(Script.SettingsAsset);
            }

            // Repaint scene after each inspector update.
            SceneView.RepaintAll();
        }
      
        private void OnDisable() {
            // Disable Unity scene tool.
            SceneTool.RestoreTool();
        }

        private void OnEnable() {
            // Get target script reference.
            Script = (AnimationPathAnimator) target;

            // Return is required assets are not referenced.
            if (!RequiredAssetsLoaded()) return;

            InitializeSerializedProperties();
            CopyIconsToGizmosFolder();
            SceneTool.RememberCurrentTool();
            FocusOnSceneView();

            // Update animated GO.
            Utilities.InvokeMethodWithReflection(
                Script,
                "HandleUpdateAnimGOInSceneView",
                null);

            SceneView.RepaintAll();
        }

        private void OnSceneGUI() {
            // Return is required assets are not referenced.
            if (!RequiredAssetsLoaded()) return;
            // Return if path asset is not referenced.
            if (Script.PathData == null) return;
            // Return if serialized properties are not initialized.
            if (!SerializedPropertiesInitialized) return;

            //HandleAnimatorEventsSubscription();

            // Disable interaction with background scene elements.
            HandleUtility.AddDefaultControl(
                GUIUtility.GetControlID(
                    FocusType.Passive));

            HandleShortcuts();

            // todo release reorder
            HandleDrawingAddButtons();
            HandleDrawingRemoveButtons();
            HandleDrawingSceneToolToggleButtons();
            HandleDrawingPathSmoothButtons();
            HandleDrawingRotationPathSmoothButtons();
            HandleDrawingEaseHandles();
            HandleDrawingTiltingHandles();
            HandleDrawingEaseLabel();
            HandleDrawingTiltingLabels();
            HandleDrawingUpdateAllModeLabel();
            HandleDrawingMoveAllModeLables();
            HandleDrawingPositionHandles();
            HandleDrawingObjectPathTangentHandles();
            HandleDrawingRotationPathTangentHandles();
            HandleDrawingRotationHandle();

            // Repaint inspector if any key was pressed.
            // Inspector needs to be redrawn after option is changed
            // with keyboard shortcut.
            if (Event.current.type == EventType.keyUp) {
                Repaint();
            }
        }

        private void HandleDrawingRotationPathSmoothButtons() {
            if (!Script.DrawNodeButtons) return;
            if ((Script.TangentMode != TangentMode.Custom)
                || (Script.RotationMode != RotationMode.Custom)) return;

            var nodePositions = Script.GetGlobalRotationPointPositions();

            var buttonStyle = Script.Skin.GetStyle(
                "SmoothNodeButton");

            SceneHandles.DrawNodeButtons(
                nodePositions,
                Script.SettingsAsset.RotationSmoothButtonOffsetH,
                Script.SettingsAsset.RotationSmoothButtonOffsetV,
                DrawRotationPathSmoothButtonsCallbackHandler,
                buttonStyle);
        }

        private void DrawRotationPathSmoothButtonsCallbackHandler(int nodeIndex) {
            Undo.RecordObject(Script.PathData, "Smooth Rotation Path node.");

            Script.PathData.SmoothRotationPathNodeTangents(nodeIndex);
        }

        private void HandleDrawingPathSmoothButtons() {
            if (!Script.DrawNodeButtons) return;
            // Custom tangent mode and Tangent node handle must be enabled.
            if (Script.TangentMode != TangentMode.Custom) return;
            if (!Script.DrawObjectPath) return;

            // Get positions positions.
            var nodePositions = Script.GetGlobalNodePositions();

            // Get style for add button.
            var buttonStyle = Script.Skin.GetStyle(
                "SmoothNodeButton");

            // Draw add node buttons.
            SceneHandles.DrawNodeButtons(
                nodePositions,
                Script.SettingsAsset.SmoothButtonOffsetH,
                Script.SettingsAsset.SmoothButtonOffsetV,
                DrawSmoothPathNodeButtonsCallbackHandler,
                buttonStyle);
        }

        // todo rename to DrawPathSmoothButtonsCallbackHandler.
        private void DrawSmoothPathNodeButtonsCallbackHandler(int nodeIndex) {
            Undo.RecordObject(Script.PathData, "Smooth Path node.");

            Script.PathData.SmoothPathNodeTangents(nodeIndex);
        }

        private void HandleDrawingMoveAllModeLables() {
            if (!Script.MoveAllMode) return;
            if (!Script.DrawObjectPath) return;

            // Get global node positions.
            var globalNodePositions = Script.GetGlobalNodePositions();

            // Create array with text to be displayed for each node.
            var labelText = MultiplyTextIntoArray(
                Script.SettingsAsset.MoveAllLabelText,
                globalNodePositions);

            SceneHandles.DrawUpdateAllLabels(
                globalNodePositions,
                labelText,
                Script.SettingsAsset.MoveAllLabelOffsetX,
                Script.SettingsAsset.MoveAllLabelOffsetY,
                Script.SettingsAsset.DefaultLabelWidth,
                Script.SettingsAsset.DefaultLabelHeight,
                Script.Skin.GetStyle("MoveAllLabel"));
        }

        private string[] MultiplyTextIntoArray(
            string text,
            List<Vector3> globalNodePositions) {

            var labelText = new string[globalNodePositions.Count];
            for (var i = 0; i < globalNodePositions.Count; i++) {
                labelText[i] = text;
            }
            return labelText;
        }

        private void HandleDrawingRotationPathTangentHandles() {
            // Don't draw when rotation path is disabled.
            if (Script.RotationMode != RotationMode.Custom) return;
            // Draw tangent handles only in custom tangent mode.
            if (Script.TangentMode != TangentMode.Custom) return;
            // Draw tangent handles only tangent node handle is selected.
            if (Script.NodeHandle != NodeHandle.Tangent) return;

            // Positions at which to draw tangent handles.
            var nodes = Script.GetGlobalRotationPointPositions();

            // Draw tangent handles.
            SceneHandles.DrawTangentHandles(
                nodes,
                Script.RotationCurveColor,
                Script.SettingsAsset.TangentHandleSize,
                UpdateRotationPathTangents);
        }

        #endregion UNITY MESSAGES

        #region DRAWING HANDLERS

        /// <summary>
        ///     Handle drawing on-scene buttons for adding new nodes.
        /// </summary>
        private void HandleDrawingAddButtons() {
            if (!Script.DrawNodeButtons) return;
            if (!Script.DrawObjectPath) return;

            // Get positions for the buttons.
            var nodePositions = AddNodeButtonPositions();

            // Get style for add button.
            var addButtonStyle = Script.Skin.GetStyle(
                "AddButton");

            // Draw add node buttons.
            SceneHandles.DrawNodeButtons(
                nodePositions,
                Script.SettingsAsset.AddButtonOffsetH,
                Script.SettingsAsset.AddButtonOffsetV,
                DrawAddNodeButtonsCallbackHandler,
                addButtonStyle);
        }

        private List<Vector3> AddNodeButtonPositions() {
            // Get node positions.
            var nodePositions = Script.GetGlobalNodePositions();
            // Remove last node's position.
            nodePositions.RemoveAt(nodePositions.Count - 1);
            return nodePositions;
        }

        /// <summary>
        ///     Handle drawine on-scene ease handles.
        /// </summary>
        private void HandleDrawingEaseHandles() {
            if (Script.HandleMode != HandleMode.Ease) return;
            if (!Script.DrawObjectPath) return;

            // Get path node positions with ease enabled.
            var easedNodePositions = Script.GetGlobalEasedNodePositions();

            // Get ease values.
            var easeCurveValues = Script.PathData.GetEaseCurveValues();

            // Value that defines how much of an arc will be draw to represent a value.
            var arcValueMultiplier =
                Script.SettingsAsset.ArcValueMultiplierNumerator
                / Script.SettingsAsset.AnimationSpeedDenominator;

            SceneHandles.DrawArcTools(
                easedNodePositions,
                easeCurveValues,
                Script.SettingsAsset.InitialEaseArcValue,
                false,
                arcValueMultiplier,
                Script.SettingsAsset.ArcHandleRadius,
                Script.SettingsAsset.ScaleHandleSize,
                Color.red,
                DrawEaseHandlesCallbackHandler);
        }

        /// <summary>
        ///     Handle drawing on-scene labes with ease values.
        /// </summary>
        private void HandleDrawingEaseLabel() {
            if (Script.HandleMode != HandleMode.Ease) return;
            if (!Script.DrawObjectPath) return;

            // Get path node positions with ease enabled.
            var easedNodePositions = Script.GetGlobalEasedNodePositions();

            SceneHandles.DrawArcHandleLabels(
                easedNodePositions,
                Script.SettingsAsset.EaseValueLabelOffsetX,
                Script.SettingsAsset.EaseValueLabelOffsetY,
                Script.SettingsAsset.DefaultLabelWidth,
                Script.SettingsAsset.DefaultLabelHeight,
                ConvertEaseToDegrees,
                Script.Skin.GetStyle("EaseValueLabel"));
        }

        /// <summary>
        ///     Handle drawing movement handles.
        /// </summary>
        private void HandleDrawingPositionHandles() {
            if (!Script.DrawObjectPath) return;

            HandleFreePositionHandle();
            HandleDefaultPositionHandle();
        }

        /// <summary>
        /// Method responsible for drawing for each node Unity's default position handle.
        /// </summary>
        private void HandleDefaultPositionHandle() {
            if (positionHandle.enumValueIndex ==
                (int) PositionHandle.Default) {

                // Get node positions.
                var nodeGlobalPositions = Script.GetGlobalNodePositions();

                SceneHandles.DrawPositionHandles(
                    nodeGlobalPositions,
                    DrawPositionHandlesCallbackHandler);
            }
        }

        /// <summary>
        /// Method responsible for drawing for each node custom position handle.
        /// </summary>
        // todo rename to HandleDrawCustomPositionHandle.
        private void HandleFreePositionHandle() {
            if (positionHandle.enumValueIndex ==
                (int) PositionHandle.Free) {

                var nodeGlobalPositions = Script.GetGlobalNodePositions();

                SceneHandles.DrawCustomPositionHandles(
                    nodeGlobalPositions,
                    Script.SettingsAsset.MovementHandleSize,
                    Script.GizmoCurveColor,
                    DrawPositionHandlesCallbackHandler);
            }
        }

        /// <summary>
        ///     Handle drawing on-scene button for removing nodes.
        /// </summary>
        private void HandleDrawingRemoveButtons() {
            if (!Script.DrawNodeButtons) return;
            if (!Script.DrawObjectPath) return;

            // Get positions for the buttons.
            var nodePositions = RemoveNodeButtonPositions();

            // Get style for add button.
            var removeButtonStyle = Script.Skin.GetStyle(
                "RemoveButton");

            // Draw add node buttons.
            SceneHandles.DrawNodeButtons(
                nodePositions,
                Script.SettingsAsset.RemoveButtonH,
                Script.SettingsAsset.RemoveButtonV,
                DrawRemoveNodeButtonsCallbackHandler,
                removeButtonStyle);
        }

        private List<Vector3> RemoveNodeButtonPositions() {
            // Node positions.
            var nodePositions = Script.GetGlobalNodePositions();
            // Remove extreme nodes.
            // Extreme nodes can't be removed.
            nodePositions.RemoveAt(0);
            nodePositions.RemoveAt(nodePositions.Count - 1);
            return nodePositions;
        }

        /// <summary>
        ///     Handle drawing on-scene rotation handle in Custom rotation mode.
        /// </summary>
        private void HandleDrawingRotationHandle() {
            if (Script.RotationMode != RotationMode.Custom) return;
            if (Script.NodeHandle != NodeHandle.Position) return;
            if (!Script.DrawRotationPathCurve) return;

            // Return if current animation time is not equal to any node
            // timestamp.
            var nodeTimestamps = Script.PathData.GetPathTimestamps();
            var index = Array.FindIndex(
                nodeTimestamps,
                x => Utilities.FloatsEqual(
                    x,
                    Script.AnimationTime,
                    GlobalConstants.FloatPrecision));
            if (index < 0) return;

            var rotationPointPosition =
                Script.PathData.GetRotationAtTime(Script.AnimationTime);
            var rotationPointGlobalPosition =
                Script.transform.TransformPoint(rotationPointPosition);

            HandleDrawFreeRotationHandle(rotationPointGlobalPosition);
            HandleDrawDefaultRotationHandle(rotationPointGlobalPosition);
        }

        private void HandleDrawDefaultRotationHandle(
            Vector3 rotationPointGlobalPosition) {

            if (positionHandle.enumValueIndex != (int) PositionHandle.Default) {
                return;
            }

            SceneHandles.DrawDefaultRotationHandle(
                rotationPointGlobalPosition,
                DrawRotationHandlesCallbackHandler);
        }

        private void HandleDrawFreeRotationHandle(
            Vector3 rotationPointGlobalPosition) {

            if (positionHandle.enumValueIndex != (int) PositionHandle.Free) {
                return;
            }

            SceneHandles.DrawCustomRotationHandle(
                rotationPointGlobalPosition,
                Script.SettingsAsset.RotationHandleSize,
                Script.SettingsAsset.RotationHandleColor,
                DrawRotationHandlesCallbackHandler);
        }

        /// <summary>
        ///     Handle drawing on-scene tilting handles.
        /// </summary>
        private void HandleDrawingTiltingHandles() {
            if (Script.HandleMode != HandleMode.Tilting) return;
            if (!Script.DrawObjectPath) return;

            // Get path node positions.
            var nodePositions =
                Script.GetGlobalTiltedNodePositions();

            // Get tilting curve values.
            var tiltingCurveValues = Script.PathData.GetTiltingCurveValues();

            SceneHandles.DrawArcTools(
                nodePositions,
                tiltingCurveValues,
                Script.SettingsAsset.InitialTiltingArcValue,
                true,
                Script.SettingsAsset.TiltingValueMultiplierDenominator,
                Script.SettingsAsset.ArcHandleRadius,
                Script.SettingsAsset.ScaleHandleSize,
                Color.green,
                DrawTiltingHandlesCallbackHandler);
        }

        /// <summary>
        ///     Handle drawing on-scene tilting value labels.
        /// </summary>
        private void HandleDrawingTiltingLabels() {
            if (Script.HandleMode != HandleMode.Tilting) return;
            if (!Script.DrawObjectPath) return;

            // Get path node positions with ease enabled.
            var tiltedNodePositions = Script.GetGlobalTiltedNodePositions();

            SceneHandles.DrawArcHandleLabels(
                tiltedNodePositions,
                Script.SettingsAsset.EaseValueLabelOffsetX,
                Script.SettingsAsset.EaseValueLabelOffsetY,
                Script.SettingsAsset.DefaultLabelWidth,
                Script.SettingsAsset.DefaultLabelHeight,
                ConvertTiltToDegrees,
                Script.Skin.GetStyle("TiltValueLabel"));
        }

        /// <summary>
        ///     Handle drawing on-scene label for "Update All" inspector option.
        /// </summary>
        private void HandleDrawingUpdateAllModeLabel() {
            if (!Script.UpdateAllMode) return;
            if (!Script.DrawObjectPath) return;

            // Get global node positions.
            var globalNodePositions = Script.GetGlobalNodePositions();

            // Create array with text to be displayed for each node.
            var labelText = MultiplyTextIntoArray(
                Script.SettingsAsset.UpdateAllLabelText,
                globalNodePositions);

            SceneHandles.DrawUpdateAllLabels(
                globalNodePositions,
                labelText,
                Script.SettingsAsset.UpdateAllLabelOffsetX,
                Script.SettingsAsset.UpdateAllLabelOffsetY,
                Script.SettingsAsset.DefaultLabelWidth,
                Script.SettingsAsset.DefaultLabelHeight,
                Script.Skin.GetStyle("UpdateAllLabel"));
        }

        #endregion DRAWING HANDLERS
        #region CALLBACK HANDLERS

        /// <summary>
        /// Offset rotation path node tangents by given value.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="inOutTangentOffset"></param>
        private void UpdateRotationPathTangents(
            int index,
            Vector3 inOutTangentOffset) {

            // Make snapshot of the target object.
            Undo.RecordObject(Script.PathData, "Update rotation path tangents.");

            Script.PathData.OffsetRotationPathNodeTangents(index, inOutTangentOffset);
        }

        /// <summary>
        /// Offset animated object path node tangents by given value.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="inOutTangentOffset"></param>
        private void UpdateObjectPathTangents(
            int index,
            Vector3 inOutTangentOffset) {

            // Make snapshot of the target object.
            Undo.RecordObject(Script.PathData, "Update node tangents.");

            Script.PathData.OffsetPathNodeTangents(index, inOutTangentOffset);
            Script.PathData.DistributeTimestamps(
                DistributeTimestampsCallbackHandler);
        }

        private void DrawSceneToolToggleButtonsCallbackHandler(int index) {
            Undo.RecordObject(Script.PathData, "Toggle node tool.");

            switch (Script.HandleMode) {
                case HandleMode.Ease:
                    // Toggle ease tool.
                    HandleToggleEaseTool(index);
                    break;
                case HandleMode.Tilting:
                    // Toggle tilting tool.
                    HandleToggleTiltingTool(index);
                    break;
            }
        }


        /// <summary>
        ///     Add node button pressed callback handler.
        /// </summary>
        /// <param name="nodeIndex"></param>
        private void DrawAddNodeButtonsCallbackHandler(int nodeIndex) {
            // Make snapshot of the target object.
            Undo.RecordObject(Script.PathData, "Change path");

            // Add a new node.
            AddNodeBetween(nodeIndex);
            HandleUpdateRotationPathWidhAddedKeys();
            HandleUnsyncedObjectAndRotationPaths();
            HandleSmoothTangentMode();
            HandleLinearTangentMode();
            Script.PathData.DistributeTimestamps(
                DistributeTimestampsCallbackHandler);

            // Update animated object.
            Utilities.InvokeMethodWithReflection(
                Script,
                "HandleUpdateAnimGOInSceneView",
                null);

            SceneView.RepaintAll();

            EditorUtility.SetDirty(Script.PathData);
        }

        private void DrawEaseHandlesCallbackHandler(
            int keyIndex,
            float newValue) {
            Undo.RecordObject(Script.PathData, "Ease curve changed.");

            // If update all mode is set..
            if (Script.UpdateAllMode) {
                var oldValue = Script.PathData.GetEaseValueAtIndex(keyIndex);
                MultiplyEaseValues(oldValue, newValue);
            }
            else {
                // Update ease for single node.
                Script.PathData.UpdateEaseValue(keyIndex, newValue);
            }

            EditorUtility.SetDirty(Script.PathData);
        }

        private void DrawPositionHandlesCallbackHandler(
            int movedNodeIndex,
            Vector3 newGlobalPos) {

            // Return if any Alt key is pressed.
            if (FlagsHelper.IsSet(Event.current.modifiers, EventModifiers.Alt)) {
                return;
            }

            Undo.RecordObject(Script.PathData, "Change node/s position.");

            HandleMoveSingleMode(movedNodeIndex, newGlobalPos);
            HandleMoveAllMode(movedNodeIndex, newGlobalPos);

            EditorUtility.SetDirty(Script.PathData);
        }

        private void HandleMoveAllMode(int movedNodeIndex, Vector3 newGlobalPos) {
            // Return if move all mode is disabled.
            if (!Script.MoveAllMode) return;

            var oldNodeLocalPosition =
                Script.PathData.GetNodePosition(movedNodeIndex);
            var newNodeLocalPosition =
                Script.transform.InverseTransformPoint(newGlobalPos);

            // Calculate movement delta.
            var moveDelta = newNodeLocalPosition - oldNodeLocalPosition;

            Script.PathData.OffsetNodePositions(moveDelta);
            HandleOffsetRotationPathPosition(moveDelta);
        }

        /// <summary>
        /// Method responsible for offseting rotation path position by a given delta.
        /// </summary>
        /// <param name="moveDelta"></param>
        private void HandleOffsetRotationPathPosition(Vector3 moveDelta) {
            if (Script.RotationMode != RotationMode.Custom) return;
            
            Script.PathData.OffsetRotationPathPosition(moveDelta);
        }

        // todo rename to HandleMoveSingleNode
        private void HandleMoveSingleMode(int movedNodeIndex, Vector3 newGlobalPos) {
            // Return if node handle is not set to position.
            if (Script.NodeHandle != NodeHandle.Position) return;
            // Return if move all mode is enabled.
            if (Script.MoveAllMode) return;

            // Remember path length before applying changes.
            var oldAnimGoPathLength = Script.PathData.GetPathLength(
                Script.SettingsAsset.PathLengthSampling);

            // Calculate node new local position.
            var newNodeLocalPosition =
                Script.transform.InverseTransformPoint(newGlobalPos);

            // Move node.
            Script.PathData.MoveNodeToPosition(
                movedNodeIndex,
                newNodeLocalPosition);

            // Handle tangent mode.
            HandleSmoothTangentMode();
            HandleLinearTangentMode();

            // Distribute timestamps.
            Script.PathData.DistributeTimestamps(
                DistributeTimestampsCallbackHandler);

            // Current path length.
            var newAnimGoPathLength = Script.PathData.GetPathLength(
                Script.SettingsAsset.PathLengthSampling);

            DistributeEaseValues(oldAnimGoPathLength, newAnimGoPathLength);
        }

        private void DistributeTimestampsCallbackHandler(
            List<float> distributedTimestamps) {
            
            HandleUpdateRotationPathTimestamps(distributedTimestamps);
        }

        private void DrawRemoveNodeButtonsCallbackHandler(int index) {
            Undo.RecordObject(Script.PathData, "Change path");

            // Increment node index.
            // Indexes passed through arg. don't include extreme nodes.
            var nodeIndex = index + 1;

            Script.PathData.RemoveNode(nodeIndex);
            HandleUpdateRotationPathWithRemovedKeys();
            HandleSmoothTangentMode();
            HandleLinearTangentMode();
            Script.PathData.DistributeTimestamps(
                DistributeTimestampsCallbackHandler);

            // todo remove
            Logger.LogString("NodesNo: {0}", Script.PathData.NodesNo);
            Logger.LogString(
                "TiltingToolState: {0}",
                Script.PathData.TiltingToolState.Count);
            var enabledTiltingToolsNo =
                Script.PathData.GetTiltedNodeTimestamps();
            Logger.LogString("TiltingToolState({0})", enabledTiltingToolsNo.Count);
            Logger.LogString(
                "TiltingCurve: {0}",
                Script.PathData.TiltingCurve.length);

            // Update animated object.
            Utilities.InvokeMethodWithReflection(
                Script,
                "HandleUpdateAnimGOInSceneView",
                null);

            EditorUtility.SetDirty(Script.PathData);
        }

        private void DrawRotationHandlesCallbackHandler(
            Vector3 newPosition) {

            Undo.RecordObject(Script.PathData, "Rotation path changed.");

            var newLocalPos =
                Script.transform.InverseTransformPoint(newPosition);

            Script.PathData.UpdateRotationPointAtTimestamp(
                Script.AnimationTime,
                newLocalPos,
                ChangeRotationAtTimestampCallbackHandler);

            HandleLinearTangentMode();

            EditorUtility.SetDirty(Script.PathData);
        }

        private void ChangeRotationAtTimestampCallbackHandler() {
            if (Script.TangentMode == TangentMode.Custom) return;

            Script.PathData.SmoothAllRotationPathNodes();
        }

        private void DrawTiltingHandlesCallbackHandler(
            int keyIndex,
            float newValue) {
            Undo.RecordObject(Script.PathData, "Tilting curve changed.");

            if (Script.UpdateAllMode) {
                MultiplyTiltingValues(
                    Script.PathData.GetTiltingValueAtIndex(keyIndex),
                    newValue);
            }
            else {
                Script.PathData.UpdateTiltingValue(keyIndex, newValue);
            }

            Utilities.InvokeMethodWithReflection(
                Script,
                "HandleUpdateAnimGOInSceneView",
                null);

            EditorUtility.SetDirty(Script.PathData);
        }

        #endregion CALLBACK HANDLERS

        #region MODE HANDLERS

        /// <summary>
        ///     Defines what to do when tangent mode is changed from custom to something else.
        /// </summary>
        private bool HandleAskIfExitTangentMode(
            TangentMode prevTangentMode) {

            // Return true if user is not trying to exit Custom rotation mode.
            if (prevTangentMode != TangentMode.Custom) return true;

            // Display confirmation dialog
            var canExit = EditorUtility.DisplayDialog(
                "Are you sure want to exit custom tangent mode?",
                "If you exit this mode, all custom rotation data will be lost.",
                "Continue",
                "Cancel");

            return (canExit);
        }

        private void HandleLinearTangentMode() {
            if (Script.TangentMode == TangentMode.Linear) {
                Script.PathData.SetPathTangentsToLinear();
                Script.PathData.SetRotationPathTangentsToLineear();
            }
        }

        private void HandleSmoothTangentMode() {
            if (Script.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothPathNodeTangents();
                HandleSmoothRotationPathTangents();
            }
        }

        /// <summary>
        /// Method responsible for applying smooth to rotation path node tangents.
        /// </summary>
        private void HandleSmoothRotationPathTangents() {
            if (Script.RotationMode != RotationMode.Custom) return;
            
            Script.PathData.SmoothRotationPathTangents();
        }

        /// <summary>
        ///     Handles tangent mode change.
        /// </summary>
        /// <param name="prevTangentMode"></param>
        private void HandleTangentModeChange(TangentMode prevTangentMode) {
            if (Script.PathData == null) return;

            Undo.RecordObject(Script.PathData, "Smooth path node tangents.");

            // Display modal window.
            var canExitCustomTangentMode =
                HandleAskIfExitTangentMode(prevTangentMode);
            // If user canceled operation..
            if (!canExitCustomTangentMode) {
                // Restore Custom tangent mode.
                Script.TangentMode = TangentMode.Custom;

                return;
            }

            // Handle selected tangent mode.
            HandleSmoothTangentMode();
            HandleLinearTangentMode();

            SceneView.RepaintAll();
        }

        #endregion

        #region OTHER HANDLERS

        /// <summary>
        ///     Handles adding/removing reference to target game object.
        /// </summary>
        /// <param name="prevTargetGO"></param>
        private void HandleTargetGOFieldChange(Transform prevTargetGO) {
            // Handle adding reference.
            if (Script.TargetGO != prevTargetGO
                && prevTargetGO == null) {

                Script.RotationMode = RotationMode.Target;

                Utilities.InvokeMethodWithReflection(
                    Script,
                    "HandleUpdateAnimGOInSceneView",
                    null);
            }
            // Handle removing reference.
            else if (Script.TargetGO != prevTargetGO
                     && prevTargetGO != null
                     && Script.TargetGO == null) {

                Script.RotationMode = RotationMode.Forward;

                Utilities.InvokeMethodWithReflection(
                    Script,
                    "HandleUpdateAnimGOInSceneView",
                    null);
            }
        }

        /// <summary>
        ///     Handles situation when adding new node to the path would result in
        ///     anim. GO path and rotation path having different number of nodes.
        /// </summary>
        /// <remarks>Such situation would be caused by placing rotation nodes too close to each other.</remarks>
        private void HandleUnsyncedObjectAndRotationPaths() {
            // Don't check for sync if rotation path is disabled.
            if (Script.RotationMode != RotationMode.Custom) return;

            // Return if object and rotation path have the same number of nodes.
            if (Script.PathData.NodesNo == Script.PathData.RotationPathNodesNo) {
                return;
            }

            // Undo operatio that lead to unsynced state.
            Undo.PerformUndo();

            // Display modal window.
            var reset = EditorUtility.DisplayDialog(
                "Can't add new node!",
                "Nodes in the rotation path are placed too dense.\n" +
                "Click \"Reset\" to reset rotation path or \"Cancel\" " +
                "to update rotation path manually.",
                "Reset",
                "Cancel");

            // Handle reset option.
            if (reset) {
                Undo.RecordObject(Script.PathData, "Reset rotation path.");

                // Reset curves to its default state.
                Script.PathData.ResetRotationPath();
                Script.PathData.SmoothRotationPathTangents();

                SceneView.RepaintAll();
            }
            //else {
            //    // Set rotation mode to Custom so that user can see how
            //    // changes to rotation path affect animated object.
            //    Script.RotationMode = RotationMode.Custom;
            //}
        }

        /// <summary>
        ///     Disable selected node tool.
        /// </summary>
        /// <param name="index">Index of node which tool will be disabled.</param>
        /// <param name="timestamp">Timestamp of node which tool will be disabled.</param>
        private void HandleDisablingEaseTool(int index, float timestamp) {
            // todo Create PathData.EaseCurveLength property and use instead.
            var prevEaseCurveNodesNo = Script.PathData.EaseCurve.length;

            // Remove key from ease curve.
            Script.PathData.RemoveKeyFromEaseCurve(timestamp);

            Utilities.Assert(
                   () => Script.PathData.EaseCurve.length
                       == prevEaseCurveNodesNo - 1,
                   String.Format("Key wasn't removed. Previous keys number: {0};" +
                                 " Current keys number: {1}",
                                 prevEaseCurveNodesNo,
                                 Script.PathData.EaseCurve.length));

            // Disable ease tool.
            Script.PathData.EaseToolState[index] = false;
        }

        private void HandleDisablingTiltingTool(int index, float nodeTimestamp) {
            // todo Create PathData.TiltingCurveLength property and use instead.
            var prevTiltingCurveNodesNo = Script.PathData.TiltingCurve.length;

            // Remove key from ease curve.
            Script.PathData.RemoveKeyFromTiltingCurve(nodeTimestamp);

            Utilities.Assert(
                () => Script.PathData.TiltingCurve.length
                    == prevTiltingCurveNodesNo - 1,
                String.Format("Key wasn't removed. Previous keys number: {0};" +
                              " Current keys number: {1}",
                              prevTiltingCurveNodesNo,
                              Script.PathData.TiltingCurve.length));

            // Disable ease tool.
            Script.PathData.TiltingToolState[index] = false;
        }

        private void HandleDrawingSceneToolToggleButtons() {
            if (!Script.DrawNodeButtons) return;
            if (!Script.DrawObjectPath) return;

            // todo this is redundant. Button won't display until at lease one
            // of those mode is active.
            if ((Script.HandleMode != HandleMode.Ease)
                && (Script.HandleMode != HandleMode.Tilting)) {

                return;
            }

            // Get node positions.
            var nodePositions = Script.GetGlobalNodePositions();
            // Remove extreme nodes.
            nodePositions.RemoveAt(0);
            nodePositions.RemoveAt(nodePositions.Count - 1);

            // Get style for add button.
            var toggleButtonStyle = Script.Skin.GetStyle(
                "SceneToolToggleButton");

            // Draw add node buttons.
            SceneHandles.DrawNodeButtons(
                nodePositions,
                Script.SettingsAsset.SceneToolToggleOffsetH,
                Script.SettingsAsset.SceneToolToggleOffsetV,
                DrawSceneToolToggleButtonsCallbackHandler,
                toggleButtonStyle);
        }

        private void HandleDrawingObjectPathTangentHandles() {
            // Draw tangent handles only in custom tangent mode.
            if (Script.TangentMode != TangentMode.Custom) return;
            // Draw tangent handles only tangent node handle is selected.
            if (Script.NodeHandle != NodeHandle.Tangent) return;

            // Positions at which to draw tangent handles.
            var nodes = Script.GetGlobalNodePositions();

            // Draw tangent handles.
            SceneHandles.DrawTangentHandles(
                nodes,
                Script.GizmoCurveColor,
                Script.SettingsAsset.TangentHandleSize,
                UpdateObjectPathTangents);
        }

        /// <summary>
        ///     Enable selected node tool.
        /// </summary>
        /// <param name="index">Index of node which tool will be enabled.</param>
        /// <param name="timestamp">Timestamp of node which tool will be enabled.</param>
        private void HandleEnablingEaseTool(int index, float timestamp) {
            // todo Create PathData.EaseCurveLength property and use instead.
            var prevEaseCurveNodesNo = Script.PathData.EaseCurve.length;

            // Add new key to ease curve.
            Script.PathData.AddKeyToEaseCurve(timestamp);

            Utilities.Assert(
                () => Script.PathData.EaseCurve.length
                    == prevEaseCurveNodesNo + 1,
                String.Format("Key wasn't added. Previous keys number: {0};" +
                              " Current keys number: {1}",
                              prevEaseCurveNodesNo,
                              Script.PathData.EaseCurve.length));

            // Enable ease tool for the node.
            Script.PathData.EaseToolState[index] = true;
        }

        private void HandleEnablingTiltingTool(int index, float nodeTimestamp) {
            // todo Create PathData.TiltingCurveLength property and use instead.
            var prevTiltingCurveNodesNo = Script.PathData.TiltingCurve.length;

            // Add new key to ease curve.
            Script.PathData.AddKeyToTiltingCurve(nodeTimestamp);

            Utilities.Assert(
                () => Script.PathData.TiltingCurve.length
                    == prevTiltingCurveNodesNo + 1,
                String.Format("Key wasn't added. Previous keys number: {0};" +
                              " Current keys number: {1}",
                              prevTiltingCurveNodesNo,
                              Script.PathData.TiltingCurve.length));

            // Enable ease tool for the node.
            Script.PathData.TiltingToolState[index] = true;
        }

        private void HandleToggleEaseTool(int pressedButtonIndex) {
            // Calculate index of path node for which the ease tool should be toggled.
            var pathNodeIndex = pressedButtonIndex + 1;
            // Get node timestamp.
            var pathNodeTimestamp =
                Script.PathData.GetNodeTimestamp(pathNodeIndex);
            // If tool enabled for node at index..
            if (Script.PathData.EaseToolState[pathNodeIndex]) {
                HandleDisablingEaseTool(pathNodeIndex, pathNodeTimestamp);
            }
            else {
                HandleEnablingEaseTool(pathNodeIndex, pathNodeTimestamp);
            }
        }

        private void HandleToggleTiltingTool(int pressedButtonIndex) {
            // Calculate index of path node for which the ease tool should be toggled.
            var pathNodeIndex = pressedButtonIndex + 1;
            // Get node timestamp.
            var nodeTimestamp = Script.PathData.GetNodeTimestamp(pathNodeIndex);
            // If tool enabled for node at index..
            if (Script.PathData.TiltingToolState[pathNodeIndex]) {
                HandleDisablingTiltingTool(pathNodeIndex, nodeTimestamp);
            }
            else {
                HandleEnablingTiltingTool(pathNodeIndex, nodeTimestamp);
            }
        }

        private void HandleUpdateRotationPathTimestamps(
            List<float> distributedTimestamps) {

            if (Script.RotationMode == RotationMode.Custom) {
                Script.PathData.UpdateRotationPathTimestamps(distributedTimestamps);
            }
        }

        private void HandleUpdateRotationPathWithRemovedKeys() {
            if (Script.RotationMode == RotationMode.Custom) {
                Script.PathData.UpdateRotationPathWithRemovedKeys();
            }
        }

        private void HandleUpdateRotationPathWidhAddedKeys() {
            if (Script.RotationMode == RotationMode.Custom) {
                Script.PathData.UpdateRotationPathWithAddedKeys();
            }
        }
        #endregion

        #region METHODS

        private static void FocusOnSceneView() {
            if (SceneView.sceneViews.Count > 0) {
                var sceneView = (SceneView) SceneView.sceneViews[0];
                sceneView.Focus();
            }
        }

        /// <summary>
        ///     Add new path node between two others, exactly in the middle.
        /// </summary>
        /// <param name="nodeIndex">Node index after which a new node will be placed.</param>
        private void AddNodeBetween(int nodeIndex) {
            // Timestamp of node that was taken action on.
            var currentKeyTime = Script.PathData.GetNodeTimestamp(nodeIndex);
            // Get timestamp of the next node.
            var nextKeyTime = Script.PathData.GetNodeTimestamp(nodeIndex + 1);

            // Calculate timestamps for new key. It'll be placed exactly
            // between the two nodes.
            var newKeyTime =
                currentKeyTime +
                ((nextKeyTime - currentKeyTime) / 2);

            // Return if timestamp for new node would be too close to the
            // neighbouring nodes.
            if (Mathf.Abs(currentKeyTime - newKeyTime)
                < Script.SettingsAsset.MinNodeTimeSeparation) {

                Debug.LogWarning(
                    "Cannot add this node. Time difference to " +
                    "previous and next node is too small. " +
                    "Move nodes more far away.");
                return;
            }

            // Add node to the animation curves.
            Script.PathData.CreateNodeAtTime(newKeyTime);
        }

        /// <summary>
        ///     Converts ease value to degrees that can be displyed with arc handle.
        /// </summary>
        /// <param name="nodeIndex">Node index with the ease value to be converted.</param>
        /// <returns>Ease value as degrees.</returns>
        private float ConvertEaseToDegrees(int nodeIndex) {
            // Calculate value to display.
            var easeValue = Script.PathData.GetNodeEaseValue(nodeIndex);
            var arcValueMultiplier =
                Script.SettingsAsset.ArcValueMultiplierNumerator
                / Script.SettingsAsset.AnimationSpeedDenominator;
            var easeValueInDegrees = easeValue * arcValueMultiplier;

            return easeValueInDegrees;
        }

        private float ConvertTiltToDegrees(int nodeIndex) {
            var rotationValue = Script.PathData.GetNodeTiltValue(nodeIndex);

            return rotationValue;
        }

        /// <summary>
        ///     Copies gizmo icons from component Resource folder to Assets/Gizmos/ATP.
        /// </summary>
        private void CopyIconsToGizmosFolder() {
            // Path to Unity Gizmos folder.
            var gizmosDir = Application.dataPath + "/Gizmos";

            // Create Asset/Gizmos folder if not exists.
            if (!Directory.Exists(gizmosDir)) {
                Directory.CreateDirectory(gizmosDir);
            }

            // Create Asset/Gizmos/ATP folder if not exists.
            if (!Directory.Exists(gizmosDir + "/ATP")) {
                Directory.CreateDirectory(gizmosDir + "/ATP");
            }

            // Check if settings asset has icons specified.
            if (Script.SettingsAsset.GizmoIcons == null) return;

            // For each icon..
            foreach (var icon in Script.SettingsAsset.GizmoIcons) {
                // Get icon path.
                var iconPath = AssetDatabase.GetAssetPath(icon);

                // Copy icon to Gizmos folder.
                AssetDatabase.CopyAsset(
                    iconPath,
                    "Assets/Gizmos/ATP/" + Path.GetFileName(iconPath));
            }
        }

        /// <summary>
        ///     Adjust ease values to path length. Making path longer will decrease ease values
        ///     to  maintain constant speed.
        /// </summary>
        /// <param name="oldAnimGoLength">Anim. Go path length before path update.</param>
        /// <param name="newAnimGoLength">Anim. Go path length after path update.</param>
        private void DistributeEaseValues(
            float oldAnimGoLength,
            float newAnimGoLength) {

            // Calculate multiplier.
            var multiplier = oldAnimGoLength / newAnimGoLength;

            // Multiply each single ease value.
            Script.PathData.MultiplyEaseCurveValues(multiplier);
        }

        private void HandleDrawEaseCurve() {
            if (Script.PathData == null) return;

            Script.PathData.EaseCurve = EditorGUILayout.CurveField(
                new GUIContent(
                    "Ease Curve",
                    ""),
                Script.PathData.EaseCurve);
        }

        private void DrawInspector() {
            DrawPathDataAssetField();

            EditorGUILayout.BeginHorizontal();

            DrawCreatePathAssetButton();
            DrawResetPathInspectorButton();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUILayout.Label("References", EditorStyles.boldLabel);

            DrawAnimatedGOField();
            DrawTargetGOField();

            // Hide inspector options if path is not selected.
            if (Script.PathData == null) return;

            EditorGUILayout.Space();

            GUILayout.Label("Scene Tools", EditorStyles.boldLabel);

            //DrawRotationPathToggle();
            DrawObjectCurveToggle();
            DrawRotationCurveToggle();
            DrawNodeButtonsToggle();

            EditorGUILayout.Space();

            DrawTangentModeDropdown();
            DrawRotationModeDropdown(DrawRotationModeDropdownCallbackHandler);
            HandleDrawForwardPointOffsetSlider();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            DrawHandleModeDropdown();
            HandleDrawUpdateAllToggle();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            DrawNodeHandleDropdown();
            HandleDrawMoveAllToggle();
            EditorGUILayout.EndHorizontal();

            DrawPositionHandleDropdown();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            DrawResetEaseButton();
            DrawResetRotationPathButton();
            DrawResetTiltingButton();
            EditorGUILayout.EndHorizontal();

            //DrawSceneToolShortcutsInfoLabel();

            EditorGUILayout.Space();

            GUILayout.Label("Player", EditorStyles.boldLabel);

            DrawAnimationTimeValue();
            DrawPlayerControls();
            //DrawShortcutsInfoLabel();

            EditorGUILayout.Space();

            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.Space();

            GUILayout.Label("Player Options", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            DrawAutoPlayControl();
            DrawAutoPlayDelayField();
            EditorGUILayout.EndHorizontal();

            DrawEnableControlsInPlayModeToggle();

            EditorGUILayout.Space();

            //EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.EndHorizontal();

            DrawWrapModeDropdown();

            EditorGUILayout.Space();

            DrawPositionSpeedSlider();
            DrawRotationSpeedSlider();

            EditorGUILayout.Space();

            GUILayout.Label("Other", EditorStyles.boldLabel);

            DrawExportControls();

            EditorGUILayout.Space();

            DrawAdvancedSettingsFoldout();
            DrawAdvancedSettingsControls();
        }

        private void DrawNodeButtonsToggle() {
            Script.DrawNodeButtons = EditorGUILayout.Toggle(
                new GUIContent(
                    "Draw Buttons",
                    "Draw on-scene node buttons."),
                Script.DrawNodeButtons);
        }

        private void DrawRotationModeDropdownCallbackHandler(
            RotationMode prevRotationMode,
            RotationMode currentRotationMode) {

            Script.HandleMode = HandleMode.None;

            // If custom rotation mode was just select, apply selected mode.
            if (currentRotationMode == RotationMode.Custom) {
                Script.RotationMode = RotationMode.Custom;
                Script.DrawRotationPathCurve = true;
                Script.PathData.ResetRotationPath();
                return;
            }

            // Display modal window.
            var canDisableRotationPath = EditorUtility.DisplayDialog(
                "Are you sure want to disable rotation path?",
                "If you disable rotation path, all rotation path data " +
                "will be lost.",
                "Continue",
                "Cancel");

            // If user continues, apply selected rotation mode.
            if (canDisableRotationPath) {
                Script.RotationMode = currentRotationMode;
            }

            //HandleRotationModeDropdownChange(
            //    prevRotationMode,
            //    currentRotationMode);

            Utilities.InvokeMethodWithReflection(
                Script,
                "HandleUpdateAnimGOInSceneView",
                null);
        }

        private void DrawObjectCurveToggle() {
            Undo.RecordObject(Script, "Toggle drawing object path.");

            Script.DrawObjectPath = EditorGUILayout.Toggle(
                new GUIContent(
                    "Draw Object Path",
                    ""),
                Script.DrawObjectPath);
        }

        private void DrawRotationCurveToggle() {
            var disable = Script.RotationMode != RotationMode.Custom;

            EditorGUI.BeginDisabledGroup(disable);
            //EditorGUIUtility.labelWidth = 65;

            Script.DrawRotationPathCurve = EditorGUILayout.Toggle(
                new GUIContent(
                    "Draw Rotation Path",
                    ""),
                Script.DrawRotationPathCurve);

            EditorGUI.EndDisabledGroup();

            //EditorGUIUtility.labelWidth = 0;
        }

        private void HandleDrawMoveAllToggle() {
            bool disabled = Script.NodeHandle != NodeHandle.Position;

            EditorGUI.BeginDisabledGroup(disabled);

            EditorGUIUtility.labelWidth = 65;

            Script.MoveAllMode = EditorGUILayout.Toggle(
                new GUIContent(
                    "Move All",
                    "When checked, all nodes will move together. " +
                    "Default shortcut: P."),
                Script.MoveAllMode);

            EditorGUIUtility.labelWidth = 0;

            EditorGUI.EndDisabledGroup();
        }

        private void DrawNodeHandleDropdown() {
            var disable = Script.TangentMode != TangentMode.Custom;

            EditorGUI.BeginDisabledGroup(disable);

            Script.NodeHandle =
                (NodeHandle) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Node Handle",
                        "On-scene node handle."),
                    Script.NodeHandle);

            EditorGUI.EndDisabledGroup();
        }

        private void DrawTiltingCurve() {
            if (Script.PathData == null) return;

            Script.PathData.TiltingCurve = EditorGUILayout.CurveField(
                new GUIContent(
                    "Tilting Curve",
                    ""),
                Script.PathData.TiltingCurve);
        }

        private void HandleRotationModeDropdownChange(
            RotationMode prevRotationMode,
            RotationMode currentRotationMode) {

        }

        /// <summary>
        ///     Defines what to do when undo event is performed.
        /// </summary>
        private void HandleUndoEvent() {
            if (Event.current.type == EventType.ValidateCommand
                && Event.current.commandName == "UndoRedoPerformed") {

                // Repaint inspector.
                Repaint();

                // Update path with new tangent setting.
                HandleSmoothTangentMode();
                HandleLinearTangentMode();

                // Update animated object.
                Utilities.InvokeMethodWithReflection(
                    Script,
                    "HandleUpdateAnimGOInSceneView",
                    null);

                // Fire event.
                Utilities.InvokeMethodWithReflection(
                    Script,
                    "FireUndoRedoPerformedEvent",
                    null);

                SceneView.RepaintAll();
            }
        }

        private void InitializeSerializedProperties() {
            animatedGO = serializedObject.FindProperty("animatedGO");
            targetGO = serializedObject.FindProperty("targetGO");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");
            enableControlsInPlayMode =
                serializedObject.FindProperty(
                    "enableControlsInPlayMode");
            skin = serializedObject.FindProperty("skin");
            settings = serializedObject.FindProperty("settingsAsset");
            gizmoCurveColor =
                serializedObject.FindProperty("gizmoCurveColor");
            rotationCurveColor =
                serializedObject.FindProperty("rotationCurveColor");
            shortJumpValue =
                serializedObject.FindProperty("shortJumpValue");
            longJumpValue =
                serializedObject.FindProperty("longJumpValue");
            animationTime =
                serializedObject.FindProperty("animationTime");
            positionHandle =
                serializedObject.FindProperty("positionHandle");
            autoPlayDelay =
                serializedObject.FindProperty("autoPlayDelay");

            SerializedPropertiesInitialized = true;
        }

        private bool RequiredAssetsLoaded() {
            var assetsLoaded = (bool) Utilities.InvokeMethodWithReflection(
                Script,
                "RequiredAssetsLoaded",
                null);

            return assetsLoaded;
        }

        #endregion PRIVATE METHODS

        #region HELPER METHODS

        /// <summary>
        ///     Multiply each ease value by a difference between two given values.
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        private void MultiplyEaseValues(float oldValue, float newValue) {
            // Guard against null division.
            if (Utilities.FloatsEqual(
                oldValue,
                0,
                GlobalConstants.FloatPrecision)) return;

            // Calculate multiplier.
            var multiplier = newValue / oldValue;

            // Don't let ease value reach zero.
            if (Utilities.FloatsEqual(
                multiplier,
                0,
                GlobalConstants.FloatPrecision)) return;

            // Multiply each single ease value.
            Script.PathData.MultiplyEaseCurveValues(multiplier);
        }

        /// <summary>
        ///     Multiply each tilting value by a difference between two given values.
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        private void MultiplyTiltingValues(float oldValue, float newValue) {
            // Guard against null division.
            if (Utilities.FloatsEqual(
                oldValue,
                0,
                GlobalConstants.FloatPrecision)) return;

            // Calculate multiplier.
            var multiplier = newValue / oldValue;

            // Don't let tilting value reach zero.
            if (Utilities.FloatsEqual(
                multiplier,
                0,
                GlobalConstants.FloatPrecision)) return;

            // Multiply each single ease value.
            Script.PathData.MultiplyTiltingCurveValues(multiplier);
        }

        #endregion

        #region SHORTCUTS

        private void HandleEaseModeShortcut() {
            // Ease handle mode.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.EaseModeKey) {

                Script.HandleMode = HandleMode.Ease;
            }
        }

        private void HandleJumpToEndShortcut() {
            // Jump to end.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.JumpToEndKey
                && FlagsHelper.IsSet(
                    Event.current.modifiers,
                    Script.SettingsAsset.ModKey)) {

                Script.AnimationTime = 1;

                // Call event.
                Utilities.InvokeMethodWithReflection(
                    Script,
                    "FireJumpedToNodeEvent",
                    null);
            }
        }

        private void HandleJumpToNextNodeShortcut() {
            // Jump to next node.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.JumpToNextNodeKey) {

                var prevAnimationTime = Script.AnimationTime;

                Script.AnimationTime =
                    (float) Utilities.InvokeMethodWithReflection(
                        Script,
                        "GetNearestForwardNodeTimestamp",
                        null);

                var deltaTime = Script.AnimationTime - prevAnimationTime;

                // Call JumpedToNode event.
                Utilities.InvokeMethodWithReflection(
                    Script,
                    "FireJumpedToNodeEvent",
                    null);

                // Fire event.
                Utilities.InvokeMethodWithReflection(
                    Script,
                    "OnJumpPerformed",
                    new object[] { deltaTime });
            }
        }

        private void HandleJumpToPreviousNodeShortcut() {
            // Jump to previous node.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.JumpToPreviousNodeKey) {

                var prevAnimationTime = Script.AnimationTime;

                Script.AnimationTime =
                    (float) Utilities.InvokeMethodWithReflection(
                        Script,
                        "GetNearestBackwardNodeTimestamp",
                        null);

                var deltaTime = Script.AnimationTime - prevAnimationTime;

                // Call JumpedToNode event.
                Utilities.InvokeMethodWithReflection(
                    Script,
                    "FireJumpedToNodeEvent",
                    null);

                // Fire event.
                Utilities.InvokeMethodWithReflection(
                    Script,
                    "OnJumpPerformed",
                    new object[] { deltaTime });
            }
        }

        private void HandleJumpToStartShortcut() {
            // Jump to start.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.JumpToStartKey
                && FlagsHelper.IsSet(
                    Event.current.modifiers,
                    Script.SettingsAsset.ModKey)) {

                Script.AnimationTime = 0;

                // Call JumpedToNode event.
                Utilities.InvokeMethodWithReflection(
                    Script,
                    "FireJumpedToNodeEvent",
                    null);
            }
        }

        private void HandleLongJumpBackwardShortcut() {
            // Long jump backward.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.LongJumpBackwardKey
                && !FlagsHelper.IsSet(
                    Event.current.modifiers,
                    Script.SettingsAsset.ModKey)) {

                Script.AnimationTime -= Script.LongJumpValue;
                Logger.LogString("AnimationTime: {0}", Script.AnimationTime);

                // Fire event.
                Utilities.InvokeMethodWithReflection(
                    Script,
                    "OnJumpPerformed",
                    new object[] {- Script.LongJumpValue});
            }
        }

        private void HandleLongJumpForwardShortcut() {
            // Long jump forward.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.LongJumpForwardKey
                && !FlagsHelper.IsSet(
                    Event.current.modifiers,
                    Script.SettingsAsset.ModKey)) {

                Script.AnimationTime += Script.LongJumpValue;

                // Fire event.
                Utilities.InvokeMethodWithReflection(
                    Script,
                    "OnJumpPerformed",
                    new object[] {Script.LongJumpValue});
            }
        }

        private void HandleNoneHandleModeShortcut() {
            // None mode key.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.NoneModeKey) {

                Script.HandleMode = HandleMode.None;
                HandleModeChange();
            }
        }

        private void HandlePlayPauseButton() {
            Utilities.InvokeMethodWithReflection(
                Script,
                "HandlePlayPause",
                null);
        }

        private void HandlePlayPauseShortcut() {
            // Play/pause animation.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.PlayPauseKey) {

                HandlePlayPauseButton();
            }
        }

        private void HandlePositionHandleShortcut() {
            // Update position handle.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.PositionHandleKey) {

                // Change to Position mode.
                if (Script.PositionHandle == PositionHandle.Free) {
                    Script.PositionHandle =
                        PositionHandle.Default;
                }
                // Change to Free mode.
                else {
                    Script.PositionHandle =
                        PositionHandle.Free;
                }
            }
        }

        private void HandleShortcuts() {
            HandleEaseModeShortcut();
            HandleNodeHandleModeShortcut();
            HandleTiltingModeShortcut();
            HandleUpdateAllModeShortcut();
            HandlePositionHandleShortcut();
            HandleMoveAllModeShortcut();
            HandlePlayPauseShortcut();
            HandleNoneHandleModeShortcut();
            HandleToggleButtonsShortcut();

            HandleShortJumpForwardShortcut();
            HandleShortJumpBackwardShortcut();
            HandleLongJumpForwardShortcut();
            HandleLongJumpBackwardShortcut();
            HandleJumpToNextNodeShortcut();
            HandleJumpToPreviousNodeShortcut();
            HandleJumpToStartShortcut();
            HandleJumpToEndShortcut();
        }

        private void HandleToggleButtonsShortcut() {
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.ToggleButtons) {

                Script.DrawNodeButtons = !Script.DrawNodeButtons;
            }
        }

        private void HandleNodeHandleModeShortcut() {
            if (Script.TangentMode != TangentMode.Custom) return;

            // Node handle mode.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.NodeHandleMode) {

                // Select other mode.
                Script.NodeHandle = (Script.NodeHandle == NodeHandle.Position)
                    ? NodeHandle.Tangent
                    : NodeHandle.Position;
            }
        }

        private void HandleMoveAllModeShortcut() {
            // Handle shortcut only if node handle is set to position.
            if (Script.NodeHandle != NodeHandle.Position) return;

            // Move all mode.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.MoveAllKey) {

                // Toggle mode.
                Script.MoveAllMode = !Script.MoveAllMode;
            }
        }

        private void HandleShortJumpBackwardShortcut() {
            // Short jump backward.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.ShortJumpBackwardKey
                && FlagsHelper.IsSet(
                    Event.current.modifiers,
                    Script.SettingsAsset.ModKey)) {

                var newAnimationTimeRatio =
                    Script.AnimationTime
                    - Script.ShortJumpValue;

                Script.AnimationTime =
                    (float) (Math.Round(newAnimationTimeRatio, 4));

                // Fire event.
                Utilities.InvokeMethodWithReflection(
                    Script,
                    "OnJumpPerformed",
                    new object[] {- Script.ShortJumpValue});
            }
        }

        private void HandleShortJumpForwardShortcut() {
            // Short jump forward.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.ShortJumpForwardKey
                && FlagsHelper.IsSet(
                    Event.current.modifiers,
                    Script.SettingsAsset.ModKey)) {

                var newAnimationTimeRatio =
                    Script.AnimationTime
                    + Script.ShortJumpValue;

                Script.AnimationTime =
                    (float) (Math.Round(newAnimationTimeRatio, 4));

                // Fire event.
                Utilities.InvokeMethodWithReflection(
                    Script,
                    "OnJumpPerformed",
                    new object[] {Script.ShortJumpValue});
            }
        }

        private void HandleTiltingModeShortcut() {
            // Tilting mode key.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.TiltingModeKey) {

                Script.HandleMode = HandleMode.Tilting;
            }
        }

        private void HandleUpdateAllModeShortcut() {
            // Handle shortcut only in Ease and Tilting handle mode.
            if ((Script.HandleMode != HandleMode.Ease)
                && (Script.HandleMode != HandleMode.Tilting)) {

                return;
            }

            // Update all mode.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.UpdateAllKey) {

                Script.UpdateAllMode =
                    !Script.UpdateAllMode;
            }
        }

        #endregion

        #region CONTROLS

        private void DrawAdvancedSettingsControls() {
            if (advancedSettingsFoldout.boolValue) {
                HandleDrawEaseCurve();
                DrawTiltingCurve();

                EditorGUILayout.Space();

                DrawShortJumpValueField();
                DrawLongJumpValueField();

                EditorGUILayout.Space();

                DrawGizmoCurveColorPicker();
                DrawRotationCurveColorPicker();

                EditorGUILayout.Space();

                DrawSettingsAssetField();
                DrawSkinSelectionControl();
            }
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

        private void DrawAnimatedGOField() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                animatedGO,
                new GUIContent(
                    "Animated Object",
                    "Game object to animate."));

            serializedObject.ApplyModifiedProperties();
        }

        private float DrawAnimationTimeSlider() {
            var newTimeRatio = EditorGUILayout.Slider(
                new GUIContent(
                    "Animation Time",
                    "Normalized animation time. Animated game object will be " +
                    "updated accordingly to the animation time value."),
                Script.AnimationTime,
                0,
                1);

            return newTimeRatio;
        }

        private void DrawAnimationTimeValue() {
            Undo.RecordObject(target, "Update AnimationTime");

            var newTimeRatio = DrawAnimationTimeSlider();

            // Update animation time only when value was changed.
            if (!Utilities.FloatsEqual(
                newTimeRatio,
                Script.AnimationTime,
                GlobalConstants.FloatPrecision)) {

                serializedObject.Update();
                animationTime.floatValue = newTimeRatio;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawAutoPlayControl() {
            Script.AutoPlay = EditorGUILayout.Toggle(
                new GUIContent(
                    "Auto Play",
                    "Start playing animation after entering play mode."),
                Script.AutoPlay);
        }

        private void DrawAutoPlayDelayField() {
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 50;

            EditorGUILayout.PropertyField(
                autoPlayDelay,
                new GUIContent(
                    "Delay",
                    "Auto play delay in seconds."));

            EditorGUIUtility.labelWidth = 0;

            // Limit value to greater than zero.
            if (autoPlayDelay.floatValue < 0) autoPlayDelay.floatValue = 0;

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCreatePathAssetButton() {
            // Draw button.
            if (GUILayout.Button(
                new GUIContent(
                    "New Path",
                    "Create new path asset file."))) {

                // Display save panel.
                var savePath = EditorUtility.SaveFilePanelInProject(
                    "Save Path Asset File",
                    Script.SettingsAsset.PathDataAssetDefaultName,
                    "asset",
                    "");

                // Path cannot be empty.
                if (savePath == "") return;

                // Create new path asset.
                var asset = ScriptableObjectUtility.CreateAsset<PathData>(
                    fullPath: savePath);

                // Assign asset as the current path.
                Script.PathData = asset;
            }
        }

        private void DrawEnableControlsInPlayModeToggle() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(
                enableControlsInPlayMode,
                new GUIContent(
                    "Play Mode Controls",
                    "Enable keybord controls in play mode."));
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGizmoCurveColorPicker() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                gizmoCurveColor,
                new GUIContent("Curve Color", ""));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHandleModeDropdown() {
            Undo.RecordObject(Script.SettingsAsset, "Change handle mode.");

            var prevHandleMode = Script.HandleMode;

            Script.HandleMode =
                (HandleMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Scene Tool",
                        "Tool displayed next to each node. Default " +
                        "shortcuts: Y, U, I, O."),
                    Script.HandleMode);

            // Return if handle mode wasn't changed.
            if (Script.HandleMode == prevHandleMode) return;

            HandleModeChange();
        }

        private void DrawInfoLabel(string text) {
            EditorGUILayout.HelpBox(text, MessageType.Error);
        }

        private void DrawLongJumpValueField() {
            serializedObject.Update();

            longJumpValue.floatValue = EditorGUILayout.Slider(
                new GUIContent(
                    "Long Jump Value",
                    "Fraction of animation time used to jump forward/backward "
                    + "in time with keyboard keys."),
                longJumpValue.floatValue,
                0.004f,
                0.1f);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPathDataAssetField() {
            Undo.RecordObject(Script, "Change PathData inspector field.");

            Script.PathData = (PathData) EditorGUILayout.ObjectField(
                new GUIContent("Path Asset", "Asset containing all path data."),
                Script.PathData,
                typeof (PathData),
                false);
        }

        private void DrawPlayerControls() {
            // Play/Pause button text.
            string playPauseBtnText;
            if (!Script.IsPlaying) {
                playPauseBtnText = "Play";
            }
            else {
                playPauseBtnText = "Pause";
            }

            EditorGUILayout.BeginHorizontal();

            // Draw Play/Pause button.
            if (GUILayout.Button(
                new GUIContent(
                    playPauseBtnText,
                    ""))) {

                Utilities.InvokeMethodWithReflection(
                    Script,
                    "HandlePlayPause",
                    null);
            }

            // Draw Stop button.
            if (GUILayout.Button(
                new GUIContent(
                    "Stop",
                    ""))) {

                // todo create Animator.Stop().
                Script.IsPlaying = false;
                Script.AnimationTime = 0;

                Utilities.InvokeMethodWithReflection(
                    Script,
                    "HandleUpdateAnimGOInSceneView",
                    null);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPositionHandleDropdown() {
            var disable = Script.NodeHandle != NodeHandle.Position;

            // In tangent mode draw only free movement handle.
            if (Script.TangentMode == TangentMode.Custom
                && Script.NodeHandle == NodeHandle.Tangent) {

                Script.PositionHandle = PositionHandle.Free;
                //return;
            }

            EditorGUI.BeginDisabledGroup(disable);

            serializedObject.Update();

            // todo don't use serialized property
            EditorGUILayout.PropertyField(
                positionHandle,
                new GUIContent(
                    "Position Handle",
                    "Handle used to move nodes on scene. Default " +
                    "shortcut: G"));

            serializedObject.ApplyModifiedProperties();

            EditorGUI.EndDisabledGroup();
        }

        private void DrawPositionSpeedSlider() {
            Script.PositionLerpSpeed = EditorGUILayout.Slider(
                new GUIContent(
                    "Position Lerp",
                    "Controls how much time it'll take the " +
                    "animated object to reach position that it should be " +
                    "at the current animation time. " +
                    "1 means no delay."),
                Script.PositionLerpSpeed,
                Script.SettingsAsset.MinPositionLerpSpeed,
                Script.SettingsAsset.MaxPositionLerpSpeed);
        }

        private void DrawResetEaseButton() {
            if (GUILayout.Button(
                new GUIContent(
                    "Reset Ease",
                    "Reset Ease Tool values."))) {

                if (Script.PathData == null) return;

                Undo.RecordObject(Script.PathData, "Reset ease curve.");

                // Reset curves to its default state.
                Script.PathData.ResetEaseCurve();

                SceneView.RepaintAll();
            }
        }

        private void DrawResetPathInspectorButton() {
            // Draw button.
            if (GUILayout.Button(
                new GUIContent(
                    "Reset Path",
                    "Reset path to default."))) {

                if (Script.PathData == null) return;

                // Allow undo this operation.
                Undo.RecordObject(Script.PathData, "Change path");

                // Reset curves to its default state.
                Script.PathData.ResetPath();

                // Reset inspector options.
                Script.AnimationTime = 0;
                Script.HandleMode = HandleMode.None;

                Utilities.InvokeMethodWithReflection(
                    Script,
                    "HandleUpdateAnimGOInSceneView",
                    null);
            }
        }

        private void DrawResetRotationPathButton() {
            if (GUILayout.Button(
                new GUIContent(
                    "Reset Rotation",
                    "Reset Rotation Tool values."))) {

                if (Script.PathData == null) return;

                Undo.RecordObject(Script.PathData, "Reset rotation path.");

                // Reset curves to its default state.
                Script.PathData.ResetRotationPath();
                //Script.PathData.SmoothRotationPathTangents();

                // Change rotation mode.
                Script.RotationMode = RotationMode.Custom;

                SceneView.RepaintAll();
            }
        }

        private void DrawResetTiltingButton() {
            if (GUILayout.Button(
                new GUIContent(
                    "Reset Tilting",
                    "Reset Tilting Tool values."))) {

                if (Script.PathData == null) return;

                Undo.RecordObject(Script.PathData, "Reset tilting curve.");

                // Reset curves to its default state.
                Script.PathData.ResetTiltingCurve();

                SceneView.RepaintAll();
            }
        }

        private void DrawRotationCurveColorPicker() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(rotationCurveColor);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRotationModeDropdown(
            Action<RotationMode, RotationMode> callback) {

            Undo.RecordObject(Script.SettingsAsset, "Change rotation mode.");

            // Remember current RotationMode.
            var prevRotationMode = Script.RotationMode;

            // Draw RotationMode dropdown.
            var currentRotationMode =
                (RotationMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Rotation Mode",
                        "Mode that controls animated game object rotation."),
                    Script.RotationMode);

            // Return if rotation mode not changed.
            if (currentRotationMode == prevRotationMode) return;

            callback(prevRotationMode, currentRotationMode);
        }

        private void DrawRotationSpeedSlider() {
            Script.RotationSlerpSpeed =
                EditorGUILayout.Slider(
                    new GUIContent(
                        "Rotation Slerp",
                        "Controls how much time it'll take the " +
                        "animated object to finish rotation towards followed target."),
                    Script.RotationSlerpSpeed,
                    Script.SettingsAsset.MinRotationSlerpSpeed,
                    Script.SettingsAsset.MaxRotationSlerpSpeed);
        }

        private void DrawSceneToolShortcutsInfoLabel() {
            EditorGUILayout.HelpBox(
                "Shortcuts (editor): G, Y, U, I, O, P.",
                MessageType.Info);
        }

        private void DrawSettingsAssetField() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                settings,
                new GUIContent(
                    "Settings Asset",
                    "Asset that contains all setting for the animator " +
                    "component"));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawShortcutsInfoLabel() {
            EditorGUILayout.HelpBox(
                "Scene shortcuts to control animation (editor/play mode): " +
                "[Alt] H, [Alt] J, [Alt] K, [Alt] L. (play mode): Space",
                MessageType.Info);
        }

        private void DrawShortJumpValueField() {
            serializedObject.Update();

            shortJumpValue.floatValue = EditorGUILayout.Slider(
                new GUIContent(
                    "Short Jump Value",
                    "Fraction of animation time used to jump forward/backward "
                    + "in time with keyboard keys."),
                shortJumpValue.floatValue,
                Script.SettingsAsset.ShortJumpMinValue,
                Script.SettingsAsset.ShortJumpMaxValue);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSkinSelectionControl() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(
                skin,
                new GUIContent(
                    "Skin Asset",
                    "Asset containing styles for animator component."));
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTangentModeDropdown() {
            // Remember current tangent mode.
            var prevTangentMode = Script.TangentMode;

            // Draw tangent mode dropdown.
            Script.TangentMode =
                (TangentMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Tangent Mode",
                        "Tangent mode applied to each path node."),
                    Script.TangentMode);

            // Update gizmo curve is tangent mode changed.
            if (Script.TangentMode != prevTangentMode) {
                HandleTangentModeChange(prevTangentMode);
            }
        }

        private void DrawTargetGOField() {
            var prevTargetGO = Script.TargetGO;
            serializedObject.Update();
            EditorGUILayout.PropertyField(
                targetGO,
                new GUIContent(
                    "Target Object",
                    "Object that the animated object will be looking at."));
            serializedObject.ApplyModifiedProperties();

            HandleTargetGOFieldChange(prevTargetGO);
        }

        private void DrawWrapModeDropdown() {
            Script.WrapMode =
                (AnimatorWrapMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Wrap Mode",
                        "Determines animator behaviour after animation end."),
                    Script.WrapMode);
        }

        private void HandleDrawForwardPointOffsetSlider() {
            if (Script.RotationMode != RotationMode.Forward) return;

            Script.ForwardPointOffset = EditorGUILayout.Slider(
                new GUIContent(
                    "Forward Point",
                    "Distance from animated object to point used as " +
                    "target in Forward rotation mode."),
                Script.ForwardPointOffset,
                Script.SettingsAsset.ForwardPointOffsetMinValue,
                Script.SettingsAsset.ForwardPointOffsetMaxValue);
        }

        private void HandleDrawUpdateAllToggle() {
            bool disable = (Script.HandleMode != HandleMode.Ease)
                           && (Script.HandleMode != HandleMode.Tilting);

            EditorGUI.BeginDisabledGroup(disable);

            EditorGUIUtility.labelWidth = 65;

            Script.UpdateAllMode = EditorGUILayout.Toggle(
                new GUIContent(
                    "Update All",
                    "When checked, values will be changed for all nodes. " +
                    "Default shortcut: P."),
                Script.UpdateAllMode);

            EditorGUIUtility.labelWidth = 0;

            EditorGUI.EndDisabledGroup();
        }

        private void HandleModeChange() {
            // If handle mode was changed to None..
            if (Script.HandleMode == HandleMode.None) {
                // Don't display update all values mode label.
                Script.UpdateAllMode = false;
            }
            // Handle changed to ease or tilting.
            else {
                Script.PositionHandle = PositionHandle.Free;
            }
        }

        #endregion
    }

}