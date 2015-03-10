using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathAnimator.APAnimatorComponent {

    [CustomEditor(typeof (APAnimator))]
    public sealed class APAnimatorEditor : Editor {
        
        #region PROPERTIES
        private GizmoIcons GizmoIcons { get; set; }

        private SceneHandles SceneHandles { get; set; }

        private SerializedObject SettingsSerObj { get; set; }
        //public SerializedObject PathExporterSerObj { get; private set; }

        /// <summary>
        ///     Reference to target script.
        /// </summary>
        private APAnimator Script { get; set; }

        private Shortcuts Shortcuts { get; set; }

        private APAnimatorSettings Settings { get; set; }

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
        //private SerializedProperty positionLerpSpeed;
        private SerializedProperty rotationCurveColor;
        private SerializedProperty rotationSlerpSpeed;
        private SerializedProperty skin;
        private SerializedProperty targetGO;
        private SerializedProperty settings;
        private SerializedProperty shortJumpValue;
        private SerializedProperty longJumpValue;
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

            GUILayout.Label("References", EditorStyles.boldLabel);

            DrawAnimatedGOField();
            DrawTargetGOField();

            EditorGUILayout.Space();


            GUILayout.Label("Scene Tools", EditorStyles.boldLabel);

            DrawHandleModeDropdown();
            DrawTangentModeDropdown();
            DrawUpdateAllToggle();

            EditorGUILayout.BeginHorizontal();

            DrawResetEaseButton();
            DrawResetRotationPathButton();
            DrawResetTiltingButton();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUILayout.Label("Player", EditorStyles.boldLabel);

            DrawAnimationTimeValue();

            DrawAutoPlayControl();
            DrawEnableControlsInPlayModeToggle();

            //EditorGUILayout.Space();

            DrawPlayerControls();

            EditorGUILayout.Space();

            EditorGUIUtility.labelWidth = 0;

            //DrawShortcutsHelpBox();
            EditorGUILayout.Space();

            GUILayout.Label("Player Options", EditorStyles.boldLabel);

            DrawRotationModeDropdown();
            DrawWrapModeDropdown();

            EditorGUILayout.Space();

            DrawForwardPointOffsetSlider();

            DrawPositionSpeedSlider();
            
            EditorGUIUtility.labelWidth = 208;

            DrawRotationSpeedField();

            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.Space();

            GUILayout.Label("Other", EditorStyles.boldLabel);

            PathExporter.DrawExportControls();

            DrawAdvancedSettingsFoldout();
            DrawAdvancedSettingsControls();

            // Validate inspector messageSettings.
            // Not all inspector controls can be validated with OnValidate().
            if (GUI.changed) ValidateInspectorSettings();

            // Repaint scene after each inspector update.
            SceneView.RepaintAll();
        }

        private void OnEnable() {
            // Get target script reference.
            Script = (APAnimator) target;

            if (Script.Settings != null) {
                // Initialize messageSettings property.
                Settings = Script.Settings;
            }

            InstantiateCompositeClasses();
            InitializeSerializedProperties();

            SceneTool.RememberCurrentTool();
            FocusOnSceneView();
            GizmoIcons.CopyIconsToGizmosFolder();
        }
        private void OnDisable() {
            SceneTool.RestoreTool();
        }

        private void OnSceneGUI() {
            CheckForSkinAsset();

            // Return if path asset does not exist.
            if (Script.PathData == null) return;

            // Return is messageSettings asset is not assigned in the inspector.
            if (Settings == null) return;

            // Disable interaction with background scene elements.
            HandleUtility.AddDefaultControl(
                GUIUtility.GetControlID(
                    FocusType.Passive));

            Shortcuts.HandleShortcuts();

            //Script.UpdateWrapMode();

            HandleDrawingEaseHandles();
            HandleDrawingTiltingHandles();
            HandleDrawingEaseLabel();
            HandleDrawingTiltLabel();
            HandleDrawingUpdateAllModeLabel();
            HandleDrawingPositionHandles();
            HandleDrawingRotationHandle();
            HandleDrawingAddButtons();
            HandleDrawingRemoveButtons();

            // Repaint inspector if any key was pressed.
            // Inspector needs to be redrawn after option is changed
            // with keyboard shortcut.
            if (Event.current.type == EventType.keyUp) {
                Repaint();
            }
        }
        #endregion UNITY MESSAGES

        #region INSPECTOR
        private void DrawShortcutsHelpBox() {
            EditorGUILayout.HelpBox(
                "Check messageSettings Asset for shortcuts.",
                MessageType.Info);
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

        private void HandleDrawingUpdateAllModeLabel() {
            if (!Settings.UpdateAllMode) return;

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
                    "Advanced messageSettings",
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

                EditorGUILayout.Space();

                DrawShortJumpValueField();
                DrawLongJumpValueField();

                EditorGUILayout.Space();

                DrawSettingsAssetField();
                DrawSkinSelectionControl();
            }
        }

        private void DrawLongJumpValueField() {
            SettingsSerObj.Update();

            EditorGUILayout.PropertyField(longJumpValue);

            SettingsSerObj.ApplyModifiedProperties();
        }

        private void DrawShortJumpValueField() {
            SettingsSerObj.Update();

            EditorGUILayout.PropertyField(shortJumpValue);

            SettingsSerObj.ApplyModifiedProperties();
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

        private void DrawAnimationTimeValue() {
            Undo.RecordObject(target, "Update AnimationTime");

            float newTimeRatio = 0;

            newTimeRatio = DrawAnimationTimeSlider();

            // Update AnimationTime only when value was changed.
            if (Math.Abs(newTimeRatio - Script.AnimationTime)
                > GlobalConstants.FloatPrecision) {

                Script.AnimationTime = newTimeRatio;
            }
        }

        private float DrawAnimationTimeFloatField() {
            var newTimeRatio = EditorGUILayout.FloatField(
                new GUIContent(
                    "Animation Time",
                    ""),
                Script.AnimationTime);

            return newTimeRatio;
        }

        private float DrawAnimationTimeSlider() {
            var newTimeRatio = EditorGUILayout.Slider(
                new GUIContent(
                    "Animation Time",
                    "Current animation time."),
                Script.AnimationTime,
                0,
                1);

            return newTimeRatio;
        }

        private void DrawAutoPlayControl() {
            Settings.AutoPlay = EditorGUILayout.Toggle(
                new GUIContent(
                    "Auto Play",
                    ""),
                Settings.AutoPlay);
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

        private void DrawForwardPointOffsetSlider() {
            Settings.ForwardPointOffset = EditorGUILayout.Slider(
                new GUIContent(
                    "Forward Point Offset",
                    ""), 
                Settings.ForwardPointOffset,
                // TODO Create field in messageSettings asset.
                0.001f,
                1);
        }

        private void DrawGizmoCurveColorPicker() {
            SettingsSerObj.Update();

            EditorGUILayout.PropertyField(
                gizmoCurveColor,
                new GUIContent("Curve Color", ""));

            SettingsSerObj.ApplyModifiedProperties();
        }

        private void DrawHandleModeDropdown() {
            Undo.RecordObject(Settings, "Change handle mode.");

            Settings.HandleMode = (HandleMode) EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Handle Mode",
                    ""),
                Settings.HandleMode);
        }

        private void DrawMaxAnimationSpeedField() {

            serializedObject.Update();
            EditorGUILayout.PropertyField(maxAnimationSpeed);
            serializedObject.ApplyModifiedProperties();
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

        private void DrawPositionSpeedSlider() {
            Settings.PositionLerpSpeed = EditorGUILayout.Slider(
                new GUIContent(
                    "Position Lerp Speed",
                    ""),
                Settings.PositionLerpSpeed,
                0,
                1);
        }

        private void DrawRotationCurveColorPicker() {
            SettingsSerObj.Update();

            EditorGUILayout.PropertyField(rotationCurveColor);

            SettingsSerObj.ApplyModifiedProperties();
        }

        private void DrawRotationSpeedField() {
            SettingsSerObj.Update();

            EditorGUILayout.PropertyField(
                rotationSlerpSpeed,
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
            var prevTangentMode = Settings.TangentMode;

            // Draw tangent mode dropdown.
            Settings.TangentMode =
                (TangentMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Tangent Mode",
                        ""),
                    Settings.TangentMode);

            // Update gizmo curve is tangent mode changed.
            if (Settings.TangentMode != prevTangentMode) {
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
            Settings.UpdateAllMode = EditorGUILayout.Toggle(
                new GUIContent(
                    "Update All Values",
                    ""),
                Settings.UpdateAllMode);
        }

        private void DrawWrapModeDropdown() {
            Settings.WrapMode = (WrapMode) EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Wrap Mode",
                    ""),
                Settings.WrapMode);
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
            Undo.RecordObject(Settings, "Change rotation mode.");

            // Remember current RotationMode.
            var prevRotationMode = Settings.RotationMode;

            // Draw RotationMode dropdown.
            Settings.RotationMode =
                (RotationMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Rotation Mode",
                        ""),
                    Settings.RotationMode);

            // TODO Execute it as a callback.
            // If value changed, update animated GO in the scene.
            if (Settings.RotationMode != prevRotationMode) {
                HandleRotationModeChange();
            }
        }

        /// <summary>
        /// Called on rotation mode change.
        /// </summary>
        private void HandleRotationModeChange() {
            Script.UpdateAnimation();
            Settings.HandleMode = HandleMode.None;
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
            if (Settings.HandleMode != HandleMode.Ease) return;

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
            if (Settings.HandleMode != HandleMode.Ease) return;

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
            if (Settings.HandleMode != HandleMode.Rotation) return;

            var currentAnimationTime = Script.AnimationTime;
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
            if (Settings.HandleMode != HandleMode.Tilting) return;

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
            if (Settings.HandleMode != HandleMode.Tilting) return;

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
            if (Settings.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
            // In Linear mode set node tangents to linear.
            else if (Settings.TangentMode == TangentMode.Linear) {
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

            Script.PathData.MoveNodeToPosition(movedNodeIndex, position);
            Script.PathData.DistributeTimestamps();

            HandleSmoothTangentMode();
            HandleLinearTangentMode();
        }

        private void DrawRemoveNodeButtonsCallbackHandles(
            int nodeIndex) {
            Undo.RecordObject(Script.PathData, "Change path");

            Script.PathData.RemoveNode(nodeIndex);
            Script.PathData.DistributeTimestamps();

            // In Smooth mode mooth node tangents.
            if (Settings.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
            // In Linear mode set node tangents to linear.
            else if (Settings.TangentMode == TangentMode.Linear) {
                Script.PathData.SetNodesLinear();
            }

            // Update animated object.
            Script.UpdateAnimation();
        }

        private void DrawEaseHandlesCallbackHandler(
            int keyIndex,
            float newValue) {
            Undo.RecordObject(Script.PathData, "Ease curve changed.");

            if (Settings.UpdateAllMode) {
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

            if (Settings.UpdateAllMode) {
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
            if (Settings.TangentMode == TangentMode.Linear) {
                Script.PathData.SetNodesLinear();
            }
        }

        private void HandleSmoothTangentMode() {
            if (Settings.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
        }

        private void HandleTangentModeChange() {
            if (Script.PathData == null) return;

            // Update path node tangents.
            if (Settings.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
            else if (Settings.TangentMode == TangentMode.Linear) {
                Script.PathData.SetNodesLinear();
            }

            SceneView.RepaintAll();
        }

        #endregion
        #region METHODS
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
            rotationSlerpSpeed = SettingsSerObj.FindProperty("rotationSlerpSpeed");
            animatedGO = serializedObject.FindProperty("animatedGO");
            targetGO = serializedObject.FindProperty("targetGO");
            forwardPointOffset =
                SettingsSerObj.FindProperty("forwardPointOffset");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");
            maxAnimationSpeed =
                SettingsSerObj.FindProperty("MaxAnimationSpeed");
            //positionLerpSpeed =
            //    SettingsSerObj.FindProperty("positionLerpSpeed");
            pathData = serializedObject.FindProperty("pathData");
            enableControlsInPlayMode =
                SettingsSerObj.FindProperty("EnableControlsInPlayMode");
            skin = serializedObject.FindProperty("skin");
            settings = serializedObject.FindProperty("settings");
            gizmoCurveColor = SettingsSerObj.FindProperty("gizmoCurveColor");
            rotationCurveColor =
                SettingsSerObj.FindProperty("rotationCurveColor");
            shortJumpValue = SettingsSerObj.FindProperty("shortJumpValue");
            longJumpValue = SettingsSerObj.FindProperty("longJumpValue");
        }

        private void InstantiateCompositeClasses() {
            SceneHandles = new SceneHandles(Script);
            Shortcuts = new Shortcuts(Script);
            PathExporter = new PathExporter(Script);

            if (Settings == null) return;

            GizmoIcons = new GizmoIcons(Settings);
            SettingsSerObj = new SerializedObject(Settings);
        }
        private void ValidateInspectorSettings() {
            if (Settings == null) return;

            // Limit PositionLerpSpeed value.
            //if (messageSettings.PositionLerpSpeed < 0) {
            //    messageSettings.PositionLerpSpeed = 0;
            //}
            //else if (messageSettings.PositionLerpSpeed > 1) {
            //    messageSettings.PositionLerpSpeed = 1;
            //}

            // Limit RotationSpeed value.
            if (Settings.RotationSlerpSpeed < 0) {
                Settings.RotationSlerpSpeed = 0;
            }

            // Limit ForwardPointOffset value.
            //if (messageSettings.ForwardPointOffset < 0.001f) {
            //    messageSettings.ForwardPointOffset = 0.001f;
            //}
            //else if (messageSettings.ForwardPointOffset > 1) {
            //    messageSettings.ForwardPointOffset = 1;
            //}

            // Limit ExmportSamplingFrequency value.
            if (Settings.ExportSamplingFrequency < 1) {
                Settings.ExportSamplingFrequency = 1;
            }
            //else if (messageSettings.ExportSamplingFrequency > 100) {
            //    messageSettings.ExportSamplingFrequency = 100;
            //}

            // Limit ShortJumpValue.
            //if (messageSettings.ShortJumpValue < 0) {
            //    messageSettings.ShortJumpValue = 0;
            //}
            //else if (messageSettings.ShortJumpValue > 1) {
            //    messageSettings.ShortJumpValue = 1;
            //}

            // Limit LongJumpValue.
            //if (messageSettings.LongJumpValue < 0) {
            //    messageSettings.LongJumpValue = 0;
            //}
            //else if (messageSettings.LongJumpValue > 1) {
            //    messageSettings.LongJumpValue = 1;
            //}
        }

        #endregion PRIVATE METHODS
    }

}