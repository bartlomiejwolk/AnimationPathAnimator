using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ATP.SimplePathAnimator.Animator {

    [CustomEditor(typeof (PathAnimator))]
    public sealed class PathAnimatorEditor : Editor {
        
        #region PROPERTIES

        private SceneHandles SceneHandles { get; set; }

        private SerializedObject SettingsSerObj { get; set; }
        //public SerializedObject PathExporterSerObj { get; private set; }

        /// <summary>
        ///     Reference to target script.
        /// </summary>
        private PathAnimator Script { get; set; }

        private Shortcuts Shortcuts { get; set; }

        private PathAnimatorSettings PathAnimatorSettings { get; set; }

        private PathExporter PathExporter { get; set; }

        #endregion 

        #region SERIALIZED PROPERTIES

        private SerializedProperty advancedSettingsFoldout;
        private SerializedProperty animatedGO;
        private SerializedProperty enableControlsInPlayMode;
        private SerializedProperty forwardPointOffset;
        private SerializedProperty gizmoCurveColor;
        private SerializedProperty maxAnimationSpeed;
        private SerializedProperty pathData;
        private SerializedProperty positionLerpSpeed;
        private SerializedProperty rotationCurveColor;
        private SerializedProperty rotationSpeed;
        private SerializedProperty skin;
        private SerializedProperty targetGO;
        private SerializedProperty settings;


        #endregion SERIALIZED PROPERTIES

        #region UNITY MESSAGES

        public override void OnInspectorGUI() {
            HandleUndo();

            // TODO Rename to DrawPathDataAssetField().
            DrawPathDataAssetField();

            EditorGUILayout.BeginHorizontal();

            DrawCreatePathAssetButton();
            DrawResetPathInspectorButton();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            DrawAnimatedGOField();
            DrawTargetGOField();

            EditorGUILayout.Space();

            DrawAnimationTimeControl();

            DrawRotationModeDropdown();
            DrawHandleModeDropdown();
            DrawMovementModeDropdown();
            DrawTangentModeDropdown();
            DrawWrapModeDropdown();

            EditorGUILayout.Space();

            EditorGUIUtility.labelWidth = 180;

            DrawPositionLerpSpeedControl();
            DrawRotationSlerpSpeedField();
            DrawForwardPointOffsetField();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            DrawResetEaseButton();
            DrawResetRotationPathButton();
            DrawResetTiltingButton();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            DrawAutoPlayControl();
            DrawEnableControlsInPlayModeToggle();
            DrawUpdateAllToggle();

            EditorGUILayout.Space();

            DrawPlayerControls();

            EditorGUILayout.Space();

            EditorGUIUtility.labelWidth = 0;

            PathExporter.DrawExportControls();

            EditorGUILayout.Space();

            DrawSettingsAssetField();
            DrawSkinSelectionControl();

            EditorGUILayout.Space();

            DrawAdvancedSettingsFoldout();
            DrawAdvancedSettingsControls();

            // Validate inspector settings.
            // Not all inspector controls can be validated with OnValidate().
            if (GUI.changed) Script.ValidateInspectorSettings();
        }

        private void HandleUndo() {
            if (Event.current.type == EventType.ValidateCommand
                && Event.current.commandName == "UndoRedoPerformed") {

                // Repaint inspector.
                Repaint();
                // Update path with new tangent setting.
                HandleTangentModeChange();
                // Update animated object.
                Script.UpdateAnimation();
            }
        }

        private void OnEnable() {
            // Get target script reference.
            Script = (PathAnimator) target;

            // Initialize PathAnimatorSettings property.
            PathAnimatorSettings = Script.Settings;

            PathExporter = new PathExporter(Script);

            InstantiateCompositeClasses();
            InitializeSerializedProperties();

            SceneTool.RememberCurrentTool();

            FocusOnSceneView();
        }
        private void OnDisable() {
            SceneTool.RestoreTool();
        }

        private void OnSceneGUI() {
            CheckForSkinAsset();

            // Return if path asset does not exist.
            if (Script.PathData == null) return;

            // Disable interaction with background scene elements.
            HandleUtility.AddDefaultControl(
                GUIUtility.GetControlID(
                    FocusType.Passive));

            Shortcuts.HandleShortcuts();

            Script.UpdateWrapMode();

            HandleDrawingEaseHandles();
            HandleDrawingTiltingHandles();
            HandleDrawingEaseLabel();
            HandleDrawingTiltLabel();
            HandleDrawingUpdateAllModeLabel();
            HandleDrawingPositionHandles();
            HandleDrawingRotationHandle();
            HandleDrawingAddButtons();
            HandleDrawingRemoveButtons();
        }
        #endregion UNITY MESSAGES

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

        private void HandleDrawingUpdateAllModeLabel() {
            if (!PathAnimatorSettings.UpdateAllMode) return;

            //SceneHandles.DrawNodeLabels(
            //    Script,
            //    "A",
            //    upall
            //    Script.Skin.GetStyle("UpdateAllLabel"));

            SceneHandles.DrawUpdateAllLabels(
                Script,
                Script.Skin.GetStyle("UpdateAllLabel"));
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

        private void DrawResetRotationPathButton() {
            if (GUILayout.Button(
                            new GUIContent(
                                "Reset Rotation",
                                ""))) {

                if (Script.PathData == null) return;

                Undo.RecordObject(Script.PathData, "Reset rotatio path.");

                // Reset curves to its default state.
                // todo Create direct property for PathData.
                Script.PathData.ResetRotationPath();

                SceneView.RepaintAll();
            }
        }

        private void DrawAdvancedSettingsControls() {
            if (advancedSettingsFoldout.boolValue) {
                DrawGizmoCurveColorPicker();
                DrawRotationCurveColorPicker();

                //EditorGUILayout.Space();

                //// TODO Limit these values in OnValidate().
                //DrawPositionLerpSpeedControl();
                //DrawRotationSlerpSpeedField();
                //DrawForwardPointOffsetField();
                //DrawMaxAnimationSpeedField();

                //EditorGUILayout.Space();

                //DrawSkinSelectionControl();
            }
        }

        private void DrawAnimatedGOField() {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(
                animatedGO,
                new GUIContent(
                    "Animated Object",
                    "Object to animate."));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAnimationTimeControl() {
            Undo.RecordObject(target, "Update AnimationTimeRatio");

            var newTimeRatio = EditorGUILayout.Slider(
                new GUIContent(
                    "Animation Time",
                    "Current animation time."),
                Script.AnimationTimeRatio,
                0,
                1);

            // Update AnimationTimeRatio only when value was changed.
            if (Math.Abs(newTimeRatio - Script.AnimationTimeRatio)
                > GlobalConstants.FloatPrecision) {

                Script.AnimationTimeRatio = newTimeRatio;
            }
        }

        private void DrawAutoPlayControl() {
            PathAnimatorSettings.AutoPlay = EditorGUILayout.Toggle(
                new GUIContent(
                    "Auto Play",
                    ""),
                PathAnimatorSettings.AutoPlay);
        }

        private void DrawEnableControlsInPlayModeToggle() {
            SettingsSerObj.Update();
            EditorGUILayout.PropertyField(
                enableControlsInPlayMode,
                new GUIContent(
                    "Play Mode Controls",
                    "Enable keybord controls in play mode."));
            SettingsSerObj.ApplyModifiedProperties();
        }

        private void DrawForwardPointOffsetField() {
            SettingsSerObj.Update();

            EditorGUILayout.PropertyField(forwardPointOffset);

            SettingsSerObj.ApplyModifiedProperties();
        }

        private void DrawGizmoCurveColorPicker() {
            SettingsSerObj.Update();

            EditorGUILayout.PropertyField(
                gizmoCurveColor,
                new GUIContent("Curve Color", ""));

            SettingsSerObj.ApplyModifiedProperties();
        }

        private void DrawHandleModeDropdown() {
            Undo.RecordObject(PathAnimatorSettings, "Change handle mode.");

            PathAnimatorSettings.HandleMode = (HandleMode) EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Handle Mode",
                    ""),
                PathAnimatorSettings.HandleMode);
        }

        private void DrawMaxAnimationSpeedField() {

            serializedObject.Update();
            EditorGUILayout.PropertyField(maxAnimationSpeed);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMovementModeDropdown() {
            Undo.RecordObject(PathAnimatorSettings, "Change movement mode.");

            PathAnimatorSettings.MovementMode =
                (MovementMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Movement Mode",
                        ""),
                    PathAnimatorSettings.MovementMode);

            SceneView.RepaintAll();
        }

        private void DrawPathDataAssetField() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                pathData,
                new GUIContent(
                    "Path Asset",
                    ""));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPlayerControls() {
            // Play/Pause button text.
            string playPauseBtnText;
            if (!Script.IsPlaying || (Script.IsPlaying && Script.Pause)) {
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

                Script.HandlePlayPause();
            }

            // Draw Stop button.
            if (GUILayout.Button(
                new GUIContent(
                    "Stop",
                    ""))) {

                Script.StopEaseTimeCoroutine();
                Script.UpdateAnimation();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPositionLerpSpeedControl() {
            SettingsSerObj.Update();
            EditorGUILayout.PropertyField(
                positionLerpSpeed,
                new GUIContent(
                    "Position Lerp Speed",
                    ""));
            SettingsSerObj.ApplyModifiedProperties();
        }

        private void DrawRotationCurveColorPicker() {
            SettingsSerObj.Update();

            EditorGUILayout.PropertyField(rotationCurveColor);

            SettingsSerObj.ApplyModifiedProperties();
        }

        private void DrawRotationSlerpSpeedField() {
            SettingsSerObj.Update();

            EditorGUILayout.PropertyField(
                rotationSpeed,
                new GUIContent(
                    "Rotation Slerp Speed",
                    "Controls how much time (in seconds) it'll take the " +
                    "animated object to finish rotation towards followed target."));

            SettingsSerObj.ApplyModifiedProperties();
        }

        private void DrawSkinSelectionControl() {

            serializedObject.Update();
            EditorGUILayout.PropertyField(
                skin,
                new GUIContent(
                    "Skin Asset",
                    ""));
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTangentModeDropdown() {
            // Remember current tangent mode.
            var prevTangentMode = PathAnimatorSettings.TangentMode;

            // Draw tangent mode dropdown.
            PathAnimatorSettings.TangentMode =
                (TangentMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Tangent Mode",
                        ""),
                    PathAnimatorSettings.TangentMode);

            // Update gizmo curve is tangent mode changed.
            if (PathAnimatorSettings.TangentMode != prevTangentMode) {
                HandleTangentModeChange();
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

        private void HandleTargetGOFieldChange(Transform prevTargetGO) {

// Handle adding reference.
            if (Script.TargetGO != prevTargetGO
                && prevTargetGO == null) {

                Script.Settings.RotationMode = RotationMode.Target;
                Script.UpdateAnimation();
            }
            // Handle removing reference.
            else if (Script.TargetGO != prevTargetGO
                     && prevTargetGO != null
                     && Script.TargetGO == null) {

                Script.Settings.RotationMode = RotationMode.Forward;
                Script.UpdateAnimation();
            }
        }

        private void DrawUpdateAllToggle() {
            PathAnimatorSettings.UpdateAllMode = EditorGUILayout.Toggle(
                new GUIContent(
                    "Update All Values",
                    ""),
                PathAnimatorSettings.UpdateAllMode);
        }

        private void DrawWrapModeDropdown() {
            PathAnimatorSettings.WrapMode = (WrapMode) EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Wrap Mode",
                    ""),
                PathAnimatorSettings.WrapMode);
        }

        private void DrawResetTiltingButton() {
            if (GUILayout.Button(
                new GUIContent(
                    "Reset Tilting",
                    ""))) {

                if (Script.PathData == null) return;

                Undo.RecordObject(Script.PathData, "Reset tilting curve.");

                // Reset curves to its default state.
                Script.PathData.ResetTiltingCurve();

                SceneView.RepaintAll();
            }
        }

        private void DrawResetEaseButton() {
            if (GUILayout.Button(
                new GUIContent(
                    "Reset Ease",
                    ""))) {

                if (Script.PathData == null) return;

                Undo.RecordObject(Script.PathData, "Reset ease curve.");

                // Reset curves to its default state.
                Script.PathData.ResetEaseCurve();

                SceneView.RepaintAll();
            }
        }

        private void DrawCreatePathAssetButton() {
            if (GUILayout.Button(
                new GUIContent(
                    "New Path",
                    ""))) {

                // Display save panel.
                var savePath = EditorUtility.SaveFilePanelInProject(
                    "Save Path Asset File",
                    // TODO Make it a property.
                    "Path",
                    "asset",
                    "");

                // Path cannot be empty.
                if (savePath == "") return;

                // Create new path asset.
                var asset = ScriptableObjectUtility.CreateAsset<PathData>(
                    savePath);

                // Assign asset as the current path.
                Script.PathData = asset;
            }
        }

        private void DrawResetPathInspectorButton() {
            if (GUILayout.Button(
                new GUIContent(
                    "Reset Path",
                    "Reset path to default."))) {

                if (Script.PathData == null) return;

                // Allow undo this operation.
                Undo.RecordObject(Script.PathData, "Change path");

                // Reset curves to its default state.
                Script.PathData.ResetPath();

                Script.UpdateAnimation();
            }
        }

        private void DrawRotationModeDropdown() {
            Undo.RecordObject(PathAnimatorSettings, "Change rotation mode.");

            // Remember current RotationMode.
            var prevRotationMode = PathAnimatorSettings.RotationMode;
            // Draw RotationMode dropdown.
            PathAnimatorSettings.RotationMode =
                (RotationMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Rotation Mode",
                        ""),
                    PathAnimatorSettings.RotationMode);

            // TODO Execute it as a callback.
            // If value changed, update animated GO in the scene.
            if (PathAnimatorSettings.RotationMode != prevRotationMode) {
                Script.UpdateAnimation();
            }
        }

        #endregion
        #region DRAWING HANDLERS

        private void HandleDrawingAddButtons() {
            // Get positions at which to draw movement handles.
            var nodePositions = Script.PathData.GetGlobalNodePositions(
                Script.ThisTransform);

            // Get style for add button.
            var addButtonStyle = Script.Skin.GetStyle(
                "AddButton");

            // Callback executed after add button was pressed.
            Action<int> callbackHandler = DrawAddNodeButtonsCallbackHandler;

            // Draw add node buttons.
            SceneHandles.DrawAddNodeButtons(
                nodePositions,
                callbackHandler,
                addButtonStyle);
        }

        private void HandleDrawingEaseHandles() {
            if (PathAnimatorSettings.HandleMode != HandleMode.Ease) return;

            // TODO Move this code to SceneHandles.DrawEaseHandles().

            // Get path node positions.
            var nodePositions =
                Script.PathData.GetGlobalNodePositions(Script.ThisTransform);

            // Get ease values.
            var easeCurveValues = Script.PathData.GetEaseCurveValues();

            // TODO Use property.
            var arcValueMultiplier = 360 / maxAnimationSpeed.floatValue;

            SceneHandles.DrawEaseHandles(
                nodePositions,
                easeCurveValues,
                arcValueMultiplier,
                DrawEaseHandlesCallbackHandler);
        }

        private void HandleDrawingEaseLabel() {
            if (PathAnimatorSettings.HandleMode != HandleMode.Ease) return;

            // Get node global positions.
            var nodeGlobalPositions = Script.PathData.GetGlobalNodePositions(
                Script.ThisTransform);

            SceneHandles.DrawArcHandleLabels(
                nodeGlobalPositions,
                ConvertEaseToDegrees,
                Script.Skin.GetStyle("EaseValueLabel"));
        }

        // TODO Use this method also for HandleDrawingMoveSinglePositionHandes().
        // .. Use parameters for differences.

        /// <summary>
        ///     Handle drawing movement handles.
        /// </summary>
        private void HandleDrawingPositionHandles() {
            SceneHandles.DrawMoveSinglePositionsHandles(
                Script,
                DrawPositionHandlesCallbackHandler);

            SceneHandles.DrawMoveAllPositionHandles(
                Script,
                DrawPositionHandlesCallbackHandler);
        }

        private void HandleDrawingRemoveButtons() {
            // Positions at which to draw movement handles.
            var nodes = Script.PathData.GetGlobalNodePositions(Script.ThisTransform);

            // Get style for add button.
            var removeButtonStyle = Script.Skin.GetStyle(
                "RemoveButton");

            // Callback to add a new node after add button was pressed.
            Action<int> removeNodeCallback =
                DrawRemoveNodeButtonsCallbackHandles;

            // Draw add node buttons.
            SceneHandles.DrawRemoveNodeButtons(
                nodes,
                removeNodeCallback,
                removeButtonStyle);
        }

        private void HandleDrawingRotationHandle() {
            if (PathAnimatorSettings.HandleMode != HandleMode.Rotation) return;

            var currentAnimationTime = Script.AnimationTimeRatio;
            var rotationPointPosition =
                Script.PathData.GetRotationAtTime(currentAnimationTime);
            var nodeTimestamps = Script.PathData.GetPathTimestamps();

            // Return if current animation time is not equal to any node
            // timestamp.
            var index = Array.FindIndex(
                nodeTimestamps,
                x => Math.Abs(x - currentAnimationTime)
                    < GlobalConstants.FloatPrecision);

            if (index < 0) return;

            SceneHandles.DrawRotationHandle(
                Script,
                rotationPointPosition,
                DrawRotationHandlesCallbackHandler);
        }

        private void HandleDrawingTiltingHandles() {
            if (PathAnimatorSettings.HandleMode != HandleMode.Tilting) return;

            Action<int, float> callbackHandler =
                DrawTiltingHandlesCallbackHandler;

            // Get path node positions.
            var nodePositions =
                Script.PathData.GetGlobalNodePositions(Script.ThisTransform);

            // Get tilting curve values.
            var tiltingCurveValues = Script.PathData.GetTiltingCurveValues();

            SceneHandles.DrawTiltingHandles(
                nodePositions,
                tiltingCurveValues,
                callbackHandler);
        }

        private void HandleDrawingTiltLabel() {
            if (PathAnimatorSettings.HandleMode != HandleMode.Tilting) return;

            // Get node global positions.
            var nodeGlobalPositions = Script.PathData.GetGlobalNodePositions(
                Script.ThisTransform);

            SceneHandles.DrawArcHandleLabels(
                nodeGlobalPositions,
                ConvertTiltToDegrees,
                Script.Skin.GetStyle("TiltValueLabel"));
        }

        #endregion DRAWING HANDLERS

        #region CALLBACK HANDLERS

        private void DrawAddNodeButtonsCallbackHandler(int nodeIndex) {
            // Make snapshot of the target object.
            Undo.RecordObject(Script.PathData, "Change path");

            // Add a new node.
            AddNodeBetween(nodeIndex);

            Script.PathData.DistributeTimestamps();

            // In Smooth mode mooth node tangents.
            if (PathAnimatorSettings.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
            // In Linear mode set node tangents to linear.
            else if (PathAnimatorSettings.TangentMode == TangentMode.Linear) {
                Script.PathData.SetNodesLinear();
            }

            // Update animated object.
            Script.UpdateAnimation();
        }

        private void DrawPositionHandlesCallbackHandler(
            int movedNodeIndex,
            Vector3 position,
            Vector3 moveDelta) {

            Undo.RecordObject(Script.PathData, "Change path");

            HandleMoveAllMovementMode(moveDelta);
            HandleMoveSingleHandleMove(movedNodeIndex, position);
        }

        private void DrawRemoveNodeButtonsCallbackHandles(
            int nodeIndex) {
            Undo.RecordObject(Script.PathData, "Change path");

            Script.PathData.RemoveNode(nodeIndex);
            Script.PathData.DistributeTimestamps();

            // In Smooth mode mooth node tangents.
            if (PathAnimatorSettings.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
            // In Linear mode set node tangents to linear.
            else if (PathAnimatorSettings.TangentMode == TangentMode.Linear) {
                Script.PathData.SetNodesLinear();
            }

            // Update animated object.
            Script.UpdateAnimation();
        }

        private void DrawEaseHandlesCallbackHandler(
            int keyIndex,
            float newValue) {
            Undo.RecordObject(Script.PathData, "Ease curve changed.");

            if (PathAnimatorSettings.UpdateAllMode) {
                var oldValue = Script.PathData.GetEaseValueAtIndex(keyIndex);
                var delta = newValue - oldValue;
                Script.PathData.UpdateEaseValues(delta);
            }
            else {
                Script.PathData.UpdateEaseValue(keyIndex, newValue);
            }
        }

        private void DrawRotationHandlesCallbackHandler(
            float timestamp,
            Vector3 newPosition) {
            Undo.RecordObject(Script.PathData, "Rotation path changed.");

            Script.PathData.ChangeRotationAtTimestamp(timestamp, newPosition);
        }

        private void DrawTiltingHandlesCallbackHandler(
            int keyIndex,
            float newValue) {
            Undo.RecordObject(Script.PathData, "Tilting curve changed.");

            if (PathAnimatorSettings.UpdateAllMode) {
                var oldValue = Script.PathData.GetTiltingValueAtIndex(keyIndex);
                var delta = newValue - oldValue;
                Script.PathData.UpdateTiltingValues(delta);
            }
            else {
                Script.PathData.UpdateNodeTilting(keyIndex, newValue);
            }
        }

        #endregion CALLBACK HANDLERS

        #region MODE HANDLERS

        private void HandleLinearTangentMode() {
            if (PathAnimatorSettings.TangentMode == TangentMode.Linear) {
                Script.PathData.SetNodesLinear();
            }
        }

        private void HandleMoveAllMovementMode(Vector3 moveDelta) {
            if (PathAnimatorSettings.MovementMode == MovementMode.MoveAll) {
                Script.PathData.OffsetNodePositions(moveDelta);
                Script.PathData.OffsetRotationPathPosition(moveDelta);
            }
        }

        private void HandleMoveSingleHandleMove(
            int movedNodeIndex,
            Vector3 position) {
            if (PathAnimatorSettings.MovementMode == MovementMode.MoveSingle) {

                Script.PathData.MoveNodeToPosition(movedNodeIndex, position);
                Script.PathData.DistributeTimestamps();

                HandleSmoothTangentMode();
                HandleLinearTangentMode();
            }
        }

        private void HandleSmoothTangentMode() {
            if (PathAnimatorSettings.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
        }

        private void HandleTangentModeChange() {
            if (Script.PathData == null) return;

            // Update path node tangents.
            if (PathAnimatorSettings.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
            else if (PathAnimatorSettings.TangentMode == TangentMode.Linear) {
                Script.PathData.SetNodesLinear();
            }

            SceneView.RepaintAll();
        }

        #endregion
        #region METHODS
        private void AddNodeBetween(int nodeIndex) {
            // Timestamp of node on which was taken action.
            var currentKeyTime = Script.PathData.GetNodeTimestamp(nodeIndex);
            // Get timestamp of the next node.
            var nextKeyTime = Script.PathData.GetNodeTimestamp(nodeIndex + 1);

            // Calculate timestamps for new key. It'll be placed exactly
            // between the two nodes.
            var newKeyTime =
                currentKeyTime +
                ((nextKeyTime - currentKeyTime) / 2);

            // Add node to the animation curves.
            Script.PathData.CreateNodeAtTime(newKeyTime);
        }

        private static void FocusOnSceneView() {
            if (SceneView.sceneViews.Count > 0) {
                var sceneView = (SceneView)SceneView.sceneViews[0];
                sceneView.Focus();
            }
        }

        //        ModKeyPressed = false;
        //    }
        //}
        private void CheckForSkinAsset() {

            if (Script.Skin == null) {
                Script.MissingReferenceError(
                    "Skin",
                    "Skin field cannot be empty. You will find default " +
                    "GUISkin in the \"animationpathtools/GUISkin\" folder");
            }
        }

        //        // Remember key state.
        //        ModKeyPressed = true;
        //    }
        //    // If modifier key was released..
        //    if (Event.current.type == EventType.keyUp
        //        && Event.current.keyCode == ModKey) {
        // TODO Move to PathData.
        private float ConvertEaseToDegrees(int nodeIndex) {
            // Calculate value to display.
            var easeValue = Script.PathData.GetNodeEaseValue(nodeIndex);
            var arcValueMultiplier = 360 / maxAnimationSpeed.floatValue;
            var easeValueInDegrees = easeValue * arcValueMultiplier;

            return easeValueInDegrees;
        }

        /// <summary>
        ///     Checked if modifier key is pressed and remember it in a class
        ///     field.
        /// </summary>
        //public void UpdateModifierKey() {
        //    // Check if modifier key is currently pressed.
        //    if (Event.current.type == EventType.keyDown
        //        && Event.current.keyCode == ModKey) {
        // TODO Move to PathData.
        private float ConvertTiltToDegrees(int nodeIndex) {
            var rotationValue = Script.PathData.GetNodeTiltValue(nodeIndex);

            return rotationValue;
        }

        private void InitializeSerializedProperties() {
            rotationSpeed = SettingsSerObj.FindProperty("rotationSpeed");
            animatedGO = serializedObject.FindProperty("animatedGO");
            targetGO = serializedObject.FindProperty("targetGO");
            forwardPointOffset =
                SettingsSerObj.FindProperty("forwardPointOffset");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");
            maxAnimationSpeed =
                SettingsSerObj.FindProperty("MaxAnimationSpeed");
            positionLerpSpeed =
                SettingsSerObj.FindProperty("positionLerpSpeed");
            pathData = serializedObject.FindProperty("pathData");
            enableControlsInPlayMode =
                SettingsSerObj.FindProperty("EnableControlsInPlayMode");
            skin = serializedObject.FindProperty("skin");
            settings = serializedObject.FindProperty("settings");
            gizmoCurveColor = SettingsSerObj.FindProperty("gizmoCurveColor");
            rotationCurveColor =
                SettingsSerObj.FindProperty("rotationCurveColor");
        }

        private void InstantiateCompositeClasses() {
            SettingsSerObj = new SerializedObject(Script.Settings);
            //PathExporterSerObj = new SerializedObject(PathExporter);
            SceneHandles = new SceneHandles(Script);
            Shortcuts = new Shortcuts(Script);
        }
  

        #endregion PRIVATE METHODS
    }

}