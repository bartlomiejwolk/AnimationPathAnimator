#define DEBUG

using System;
using System.IO;
using ATP.LoggingTools;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools.AnimatorComponent {

    /// <summary>
    /// Editor class responsible for drawing inspector and on-scene handles. All editor related functionality is defined here.
    /// </summary>
    [CustomEditor(typeof (Animator))]
    public sealed class AnimatorEditor : Editor {
        #region PROPERTIES

        /// <summary>
        ///     Reference to target script.
        /// </summary>
        private Animator Script { get; set; }

        /// <summary>
        /// Is true when serialized properties are initialized.
        /// </summary>
        private bool SerializedPropertiesInitialized { get; set; }

        /// <summary>
        /// <c>SerializedObject</c> for <c>AnimatorSettings</c> asset.
        /// </summary>
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
        private SerializedProperty positionHandle;
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

            // Validate inspector settings.
            // Settings stored in settings asset cannot be validated with OnValidate().
            if (GUI.changed) {
                ValidateInspectorSettings();
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
            Script = (Animator) target;

            // Return is required assets are not referenced.
            if (!RequiredAssetsLoaded()) return;

            // Initialize serialized object for settings asset.
            SettingsSerializedObject = new SerializedObject(
                Script.SettingsAsset);

            InitializeSerializedProperties();
            CopyIconsToGizmosFolder();
            SceneTool.RememberCurrentTool();
            FocusOnSceneView();

            // Update animated GO.
            Utilities.InvokeMethodWithReflection(
                Script,
                "HandleUpdateAnimGOInSceneView",
                null);
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
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(
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
        private void DrawSceneToolShortcutsInfoLabel() {
            EditorGUILayout.HelpBox(
                "Shortcuts (editor): G, Y, U, I, O, P.",
                MessageType.Info);
        }

        private void DrawShortcutsInfoLabel() {
            EditorGUILayout.HelpBox(
                "Scene shortcuts to control animation (editor/play mode): " +
                "[Alt] H, [Alt] J, [Alt] K, [Alt] L. (play mode): Space",
                MessageType.Info);
        }


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
            Script.SettingsAsset.AutoPlay = EditorGUILayout.Toggle(
                new GUIContent(
                    "Auto Play",
                    "Start playing animation after entering play mode."),
                Script.SettingsAsset.AutoPlay);
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
                    "Distance from animated object to the point used as " +
                    "a look at target in Forward rotation mode."),
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
                        "Scene Tool",
                        "Tool displayed next to each node. Default " +
                        "shortcuts: Y, U, I, O."),
                    Script.SettingsAsset.HandleMode);
        }

        private void DrawInfoLabel(string text) {
            EditorGUILayout.HelpBox(text, MessageType.Error);
        }

        private void DrawLongJumpValueField() {
            SettingsSerializedObject.Update();

            longJumpValue.floatValue = EditorGUILayout.Slider(
                new GUIContent(
                    "Long Jump Value",
                    "Fraction of animation time used to jump forward/backward "
                    + "in time with keyboard keys."),
                longJumpValue.floatValue,
                0,
                1);

            SettingsSerializedObject.ApplyModifiedProperties();
        }

        private void DrawPathDataAssetField() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                pathData,
                new GUIContent(
                    "Path Asset",
                    "Asset containing all path data."));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPlayerControls() {
            // Play/Pause button text.
            string playPauseBtnText;
            if (!Script.IsRunning || (Script.IsRunning && Script.Pause)) {
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
                    "HandleUpdateAnimGOInSceneView",
                    null);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPositionHandleDropdown() {
            SettingsSerializedObject.Update();

            EditorGUILayout.PropertyField(
                positionHandle,
                new GUIContent(
                    "Position Handle",
                    "Handle used to move nodes on scene. Default " +
                    "shortcut: G"));

            SettingsSerializedObject.ApplyModifiedProperties();
        }

        private void DrawPositionSpeedSlider() {
            Script.SettingsAsset.PositionLerpSpeed = EditorGUILayout.Slider(
                new GUIContent(
                    "Position Lerp Speed",
                    "Controls how much time it'll take the " +
                    "animated object to reach position that it should be " +
                    "at the current animation time. " +
                    "1 means no delay."),
                Script.SettingsAsset.PositionLerpSpeed,
                0,
                1);
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
                Script.SettingsAsset.HandleMode = HandleMode.None;

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
                Script.PathData.SmoothRotationPathTangents();

                // Change rotation mode.
                Script.SettingsAsset.RotationMode = RotationMode.Custom;
                Script.SettingsAsset.HandleMode = HandleMode.Rotation;

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
                        "Mode that controls animated game object rotation."),
                    Script.SettingsAsset.RotationMode);

            // Return if rotation mode not changed.
            if (Script.SettingsAsset.RotationMode == prevRotationMode) return;

            // Update animated GO in the scene.
            callback();

            // If Custom mode selected, change handle mode to Rotation.
            if (Script.SettingsAsset.RotationMode == RotationMode.Custom) {
                Script.SettingsAsset.HandleMode = HandleMode.Rotation;
            }
        }

        private void DrawRotationSpeedField() {
            SettingsSerializedObject.Update();

            EditorGUILayout.PropertyField(
                rotationSlerpSpeed,
                new GUIContent(
                    "Rotation Slerp Speed",
                    "Controls how much time it'll take the " +
                    "animated object to finish rotation towards followed target."));

            SettingsSerializedObject.ApplyModifiedProperties();
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

        private void DrawShortJumpValueField() {
            SettingsSerializedObject.Update();

            shortJumpValue.floatValue = EditorGUILayout.Slider(
                new GUIContent(
                    "Short Jump Value",
                    "Fraction of animation time used to jump forward/backward "
                    + "in time with keyboard keys."),
                shortJumpValue.floatValue,
                0,
                1);

            SettingsSerializedObject.ApplyModifiedProperties();
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
            var prevTangentMode = Script.SettingsAsset.TangentMode;

            // Draw tangent mode dropdown.
            Script.SettingsAsset.TangentMode =
                (TangentMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Tangent Mode",
                        "Tangent mode applied to each path node."),
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
                    "When checked, values will be changed for all nodes. " +
                    "Default shortcut: P."),
                Script.SettingsAsset.UpdateAllMode);
        }

        private void DrawWrapModeDropdown() {
            Script.SettingsAsset.WrapMode =
                (AnimatorWrapMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Wrap Mode",
                        "Determines animator behaviour after animation end."),
                    Script.SettingsAsset.WrapMode);
        }
        #endregion

        #region DRAWING HANDLERS
        /// <summary>
        /// Handle drawing on-scene label for "Update All" inspector option.
        /// </summary>
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
        /// Handle drawing on-scene buttons for adding new nodes.
        /// </summary>
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

        /// <summary>
        /// Handle drawine on-scene ease handles.
        /// </summary>
        private void HandleDrawingEaseHandles() {
            if (Script.SettingsAsset.HandleMode != HandleMode.Ease) return;

            // Get path node positions.
            var nodePositions =
                Script.GetGlobalNodePositions();

            // Get ease values.
            var easeCurveValues = Script.PathData.GetEaseCurveValues();

            // Value that defines how much of an arc will be draw to represent a value.
            var arcValueMultiplier =
                Script.SettingsAsset.ArcValueMultiplierNumerator
                / Script.SettingsAsset.MaxAnimationSpeed;

            SceneHandles.DrawEaseHandles(
                nodePositions,
                easeCurveValues,
                arcValueMultiplier,
                Script.SettingsAsset.ArcHandleRadius,
                Script.SettingsAsset.InitialEaseArcValue,
                Script.SettingsAsset.ScaleHandleSize,
                DrawEaseHandlesCallbackHandler);
        }

        /// <summary>
        /// Handle drawing on-scene labes with ease values.
        /// </summary>
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
            // Get node positions.
            var nodeGlobalPositions = Script.GetGlobalNodePositions();

            // Draw custom position handles.
            if (positionHandle.enumValueIndex ==
                (int) PositionHandle.Free) {

                SceneHandles.DrawCustomPositionHandles(
                    nodeGlobalPositions,
                    Script.SettingsAsset.MovementHandleSize,
                    Script.SettingsAsset.GizmoCurveColor,
                    DrawPositionHandlesCallbackHandler);
            }
            // Draw default position handles.
            else {
                SceneHandles.DrawPositionHandles(
                    nodeGlobalPositions,
                    DrawPositionHandlesCallbackHandler);
            }
        }

        /// <summary>
        /// Handle drawing on-scene button for removing nodes.
        /// </summary>
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

        /// <summary>
        /// Handle drawing on-scene rotation handle in Custom rotation mode.
        /// </summary>
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
                x => Utilities.FloatsEqual(
                    x,
                    currentAnimationTime,
                    GlobalConstants.FloatPrecision));

            if (index < 0) return;

            SceneHandles.DrawRotationHandle(
                rotationPointGlobalPosition,
                Script.SettingsAsset.RotationHandleSize,
                Script.SettingsAsset.RotationHandleColor,
                DrawRotationHandlesCallbackHandler);
        }

        /// <summary>
        /// Handle drawing on-scene tilting handles.
        /// </summary>
        private void HandleDrawingTiltingHandles() {
            if (Script.SettingsAsset.HandleMode != HandleMode.Tilting) return;

            // todo pass directly.
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
                Script.SettingsAsset.InitialTiltingArcValue,
                Script.SettingsAsset.ScaleHandleSize,
                callbackHandler);
        }

        /// <summary>
        /// Handle drawing on-scene tilting value labels.
        /// </summary>
        // todo rename to HandleDrawingTiltingLabels().
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

        /// <summary>
        /// Add node button pressed callback handler.
        /// </summary>
        /// <param name="nodeIndex"></param>
        private void DrawAddNodeButtonsCallbackHandler(int nodeIndex) {
            // Make snapshot of the target object.
            Undo.RecordObject(Script.PathData, "Change path");

            // Add a new node.
            AddNodeBetween(nodeIndex);
            HandleUnsyncedObjectAndRotationPaths();
            Script.PathData.DistributeTimestamps();

            // In Smooth mode sooth node tangents.
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
            if (Script.SettingsAsset.UpdateAllMode) {
                MultiplyEaseValues(
                    Script.PathData.GetEaseValueAtIndex(keyIndex),
                    newValue);
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

            Undo.RecordObject(Script.PathData, "Change path");

            // Remember path length before applying changes.
            var oldAnimGoPathLength = Script.PathData.GetPathLinearLength();

            // Calculate node new local position.
            var newNodeLocalPosition =
                Script.transform.InverseTransformPoint(newGlobalPos);

            Script.PathData.MoveNodeToPosition(
                movedNodeIndex,
                newNodeLocalPosition);

            Script.PathData.DistributeTimestamps();

            HandleSmoothTangentMode();
            HandleLinearTangentMode();

            // Current path length.
            var newAnimGoPathLength = Script.PathData.GetPathLinearLength();
            DistributeEaseValues(oldAnimGoPathLength, newAnimGoPathLength);

            EditorUtility.SetDirty(Script.PathData);
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
                "HandleUpdateAnimGOInSceneView",
                null);

            EditorUtility.SetDirty(Script.PathData);
        }

        private void DrawRotationHandlesCallbackHandler(
            Vector3 newPosition) {

            Undo.RecordObject(Script.PathData, "Rotation path changed.");

            var newLocalPos =
                Script.transform.InverseTransformPoint(newPosition);

            Script.PathData.ChangeRotationAtTimestamp(
                Script.AnimationTime,
                newLocalPos);

            EditorUtility.SetDirty(Script.PathData);
        }

        private void DrawTiltingHandlesCallbackHandler(
            int keyIndex,
            float newValue) {
            Undo.RecordObject(Script.PathData, "Tilting curve changed.");

            if (Script.SettingsAsset.UpdateAllMode) {
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
        ///     Called on rotation mode change.
        /// </summary>
        private void HandleRotationModeChange() {
            Utilities.InvokeMethodWithReflection(
                Script,
                "HandleUpdateAnimGOInSceneView",
                null);
            Script.SettingsAsset.HandleMode = HandleMode.None;
        }


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

        #region OTHER HANDLERS
        /// <summary>
        /// Handles adding/removing reference to target game object.
        /// </summary>
        /// <param name="prevTargetGO"></param>
        private void HandleTargetGOFieldChange(Transform prevTargetGO) {
            // Handle adding reference.
            if (Script.TargetGO != prevTargetGO
                && prevTargetGO == null) {

                Script.SettingsAsset.RotationMode = RotationMode.Target;

                Utilities.InvokeMethodWithReflection(
                    Script,
                    "HandleUpdateAnimGOInSceneView",
                    null);
            }
            // Handle removing reference.
            else if (Script.TargetGO != prevTargetGO
                     && prevTargetGO != null
                     && Script.TargetGO == null) {

                Script.SettingsAsset.RotationMode = RotationMode.Forward;

                Utilities.InvokeMethodWithReflection(
                    Script,
                    "HandleUpdateAnimGOInSceneView",
                    null);
            }
        }


        /// <summary>
        /// Makes sure that animator is always subscribed to path events.
        /// </summary>
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

        /// <summary>
        /// Defines what to do when undo event is performed.
        /// </summary>
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
                    "HandleUpdateAnimGOInSceneView",
                    null);
            }
        }

        /// <summary>
        /// Handles situation when adding new node to the path would result in
        /// anim. GO path and rotation path having different number of nodes.
        /// </summary>
        /// <remarks>Such situation would be caused by placing rotation nodes too close to each other.</remarks>
        private void HandleUnsyncedObjectAndRotationPaths() {
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
            else {
                // Set handle mode to Rotation so that user can fix the
                // rotatio path.
                Script.SettingsAsset.HandleMode = HandleMode.Rotation;
                // Set rotation mode to Custom so that user can see how
                // changes to rotation path affect animated object.
                Script.SettingsAsset.RotationMode = RotationMode.Custom;
            }
        }

        #endregion

        #region METHODS
        /// <summary>
        /// Adjust ease values to path length. Making path longer will decrease ease values
        /// to  maintain constant speed.
        /// </summary>
        /// <param name="oldAnimGoLinearLength">Anim. Go path length before path update.</param>
        /// <param name="newAnimGoLinearLength">Anim. Go path length after path update.</param>
        private void DistributeEaseValues(
            float oldAnimGoLinearLength,
            float newAnimGoLinearLength) {

            // Calculate multiplier.
            var multiplier = oldAnimGoLinearLength / newAnimGoLinearLength;

            // Multiply each single ease value.
            Script.PathData.MultiplyEaseCurveValues(multiplier);
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

            EditorGUILayout.Space();

            GUILayout.Label("Scene Tools", EditorStyles.boldLabel);

            DrawHandleModeDropdown();
            DrawPositionHandleDropdown();
            DrawUpdateAllToggle();
            DrawSceneToolShortcutsInfoLabel();

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

            DrawShortcutsInfoLabel();

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

            EditorGUILayout.Space();

            DrawAdvancedSettingsFoldout();
            DrawAdvancedSettingsControls();
        }
        private static void FocusOnSceneView() {
            if (SceneView.sceneViews.Count > 0) {
                var sceneView = (SceneView) SceneView.sceneViews[0];
                sceneView.Focus();
            }
        }

        /// <summary>
        /// Add new path node between two others, exactly in the middle.
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
        /// Converts ease value to degrees that can be displyed with arc handle.
        /// </summary>
        /// <param name="nodeIndex">Node index with the ease value to be converted.</param>
        /// <returns>Ease value as degrees.</returns>
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

        /// <summary>
        /// Copies gizmo icons from component Resource folder to Assets/Gizmos/ATP.
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
                AssetDatabase.CopyAsset(iconPath, "Assets/Gizmos/ATP/" + Path.GetFileName(iconPath) );
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
            positionHandle =
                SettingsSerializedObject.FindProperty("positionHandle");

            SerializedPropertiesInitialized = true;
        }

        private bool RequiredAssetsLoaded() {
            var assetsLoaded = (bool) Utilities.InvokeMethodWithReflection(
                Script,
                "RequiredAssetsLoaded",
                null);

            return assetsLoaded;
        }

        /// <summary>
        /// Validate inspector settings that cannot be validated in OnValidate().
        /// </summary>
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

        #endregion PRIVATE METHODS
        #region HELPER METHODS
        /// <summary>
        /// Multiply each ease value by a difference between two given values.
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
        /// Multiply each tilting value by a difference between two given values.
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

        private void HandlePlayPauseButton() {
            Utilities.InvokeMethodWithReflection(
                Script,
                "HandlePlayPause",
                null);
        }

        private void HandleShortcuts() {
            serializedObject.Update();

            // Ease handle mode.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.EaseModeKey) {

                Script.SettingsAsset.HandleMode = HandleMode.Ease;
            }

            // Rotation mode key.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.RotationModeKey) {

                Script.SettingsAsset.HandleMode = HandleMode.Rotation;
            }

            // Tilting mode key.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.TiltingModeKey) {

                Script.SettingsAsset.HandleMode = HandleMode.Tilting;
            }

            // None mode key.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.NoneModeKey) {

                Script.SettingsAsset.HandleMode = HandleMode.None;
            }

            // Update all mode.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.UpdateAllKey) {

                Script.SettingsAsset.UpdateAllMode =
                    !Script.SettingsAsset.UpdateAllMode;
            }

            // Update position handle.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.PositionHandleKey) {

                // Change to Position mode.
                if (Script.SettingsAsset.PositionHandle == PositionHandle.Free) {
                    Script.SettingsAsset.PositionHandle =
                        PositionHandle.Position;
                }
                // Change to Free mode.
                else {
                    Script.SettingsAsset.PositionHandle =
                        PositionHandle.Free;
                }
            }

            // Short jump forward.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.ShortJumpForwardKey
                && FlagsHelper.IsSet(
                    Event.current.modifiers,
                    Script.SettingsAsset.ModKey)) {

                var newAnimationTimeRatio =
                    animationTime.floatValue
                    + Script.SettingsAsset.ShortJumpValue;

                animationTime.floatValue =
                    (float) (Math.Round(newAnimationTimeRatio, 3));
            }

            // Short jump backward.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.ShortJumpBackwardKey
                && FlagsHelper.IsSet(
                    Event.current.modifiers,
                    Script.SettingsAsset.ModKey)) {

                var newAnimationTimeRatio =
                    animationTime.floatValue
                    - Script.SettingsAsset.ShortJumpValue;

                animationTime.floatValue =
                    (float) (Math.Round(newAnimationTimeRatio, 3));
            }

            // Long jump forward.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.LongJumpForwardKey
                && !FlagsHelper.IsSet(
                    Event.current.modifiers,
                    Script.SettingsAsset.ModKey)) {

                animationTime.floatValue += Script.SettingsAsset.LongJumpValue;
            }

            // Long jump backward.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.LongJumpBackwardKey
                && !FlagsHelper.IsSet(
                    Event.current.modifiers,
                    Script.SettingsAsset.ModKey)) {

                animationTime.floatValue -= Script.SettingsAsset.LongJumpValue;
            }

            // Jump to next node.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.JumpToNextNodeKey) {

                animationTime.floatValue =
                    (float) Utilities.InvokeMethodWithReflection(
                        Script,
                        "GetNearestForwardNodeTimestamp",
                        null);
            }

            // Jump to previous node.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.JumpToPreviousNodeKey) {

                animationTime.floatValue =
                    (float) Utilities.InvokeMethodWithReflection(
                        Script,
                        "GetNearestBackwardNodeTimestamp",
                        null);
            }

            // Jump to start.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.JumpToStartKey
                && FlagsHelper.IsSet(
                    Event.current.modifiers,
                    Script.SettingsAsset.ModKey)) {

                animationTime.floatValue = 0;
            }

            // Jump to end.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.JumpToEndKey
                && FlagsHelper.IsSet(
                    Event.current.modifiers,
                    Script.SettingsAsset.ModKey)) {

                animationTime.floatValue = 1;
            }

            // Play/pause animation.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode
                == Script.SettingsAsset.PlayPauseKey) {

                HandlePlayPauseButton();
            }

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
                        "Number of points to export for 1 m of the animation path. " +
                        "If set to 0, it'll export only keys defined in " +
                        "the path."),
                    Script.SettingsAsset.ExportSamplingFrequency);

            if (GUILayout.Button("Export")) {
                Script.ExportNodes(Script.SettingsAsset.ExportSamplingFrequency);
            }

            EditorGUILayout.EndHorizontal();
        }

        #endregion
    }

}