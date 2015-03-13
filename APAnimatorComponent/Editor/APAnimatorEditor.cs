using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathAnimator.APAnimatorComponent {

    [CustomEditor(typeof (APAnimator))]
    public sealed class APAnimatorEditor : Editor {
        
        #region PROPERTIES

        private SerializedObject SettingsSerObj { get; set; }
        //public SerializedObject PathExporterSerObj { get; private set; }

        /// <summary>
        ///     Reference to target script.
        /// </summary>
        private APAnimator Script { get; set; }

        private APAnimatorSettings Settings { get; set; }
        private PathData PathData { get; set; }

        private bool SerializedPropertiesInitialized { get; set; }
        #endregion 

        #region SERIALIZED PROPERTIES

        private SerializedProperty advancedSettingsFoldout;
        private SerializedProperty animatedGO;
        private SerializedProperty enableControlsInPlayMode;
        private SerializedProperty gizmoCurveColor;
        private SerializedProperty pathData;
        private SerializedProperty rotationCurveColor;
        private SerializedProperty rotationSlerpSpeed;
        private SerializedProperty skin;
        private SerializedProperty targetGO;
        private SerializedProperty settings;
        private SerializedProperty shortJumpValue;
        private SerializedProperty longJumpValue;
        private SerializedProperty subscribedToEvents;
        private SerializedProperty animationTime;
        #endregion SERIALIZED PROPERTIES

        #region UNITY MESSAGES

        public override void OnInspectorGUI() {
            // Check for required assets.
            if (!RequiredAssetsLoaded()) {
                DrawInfoLabel(Settings.AssetsNotLoadedInfoText);

                return;
            }

            // Check if serialized properties were initialized.
            if (!SerializedPropertiesInitialized) return;

            HandleUndoEvent();

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

            DrawRotationModeDropdown(HandleRotationModeChange);
            DrawWrapModeDropdown();

            EditorGUILayout.Space();

            DrawForwardPointOffsetSlider();

            DrawPositionSpeedSlider();
            
            EditorGUIUtility.labelWidth = 208;

            DrawRotationSpeedField();

            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.Space();

            GUILayout.Label("Other", EditorStyles.boldLabel);

            DrawExportControls();

            DrawAdvancedSettingsFoldout();
            DrawAdvancedSettingsControls();

            // Validate inspector SettingsAsset.
            // Not all inspector controls can be validated with OnValidate().
            if (GUI.changed) ValidateInspectorSettings();

            // Repaint scene after each inspector update.
            SceneView.RepaintAll();
        }

        private void DrawInfoLabel(string text) {
            EditorGUILayout.HelpBox(text, MessageType.Error);
        }

        private void OnEnable() {
            // Get target script reference.
            Script = (APAnimator) target;

            // Return is required assets are not referenced.
            if (!RequiredAssetsLoaded()) return;

            // Initialize helper property.
            Settings = Script.SettingsAsset;
            PathData = Script.PathData;

            InstantiateCompositeClasses();
            InitializeSerializedProperties();

            CopyIconsToGizmosFolder();
            SceneTool.RememberCurrentTool();
            FocusOnSceneView();
        }

        private bool RequiredAssetsLoaded() {
            var assetsLoaded = (bool) Utilities.InvokeMethodWithReflection(
                Script,
                "RequiredAssetsLoaded",
                null);

            return assetsLoaded;
        }

        private void OnDisable() {
            SceneTool.RestoreTool();
        }

        private void OnSceneGUI() {
            // Return is required assets are not referenced.
            if (!RequiredAssetsLoaded()) return;
            // Return if path asset is not referenced.
            if (PathData == null) return;
            // Return if serialized properties are not initialized.
            if (!SerializedPropertiesInitialized) return;

            // Subscribe animator to path events if not subscribed already.
            // This is required after animator component reset.
            if (!subscribedToEvents.boolValue) {
                Utilities.InvokeMethodWithReflection(
                    Script,
                    "SubscribeToEvents",
                    null);
            }

            // Return is SettingsAsset asset is not assigned in the inspector.
            //if (SettingsAsset == null) return;

            // Disable interaction with background scene elements.
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(
                FocusType.Passive));

            HandleShortcuts();

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
                "Check SettingsAsset Asset for shortcuts.",
                MessageType.Info);
        }

        private void DrawSettingsAssetField() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                settings,
                new GUIContent(
                    "SettingsAsset Asset",
                    ""));

            serializedObject.ApplyModifiedProperties();
        }

        private void HandleDrawingUpdateAllModeLabel() {
            if (!Settings.UpdateAllMode) return;

            // Get global node positions.
            var globalNodePositions = Script.GetGlobalNodePositions();

            // Create array with text to be displayed for each node.
            var labelText = new string[globalNodePositions.Length];
            for (int i = 0; i < globalNodePositions.Length; i++) {
                labelText[i] = Settings.UpdateAllLabelText;
            }

            SceneHandles.DrawUpdateAllLabels(
                globalNodePositions,
                labelText,
                Settings.UpdateAllLabelOffsetX,
                Settings.UpdateAllLabelOffsetY,
                Settings.DefaultLabelWidth,
                Settings.DefaultLabelHeight,
                Script.Skin.GetStyle("UpdateAllLabel"));
        }


        private void DrawAdvancedSettingsFoldout() {
            serializedObject.Update();

            advancedSettingsFoldout.boolValue = EditorGUILayout.Foldout(
                advancedSettingsFoldout.boolValue,
                new GUIContent(
                    "Advanced SettingsAsset",
                    ""));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawResetRotationPathButton() {
            if (GUILayout.Button(
                            new GUIContent(
                                "Reset Rotation",
                                ""))) {

                if (PathData == null) return;

                Undo.RecordObject(PathData, "Reset rotatio path.");

                // Reset curves to its default state.
                PathData.ResetRotationPath();

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

                serializedObject.Update();

                animationTime.floatValue = newTimeRatio;

                serializedObject.ApplyModifiedProperties();
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
                Settings.ForwardPointOffsetMinValue,
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

        //private void DrawMaxAnimationSpeedField() {

        //    serializedObject.Update();
        //    EditorGUILayout.PropertyField(maxAnimationSpeed);
        //    serializedObject.ApplyModifiedProperties();
        //}

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

                Script.StopAnimation();

                Utilities.InvokeMethodWithReflection(
                    Script,
                    "UpdateAnimation",
                    null);
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

                Script.SettingsAsset.RotationMode = RotationMode.Target;

                Utilities.InvokeMethodWithReflection(
                    Script,
                    "UpdateAnimation",
                    null);
            }
            // Handle removing reference.
            else if (Script.TargetGO != prevTargetGO
                     && prevTargetGO != null
                     && Script.TargetGO == null) {

                Script.SettingsAsset.RotationMode = RotationMode.Forward;

                Utilities.InvokeMethodWithReflection(
                    Script,
                    "UpdateAnimation",
                    null);
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
            Settings.WrapMode = (AnimatorWrapMode) EditorGUILayout.EnumPopup(
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

                if (PathData == null) return;

                Undo.RecordObject(PathData, "Reset tilting curve.");

                // Reset curves to its default state.
                PathData.ResetTiltingCurve();

                SceneView.RepaintAll();
            }
        }

        private void DrawResetEaseButton() {
            if (GUILayout.Button(
                new GUIContent(
                    "Reset Ease",
                    ""))) {

                if (PathData == null) return;

                Undo.RecordObject(PathData, "Reset ease curve.");

                // Reset curves to its default state.
                PathData.ResetEaseCurve();

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
                    Settings.PathDataAssetDefaultName,
                    "asset",
                    "");

                // Path cannot be empty.
                if (savePath == "") return;

                // Create new path asset.
                var asset = ScriptableObjectUtility.CreateAsset<PathData>(
                    fullPath: savePath);

                // Assign asset as the current path.
                PathData = asset;
            }
        }

        private void DrawResetPathInspectorButton() {
            if (GUILayout.Button(
                new GUIContent(
                    "Reset Path",
                    "Reset path to default."))) {

                if (PathData == null) return;

                // Allow undo this operation.
                Undo.RecordObject(PathData, "Change path");

                // Reset curves to its default state.
                PathData.ResetPath();

                Utilities.InvokeMethodWithReflection(
                    Script,
                    "UpdateAnimation",
                    null);
            }
        }

        private void DrawRotationModeDropdown(Action callback) {
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

            // If value changed, update animated GO in the scene.
            if (Settings.RotationMode != prevRotationMode) {
                callback();
            }
        }

        /// <summary>
        /// Called on rotation mode change.
        /// </summary>
        private void HandleRotationModeChange() {
            Utilities.InvokeMethodWithReflection(
                Script,
                "UpdateAnimation",
                null);
            Settings.HandleMode = HandleMode.None;
        }

        #endregion
        #region DRAWING HANDLERS

        private void HandleDrawingAddButtons() {
            // Get positions at which to draw movement handles.
            var nodePositions = Script.GetGlobalNodePositions();

            // Get style for add button.
            var addButtonStyle = Script.Skin.GetStyle(
                "AddButton");

            // Callback executed after add button was pressed.
            Action<int> callbackHandler = DrawAddNodeButtonsCallbackHandler;

            // Draw add node buttons.
            SceneHandles.DrawAddNodeButtons(
                nodePositions,
                Settings.AddButtonOffsetH,
                Settings.AddButtonOffsetV,
                callbackHandler,
                addButtonStyle);
        }

        private void HandleDrawingEaseHandles() {
            if (Settings.HandleMode != HandleMode.Ease) return;

            // Get path node positions.
            var nodePositions =
                Script.GetGlobalNodePositions();

            // Get ease values.
            var easeCurveValues = PathData.GetEaseCurveValues();

            var arcValueMultiplier = Settings.ArcValueMultiplierNumerator
                / Settings.MaxAnimationSpeed;

            SceneHandles.DrawEaseHandles(
                nodePositions,
                easeCurveValues,
                arcValueMultiplier,
                Settings.ArcHandleRadius,
                Settings.InitialArcValue,
                Settings.ScaleHandleSize,
                DrawEaseHandlesCallbackHandler);
        }

        private void HandleDrawingEaseLabel() {
            if (Settings.HandleMode != HandleMode.Ease) return;

            // Get node global positions.
            var nodeGlobalPositions = Script.GetGlobalNodePositions();

            SceneHandles.DrawArcHandleLabels(
                nodeGlobalPositions,
                Settings.EaseValueLabelOffsetX,
                Settings.EaseValueLabelOffsetY,
                Settings.DefaultLabelWidth,
                Settings.DefaultLabelHeight,
                ConvertEaseToDegrees,
                Script.Skin.GetStyle("EaseValueLabel"));
        }

        /// <summary>
        ///     Handle drawing movement handles.
        /// </summary>
        private void HandleDrawingPositionHandles() {
            var nodeGlobalPositions = Script.GetGlobalNodePositions();

            SceneHandles.DrawMoveSinglePositionsHandles(
                nodeGlobalPositions,
                Settings.MovementHandleSize,
                Settings.GizmoCurveColor,
                DrawPositionHandlesCallbackHandler);
        }

        private void HandleDrawingRemoveButtons() {
            // Positions at which to draw movement handles.
            var nodes = Script.GetGlobalNodePositions();

            // Get style for add button.
            var removeButtonStyle = Script.Skin.GetStyle(
                "RemoveButton");

            // Callback to add a new node after add button was pressed.
            Action<int> removeNodeCallback =
                DrawRemoveNodeButtonsCallbackHandles;

            // Draw add node buttons.
            SceneHandles.DrawRemoveNodeButtons(
                nodes,
                Settings.RemoveButtonH,
                Settings.RemoveButtonV,
                removeNodeCallback,
                removeButtonStyle);
        }

        private void HandleDrawingRotationHandle() {
            if (Settings.HandleMode != HandleMode.Rotation) return;

            var currentAnimationTime = Script.AnimationTime;
            var rotationPointPosition =
                PathData.GetRotationAtTime(currentAnimationTime);
            var rotationPointGlobalPosition =
                Script.ThisTransform.TransformPoint(rotationPointPosition);
            var nodeTimestamps = PathData.GetPathTimestamps();

            // Return if current animation time is not equal to any node
            // timestamp.
            var index = Array.FindIndex(
                nodeTimestamps,
                x => Math.Abs(x - currentAnimationTime)
                    < GlobalConstants.FloatPrecision);

            if (index < 0) return;

            SceneHandles.DrawRotationHandle(
                rotationPointGlobalPosition,
                Settings.RotationHandleSize,
                Settings.RotationHandleColor,
                DrawRotationHandlesCallbackHandler);
        }

        private void HandleDrawingTiltingHandles() {
            if (Settings.HandleMode != HandleMode.Tilting) return;

            Action<int, float> callbackHandler =
                DrawTiltingHandlesCallbackHandler;

            // Get path node positions.
            var nodePositions =
                Script.GetGlobalNodePositions();

            // Get tilting curve values.
            var tiltingCurveValues = PathData.GetTiltingCurveValues();

            SceneHandles.DrawTiltingHandles(
                nodePositions,
                tiltingCurveValues,
                Settings.ArcHandleRadius,
                Settings.InitialArcValue,
                Settings.ScaleHandleSize,
                callbackHandler);
        }

        private void HandleDrawingTiltLabel() {
            if (Settings.HandleMode != HandleMode.Tilting) return;

            // Get node global positions.
            var nodeGlobalPositions = Script.GetGlobalNodePositions();

            SceneHandles.DrawArcHandleLabels(
                nodeGlobalPositions,
                Settings.EaseValueLabelOffsetX,
                Settings.EaseValueLabelOffsetY, 
                Settings.DefaultLabelWidth,
                Settings.DefaultLabelHeight,
                ConvertTiltToDegrees,
                Script.Skin.GetStyle("TiltValueLabel"));
        }

        #endregion DRAWING HANDLERS

        #region CALLBACK HANDLERS

        private void DrawAddNodeButtonsCallbackHandler(int nodeIndex) {
            // Make snapshot of the target object.
            Undo.RecordObject(PathData, "Change path");

            // Add a new node.
            AddNodeBetween(nodeIndex);

            PathData.DistributeTimestamps();

            // In Smooth mode mooth node tangents.
            if (Settings.TangentMode == TangentMode.Smooth) {
                PathData.SmoothAllNodeTangents();
            }
            // In Linear mode set node tangents to linear.
            else if (Settings.TangentMode == TangentMode.Linear) {
                PathData.SetNodesLinear();
            }

            // Update animated object.
            Utilities.InvokeMethodWithReflection(
                Script,
                "UpdateAnimation",
                null);
        }

        private void DrawPositionHandlesCallbackHandler(
            int movedNodeIndex,
            Vector3 newGlobalPos) {

            Undo.RecordObject(PathData, "Change path");

            // Calculate node new local position.
            var newNodeLocalPosition =
                Script.ThisTransform.InverseTransformPoint(newGlobalPos);

            PathData.MoveNodeToPosition(movedNodeIndex, newNodeLocalPosition);
            PathData.DistributeTimestamps();

            HandleSmoothTangentMode();
            HandleLinearTangentMode();
        }

        private void DrawRemoveNodeButtonsCallbackHandles(
            int nodeIndex) {
            Undo.RecordObject(PathData, "Change path");

            PathData.RemoveNode(nodeIndex);
            PathData.DistributeTimestamps();

            // In Smooth mode mooth node tangents.
            if (Settings.TangentMode == TangentMode.Smooth) {
                PathData.SmoothAllNodeTangents();
            }
            // In Linear mode set node tangents to linear.
            else if (Settings.TangentMode == TangentMode.Linear) {
                PathData.SetNodesLinear();
            }

            // Update animated object.
            Utilities.InvokeMethodWithReflection(
                Script,
                "UpdateAnimation",
                null);
        }

        private void DrawEaseHandlesCallbackHandler(
            int keyIndex,
            float newValue) {
            Undo.RecordObject(PathData, "Ease curve changed.");

            if (Settings.UpdateAllMode) {
                var oldValue = PathData.GetEaseValueAtIndex(keyIndex);
                var delta = newValue - oldValue;
                PathData.UpdateEaseValues(delta);
            }
            else {
                PathData.UpdateEaseValue(keyIndex, newValue);
            }
        }

        private void DrawRotationHandlesCallbackHandler(
            Vector3 newPosition) {

            Undo.RecordObject(PathData, "Rotation path changed.");

            var newLocalPos =
                Script.ThisTransform.InverseTransformPoint(newPosition);

            PathData.ChangeRotationAtTimestamp(
                Script.AnimationTime,
                newLocalPos);
        }

        private void DrawTiltingHandlesCallbackHandler(
            int keyIndex,
            float newValue) {
            Undo.RecordObject(PathData, "Tilting curve changed.");

            if (Settings.UpdateAllMode) {
                var oldValue = PathData.GetTiltingValueAtIndex(keyIndex);
                var delta = newValue - oldValue;
                PathData.UpdateTiltingValues(delta);
            }
            else {
                PathData.UpdateNodeTilting(keyIndex, newValue);
            }
        }

        #endregion CALLBACK HANDLERS

        #region MODE HANDLERS

        private void HandleLinearTangentMode() {
            if (Settings.TangentMode == TangentMode.Linear) {
                PathData.SetNodesLinear();
            }
        }

        private void HandleSmoothTangentMode() {
            if (Settings.TangentMode == TangentMode.Smooth) {
                PathData.SmoothAllNodeTangents();
            }
        }

        private void HandleTangentModeChange() {
            if (PathData == null) return;

            // Update path node tangents.
            if (Settings.TangentMode == TangentMode.Smooth) {
                PathData.SmoothAllNodeTangents();
            }
            else if (Settings.TangentMode == TangentMode.Linear) {
                PathData.SetNodesLinear();
            }

            SceneView.RepaintAll();
        }

        #endregion
        #region METHODS
        private void HandleUndoEvent() {
            if (Event.current.type == EventType.ValidateCommand
                && Event.current.commandName == "UndoRedoPerformed") {

                // Repaint inspector.
                Repaint();
                // Update path with new tangent setting.
                HandleTangentModeChange();
                // Update animated object.
                Utilities.InvokeMethodWithReflection(
                    Script,
                    "UpdateAnimation",
                    null);
            }
        }

        private void AddNodeBetween(int nodeIndex) {
            // Timestamp of node on which was taken action.
            var currentKeyTime = PathData.GetNodeTimestamp(nodeIndex);
            // Get timestamp of the next node.
            var nextKeyTime = PathData.GetNodeTimestamp(nodeIndex + 1);

            // Calculate timestamps for new key. It'll be placed exactly
            // between the two nodes.
            var newKeyTime =
                currentKeyTime +
                ((nextKeyTime - currentKeyTime) / 2);

            // Add node to the animation curves.
            PathData.CreateNodeAtTime(newKeyTime);
        }

        private static void FocusOnSceneView() {
            if (SceneView.sceneViews.Count > 0) {
                var sceneView = (SceneView)SceneView.sceneViews[0];
                sceneView.Focus();
            }
        }

        private float ConvertEaseToDegrees(int nodeIndex) {
            // Calculate value to display.
            var easeValue = PathData.GetNodeEaseValue(nodeIndex);
            var arcValueMultiplier = Settings.ArcValueMultiplierNumerator
                / Settings.MaxAnimationSpeed;
            var easeValueInDegrees = easeValue * arcValueMultiplier;

            return easeValueInDegrees;
        }

        private float ConvertTiltToDegrees(int nodeIndex) {
            var rotationValue = PathData.GetNodeTiltValue(nodeIndex);

            return rotationValue;
        }

        private void InitializeSerializedProperties() {
            rotationSlerpSpeed = SettingsSerObj.FindProperty("rotationSlerpSpeed");
            animatedGO = serializedObject.FindProperty("animatedGO");
            targetGO = serializedObject.FindProperty("targetGO");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");
            pathData = serializedObject.FindProperty("pathData");
            enableControlsInPlayMode =
                SettingsSerObj.FindProperty("enableControlsInPlayMode");
            skin = serializedObject.FindProperty("skin");
            settings = serializedObject.FindProperty("settingsAsset");
            gizmoCurveColor = SettingsSerObj.FindProperty("gizmoCurveColor");
            rotationCurveColor =
                SettingsSerObj.FindProperty("rotationCurveColor");
            shortJumpValue = SettingsSerObj.FindProperty("shortJumpValue");
            longJumpValue = SettingsSerObj.FindProperty("longJumpValue");
            subscribedToEvents =
                serializedObject.FindProperty("subscribedToEvents");
            animationTime =
                serializedObject.FindProperty("animationTime");

            SerializedPropertiesInitialized = true;
        }

        private void InstantiateCompositeClasses() {
            SettingsSerObj = new SerializedObject(Settings);
        }
        private void ValidateInspectorSettings() {
            if (Settings == null) return;

            // Limit PositionLerpSpeed value.
            //if (SettingsAsset.PositionLerpSpeed < 0) {
            //    SettingsAsset.PositionLerpSpeed = 0;
            //}
            //else if (SettingsAsset.PositionLerpSpeed > 1) {
            //    SettingsAsset.PositionLerpSpeed = 1;
            //}

            // Limit RotationSpeed value.
            if (Settings.RotationSlerpSpeed < 0) {
                Settings.RotationSlerpSpeed = 0;
            }

            // Limit ForwardPointOffset value.
            //if (SettingsAsset.ForwardPointOffset < 0.001f) {
            //    SettingsAsset.ForwardPointOffset = 0.001f;
            //}
            //else if (SettingsAsset.ForwardPointOffset > 1) {
            //    SettingsAsset.ForwardPointOffset = 1;
            //}

            // Limit ExmportSamplingFrequency value.
            if (Settings.ExportSamplingFrequency < 1) {
                Settings.ExportSamplingFrequency = 1;
            }
            //else if (SettingsAsset.ExportSamplingFrequency > 100) {
            //    SettingsAsset.ExportSamplingFrequency = 100;
            //}

            // Limit ShortJumpValue.
            //if (SettingsAsset.ShortJumpValue < 0) {
            //    SettingsAsset.ShortJumpValue = 0;
            //}
            //else if (SettingsAsset.ShortJumpValue > 1) {
            //    SettingsAsset.ShortJumpValue = 1;
            //}

            // Limit LongJumpValue.
            //if (SettingsAsset.LongJumpValue < 0) {
            //    SettingsAsset.LongJumpValue = 0;
            //}
            //else if (SettingsAsset.LongJumpValue > 1) {
            //    SettingsAsset.LongJumpValue = 1;
            //}
        }

        private void CopyIconsToGizmosFolder() {
            // Path to Unity Gizmos folder.
            var gizmosDir = Application.dataPath + "/Gizmos";

            // Path to ATP folder inside Gizmos.

            // Create Asset/Gizmos folder if not exists.
            if (!Directory.Exists(gizmosDir + "ATP")) {
                Directory.CreateDirectory(gizmosDir + "ATP");
            }

            // Check if messageSettings asset has icons specified.
            if (Settings.GizmoIcons == null) return;

            // For each icon..
            foreach (var icon in Settings.GizmoIcons) {
                // Get icon path.
                var iconPath = AssetDatabase.GetAssetPath(icon);

                // Copy icon to Gizmos folder.
                AssetDatabase.CopyAsset(iconPath, gizmosDir + "/ATP");
            }
        }
        #endregion PRIVATE METHODS

        #region SHORTCUTS

        private void HandleShortcuts() {
            serializedObject.Update();

            Utilities.HandleUnmodShortcut(
                Settings.EaseModeKey,
                () => Settings.HandleMode = HandleMode.Ease);

            Utilities.HandleUnmodShortcut(
                Settings.RotationModeKey,
                () => Settings.HandleMode = HandleMode.Rotation);

            Utilities.HandleUnmodShortcut(
                Settings.TiltingModeKey,
                () => Settings.HandleMode = HandleMode.Tilting);

            Utilities.HandleUnmodShortcut(
                Settings.NoneModeKey,
                () => Settings.HandleMode = HandleMode.None);

            Utilities.HandleUnmodShortcut(
                Settings.UpdateAllKey,
                () => Settings.UpdateAllMode = !Settings.UpdateAllMode);

            // Short jump forward.
            Utilities.HandleModShortcut(
                () => {
                    var newAnimationTimeRatio =
                        animationTime.floatValue + Settings.ShortJumpValue;

                    animationTime.floatValue =
                        (float)(Math.Round(newAnimationTimeRatio, 3));
                },
                Settings.ShortJumpForwardKey,
                Event.current.alt);

            // Short jump backward.
            Utilities.HandleModShortcut(
                () => {
                    var newAnimationTimeRatio =
                        animationTime.floatValue - Settings.ShortJumpValue;

                    animationTime.floatValue =
                        (float)(Math.Round(newAnimationTimeRatio, 3));
                },
                Settings.ShortJumpBackwardKey,
                Event.current.alt);

            // Long jump forward.
            Utilities.HandleUnmodShortcut(
                Settings.LongJumpForwardKey,
                () => animationTime.floatValue +=
                    Settings.LongJumpValue);

            // Long jump backward.
            Utilities.HandleUnmodShortcut(
                Settings.LongJumpBackwardKey,
                () => animationTime.floatValue -=
                    Settings.LongJumpValue);

            // Jump to next node.
            Utilities.HandleUnmodShortcut(
                Settings.JumpToNextNodeKey,
                () => animationTime.floatValue =
                    GetNearestForwardNodeTimestamp());

            // Jump to previous node.
            Utilities.HandleUnmodShortcut(
                Settings.JumpToPreviousNodeKey,
                () => animationTime.floatValue =
                    GetNearestBackwardNodeTimestamp());

            // Jump to start.
            Utilities.HandleModShortcut(
                () => animationTime.floatValue = 0,
                Settings.JumpToStartKey,
                //ModKeyPressed);
                Event.current.alt);

            // Jump to end.
            Utilities.HandleModShortcut(
                () => animationTime.floatValue = 1,
                Settings.JumpToEndKey,
                //ModKeyPressed);
                Event.current.alt);

            // Play/pause animation.
            Utilities.HandleUnmodShortcut(
                Settings.PlayPauseKey,
                HandlePlayPause);

            serializedObject.ApplyModifiedProperties();
        }

        private void HandlePlayPause() {
            Utilities.InvokeMethodWithReflection(
               Script,
               "HandlePlayPause",
               null);
        }

        private float GetNearestBackwardNodeTimestamp() {
            var pathTimestamps = PathData.GetPathTimestamps();

            for (var i = pathTimestamps.Length - 1; i >= 0; i--) {
                if (pathTimestamps[i] < animationTime.floatValue) {
                    return pathTimestamps[i];
                }
            }

            // Return timestamp of the last node.
            return 0;
        }

        private float GetNearestForwardNodeTimestamp() {
            var pathTimestamps = PathData.GetPathTimestamps();

            foreach (var timestamp in pathTimestamps
                .Where(timestamp => timestamp > animationTime.floatValue)) {
                return timestamp;
            }

            // Return timestamp of the last node.
            return 1.0f;
        }

        #endregion
        #region EXPORTER

        public void DrawExportControls() {
            EditorGUILayout.BeginHorizontal();

            Settings.ExportSamplingFrequency = EditorGUILayout.IntField(
                new GUIContent(
                    "Export Sampling",
                    "Number of points to export for 1 m of the curve. " +
                    "If set to 0, it'll export only keys defined in " +
                    "the curve."),
                Settings.ExportSamplingFrequency);

            if (GUILayout.Button("Export")) {
                ExportNodes(
                    PathData,
                    Script.ThisTransform,
                    Settings.ExportSamplingFrequency);
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        ///     Export Animation Path nodes as transforms.
        /// </summary>
        /// <param name="exportSampling">
        ///     Amount of result transforms for one meter of Animation Path.
        /// </param>
        private void ExportNodes(
            PathData pathData,
            Transform transform,
            int exportSampling) {

            // exportSampling cannot be less than 0.
            if (exportSampling < 0) return;

            // Points to be exported.
            List<Vector3> points;

            // Initialize points array with nodes to export.
            points = pathData.SampleAnimationPathForPoints(
                exportSampling);

            // Convert points to global coordinates.
            Utilities.ConvertToGlobalCoordinates(ref points, transform);
            //for (int i = 0; i < points.Count; i++) {
            //    points[i] = transform.TransformPoint(points[i]);
            //}

            // Create parent GO.
            var exportedPath = new GameObject("exported_path");

            // Create child GOs.
            for (var i = 0; i < points.Count; i++) {
                // Create child GO.
                var nodeGo = new GameObject("Node " + i);

                // Move node under the path GO.
                nodeGo.transform.parent = exportedPath.transform;

                // Assign node local position.
                nodeGo.transform.localPosition = points[i];
            }
        }

        #endregion
    }

}