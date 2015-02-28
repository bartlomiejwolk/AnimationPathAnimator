using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools {

    [CustomEditor(typeof (AnimationPathAnimator))]
    public class AnimatorEditor : Editor {
        #region DRAWING METHODS

        private void DrawRotationHandle(Action<float, Vector3> callback) {
            var currentAnimationTime = Script.AnimationTimeRatio;
            var rotationPointPosition =
                Script.PathData.GetRotationAtTime(currentAnimationTime);
            var nodeTimestamps = Script.PathData.GetPathTimestamps();

            // Return if current animation time is not equal to any node
            // timestamp.
            var index = Array.FindIndex(
                nodeTimestamps,
                x => Math.Abs(x - currentAnimationTime) < FloatPrecision);
            if (index < 0) return;

            Handles.color = Color.magenta;
            var handleSize = HandleUtility.GetHandleSize(rotationPointPosition);
            var sphereSize = handleSize * RotationHandleSize;

            var rotationPointGlobalPos =
                Script.transform.TransformPoint(rotationPointPosition);

            // Draw node's handle.
            var newGlobalPosition = Handles.FreeMoveHandle(
                rotationPointGlobalPos,
                Quaternion.identity,
                sphereSize,
                Vector3.zero,
                Handles.SphereCap);

            if (newGlobalPosition != rotationPointGlobalPos) {
                var newPointLocalPosition =
                    Script.transform.InverseTransformPoint(newGlobalPosition);
                // Execute callback.
                callback(currentAnimationTime, newPointLocalPosition);
            }
        }

        #endregion DRAWING METHODS

        #region FIELDS

        /// <summary>
        ///     Reference to target script.
        /// </summary>
        private AnimationPathAnimator script;

        public AnimatorHandles AnimatorHandles { get; private set; }

        public SerializedObject GizmoDrawer { get; private set; }

        /// <summary>
        ///     Reference to target script.
        /// </summary>
        public AnimationPathAnimator Script {
            get { return script; }
        }

        #region SERIALIZED PROPERTIES

        private SerializedProperty advancedSettingsFoldout;
        private SerializedProperty animatedGO;
        private SerializedProperty animTimeRatio;
        private SerializedProperty enableControlsInPlayMode;
        private SerializedProperty exportSamplingFrequency;
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

        #endregion FIELDS

        #region CONSTANT VALUES

        public virtual float FloatPrecision {
            get { return 0.001f; }
        }

        public virtual float LongJumpValue {
            get { return 0.01f; }
        }

        public virtual Color MoveAllModeColor {
            get { return Color.red; }
        }

        public virtual float RotationHandleSize {
            get { return 0.25f; }
        }

        #endregion

        #region UNITY MESSAGES

        public override void OnInspectorGUI() {
            DrawPathDataAssetControl();
            DrawAnimationTimeControl();
            DrawWrapModeDropdown();
            DrawHandleModeDropdown();
            DrawUpdateAllToggle();
            DrawPositionLerpSpeedControl();
            DrawRotationModeControls();

            EditorGUILayout.Space();

            DrawTangentModeDropdown();
            DrawMovementModeDropdown();
            DrawResetPathInspectorButton();

            EditorGUILayout.Space();

            DrawAnimatedGOControl();
            DrawTargetGOControl();

            EditorGUILayout.Space();

            DrawPlayerControls();
            DrawAutoPlayControl();
            DrawEnableControlsInPlayModeToggle();

            EditorGUILayout.Space();

            PathExporter.DrawExportControls(
                serializedObject,
                exportSamplingFrequency,
                Script.PathData);

            EditorGUILayout.Space();

            DrawAdvancedSettingsFoldout();
            DrawAdvanceSettingsControls();
        }

        private void DrawRotationModeControls() {
            // Remember current RotationMode.
            var prevRotationMode = Script.RotationMode;
            // Draw RotationMode dropdown.
            Script.RotationMode =
                (AnimatorRotationMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Rotation Mode",
                        ""),
                    Script.RotationMode);

            // If value changed, update animated GO in the scene.
            if (Script.RotationMode != prevRotationMode) {
                Script.UpdateAnimatedGO();
            }

            serializedObject.Update();

            if (Script.RotationMode == AnimatorRotationMode.Forward) {
                EditorGUILayout.PropertyField(forwardPointOffset);
            }

            EditorGUILayout.PropertyField(
                rotationSpeed,
                new GUIContent(
                    "Rotation Speed",
                    "Controls how much time (in seconds) it'll take the " +
                    "animated object to finish rotation towards followed target."));

            serializedObject.ApplyModifiedProperties();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnDisable() {
            SceneTool.RestoreTool();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnEnable() {
            // Get target script reference.
            script = (AnimationPathAnimator) target;

            SceneTool.RememberCurrentTool();

            GizmoDrawer = new SerializedObject(Script.AnimatorGizmos);
            AnimatorHandles = new AnimatorHandles();

            rotationSpeed = serializedObject.FindProperty("rotationSpeed");
            animTimeRatio = serializedObject.FindProperty("animTimeRatio");
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
            gizmoCurveColor = serializedObject.FindProperty("gizmoCurveColor");
            exportSamplingFrequency =
                serializedObject.FindProperty("exportSamplingFrequency");
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnSceneGUI() {
            CheckForSkinAsset();

            // Return if path asset does not exist.
            if (Script.PathData == null) return;

            HandleShortcuts();

            ShortcutHandler.HandlePlayPauseShortcut(HandlePlayPause);

            // Change current animation time with arrow keys.
            //ShortcutHandler.HandleModifiedJumpShortcuts(
            //    ModJumpForwardCallbackHandler,
            //    ModJumpBackwardCallbackHandler,
            //    JumpToNextNodeCallbackHandler,
            //    JumpToPreviousNodeCallbackHandler,
            //    AnyModJumpKeyPressedCallbackHandler);

            //ShortcutHandler.HandleUnmodifiedJumpShortcuts(
            //    JumpBackwardCallbackHandler,
            //    JumpForwardCallbackHandler,
            //    JumpToStartCallbackHandler,
            //    JumpToEndCallbackHandler,
            //    AnyJumpKeyPressedCallbackHandler);

            HandleWrapModeDropdown();
            HandleDrawingEaseHandles();
            HandleDrawingRotationHandle();
            HandleDrawingTiltingHandles();
            HandleDrawingEaseLabel();
            HandleDrawingTiltLabel();
            HandleDrawingPositionHandles();
            HandleDrawingAddButtons();
            HandleDrawingRemoveButtons();
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

        protected virtual void DrawAdvanceSettingsControls() {
            if (advancedSettingsFoldout.boolValue) {
                serializedObject.Update();
                EditorGUILayout.PropertyField(
                    gizmoCurveColor,
                    new GUIContent("Curve Color", ""));
                serializedObject.ApplyModifiedProperties();

                GizmoDrawer.Update();
                EditorGUILayout.PropertyField(rotationCurveColor);
                GizmoDrawer.ApplyModifiedProperties();

                serializedObject.Update();
                EditorGUILayout.PropertyField(maxAnimationSpeed);
                EditorGUILayout.PropertyField(skin);
                serializedObject.ApplyModifiedProperties();
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
            serializedObject.Update();
            animTimeRatio.floatValue = EditorGUILayout.FloatField(
                new GUIContent(
                    "Animation Time",
                    "Current animation time."),
                animTimeRatio.floatValue);
            serializedObject.ApplyModifiedProperties();
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

        protected virtual void DrawHandleModeDropdown() {
            Script.HandleMode = (AnimatorHandleMode) EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Handle Mode",
                    ""),
                Script.HandleMode);
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

                HandlePlayPause();
            }

            // Draw Stop button.
            if (GUILayout.Button(
                new GUIContent(
                    "Stop",
                    ""))) {
                Script.StopEaseTimeCoroutine();
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
            if (Script.TangentMode != prevTangentMode)
                HandleTangentModeChange();
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
                    "Update All",
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
            AnimatorHandles.DrawAddNodeButtons(
                nodePositions,
                callbackHandler,
                addButtonStyle);
        }

        private void HandleDrawingEaseHandles() {
            if (Script.HandleMode != AnimatorHandleMode.Ease) return;

            // Get path node positions.
            var nodePositions =
                Script.GetGlobalNodePositions();

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
            var nodeGlobalPositions = Script.GetGlobalNodePositions();

            AnimatorHandles.DrawNodeLabels(
                nodeGlobalPositions,
                ConvertEaseToDegrees,
                Script.Skin.GetStyle("EaseValueLabel"));
        }

        private void HandleDrawingMoveAllPositionHandles(
            Action<int, Vector3, Vector3> callback) {

            if (script.MovementMode !=
                AnimationPathBuilderHandleMode.MoveAll) return;

            // Node global positions.
            var nodes = Script.GetGlobalNodePositions();

            // Cap function used to draw handle.
            Handles.DrawCapFunction capFunction = Handles.CircleCap;

            // For each node..
            for (var i = 0; i < nodes.Length; i++) {
                var handleColor = MoveAllModeColor;

                // Draw position handle.
                var newPos = AnimatorHandles.DrawPositionHandle(
                    nodes[i],
                    handleColor,
                    capFunction);

                // If node was moved..
                if (newPos != nodes[i]) {
                    // Calculate node old local position.
                    var oldNodeLocalPosition =
                        Script.transform.InverseTransformPoint(nodes[i]);

                    // Calculate node new local position.
                    var newNodeLocalPosition =
                        Script.transform.InverseTransformPoint(newPos);

                    // Calculate movement delta.
                    var moveDelta = newNodeLocalPosition - oldNodeLocalPosition;

                    // Execute callback.
                    callback(i, newNodeLocalPosition, moveDelta);
                }
            }
        }

        private void HandleDrawingMoveSinglePositionsHandles(
            Action<int, Vector3, Vector3> callback) {

            if (script.MovementMode !=
                AnimationPathBuilderHandleMode.MoveSingle) return;

            // Node global positions.
            var nodes = Script.GetGlobalNodePositions();

            // Cap function used to draw handle.
            Handles.DrawCapFunction capFunction = Handles.CircleCap;

            // For each node..
            for (var i = 0; i < nodes.Length; i++) {
                var handleColor = script.GizmoCurveColor;

                // Draw position handle.
                var newPos = AnimatorHandles.DrawPositionHandle(
                    nodes[i],
                    handleColor,
                    capFunction);

                // TODO Make it into callback.
                // If node was moved..
                if (newPos != nodes[i]) {
                    // Calculate node old local position.
                    var oldNodeLocalPosition =
                        Script.transform.InverseTransformPoint(nodes[i]);

                    // Calculate node new local position.
                    var newNodeLocalPosition =
                        Script.transform.InverseTransformPoint(newPos);

                    // Calculate movement delta.
                    var moveDelta = newNodeLocalPosition - oldNodeLocalPosition;

                    // Execute callback.
                    callback(i, newNodeLocalPosition, moveDelta);
                }
            }
        }

        /// <summary>
        ///     Handle drawing movement handles.
        /// </summary>
        private void HandleDrawingPositionHandles() {
            // Callback to call when a node is moved on the scene.
            Action<int, Vector3, Vector3> handlerCallback =
                DrawPositionHandlesCallbackHandler;

            // Draw handles.
            HandleDrawingMoveSinglePositionsHandles(handlerCallback);
            HandleDrawingMoveAllPositionHandles(handlerCallback);
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
            AnimatorHandles.DrawRemoveNodeButtons(
                nodes,
                removeNodeCallback,
                removeButtonStyle);
        }

        private void HandleDrawingRotationHandle() {
            if (Script.HandleMode != AnimatorHandleMode.Rotation) return;

            // Draw handles.
            DrawRotationHandle(DrawRotationHandlesCallbackHandler);
        }

        private void HandleDrawingTiltingHandles() {
            if (Script.HandleMode != AnimatorHandleMode.Tilting) return;

            Action<int, float> callbackHandler =
                DrawTiltingHandlesCallbackHandler;

            // Get path node positions.
            var nodePositions =
                Script.GetGlobalNodePositions();

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
            var nodeGlobalPositions = Script.GetGlobalNodePositions();

            AnimatorHandles.DrawNodeLabels(
                nodeGlobalPositions,
                ConvertTiltToDegrees,
                Script.Skin.GetStyle("TiltValueLabel"));
        }

        #endregion DRAWING HANDLERS

        #region OTHER HANDLERS
        private void HandleShortcuts() {
            ShortcutHandler.HandleShortcut(
                () => Script.HandleMode = AnimatorHandleMode.Ease,
                KeyCode.C);

            ShortcutHandler.HandleShortcut(
                () => Script.HandleMode = AnimatorHandleMode.Rotation,
                KeyCode.V);

            ShortcutHandler.HandleShortcut(
                () => Script.HandleMode = AnimatorHandleMode.Tilting,
                KeyCode.B);

            ShortcutHandler.HandleShortcut(
                () => Script.HandleMode = AnimatorHandleMode.None,
                KeyCode.N);

            ShortcutHandler.HandleShortcut(
                () => Script.UpdateAllMode = !Script.UpdateAllMode,
                KeyCode.M);

            ShortcutHandler.HandleShortcut(
                () => Script.MovementMode =
                    AnimationPathBuilderHandleMode.MoveAll,
                KeyCode.U);

            ShortcutHandler.HandleShortcut(
                () => Script.MovementMode =
                    AnimationPathBuilderHandleMode.MoveSingle,
                KeyCode.Y);

            // Short jump forward.
            ShortcutHandler.HandleShortcut(
                () => {
                    var newAnimationTimeRatio =
                        Script.AnimationTimeRatio + Script.ShortJumpValue;

                    Script.AnimationTimeRatio =
                        (float)(Math.Round(newAnimationTimeRatio, 3));
                },
                KeyCode.K);

            // Short jump backward.
            ShortcutHandler.HandleShortcut(
                () => {
                    var newAnimationTimeRatio =
                        Script.AnimationTimeRatio - Script.ShortJumpValue;

                    Script.AnimationTimeRatio =
                        (float)(Math.Round(newAnimationTimeRatio, 3));
                },
                KeyCode.K);

            // Long jump forward.
            ShortcutHandler.HandleShortcut(
                () => Script.AnimationTimeRatio += LongJumpValue,
                KeyCode.K,
                KeyCode.J);

            // Long jump backward.
            ShortcutHandler.HandleShortcut(
                () => Script.AnimationTimeRatio -= LongJumpValue,
                KeyCode.H,
                KeyCode.J);

            // Jump to next node.
            ShortcutHandler.HandleShortcut(
                () => Script.AnimationTimeRatio =
                    GetNearestForwardNodeTimestamp(),
                KeyCode.L);

            // Jump to previous node.
            ShortcutHandler.HandleShortcut(
                () => Script.AnimationTimeRatio =
                    GetNearestBackwardNodeTimestamp(),
                KeyCode.G);

            // Jump to start.
            ShortcutHandler.HandleShortcut(
                () => Script.AnimationTimeRatio = 0,
                KeyCode.G,
                KeyCode.J);

            // Jump to end.
            ShortcutHandler.HandleShortcut(
                () => Script.AnimationTimeRatio = 1,
                KeyCode.L,
                KeyCode.J);

        }


        public void HandlePlayPause() {
            // Pause animation.
            if (Script.IsPlaying) {
                Script.Pause = true;
                Script.IsPlaying = false;
            }
            // Unpause animation.
            else if (Script.Pause) {
                Script.Pause = false;
                Script.IsPlaying = true;
            }
            // Start animation.
            else {
                Script.IsPlaying = true;
                // Start animation.
                Script.StartEaseTimeCoroutine();
            }
        }

        private void HandleLinearTangentMode() {
            if (Script.TangentMode == AnimationPathBuilderTangentMode.Linear) {
                Script.PathData.SetNodesLinear();
            }
        }

        private void HandleMoveAllMovementMode(Vector3 moveDelta) {
            if (Script.MovementMode == AnimationPathBuilderHandleMode.MoveAll) {
                Script.PathData.OffsetNodePositions(moveDelta);
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
            // Update path node tangents.
            if (Script.TangentMode == AnimationPathBuilderTangentMode.Smooth) {
                Script.PathData.SmoothAllNodeTangents();
            }
            else if (Script.TangentMode == AnimationPathBuilderTangentMode.Linear) {
                Script.PathData.SetNodesLinear();
            }
        }

        private void HandleWrapModeDropdown() {
            Script.UpdateWrapMode();
        }

        #endregion

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

        private void AnyJumpKeyPressedCallbackHandler() {
            if (Application.isPlaying) Script.UpdateAnimatedGO();
            if (!Application.isPlaying) Script.Animate();
        }

        private void AnyModJumpKeyPressedCallbackHandler() {
            if (Application.isPlaying) Script.UpdateAnimatedGO();
            if (!Application.isPlaying) Script.Animate();
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

            Script.PathData.UpdateNodeTilting(keyIndex, newValue);
        }

        //private void JumpBackwardCallbackHandler() {
        //    // Update animTimeRatio.
        //    var newAnimationTimeRatio =
        //        Script.AnimationTimeRatio - Script.ShortJumpValue;

        //    Script.AnimationTimeRatio =
        //        (float) (Math.Round(newAnimationTimeRatio, 3));
        //}

        //private void JumpForwardCallbackHandler() {
        //    // Update animTimeRatio.
        //    var newAnimationTimeRatio =
        //        Script.AnimationTimeRatio + Script.ShortJumpValue;

        //    Script.AnimationTimeRatio =
        //        (float) (Math.Round(newAnimationTimeRatio, 3));
        //}

        //private void JumpToEndCallbackHandler() {
        //    // Update animTimeRatio.
        //    Script.AnimationTimeRatio = 1;
        //}

        //private void JumpToNextNodeCallbackHandler() {
        //    // Jump to next node.
        //    Script.AnimationTimeRatio = GetNearestForwardNodeTimestamp();
        //}

        //private void JumpToPreviousNodeCallbackHandler() {
        //    // Jump to next node.
        //    Script.AnimationTimeRatio = GetNearestBackwardNodeTimestamp();
        //}

        //private void JumpToStartCallbackHandler() {
        //    // Update animTimeRatio.
        //    Script.AnimationTimeRatio = 0;
        //}

        //private void ModJumpBackwardCallbackHandler() {
        //    // Update animation time.
        //    Script.AnimationTimeRatio -= LongJumpValue;
        //}

        //private void ModJumpForwardCallbackHandler() {
        //    // Update animation time.
        //    Script.AnimationTimeRatio += LongJumpValue;
        //}

        #endregion CALLBACK HANDLERS

        #region METHODS

        private void CheckForSkinAsset() {

            if (Script.Skin == null) {
                Script.MissingReferenceError(
                    "Skin",
                    "Skin field cannot be empty. You will find default " +
                    "GUISkin in the \"animationpathtools/GUISkin\" folder");
            }
        }

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

        private float ConvertEaseToDegrees(int nodeIndex) {
            // Calculate value to display.
            var easeValue = Script.PathData.GetNodeEaseValue(nodeIndex);
            var arcValueMultiplier = 360 / maxAnimationSpeed.floatValue;
            var easeValueInDegrees = easeValue * arcValueMultiplier;

            return easeValueInDegrees;
        }

        private float ConvertTiltToDegrees(int nodeIndex) {
            var rotationValue = Script.PathData.GetNodeTiltValue(nodeIndex);

            return rotationValue;
        }

        private void DrawResetPathInspectorButton() {
            if (GUILayout.Button(
                new GUIContent(
                    "Reset Path",
                    "Reset path to default."))) {
                // Allow undo this operation.
                Undo.RecordObject(Script.PathData, "Change path");

                // Reset curves to its default state.
                Script.PathData.ResetPath();
            }
        }

        private float GetNearestBackwardNodeTimestamp() {
            var pathTimestamps = Script.PathData.GetPathTimestamps();

            for (var i = pathTimestamps.Length - 1; i >= 0; i--) {
                if (pathTimestamps[i] < animTimeRatio.floatValue) {
                    return pathTimestamps[i];
                }
            }

            // Return timestamp of the last node.
            return 0;
        }

        private float GetNearestForwardNodeTimestamp() {
            var pathTimestamps = Script.PathData.GetPathTimestamps();

            foreach (var timestamp in pathTimestamps
                .Where(timestamp => timestamp > animTimeRatio.floatValue)) {
                return timestamp;
            }

            // Return timestamp of the last node.
            return 1.0f;
        }

        #endregion PRIVATE METHODS
    }

}