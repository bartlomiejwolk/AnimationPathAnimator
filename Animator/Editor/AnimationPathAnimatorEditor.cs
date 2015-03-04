using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ATP.SimplePathAnimator.Animator {

    [CustomEditor(typeof (Animator))]
    public class AnimatorEditor : Editor {
        #region FIELDS

        /// <summary>
        ///     Reference to target script.
        /// </summary>
        private Animator script;

        private AnimatorShortcuts animatorShortcuts;

        #endregion FIELDS
        #region PROPERTIES

        public AnimatorHandles AnimatorHandles { get; private set; }

        // TODO Rename to GizmoDrawerSerObj.
        // TODO Make private.
        public SerializedObject GizmoDrawer { get; private set; }
        private SerializedObject SettingsSerObj { get; set; }
        //public SerializedObject PathExporterSerObj { get; private set; }

        /// <summary>
        ///     Reference to target script.
        /// </summary>
        public Animator Script {
            get { return script; }
        }

        public AnimatorShortcuts AnimatorShortcuts {
            get { return animatorShortcuts; }
            set { animatorShortcuts = value; }
        }

        public AnimatorSettings AnimatorSettings { get; private set; }
        private PathExporter PathExporter { get; set; }

        #endregion 

        #region SERIALIZED PROPERTIES

        private SerializedProperty advancedSettingsFoldout;
        private SerializedProperty animatedGO;
        private SerializedProperty animationTimeRatio;
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
            DrawRotationLerpSpeedField();
            DrawForwardPointOffsetField();
            //DrawMaxAnimationSpeedField();

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

            // TODO Create control section for Lerp settings.

            //EditorGUILayout.Space();

            //DrawAdvancedSettingsFoldout();
            //DrawAdvanceSettingsControls();

            DrawSettingsAssetField();
            DrawSkinSelectionControl();
        }

        private void DrawSettingsAssetField() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                settings,
                new GUIContent(
                    "Settings Asset",
                    ""));

            serializedObject.ApplyModifiedProperties();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        protected virtual void OnEnable() {
            // Get target script reference.
            script = (Animator) target;

            // Initialize AnimatorSettings property.
            AnimatorSettings = Script.Settings;

            PathExporter = new PathExporter(Script);

            InstantiateCompositeClasses();
            InitializeSerializedProperties();

            SceneTool.RememberCurrentTool();

            FocusOnSceneView();
        }
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnDisable() {
            SceneTool.RestoreTool();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnSceneGUI() {
            CheckForSkinAsset();

            // Return if path asset does not exist.
            if (Script.PathData == null) return;

            // Disable interaction with background scene elements.
            HandleUtility.AddDefaultControl(
                GUIUtility.GetControlID(
                    FocusType.Passive));

            AnimatorShortcuts.HandleShortcuts();

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
        private void HandleDrawingUpdateAllModeLabel() {
            if (!AnimatorSettings.UpdateAllMode) return;

            //AnimatorHandles.DrawNodeLabels(
            //    Script,
            //    "A",
            //    upall
            //    Script.Skin.GetStyle("UpdateAllLabel"));

            AnimatorHandles.DrawUpdateAllLabels(
                Script,
                Script.Skin.GetStyle("UpdateAllLabel"));
        }


        protected virtual void DrawAdvancedSettingsFoldout() {
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

        protected virtual void DrawAdvanceSettingsControls() {
            if (advancedSettingsFoldout.boolValue) {
                DrawGizmoCurveColorPicker();
                DrawRotationCurveColorPicker();

                EditorGUILayout.Space();

                // TODO Limit these values in OnValidate().
                DrawPositionLerpSpeedControl();
                DrawRotationLerpSpeedField();
                DrawForwardPointOffsetField();
                DrawMaxAnimationSpeedField();

                EditorGUILayout.Space();

                DrawSkinSelectionControl();
            }
        }

        protected virtual void DrawAnimatedGOField() {
            EditorGUILayout.PropertyField(
                animatedGO,
                new GUIContent(
                    "Animated Object",
                    "Object to animate."));
        }

        protected virtual void DrawAnimationTimeControl() {
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

        protected virtual void DrawAutoPlayControl() {
            AnimatorSettings.AutoPlay = EditorGUILayout.Toggle(
                new GUIContent(
                    "Auto Play",
                    ""),
                AnimatorSettings.AutoPlay);
        }

        protected virtual void DrawEnableControlsInPlayModeToggle() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(
                enableControlsInPlayMode,
                new GUIContent(
                    "Play Mode Controls",
                    "Enable keybord controls in play mode."));
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawForwardPointOffsetField() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(forwardPointOffset);
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawGizmoCurveColorPicker() {

            GizmoDrawer.Update();
            EditorGUILayout.PropertyField(
                gizmoCurveColor,
                new GUIContent("Curve Color", ""));
            GizmoDrawer.ApplyModifiedProperties();
        }

        protected virtual void DrawHandleModeDropdown() {
            AnimatorSettings.HandleMode = (AnimatorHandleMode) EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Handle Mode",
                    ""),
                AnimatorSettings.HandleMode);
        }

        protected virtual void DrawMaxAnimationSpeedField() {

            serializedObject.Update();
            EditorGUILayout.PropertyField(maxAnimationSpeed);
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawMovementModeDropdown() {
            AnimatorSettings.MovementMode =
                (MovementMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Movement Mode",
                        ""),
                    AnimatorSettings.MovementMode);
        }

        protected virtual void DrawPathDataAssetField() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                pathData,
                new GUIContent(
                    "Path Asset",
                    ""));

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawPlayerControls() {
            EditorGUILayout.BeginHorizontal();

            // Play/Pause button text.
            string playPauseBtnText;
            if (!Script.IsPlaying || (Script.IsPlaying && Script.Pause)) {
                playPauseBtnText = "Play";
            }
            else {
                playPauseBtnText = "Pause";
            }

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

        protected virtual void DrawPositionLerpSpeedControl() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(
                positionLerpSpeed,
                new GUIContent(
                    "Position Lerp Speed",
                    ""));
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawRotationCurveColorPicker() {

            GizmoDrawer.Update();
            EditorGUILayout.PropertyField(rotationCurveColor);
            GizmoDrawer.ApplyModifiedProperties();
        }

        protected virtual void DrawRotationLerpSpeedField() {
            EditorGUILayout.PropertyField(
                rotationSpeed,
                new GUIContent(
                    "Rotation Lerp Speed",
                    "Controls how much time (in seconds) it'll take the " +
                    "animated object to finish rotation towards followed target."));
        }

        protected virtual void DrawSkinSelectionControl() {

            serializedObject.Update();
            EditorGUILayout.PropertyField(
                skin,
                new GUIContent(
                    "Skin Asset",
                    ""));
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawTangentModeDropdown() {
            // Remember current tangent mode.
            var prevTangentMode = AnimatorSettings.TangentMode;

            // Draw tangent mode dropdown.
            AnimatorSettings.TangentMode =
                (TangentMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Tangent Mode",
                        ""),
                    AnimatorSettings.TangentMode);

            // Update gizmo curve is tangent mode changed.
            if (AnimatorSettings.TangentMode != prevTangentMode) {
                HandleTangentModeChange();
            }
        }

        protected virtual void DrawTargetGOField() {
            var prevTargetGO = Script.TargetGO;
            serializedObject.Update();
            EditorGUILayout.PropertyField(
                targetGO,
                new GUIContent(
                    "Target Object",
                    "Object that the animated object will be looking at."));
            serializedObject.ApplyModifiedProperties();

            if (Script.TargetGO != prevTargetGO
                && prevTargetGO == null) {

                Script.Settings.RotationMode = AnimatorRotationMode.Target;
            }
            else if (Script.TargetGO != prevTargetGO
                && prevTargetGO != null
                     && Script.TargetGO == null) {
                
                Script.Settings.RotationMode = AnimatorRotationMode.Forward;
            }
        }

        protected virtual void DrawUpdateAllToggle() {
            AnimatorSettings.UpdateAllMode = EditorGUILayout.Toggle(
                new GUIContent(
                    "Update All Values",
                    ""),
                AnimatorSettings.UpdateAllMode);
        }

        protected virtual void DrawWrapModeDropdown() {
            AnimatorSettings.WrapMode = (WrapMode) EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Wrap Mode",
                    ""),
                AnimatorSettings.WrapMode);
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
                    "Path1",
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
            // Remember current RotationMode.
            var prevRotationMode = AnimatorSettings.RotationMode;
            // Draw RotationMode dropdown.
            AnimatorSettings.RotationMode =
                (AnimatorRotationMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Rotation Mode",
                        ""),
                    AnimatorSettings.RotationMode);

            // TODO Execute it as a callback.
            // If value changed, update animated GO in the scene.
            if (AnimatorSettings.RotationMode != prevRotationMode) {
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
            AnimatorHandles.DrawAddNodeButtons(
                nodePositions,
                callbackHandler,
                addButtonStyle);
        }

        private void HandleDrawingEaseHandles() {
            if (AnimatorSettings.HandleMode != AnimatorHandleMode.Ease) return;

            // TODO Move this code to AnimatorHandles.DrawEaseHandles().

            // Get path node positions.
            var nodePositions =
                Script.PathData.GetGlobalNodePositions(Script.ThisTransform);

            // Get ease values.
            var easeCurveValues = Script.PathData.GetEaseCurveValues();

            // TODO Use property.
            var arcValueMultiplier = 360 / maxAnimationSpeed.floatValue;

            AnimatorHandles.DrawEaseHandles(
                nodePositions,
                easeCurveValues,
                arcValueMultiplier,
                DrawEaseHandlesCallbackHandler);
        }

        private void HandleDrawingEaseLabel() {
            if (AnimatorSettings.HandleMode != AnimatorHandleMode.Ease) return;

            // Get node global positions.
            var nodeGlobalPositions = Script.PathData.GetGlobalNodePositions(
                Script.ThisTransform);

            AnimatorHandles.DrawArcHandleLabels(
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
            AnimatorHandles.DrawMoveSinglePositionsHandles(
                Script,
                DrawPositionHandlesCallbackHandler);

            AnimatorHandles.DrawMoveAllPositionHandles(
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
            AnimatorHandles.DrawRemoveNodeButtons(
                nodes,
                removeNodeCallback,
                removeButtonStyle);
        }

        private void HandleDrawingRotationHandle() {
            if (AnimatorSettings.HandleMode != AnimatorHandleMode.Rotation) return;

            var currentAnimationTime = script.AnimationTimeRatio;
            var rotationPointPosition =
                script.PathData.GetRotationAtTime(currentAnimationTime);
            var nodeTimestamps = script.PathData.GetPathTimestamps();

            // Return if current animation time is not equal to any node
            // timestamp.
            var index = Array.FindIndex(
                nodeTimestamps,
                x => Math.Abs(x - currentAnimationTime)
                    < GlobalConstants.FloatPrecision);

            if (index < 0) return;

            AnimatorHandles.DrawRotationHandle(
                Script,
                rotationPointPosition,
                DrawRotationHandlesCallbackHandler);
        }

        private void HandleDrawingTiltingHandles() {
            if (AnimatorSettings.HandleMode != AnimatorHandleMode.Tilting) return;

            Action<int, float> callbackHandler =
                DrawTiltingHandlesCallbackHandler;

            // Get path node positions.
            var nodePositions =
                Script.PathData.GetGlobalNodePositions(Script.ThisTransform);

            // Get tilting curve values.
            var tiltingCurveValues = Script.PathData.GetTiltingCurveValues();

            AnimatorHandles.DrawTiltingHandles(
                nodePositions,
                tiltingCurveValues,
                callbackHandler);
        }

        private void HandleDrawingTiltLabel() {
            if (AnimatorSettings.HandleMode != AnimatorHandleMode.Tilting) return;

            // Get node global positions.
            var nodeGlobalPositions = Script.PathData.GetGlobalNodePositions(
                Script.ThisTransform);

            AnimatorHandles.DrawArcHandleLabels(
                nodeGlobalPositions,
                ConvertTiltToDegrees,
                Script.Skin.GetStyle("TiltValueLabel"));
        }

        #endregion DRAWING HANDLERS

        #region CALLBACK HANDLERS

        protected virtual void DrawAddNodeButtonsCallbackHandler(int nodeIndex) {
            // Make snapshot of the target object.
            Undo.RecordObject(Script.PathData, "Change path");

            // Add a new node.
            AddNodeBetween(nodeIndex);

            Script.PathData.DistributeTimestamps();

            if (AnimatorSettings.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
            else if (AnimatorSettings.TangentMode == TangentMode.Linear) {
                Script.PathData.SetNodesLinear();
            }
        }

        protected virtual void DrawPositionHandlesCallbackHandler(
            int movedNodeIndex,
            Vector3 position,
            Vector3 moveDelta) {

            Undo.RecordObject(Script.PathData, "Change path");

            HandleMoveAllMovementMode(moveDelta);
            HandleMoveSingleHandleMove(movedNodeIndex, position);
        }

        protected virtual void DrawRemoveNodeButtonsCallbackHandles(
            int nodeIndex) {
            Undo.RecordObject(Script.PathData, "Change path");

            Script.PathData.RemoveNode(nodeIndex);
            Script.PathData.DistributeTimestamps();

            if (AnimatorSettings.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
            else if (AnimatorSettings.TangentMode == TangentMode.Linear) {
                Script.PathData.SetNodesLinear();
            }
        }

        private void DrawEaseHandlesCallbackHandler(
            int keyIndex,
            float newValue) {
            Undo.RecordObject(Script.PathData, "Ease curve changed.");

            if (AnimatorSettings.UpdateAllMode) {
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

            if (AnimatorSettings.UpdateAllMode) {
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
            if (AnimatorSettings.TangentMode == TangentMode.Linear) {
                Script.PathData.SetNodesLinear();
            }
        }

        private void HandleMoveAllMovementMode(Vector3 moveDelta) {
            if (AnimatorSettings.MovementMode == MovementMode.MoveAll) {
                Script.PathData.OffsetNodePositions(moveDelta);
                Script.PathData.OffsetRotationPathPosition(moveDelta);
            }
        }

        private void HandleMoveSingleHandleMove(
            int movedNodeIndex,
            Vector3 position) {
            if (AnimatorSettings.MovementMode == MovementMode.MoveSingle) {

                Script.PathData.MoveNodeToPosition(movedNodeIndex, position);
                Script.PathData.DistributeTimestamps();

                HandleSmoothTangentMode();
                HandleLinearTangentMode();
            }
        }

        private void HandleSmoothTangentMode() {
            if (AnimatorSettings.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
        }

        private void HandleTangentModeChange() {
            if (Script.PathData == null) return;

            // Update path node tangents.
            if (AnimatorSettings.TangentMode == TangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
            else if (AnimatorSettings.TangentMode == TangentMode.Linear) {
                Script.PathData.SetNodesLinear();
            }
        }

        #endregion
        #region METHODS
        protected void AddNodeBetween(int nodeIndex) {
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
            animationTimeRatio =
                serializedObject.FindProperty("animationTimeRatio");
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
            rotationCurveColor = GizmoDrawer.FindProperty("rotationCurveColor");
            gizmoCurveColor = GizmoDrawer.FindProperty("gizmoCurveColor");
            settings = serializedObject.FindProperty("settings");
        }

        private void InstantiateCompositeClasses() {
            GizmoDrawer = new SerializedObject(Script.AnimatorGizmos);
            SettingsSerObj = new SerializedObject(Script.Settings);
            //PathExporterSerObj = new SerializedObject(PathExporter);
            AnimatorHandles = new AnimatorHandles(Script);
            AnimatorShortcuts = new AnimatorShortcuts(Script);
        }
  

        #endregion PRIVATE METHODS
    }

}