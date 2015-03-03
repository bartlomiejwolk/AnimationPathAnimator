using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Assets.Extensions.animationpathtools.Include.Editor;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools {

    [CustomEditor(typeof (AnimationPathAnimator))]
    public class AnimatorEditor : Editor {
        #region FIELDS

        /// <summary>
        ///     Reference to target script.
        /// </summary>
        private AnimationPathAnimator script;

        private AnimatorShortcuts animatorShortcuts;
        #endregion FIELDS
        #region PROPERTIES

        public AnimatorHandles AnimatorHandles { get; private set; }

        public SerializedObject GizmoDrawer { get; private set; }

        public bool ModKeyPressed { get; private set; }

        /// <summary>
        ///     Reference to target script.
        /// </summary>
        public AnimationPathAnimator Script {
            get { return script; }
        }

        public AnimatorShortcuts AnimatorShortcuts {
            get { return animatorShortcuts; }
            set { animatorShortcuts = value; }
        }
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


        #endregion SERIALIZED PROPERTIES

        #region UNITY MESSAGES

        public override void OnInspectorGUI() {
            DrawPathDataAssetControl();

            EditorGUILayout.BeginHorizontal();

            DrawCreatePathAssetButton();
            DrawResetPathInspectorButton();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            DrawResetRotationPathButton();
            DrawResetEaseButton();
            DrawResetTiltingButton();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            DrawAnimatedGOControl();
            DrawTargetGOControl();

            EditorGUILayout.Space();

            DrawAnimationTimeControl();
            DrawRotationModeDropdown();
            DrawHandleModeDropdown();
            DrawMovementModeDropdown();
            DrawTangentModeDropdown();
            DrawWrapModeDropdown();

            EditorGUILayout.Space();

            DrawAutoPlayControl();
            DrawEnableControlsInPlayModeToggle();
            DrawUpdateAllToggle();

            EditorGUILayout.Space();

            DrawPlayerControls();

            EditorGUILayout.Space();

            PathExporter.DrawExportControls(Script);

            EditorGUILayout.Space();

            DrawAdvancedSettingsFoldout();
            DrawAdvanceSettingsControls();
        }
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        protected virtual void OnEnable() {
            // Get target script reference.
            script = (AnimationPathAnimator) target;

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

        private void HandleDrawingUpdateAllModeLabel() {
            if (!Script.UpdateAllMode) return;

            //AnimatorHandles.DrawNodeLabels(
            //    Script,
            //    "A",
            //    upall
            //    Script.Skin.GetStyle("UpdateAllLabel"));

            AnimatorHandles.DrawUpdateAllLabels(
                Script,
                Script.Skin.GetStyle("UpdateAllLabel"));
        }

        #endregion UNITY MESSAGES

        #region INSPECTOR

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
                DrawRotationSpeedField();
                DrawForwardPointOffsetField();
                DrawMaxAnimationSpeedField();

                EditorGUILayout.Space();

                DrawSkinSelectionControl();
            }
        }

        protected virtual void DrawAnimatedGOControl() {
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
            Script.AutoPlay = EditorGUILayout.Toggle(
                new GUIContent(
                    "Auto Play",
                    ""),
                Script.AutoPlay);
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
            Script.HandleMode = (AnimatorHandleMode) EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Handle Mode",
                    ""),
                Script.HandleMode);
        }

        protected virtual void DrawMaxAnimationSpeedField() {

            serializedObject.Update();
            EditorGUILayout.PropertyField(maxAnimationSpeed);
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawMovementModeDropdown() {
            Script.MovementMode =
                (AnimationPathBuilderHandleMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Movement Mode",
                        ""),
                    Script.MovementMode);
        }

        protected virtual void DrawPathDataAssetControl() {
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

        protected virtual void DrawRotationSpeedField() {
            EditorGUILayout.PropertyField(
                rotationSpeed,
                new GUIContent(
                    "Rotation Speed",
                    "Controls how much time (in seconds) it'll take the " +
                    "animated object to finish rotation towards followed target."));
        }

        protected virtual void DrawSkinSelectionControl() {

            serializedObject.Update();
            EditorGUILayout.PropertyField(skin);
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawTangentModeDropdown() {
            // Remember current tangent mode.
            var prevTangentMode = Script.TangentMode;

            // Draw tangent mode dropdown.
            Script.TangentMode =
                (AnimationPathBuilderTangentMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Tangent Mode",
                        ""),
                    Script.TangentMode);

            // Update gizmo curve is tangent mode changed.
            if (Script.TangentMode != prevTangentMode) {
                HandleTangentModeChange();
            }
        }

        protected virtual void DrawTargetGOControl() {
            EditorGUILayout.PropertyField(
                targetGO,
                new GUIContent(
                    "Target Object",
                    "Object that the animated object will be looking at."));
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawUpdateAllToggle() {
            Script.UpdateAllMode = EditorGUILayout.Toggle(
                new GUIContent(
                    "Update All Values",
                    ""),
                Script.UpdateAllMode);
        }

        protected virtual void DrawWrapModeDropdown() {
            Script.WrapMode = (WrapMode) EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Wrap Mode",
                    ""),
                Script.WrapMode);
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
                    "New Asset",
                    ""))) {

                // Create new path asset.
                var asset = ScriptableObjectUtility.CreateAsset<PathData>();

                // Assign asset as the current path.
                Script.PathData = asset;
            }
        }

        private void DrawResetPathInspectorButton() {
            if (GUILayout.Button(
                new GUIContent(
                    "Reset Asset",
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
            var prevRotationMode = Script.RotationMode;
            // Draw RotationMode dropdown.
            Script.RotationMode =
                (AnimatorRotationMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Rotation Mode",
                        ""),
                    Script.RotationMode);

            // TODO Execute it as a callback.
            // If value changed, update animated GO in the scene.
            if (Script.RotationMode != prevRotationMode) {
                Script.UpdateAnimation();
            }
        }

        #endregion
        #region DRAWING HANDLERS

        private void HandleDrawingAddButtons() {
            // Get positions at which to draw movement handles.
            var nodePositions = Script.PathData.GetGlobalNodePositions(
                Script.Transform);

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
            if (Script.HandleMode != AnimatorHandleMode.Ease) return;

            // TODO Move this code to AnimatorHandles.DrawEaseHandles().

            // Get path node positions.
            var nodePositions =
                Script.PathData.GetGlobalNodePositions(Script.Transform);

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
            if (Script.HandleMode != AnimatorHandleMode.Ease) return;

            // Get node global positions.
            var nodeGlobalPositions = Script.PathData.GetGlobalNodePositions(
                Script.Transform);

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
            var nodes = Script.PathData.GetGlobalNodePositions(Script.Transform);

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
            if (Script.HandleMode != AnimatorHandleMode.Rotation) return;

            AnimatorHandles.DrawRotationHandle(
                Script,
                DrawRotationHandlesCallbackHandler);
        }

        private void HandleDrawingTiltingHandles() {
            if (Script.HandleMode != AnimatorHandleMode.Tilting) return;

            Action<int, float> callbackHandler =
                DrawTiltingHandlesCallbackHandler;

            // Get path node positions.
            var nodePositions =
                Script.PathData.GetGlobalNodePositions(Script.Transform);

            // Get tilting curve values.
            var tiltingCurveValues = Script.PathData.GetTiltingCurveValues();

            AnimatorHandles.DrawTiltingHandles(
                nodePositions,
                tiltingCurveValues,
                callbackHandler);
        }

        private void HandleDrawingTiltLabel() {
            if (Script.HandleMode != AnimatorHandleMode.Tilting) return;

            // Get node global positions.
            var nodeGlobalPositions = Script.PathData.GetGlobalNodePositions(
                Script.Transform);

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

            if (Script.TangentMode == AnimationPathBuilderTangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
            else if (Script.TangentMode == AnimationPathBuilderTangentMode.Linear) {
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

            if (Script.TangentMode == AnimationPathBuilderTangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
            else if (Script.TangentMode == AnimationPathBuilderTangentMode.Linear) {
                Script.PathData.SetNodesLinear();
            }
        }

        private void DrawEaseHandlesCallbackHandler(
            int keyIndex,
            float newValue) {
            Undo.RecordObject(Script.PathData, "Ease curve changed.");

            if (Script.UpdateAllMode) {
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

            if (Script.UpdateAllMode) {
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
            if (Script.TangentMode == AnimationPathBuilderTangentMode.Linear) {
                Script.PathData.SetNodesLinear();
            }
        }

        private void HandleMoveAllMovementMode(Vector3 moveDelta) {
            if (Script.MovementMode == AnimationPathBuilderHandleMode.MoveAll) {
                Script.PathData.OffsetNodePositions(moveDelta);
                Script.PathData.OffsetRotationPathPosition(moveDelta);
            }
        }

        private void HandleMoveSingleHandleMove(
            int movedNodeIndex,
            Vector3 position) {
            if (Script.MovementMode == AnimationPathBuilderHandleMode.MoveSingle) {

                Script.PathData.MoveNodeToPosition(movedNodeIndex, position);
                Script.PathData.DistributeTimestamps();

                HandleSmoothTangentMode();
                HandleLinearTangentMode();
            }
        }

        private void HandleSmoothTangentMode() {
            if (Script.TangentMode == AnimationPathBuilderTangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
        }

        private void HandleTangentModeChange() {
            if (Script.PathData == null) return;

            // Update path node tangents.
            if (Script.TangentMode == AnimationPathBuilderTangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
            else if (Script.TangentMode == AnimationPathBuilderTangentMode.Linear) {
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
            rotationSpeed = serializedObject.FindProperty("rotationSpeed");
            animationTimeRatio =
                serializedObject.FindProperty("animationTimeRatio");
            animatedGO = serializedObject.FindProperty("animatedGO");
            targetGO = serializedObject.FindProperty("targetGO");
            forwardPointOffset =
                serializedObject.FindProperty("forwardPointOffset");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");
            maxAnimationSpeed =
                serializedObject.FindProperty("MaxAnimationSpeed");
            positionLerpSpeed =
                serializedObject.FindProperty("positionLerpSpeed");
            pathData = serializedObject.FindProperty("pathData");
            enableControlsInPlayMode =
                serializedObject.FindProperty("EnableControlsInPlayMode");
            skin = serializedObject.FindProperty("skin");
            rotationCurveColor = GizmoDrawer.FindProperty("rotationCurveColor");
            gizmoCurveColor = GizmoDrawer.FindProperty("gizmoCurveColor");
        }

        private void InstantiateCompositeClasses() {
            GizmoDrawer = new SerializedObject(Script.AnimatorGizmos);
            AnimatorHandles = new AnimatorHandles();
            AnimatorShortcuts = new AnimatorShortcuts(Script);
        }
  

        #endregion PRIVATE METHODS
    }

}