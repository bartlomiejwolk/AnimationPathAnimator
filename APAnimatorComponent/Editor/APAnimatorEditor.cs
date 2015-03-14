using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathAnimator.APAnimatorComponent {

    [CustomEditor(typeof (APAnimator))]
    public sealed class APAnimatorEditor : Editor {
        #region PROPERTIES

        /// <summary>
        ///     Reference to target script.
        /// </summary>
        private APAnimator Script { get; set; }

        private bool SerializedPropertiesInitialized { get; set; }

        private SerializedObject SettingsSerializedObject { get; set; }

        #endregion

        #region SERIALIZED PROPERTIES

        private SerializedProperty advancedSettingsFoldout;
        private SerializedProperty animatedGO;
        private SerializedProperty animationTime;
        private SerializedProperty enableControlsInPlayMode;
        private SerializedProperty gizmoCurveColor;
        private SerializedProperty longJumpValue;
        private SerializedProperty pathData;
        private SerializedProperty rotationCurveColor;
        private SerializedProperty rotationSlerpSpeed;
        private SerializedProperty settings;
        private SerializedProperty shortJumpValue;
        private SerializedProperty skin;
        private SerializedProperty subscribedToEvents;
        private SerializedProperty targetGO;

        #endregion SERIALIZED PROPERTIES

        #region UNITY MESSAGES

        public override void OnInspectorGUI() {
            // Check for required assets.
            if (!RequiredAssetsLoaded()) {
                DrawInfoLabel(Script.SettingsAsset.AssetsNotLoadedInfoText);

                return;
            }

            // Check if serialized properties are initialized.
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

            DrawPlayerControls();

            EditorGUILayout.Space();

            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.Space();

            GUILayout.Label("Player Options", EditorStyles.boldLabel);

            DrawRotationModeDropdown(HandleRotationModeChange);
            DrawTangentModeDropdown();
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
        private void OnDisable() {
            SceneTool.RestoreTool();
        }

        private void OnEnable() {
            // Get target script reference.
            Script = (APAnimator) target;

            // Return is required assets are not referenced.
            if (!RequiredAssetsLoaded()) return;

            InstantiateCompositeClasses();
            InitializeSerializedProperties();

            CopyIconsToGizmosFolder();
            SceneTool.RememberCurrentTool();
            FocusOnSceneView();
        }

        private void OnSceneGUI() {
            // Return is required assets are not referenced.
            if (!RequiredAssetsLoaded()) return;
            // Return if path asset is not referenced.
            if (Script.PathData == null) return;
            // Return if serialized properties are not initialized.
            if (!SerializedPropertiesInitialized) return;

            HandleAnimatorEventsSubscription();

            // Disable interaction with background scene elements.
            HandleUtility.AddDefaultControl(
                GUIUtility.GetControlID(
                    FocusType.Passive));

            HandleShortcuts();
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

        private void DrawAdvancedSettingsControls() {
            if (advancedSettingsFoldout.boolValue) {
                DrawSettingsAssetField();
                DrawSkinSelectionControl();

                DrawGizmoCurveColorPicker();
                DrawRotationCurveColorPicker();

                DrawShortJumpValueField();
                DrawLongJumpValueField();
            }
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

        private void DrawAnimatedGOField() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                animatedGO,
                new GUIContent(
                    "Animated Object",
                    "Object to animate."));

            serializedObject.ApplyModifiedProperties();
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

        private void DrawAnimationTimeValue() {
            Undo.RecordObject(target, "Update AnimationTime");

            float newTimeRatio;

            newTimeRatio = DrawAnimationTimeSlider();

            // Update AnimationTime only when value was changed.
            if (Math.Abs(newTimeRatio - Script.AnimationTime)
                > GlobalConstants.FloatPrecision) {

                serializedObject.Update();

                animationTime.floatValue = newTimeRatio;

                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawAutoPlayControl() {
            Script.SettingsAsset.AutoPlay = EditorGUILayout.Toggle(
                new GUIContent(
                    "Auto Play",
                    ""),
                Script.SettingsAsset.AutoPlay);
        }

        private void DrawCreatePathAssetButton() {
            if (GUILayout.Button(
                new GUIContent(
                    "New Path",
                    ""))) {

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
            SettingsSerializedObject.Update();
            EditorGUILayout.PropertyField(
                enableControlsInPlayMode,
                new GUIContent(
                    "Play Mode Controls",
                    "Enable keybord controls in play mode."));
            SettingsSerializedObject.ApplyModifiedProperties();
        }

        private void DrawForwardPointOffsetSlider() {
            Script.SettingsAsset.ForwardPointOffset = EditorGUILayout.Slider(
                new GUIContent(
                    "Forward Point Offset",
                    ""),
                Script.SettingsAsset.ForwardPointOffset,
                Script.SettingsAsset.ForwardPointOffsetMinValue,
                1);
        }

        private void DrawGizmoCurveColorPicker() {
            SettingsSerializedObject.Update();

            EditorGUILayout.PropertyField(
                gizmoCurveColor,
                new GUIContent("Curve Color", ""));

            SettingsSerializedObject.ApplyModifiedProperties();
        }

        private void DrawHandleModeDropdown() {
            Undo.RecordObject(Script.SettingsAsset, "Change handle mode.");

            Script.SettingsAsset.HandleMode =
                (HandleMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Handle Mode",
                        ""),
                    Script.SettingsAsset.HandleMode);
        }

        private void DrawInfoLabel(string text) {
            EditorGUILayout.HelpBox(text, MessageType.Error);
        }

        private void DrawLongJumpValueField() {
            SettingsSerializedObject.Update();

            EditorGUILayout.PropertyField(longJumpValue);

            SettingsSerializedObject.ApplyModifiedProperties();
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
            Script.SettingsAsset.PositionLerpSpeed = EditorGUILayout.Slider(
                new GUIContent(
                    "Position Lerp Speed",
                    ""),
                Script.SettingsAsset.PositionLerpSpeed,
                0,
                1);
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
                Script.SettingsAsset.HandleMode = HandleMode.None;

                Utilities.InvokeMethodWithReflection(
                    Script,
                    "UpdateAnimation",
                    null);
            }
        }

        private void DrawResetRotationPathButton() {
            if (GUILayout.Button(
                new GUIContent(
                    "Reset Rotation",
                    ""))) {

                if (Script.PathData == null) return;

                Undo.RecordObject(Script.PathData, "Reset rotatio path.");

                // Reset curves to its default state.
                Script.PathData.ResetRotationPath();
                Script.PathData.SmoothRotationPathTangents();

                SceneView.RepaintAll();
            }
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

        private void DrawRotationCurveColorPicker() {
            SettingsSerializedObject.Update();

            EditorGUILayout.PropertyField(rotationCurveColor);

            SettingsSerializedObject.ApplyModifiedProperties();
        }

        private void DrawRotationModeDropdown(Action callback) {
            Undo.RecordObject(Script.SettingsAsset, "Change rotation mode.");

            // Remember current RotationMode.
            var prevRotationMode = Script.SettingsAsset.RotationMode;

            // Draw RotationMode dropdown.
            Script.SettingsAsset.RotationMode =
                (RotationMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Rotation Mode",
                        ""),
                    Script.SettingsAsset.RotationMode);

            // If value changed, update animated GO in the scene.
            if (Script.SettingsAsset.RotationMode != prevRotationMode) {
                callback();
            }
        }

        private void DrawRotationSpeedField() {
            SettingsSerializedObject.Update();

            EditorGUILayout.PropertyField(
                rotationSlerpSpeed,
                new GUIContent(
                    "Rotation Slerp Speed",
                    "Controls how much time (in seconds) it'll take the " +
                    "animated object to finish rotation towards followed target."));

            SettingsSerializedObject.ApplyModifiedProperties();
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

        private void DrawShortcutsHelpBox() {
            EditorGUILayout.HelpBox(
                "Check SettingsAsset Asset for shortcuts.",
                MessageType.Info);
        }

        private void DrawShortJumpValueField() {
            SettingsSerializedObject.Update();

            EditorGUILayout.PropertyField(shortJumpValue);

            SettingsSerializedObject.ApplyModifiedProperties();
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
            var prevTangentMode = Script.SettingsAsset.TangentMode;

            // Draw tangent mode dropdown.
            Script.SettingsAsset.TangentMode =
                (TangentMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Tangent Mode",
                        ""),
                    Script.SettingsAsset.TangentMode);

            // Update gizmo curve is tangent mode changed.
            if (Script.SettingsAsset.TangentMode != prevTangentMode) {
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

        private void DrawUpdateAllToggle() {
            Script.SettingsAsset.UpdateAllMode = EditorGUILayout.Toggle(
                new GUIContent(
                    "Update All Values",
                    ""),
                Script.SettingsAsset.UpdateAllMode);
        }

        private void DrawWrapModeDropdown() {
            Script.SettingsAsset.WrapMode =
                (AnimatorWrapMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Wrap Mode",
                        ""),
                    Script.SettingsAsset.WrapMode);
        }

        private void HandleDrawingUpdateAllModeLabel() {
            if (!Script.SettingsAsset.UpdateAllMode) return;

            // Get global node positions.
            var globalNodePositions = Script.GetGlobalNodePositions();

            // Create array with text to be displayed for each node.
            var labelText = new string[globalNodePositions.Length];
            for (var i = 0; i < globalNodePositions.Length; i++) {
                labelText[i] = Script.SettingsAsset.UpdateAllLabelText;
            }

            SceneHandles.DrawUpdateAllLabels(
                globalNodePositions,
                labelText,
                Script.SettingsAsset.UpdateAllLabelOffsetX,
                Script.SettingsAsset.UpdateAllLabelOffsetY,
                Script.SettingsAsset.DefaultLabelWidth,
                Script.SettingsAsset.DefaultLabelHeight,
                Script.Skin.GetStyle("UpdateAllLabel"));
        }

        /// <summary>
        ///     Called on rotation mode change.
        /// </summary>
        private void HandleRotationModeChange() {
            Utilities.InvokeMethodWithReflection(
                Script,
                "UpdateAnimation",
                null);
            Script.SettingsAsset.HandleMode = HandleMode.None;
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
                Script.SettingsAsset.AddButtonOffsetH,
                Script.SettingsAsset.AddButtonOffsetV,
                callbackHandler,
                addButtonStyle);
        }

        private void HandleDrawingEaseHandles() {
            if (Script.SettingsAsset.HandleMode != HandleMode.Ease) return;

            // Get path node positions.
            var nodePositions =
                Script.GetGlobalNodePositions();

            // Get ease values.
            var easeCurveValues = Script.PathData.GetEaseCurveValues();

            var arcValueMultiplier =
                Script.SettingsAsset.ArcValueMultiplierNumerator
                / Script.SettingsAsset.MaxAnimationSpeed;

            SceneHandles.DrawEaseHandles(
                nodePositions,
                easeCurveValues,
                arcValueMultiplier,
                Script.SettingsAsset.ArcHandleRadius,
                Script.SettingsAsset.InitialArcValue,
                Script.SettingsAsset.ScaleHandleSize,
                DrawEaseHandlesCallbackHandler);
        }

        private void HandleDrawingEaseLabel() {
            if (Script.SettingsAsset.HandleMode != HandleMode.Ease) return;

            // Get node global positions.
            var nodeGlobalPositions = Script.GetGlobalNodePositions();

            SceneHandles.DrawArcHandleLabels(
                nodeGlobalPositions,
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
            var nodeGlobalPositions = Script.GetGlobalNodePositions();

            SceneHandles.DrawPositionHandles(
                nodeGlobalPositions,
                Script.SettingsAsset.MovementHandleSize,
                Script.SettingsAsset.GizmoCurveColor,
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
                Script.SettingsAsset.RemoveButtonH,
                Script.SettingsAsset.RemoveButtonV,
                removeNodeCallback,
                removeButtonStyle);
        }

        private void HandleDrawingRotationHandle() {
            if (Script.SettingsAsset.HandleMode != HandleMode.Rotation) return;

            var currentAnimationTime = Script.AnimationTime;
            var rotationPointPosition =
                Script.PathData.GetRotationAtTime(currentAnimationTime);
            var rotationPointGlobalPosition =
                Script.transform.TransformPoint(rotationPointPosition);
            var nodeTimestamps = Script.PathData.GetPathTimestamps();

            // Return if current animation time is not equal to any node
            // timestamp.
            var index = Array.FindIndex(
                nodeTimestamps,
                x => Math.Abs(x - currentAnimationTime)
                     < GlobalConstants.FloatPrecision);

            if (index < 0) return;

            SceneHandles.DrawRotationHandle(
                rotationPointGlobalPosition,
                Script.SettingsAsset.RotationHandleSize,
                Script.SettingsAsset.RotationHandleColor,
                DrawRotationHandlesCallbackHandler);
        }

        private void HandleDrawingTiltingHandles() {
            if (Script.SettingsAsset.HandleMode != HandleMode.Tilting) return;

            Action<int, float> callbackHandler =
                DrawTiltingHandlesCallbackHandler;

            // Get path node positions.
            var nodePositions =
                Script.GetGlobalNodePositions();

            // Get tilting curve values.
            var tiltingCurveValues = Script.PathData.GetTiltingCurveValues();

            SceneHandles.DrawTiltingHandles(
                nodePositions,
                tiltingCurveValues,
                Script.SettingsAsset.ArcHandleRadius,
                Script.SettingsAsset.InitialArcValue,
                Script.SettingsAsset.ScaleHandleSize,
                callbackHandler);
        }

        private void HandleDrawingTiltLabel() {
            if (Script.SettingsAsset.HandleMode != HandleMode.Tilting) return;

            // Get node global positions.
            var nodeGlobalPositions = Script.GetGlobalNodePositions();

            SceneHandles.DrawArcHandleLabels(
                nodeGlobalPositions,
                Script.SettingsAsset.EaseValueLabelOffsetX,
                Script.SettingsAsset.EaseValueLabelOffsetY,
                Script.SettingsAsset.DefaultLabelWidth,
                Script.SettingsAsset.DefaultLabelHeight,
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
            if (Script.SettingsAsset.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothAnimObjPathTangents();
            }
            // In Linear mode set node tangents to linear.
            else if (Script.SettingsAsset.TangentMode == TangentMode.Linear) {
                Script.PathData.SetLinearAnimObjPathTangents();
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
            Undo.RecordObject(Script.PathData, "Ease curve changed.");

            if (Script.SettingsAsset.UpdateAllMode) {
                var oldValue = Script.PathData.GetEaseValueAtIndex(keyIndex);
                var delta = newValue - oldValue;
                Script.PathData.UpdateEaseCurveValues(delta);
            }
            else {
                Script.PathData.UpdateEaseValue(keyIndex, newValue);
            }
        }

        private void DrawPositionHandlesCallbackHandler(
            int movedNodeIndex,
            Vector3 newGlobalPos) {

            Undo.RecordObject(Script.PathData, "Change path");

            // Calculate node new local position.
            var newNodeLocalPosition =
                Script.transform.InverseTransformPoint(newGlobalPos);

            Script.PathData.MoveNodeToPosition(
                movedNodeIndex,
                newNodeLocalPosition);
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
            if (Script.SettingsAsset.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothAnimObjPathTangents();
            }
            // In Linear mode set node tangents to linear.
            else if (Script.SettingsAsset.TangentMode == TangentMode.Linear) {
                Script.PathData.SetLinearAnimObjPathTangents();
            }

            // Update animated object.
            Utilities.InvokeMethodWithReflection(
                Script,
                "UpdateAnimation",
                null);
        }

        private void DrawRotationHandlesCallbackHandler(
            Vector3 newPosition) {

            Undo.RecordObject(Script.PathData, "Rotation path changed.");

            var newLocalPos =
                Script.transform.InverseTransformPoint(newPosition);

            Script.PathData.ChangeRotationAtTimestamp(
                Script.AnimationTime,
                newLocalPos);
        }

        private void DrawTiltingHandlesCallbackHandler(
            int keyIndex,
            float newValue) {
            Undo.RecordObject(Script.PathData, "Tilting curve changed.");

            if (Script.SettingsAsset.UpdateAllMode) {
                var oldValue = Script.PathData.GetTiltingValueAtIndex(keyIndex);
                var delta = newValue - oldValue;
                Script.PathData.UpdateTiltingCurveValues(delta);
            }
            else {
                Script.PathData.UpdateTiltingValue(keyIndex, newValue);
            }
        }

        #endregion CALLBACK HANDLERS

        #region MODE HANDLERS

        private void HandleLinearTangentMode() {
            if (Script.SettingsAsset.TangentMode == TangentMode.Linear) {
                Script.PathData.SetLinearAnimObjPathTangents();
            }
        }

        private void HandleSmoothTangentMode() {
            if (Script.SettingsAsset.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothAnimObjPathTangents();
            }
        }

        private void HandleTangentModeChange() {
            if (Script.PathData == null) return;

            // Update path node tangents.
            if (Script.SettingsAsset.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothAnimObjPathTangents();
            }
            else if (Script.SettingsAsset.TangentMode == TangentMode.Linear) {
                Script.PathData.SetLinearAnimObjPathTangents();
            }

            SceneView.RepaintAll();
        }

        #endregion

        #region METHODS
        private void HandleAnimatorEventsSubscription() {
            // Subscribe animator to path events if not subscribed already.
            // This is required after animator component reset.
            serializedObject.Update();
            if (!subscribedToEvents.boolValue) {
                // Unsubscribe first to avoid multiple subscription after
                // animator component reset.
                Utilities.InvokeMethodWithReflection(
                    Script,
                    "UnsubscribeFromEvents",
                    null);

                // Subscribe to events.
                Utilities.InvokeMethodWithReflection(
                    Script,
                    "SubscribeToEvents",
                    null);
            }
            serializedObject.ApplyModifiedProperties();
        }


        private static void FocusOnSceneView() {
            if (SceneView.sceneViews.Count > 0) {
                var sceneView = (SceneView) SceneView.sceneViews[0];
                sceneView.Focus();
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

        private float ConvertEaseToDegrees(int nodeIndex) {
            // Calculate value to display.
            var easeValue = Script.PathData.GetNodeEaseValue(nodeIndex);
            var arcValueMultiplier =
                Script.SettingsAsset.ArcValueMultiplierNumerator
                / Script.SettingsAsset.MaxAnimationSpeed;
            var easeValueInDegrees = easeValue * arcValueMultiplier;

            return easeValueInDegrees;
        }

        private float ConvertTiltToDegrees(int nodeIndex) {
            var rotationValue = Script.PathData.GetNodeTiltValue(nodeIndex);

            return rotationValue;
        }

        private void CopyIconsToGizmosFolder() {
            // Path to Unity Gizmos folder.
            var gizmosDir = Application.dataPath + "/Gizmos";

            // Create Asset/Gizmos folder if not exists.
            if (!Directory.Exists(gizmosDir + "ATP")) {
                Directory.CreateDirectory(gizmosDir + "ATP");
            }

            // Check if messageSettings asset has icons specified.
            if (Script.SettingsAsset.GizmoIcons == null) return;

            // For each icon..
            foreach (var icon in Script.SettingsAsset.GizmoIcons) {
                // Get icon path.
                var iconPath = AssetDatabase.GetAssetPath(icon);

                // Copy icon to Gizmos folder.
                AssetDatabase.CopyAsset(iconPath, gizmosDir + "/ATP");
            }
        }

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

        private void InitializeSerializedProperties() {
            rotationSlerpSpeed =
                SettingsSerializedObject.FindProperty("rotationSlerpSpeed");
            animatedGO = serializedObject.FindProperty("animatedGO");
            targetGO = serializedObject.FindProperty("targetGO");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");
            pathData = serializedObject.FindProperty("pathData");
            enableControlsInPlayMode =
                SettingsSerializedObject.FindProperty(
                    "enableControlsInPlayMode");
            skin = serializedObject.FindProperty("skin");
            settings = serializedObject.FindProperty("settingsAsset");
            gizmoCurveColor =
                SettingsSerializedObject.FindProperty("gizmoCurveColor");
            rotationCurveColor =
                SettingsSerializedObject.FindProperty("rotationCurveColor");
            shortJumpValue =
                SettingsSerializedObject.FindProperty("shortJumpValue");
            longJumpValue =
                SettingsSerializedObject.FindProperty("longJumpValue");
            subscribedToEvents =
                serializedObject.FindProperty("subscribedToEvents");
            animationTime =
                serializedObject.FindProperty("animationTime");

            SerializedPropertiesInitialized = true;
        }

        private void InstantiateCompositeClasses() {
            SettingsSerializedObject = new SerializedObject(
                Script.SettingsAsset);
        }

        private bool RequiredAssetsLoaded() {
            var assetsLoaded = (bool) Utilities.InvokeMethodWithReflection(
                Script,
                "RequiredAssetsLoaded",
                null);

            return assetsLoaded;
        }

        private void ValidateInspectorSettings() {
            if (Script.SettingsAsset == null) return;

            // Limit RotationSpeed value.
            if (Script.SettingsAsset.RotationSlerpSpeed < 0) {
                Script.SettingsAsset.RotationSlerpSpeed = 0;
            }

            // Limit ExmportSamplingFrequency value.
            if (Script.SettingsAsset.ExportSamplingFrequency < 1) {
                Script.SettingsAsset.ExportSamplingFrequency = 1;
            }
        }

        private void HandlePlayPauseButton() {
            if (!Application.isPlaying) return;

            if (Script.IsPlaying && !Script.Pause) {
                // Pause animation.
                Script.Pause = true;
            }
            else if (Script.IsPlaying && Script.Pause) {
                // Unpause animation.
                Script.Pause = false;
            }
            // Animation ended.
            else if (!Script.IsPlaying && Script.AnimationTime >= 1) {
                Script.AnimationTime = 0;
                Script.StartAnimation();
            }
            else {
                // Start animation.
                Script.StartAnimation();
            }
        }

        #endregion PRIVATE METHODS

        #region SHORTCUTS

        private float GetNearestBackwardNodeTimestamp() {
            var pathTimestamps = Script.PathData.GetPathTimestamps();

            for (var i = pathTimestamps.Length - 1; i >= 0; i--) {
                if (pathTimestamps[i] < animationTime.floatValue) {
                    return pathTimestamps[i];
                }
            }

            // Return timestamp of the last node.
            return 0;
        }

        private float GetNearestForwardNodeTimestamp() {
            var pathTimestamps = Script.PathData.GetPathTimestamps();

            foreach (var timestamp in pathTimestamps
                .Where(timestamp => timestamp > animationTime.floatValue)) {
                return timestamp;
            }

            // Return timestamp of the last node.
            return 1.0f;
        }

        private void HandleShortcuts() {
            serializedObject.Update();

            Utilities.HandleUnmodShortcut(
                Script.SettingsAsset.EaseModeKey,
                () => Script.SettingsAsset.HandleMode = HandleMode.Ease);

            Utilities.HandleUnmodShortcut(
                Script.SettingsAsset.RotationModeKey,
                () => Script.SettingsAsset.HandleMode = HandleMode.Rotation);

            Utilities.HandleUnmodShortcut(
                Script.SettingsAsset.TiltingModeKey,
                () => Script.SettingsAsset.HandleMode = HandleMode.Tilting);

            Utilities.HandleUnmodShortcut(
                Script.SettingsAsset.NoneModeKey,
                () => Script.SettingsAsset.HandleMode = HandleMode.None);

            Utilities.HandleUnmodShortcut(
                Script.SettingsAsset.UpdateAllKey,
                () =>
                    Script.SettingsAsset.UpdateAllMode =
                        !Script.SettingsAsset.UpdateAllMode);

            // Short jump forward.
            Utilities.HandleModShortcut(
                () => {
                    var newAnimationTimeRatio =
                        animationTime.floatValue
                        + Script.SettingsAsset.ShortJumpValue;

                    animationTime.floatValue =
                        (float) (Math.Round(newAnimationTimeRatio, 3));
                },
                Script.SettingsAsset.ShortJumpForwardKey,
                Event.current.alt);

            // Short jump backward.
            Utilities.HandleModShortcut(
                () => {
                    var newAnimationTimeRatio =
                        animationTime.floatValue
                        - Script.SettingsAsset.ShortJumpValue;

                    animationTime.floatValue =
                        (float) (Math.Round(newAnimationTimeRatio, 3));
                },
                Script.SettingsAsset.ShortJumpBackwardKey,
                Event.current.alt);

            // Long jump forward.
            Utilities.HandleUnmodShortcut(
                Script.SettingsAsset.LongJumpForwardKey,
                () => animationTime.floatValue +=
                    Script.SettingsAsset.LongJumpValue);

            // Long jump backward.
            Utilities.HandleUnmodShortcut(
                Script.SettingsAsset.LongJumpBackwardKey,
                () => animationTime.floatValue -=
                    Script.SettingsAsset.LongJumpValue);

            // Jump to next node.
            Utilities.HandleUnmodShortcut(
                Script.SettingsAsset.JumpToNextNodeKey,
                () => animationTime.floatValue =
                    GetNearestForwardNodeTimestamp());

            // Jump to previous node.
            Utilities.HandleUnmodShortcut(
                Script.SettingsAsset.JumpToPreviousNodeKey,
                () => animationTime.floatValue =
                    GetNearestBackwardNodeTimestamp());

            // Jump to start.
            Utilities.HandleModShortcut(
                () => animationTime.floatValue = 0,
                Script.SettingsAsset.JumpToStartKey,
                //ModKeyPressed);
                Event.current.alt);

            // Jump to end.
            Utilities.HandleModShortcut(
                () => animationTime.floatValue = 1,
                Script.SettingsAsset.JumpToEndKey,
                //ModKeyPressed);
                Event.current.alt);

            // Play/pause animation.
            Utilities.HandleUnmodShortcut(
                Script.SettingsAsset.PlayPauseKey,
                HandlePlayPauseButton);

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region EXPORTER

        public void DrawExportControls() {
            EditorGUILayout.BeginHorizontal();

            Script.SettingsAsset.ExportSamplingFrequency =
                EditorGUILayout.IntField(
                    new GUIContent(
                        "Export Sampling",
                        "Number of points to export for 1 m of the curve. " +
                        "If set to 0, it'll export only keys defined in " +
                        "the curve."),
                    Script.SettingsAsset.ExportSamplingFrequency);

            if (GUILayout.Button("Export")) {
                ExportNodes(
                    Script.PathData,
                    Script.transform,
                    Script.SettingsAsset.ExportSamplingFrequency);
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        ///     Export Animation Path nodes as transforms.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="exportSampling">
        ///     Amount of result transforms for one meter of Animation Path.
        /// </param>
        /// <param name="pathData"></param>
        private static void ExportNodes(
            PathData pathData,
            Transform transform,
            int exportSampling) {

            // exportSampling cannot be less than 0.
            if (exportSampling < 0) return;

            // Points to export.
            var points = pathData.SampleAnimationPathForPoints(
                exportSampling);

            // Convert points to global coordinates.
            Utilities.ConvertToGlobalCoordinates(ref points, transform);

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