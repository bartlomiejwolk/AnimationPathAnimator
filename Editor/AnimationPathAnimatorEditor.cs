using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools {

    [CustomEditor(typeof (AnimationPathAnimator))]
    public class AnimatorEditor : Editor {
        #region CONSTANTS

        public const KeyCode EaseModeShortcut = KeyCode.G;

        /// <summary>
        ///     Key shortcut to jump backward.
        /// </summary>
        public const KeyCode JumpBackward = KeyCode.LeftArrow;

        /// <summary>
        ///     Key shortcut to jump forward.
        /// </summary>
        public const KeyCode JumpForward = KeyCode.RightArrow;

        /// <summary>
        ///     Key shortcut to jump to the end of the animation.
        /// </summary>
        public const KeyCode JumpToEnd = KeyCode.UpArrow;

        public const KeyCode JumpToNextNode = KeyCode.UpArrow;
        public const KeyCode JumpToPreviousNode = KeyCode.DownArrow;

        /// <summary>
        ///     Key shortcut to jump to the beginning of the animation.
        /// </summary>
        public const KeyCode JumpToStart = KeyCode.DownArrow;

        public const float JumpValue = 0.01f;

        /// <summary>
        ///     Keycode used as a modifier key.
        /// </summary>
        /// <remarks>Modifier key changes how other keys works.</remarks>
        public const KeyCode ModKey = KeyCode.A;

        public const KeyCode NoneModeShortcut = KeyCode.K;
        public const KeyCode PlayPauseShortcut = KeyCode.Space;
        public const KeyCode RotationModeShortcut = KeyCode.H;
        public const KeyCode TiltingModeShortcut = KeyCode.J;
        public const KeyCode UpdateAllShortcut = KeyCode.L;
        private const float ArcHandleRadius = 0.6f;
        private const int DefaultLabelHeight = 10;
        private const int DefaultLabelWidth = 30;
        private const int EaseValueLabelOffsetX = -20;
        private const int EaseValueLabelOffsetY = -25;
        private const float RotationHandleSize = 0.25f;
        private const int TiltValueLabelOffsetX = -20;
        private const int TiltValueLabelOffsetY = -25;
        private const float MovementHandleSize = 0.12f;
        private readonly Color moveAllModeColor = Color.red;
        private const float MoveAllModeSize = 0.15f;
        private const int AddButtonH = 25;
        private const int AddButtonV = 10;
        private const int RemoveButtonH = 44;
        private const int RemoveButtonV = 10;
        #endregion CONSTANTS

        #region FIELDS

        private SerializedObject gizmoDrawer;

        /// <summary>
        ///     If modifier is currently pressed.
        /// </summary>
        private bool modKeyPressed;

        /// <summary>
        ///     Reference to target script.
        /// </summary>
        // TODO Create property.
        private AnimationPathAnimator script;

        //private GUIStyle TiltValueLabelStyle {
        //    get {
        //        return new GUIStyle {
        //            normal = {textColor = Color.white},
        //            fontStyle = FontStyle.Bold,
        //        };
        //    }
        //}

        //private GUIStyle ForwardPointMarkerStyle {
        //    get {
        //        return new GUIStyle {
        //            normal = {textColor = Color.white},
        //            fontStyle = FontStyle.Bold,
        //        };
        //    }
        //}

        //}
        //private readonly GUIStyle TiltValueLabelStyle = new GUIStyle {
        //    normal = { textColor = Color.white },
        //    fontStyle = FontStyle.Bold,
        //};

        //private readonly GUIStyle ForwardPointMarkerStyle = new GUIStyle {
        //    normal = { textColor = Color.white },
        //    fontStyle = FontStyle.Bold,
        //};

        #endregion FIELDS

        #region SERIALIZED PROPERTIES

        private const float FloatPrecision = 0.001f;
        private const float ScaleHandleSize = 1.5f;
        private SerializedProperty advancedSettingsFoldout;

        private SerializedProperty animatedGO;

        private SerializedProperty animTimeRatio;

        private SerializedProperty enableControlsInPlayMode;
        private SerializedProperty forwardPointOffset;
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
            serializedObject.Update();
            EditorGUILayout.PropertyField(
                pathData,
                new GUIContent(
                    "Path Asset",
                    ""));
            serializedObject.ApplyModifiedProperties();

            animTimeRatio.floatValue = EditorGUILayout.FloatField(
                new GUIContent(
                    "Animation Time",
                    "Current animation time."),
                animTimeRatio.floatValue);

            script.WrapMode = (WrapMode) EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Wrap Mode",
                    ""),
                script.WrapMode);

            script.HandleMode = (AnimatorHandleMode) EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Handle Mode",
                    ""),
                script.HandleMode);

            script.UpdateAllMode = EditorGUILayout.Toggle(
                new GUIContent(
                    "Update All",
                    ""),
                script.UpdateAllMode);

            serializedObject.Update();
            EditorGUILayout.PropertyField(
                positionLerpSpeed,
                new GUIContent(
                    "Position Lerp Speed",
                    ""));
            serializedObject.ApplyModifiedProperties();

            DrawRotationModeDropdown();

            serializedObject.Update();

            if (script.RotationMode == AnimatorRotationMode.Forward) {
                EditorGUILayout.PropertyField(forwardPointOffset);
            }

            EditorGUILayout.PropertyField(
                rotationSpeed,
                new GUIContent(
                    "Rotation Speed",
                    "Controls how much time (in seconds) it'll take the " +
                    "animated object to finish rotation towards followed target."));

            EditorGUILayout.Space();

            script.MovementMode =
                (AnimationPathBuilderHandleMode)EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Movement Mode",
                    ""),
                script.MovementMode);

            EditorGUILayout.Space();

            DrawResetPathInspectorButton();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(
                animatedGO,
                new GUIContent(
                    "Animated Object",
                    "Object to animate."));

            EditorGUILayout.PropertyField(
                targetGO,
                new GUIContent(
                    "Target Object",
                    "Object that the animated object will be looking at."));
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            // Play/Pause button text.
            string playPauseBtnText;
            if (!script.IsPlaying || (script.IsPlaying && script.Pause)) {
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
                script.StopEaseTimeCoroutine();
            }

            EditorGUILayout.EndHorizontal();

            script.AutoPlay = EditorGUILayout.Toggle(
                new GUIContent(
                    "Auto Play",
                    ""),
                script.AutoPlay);

            serializedObject.Update();
            EditorGUILayout.PropertyField(
                enableControlsInPlayMode,
                new GUIContent(
                    "Play Mode Controls",
                    "Enable keybord controls in play mode."));
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            serializedObject.Update();

            advancedSettingsFoldout.boolValue = EditorGUILayout.Foldout(
                advancedSettingsFoldout.boolValue,
                new GUIContent(
                    "Advanced Settings",
                    ""));

            serializedObject.ApplyModifiedProperties();

            // Display advanced foldout content.
            if (advancedSettingsFoldout.boolValue) {
                gizmoDrawer.Update();
                EditorGUILayout.PropertyField(rotationCurveColor);
                gizmoDrawer.ApplyModifiedProperties();

                serializedObject.Update();
                EditorGUILayout.PropertyField(maxAnimationSpeed);
                EditorGUILayout.PropertyField(skin);
                serializedObject.ApplyModifiedProperties();
            }

            //if (GUI.changed) EditorUtility.SetDirty(target);
        }

        private void DrawRotationModeDropdown() {
            // Remember current RotationMode.
            var prevRotationMode = script.RotationMode;
            // Draw RotationMode dropdown.
            script.RotationMode =
                (AnimatorRotationMode) EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Rotation Mode",
                        ""),
                    script.RotationMode);
            // If value changed, update animated GO in the scene.
            if (script.RotationMode != prevRotationMode) {
                script.UpdateAnimatedGO();
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnEnable() {
            // Get target script reference.
            script = (AnimationPathAnimator) target;

            gizmoDrawer = new SerializedObject(script.GizmoDrawer);

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
            rotationCurveColor = gizmoDrawer.FindProperty("rotationCurveColor");
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnSceneGUI() {
            // Handle undo event.
            if (Event.current.type == EventType.ValidateCommand
                && Event.current.commandName == "UndoRedoPerformed") {
            }

            // Return if path asset does not exist.
            if (script.PathData == null) return;

            // Update modifier key state.
            UpdateModifierKey();
            HandleEaseModeOptionShortcut();
            HandleRotationModeOptionShortcut();
            HandleTiltingModeOptionShortcut();
            HandleNoneModeOptionShortcut();
            HandleUpdateAllOptionShortcut();
            HandlePlayPauseShortcut();

            // Change current animation time with arrow keys.
            ChangeTimeWithArrowKeys();

            HandleWrapModeDropdown();

            //HandleDrawingForwardPointMarker();
            //HandleDrawingTargetGizmo();
            HandleDrawingEaseHandles();
            HandleDrawingRotationHandle();
            HandleDrawingTiltingHandles();
            HandleDrawingEaseLabel();
            HandleDrawingTiltLabel();

            HandleDrawingPositionHandles();
            HandleDrawingAddButtons();
            HandleDrawingRemoveButtons();

            //script.UpdateAnimatedGO();
        }

        #endregion UNITY MESSAGES

        #region DRAWING HANDLERS

        private void HandleDrawingRemoveButtons() {
            // Positions at which to draw movement handles.
            var nodes = script.GetNodeGlobalPositions();

            // Get style for add button.
            var removeButtonStyle = script.Skin.GetStyle(
                        "RemoveButton");

            // Callback to add a new node after add button was pressed.
            Action<int> removeNodeCallback =
                DrawRemoveNodeButtonsCallbackHandles;

            // Draw add node buttons.
            DrawRemoveNodeButtons(
                nodes,
                removeNodeCallback,
                removeButtonStyle);
        }

        private void HandleDrawingEaseHandles() {
            if (script.HandleMode != AnimatorHandleMode.Ease) return;

            DrawEaseHandles(DrawEaseHandlesCallbackHandler);
        }

        private void HandleDrawingEaseLabel() {
            if (script.HandleMode != AnimatorHandleMode.Ease) return;

            DrawNodeLabels(
                ConvertEaseToDegrees,
                EaseValueLabelOffsetX,
                EaseValueLabelOffsetY,
                script.Skin.GetStyle("EaseValueLabel"));
        }

        private void HandleDrawingRotationHandle() {
            if (script.HandleMode != AnimatorHandleMode.Rotation) return;

            // Draw handles.
            DrawRotationHandle(DrawRotationHandlesCallbackHandler);
        }

        private void HandleDrawingTiltingHandles() {
            if (script.HandleMode != AnimatorHandleMode.Tilting) return;

            Action<int, float> callbackHandler =
                DrawTiltingHandlesCallbackHandler;

            DrawTiltingHandles(callbackHandler);
        }

        private void HandleDrawingTiltLabel() {
            if (script.HandleMode != AnimatorHandleMode.Tilting) return;

            DrawNodeLabels(
                ConvertTiltToDegrees,
                TiltValueLabelOffsetX,
                TiltValueLabelOffsetY,
                script.Skin.GetStyle("TiltValueLabel"));
        }
        /// <summary>
        /// Handle drawing movement handles.
        /// </summary>
        private void HandleDrawingPositionHandles() {
            // Callback to call when a node is moved on the scene.
            Action<int, Vector3, Vector3> handlerCallback =
                DrawPositionHandlesCallbackHandler;

            // Draw handles.
            DrawPositionHandles(handlerCallback);
        }

        private void HandleDrawingAddButtons() {
            // Get positions at which to draw movement handles.
            var nodePositions = script.GetNodeGlobalPositions();

            // Get style for add button.
            var addButtonStyle = script.Skin.GetStyle(
                        "AddButton");

            // Callback executed after add button was pressed.
            Action<int> callbackHandler = DrawAddNodeButtonsCallbackHandler;

            // Draw add node buttons.
            DrawAddNodeButtons(
                nodePositions,
                callbackHandler,
                addButtonStyle);
        }

        //private void HandleDrawingForwardPointMarker() {
        //    if (script.RotationMode != AnimatorRotationMode.Forward) return;

        //    var targetPos = script.GetForwardPoint();
        //    var globalTargetPos = script.transform.TransformPoint(targetPos);

        //    Handles.Label(globalTargetPos, "Point", ForwardPointMarkerStyle);
        //}
        //private void HandleDrawingTargetGizmo() {
        //    if (targetGO.objectReferenceValue == null) return;

        //    var targetPos =
        //        ((Transform)targetGO.objectReferenceValue).position;

        //    Handles.Label(targetPos, "Target", TargetGizmoStyle);

        #endregion DRAWING HANDLERS

        #region OTHER HANDLERS
        private void HandleWrapModeDropdown() {
            script.UpdateWrapMode();
        }

        private void HandleMoveAllMovementMode(Vector3 moveDelta) {
            if (script.MovementMode == AnimationPathBuilderHandleMode.MoveAll) {
                script.PathData.OffsetNodePositions(moveDelta);
            }
        }

        private void HandleMoveSingleHandleMove(int movedNodeIndex, Vector3 position) {
            if (script.MovementMode == AnimationPathBuilderHandleMode.MoveSingle) {

                script.PathData.MoveNodeToPosition(movedNodeIndex, position);
                script.PathData.DistributeTimestamps();

                HandleSmoothTangentMode();
                HandleLinearTangentMode();
            }
        }

        private void HandleSmoothTangentMode() {
            if (script.TangentMode == AnimationPathBuilderTangentMode.Smooth) {
                script.PathData.SmoothAllNodeTangents();
            }
        }

        private void HandleLinearTangentMode() {
            if (script.TangentMode == AnimationPathBuilderTangentMode.Linear) {
                script.PathData.SetNodesLinear();
            }
        }
        #endregion

        #region SHORTCUT HANDLERS
        private void HandleUpdateAllOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != UpdateAllShortcut) return;

            script.UpdateAllMode = !script.UpdateAllMode;
        }

        private void HandleNoneModeOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != NoneModeShortcut) return;

            script.HandleMode = AnimatorHandleMode.None;
        }

        #endregion

        #region DRAWING METHODS

        private void DrawRemoveNodeButtons(
            Vector3[] nodePositions,
            Action<int> callback,
            GUIStyle buttonStyle) {

            Handles.BeginGUI();

            // Draw remove buttons for each node except for the first and the
            // last one. Execute callback on button press.
            for (var i = 1; i < nodePositions.Length - 1; i++) {
                // Translate node's 3d position into screen coordinates.
                var guiPoint = HandleUtility.WorldToGUIPoint(
                        nodePositions[i]);

                // Draw button.
                var buttonPressed = DrawButton(
                    guiPoint,
                    RemoveButtonH,
                    RemoveButtonV,
                    15,
                    15,
                    buttonStyle);

                // Execute callback.
                if (buttonPressed) {
                    callback(i);
                }
            }

            Handles.EndGUI();
        }

        private bool DrawButton(
            Vector2 position,
            int relativeXPos,
            int relativeYPos,
            int width,
            int height,
            GUIStyle style,
            string buttonText = "") {

            // Create rectangle for the "+" button.
            var rectAdd = new Rect(
                    position.x + relativeXPos,
                    position.y + relativeYPos,
                    width,
                    height);

            // Draw the "+" button.
            var addButtonPressed = GUI.Button(rectAdd, buttonText, style);

            return addButtonPressed;
        }

        private void DrawAddNodeButtons(
            Vector3[] nodePositions,
            Action<int> callback,
            GUIStyle buttonStyle) {

            Handles.BeginGUI();

            // Draw add buttons for each node (except the last one). Execute
            // callback on button press.
            for (var i = 0; i < nodePositions.Length - 1; i++) {
                // Translate node's 3d position into screen coordinates.
                var guiPoint = HandleUtility.WorldToGUIPoint(
                        nodePositions[i]);

                // Draw button.
                var buttonPressed = DrawButton(
                    guiPoint,
                    AddButtonH,
                    AddButtonV,
                    15,
                    15,
                    buttonStyle);

                // Execute callback.
                if (buttonPressed) {
                    callback(i);
                }
            }

            Handles.EndGUI();
        }

        private void DrawPositionHandles(
            Action<int, Vector3, Vector3> callback) {

            // Node global positions.
            var nodes = script.GetNodeGlobalPositions();

            // Cap function used to draw handle.
            Handles.DrawCapFunction capFunction = Handles.CircleCap;

            // For each node..
            for (var i = 0; i < nodes.Length; i++) {
                // Draw position handle.
                var newPos = DrawPositionHandle(nodes[i], capFunction);

                // If node was moved..
                if (newPos != nodes[i]) {
                    // Calculate node old local position.
                    var oldNodeLocalPosition =
                        script.transform.InverseTransformPoint(nodes[i]);

                    // Calculate node new local position.
                    var newNodeLocalPosition =
                        script.transform.InverseTransformPoint(newPos);

                    // Calculate movement delta.
                    var moveDelta = newNodeLocalPosition - oldNodeLocalPosition;

                    // Execute callback.
                    callback(i, newNodeLocalPosition, moveDelta);
                }
            }
        }

        /// <summary>
        ///     Draw arc handle.
        /// </summary>
        /// <param name="value">Arc value.</param>
        /// <param name="position">Arc position.</param>
        /// <param name="arcValueMultiplier">If set to 1, values will be converted to degrees in relation 1 to 1.</param>
        /// <param name="minDegrees">Lower boundary for amount of degrees that will be drawn.</param>
        /// <param name="maxDegrees">Higher boundary for amount of degrees that will be drawn.</param>
        /// <param name="handleColor">Handle color.</param>
        /// <param name="callback">Callback that will be executed when arc value changes. It takes changed value as an argument.</param>
        private void DrawArcHandle(
            float value,
            Vector3 position,
            float arcValueMultiplier,
            int minDegrees,
            int maxDegrees,
            Color handleColor,
            Action<float> callback) {
            var arcValue = value*arcValueMultiplier;
            var handleSize = HandleUtility.GetHandleSize(position);
            var arcRadius = handleSize*ArcHandleRadius;

            Handles.color = handleColor;

            Handles.DrawWireArc(
                position,
                Vector3.up,
                Quaternion.AngleAxis(
                    0,
                    Vector3.up)*Vector3.forward,
                arcValue,
                arcRadius);

            Handles.color = handleColor;

            // Set initial arc value to other than zero. If initial value
            // is zero, handle will always return zero.
            arcValue = Math.Abs(arcValue) < FloatPrecision ? 10f : arcValue;

            var scaleHandleSize = handleSize*ScaleHandleSize;
            var newArcValue = Handles.ScaleValueHandle(
                arcValue,
                position + Vector3.forward*arcRadius
                *1.3f,
                Quaternion.identity,
                scaleHandleSize,
                Handles.ConeCap,
                1);

            // Limit handle value.
            if (newArcValue > maxDegrees) newArcValue = maxDegrees;
            if (newArcValue < minDegrees) newArcValue = minDegrees;

            if (Math.Abs(newArcValue - arcValue) > FloatPrecision) {
                callback(newArcValue/arcValueMultiplier);
            }
        }

        private void DrawEaseHandles(Action<int, float> callback) {
            // Get path node positions.
            var nodePositions =
                script.GetNodeGlobalPositions();

            // Get ease values.
            //var easeCurveValues = new float[script.PathData.EaseCurveKeysNo];
            var easeCurveValues = script.PathData.GetEaseCurveValues();
            //for (var i = 0; i < script.PathData.EaseCurveKeysNo; i++) {
            //    easeCurveValues[i] = script.PathData.EaseCurve.keys[i].value;
            //}

            var arcValueMultiplier = 360/maxAnimationSpeed.floatValue;

            // For each path node..
            for (var i = 0; i < nodePositions.Length; i++) {
                DrawArcHandle(
                    easeCurveValues[i],
                    nodePositions[i],
                    arcValueMultiplier,
                    0,
                    360,
                    Color.red,
                    value => callback(i, value));
            }
        }

        private void DrawNodeLabel(
            int nodeIndex,
            string value,
            int offsetX,
            int offsetY,
            GUIStyle style) {
            // Get node position.
            //var nodePosition = script.GetNodePosition(nodeIndex);
            var nodePosition = script.GetGlobalNodePosition(nodeIndex);

            // Translate node's 3d position into screen coordinates.
            var guiPoint = HandleUtility.WorldToGUIPoint(nodePosition);

            // Create rectangle for the label.
            var labelPosition = new Rect(
                guiPoint.x + offsetX,
                guiPoint.y + offsetY,
                DefaultLabelWidth,
                DefaultLabelHeight);

            Handles.BeginGUI();

            // Draw label.
            GUI.Label(
                labelPosition,
                value,
                style);

            Handles.EndGUI();
        }

        private void DrawNodeLabels(
            Func<int, float> calculateValueCallback,
            int offsetX,
            int offsetY,
            GUIStyle style) {
            var nodesNo = script.AnimationPathBuilder.NodesNo;

            // For each path node..
            for (var i = 0; i < nodesNo; i++) {
                // Get value to display.
                var arcValue = String.Format(
                    "{0:0}",
                    calculateValueCallback(i));

                DrawNodeLabel(i, arcValue, offsetX, offsetY, style);
            }
        }

        private void DrawRotationHandle(Action<float, Vector3> callback) {
            var currentAnimationTime = script.AnimationTimeRatio;
            var rotationPointPosition =
                script.PathData.GetRotationAtTime(currentAnimationTime);
            var nodeTimestamps = script.PathData.GetPathTimestamps();

            // Return if current animation time is not equal to any node
            // timestamp.
            var index = Array.FindIndex(
                nodeTimestamps,
                x => Math.Abs(x - currentAnimationTime) < FloatPrecision);
            if (index < 0) return;

            Handles.color = Color.magenta;
            var handleSize = HandleUtility.GetHandleSize(rotationPointPosition);
            var sphereSize = handleSize*RotationHandleSize;

            var rotationPointGlobalPos =
                script.transform.TransformPoint(rotationPointPosition);

            // draw node's handle.
            var newGlobalPosition = Handles.FreeMoveHandle(
                //rotationPointPosition,
                rotationPointGlobalPos,
                Quaternion.identity,
                sphereSize,
                Vector3.zero,
                Handles.SphereCap);

            if (newGlobalPosition != rotationPointGlobalPos) {
                var newPointLocalPosition =
                    script.transform.InverseTransformPoint(newGlobalPosition);
                // Execute callback.
                callback(currentAnimationTime, newPointLocalPosition);
            }
        }

        private void DrawTiltingHandles(Action<int, float> callback) {
            // Get path node positions.
            //var nodePositions = script.AnimationPathBuilder.GetNodePositions();
            var nodePositions =
                script.GetNodeGlobalPositions();

            // Get tilting curve values.
            //var tiltingCurveValues = new float[script.PathData.EaseCurveKeysNo];
            var tiltingCurveValues = script.PathData.GetTiltingCurveValues();
            //for (var i = 0; i < script.PathData.TiltingCurveKeysNo; i++) {
            //    tiltingCurveValues[i] =
            //        script.PathData.TiltingCurve.keys[i].value;
            //}

            // Set arc value multiplier.
            const int arcValueMultiplier = 1;

            // For each path node..
            for (var i = 0; i < nodePositions.Length; i++) {
                DrawArcHandle(
                    tiltingCurveValues[i],
                    nodePositions[i],
                    arcValueMultiplier,
                    -90,
                    90,
                    Color.green,
                    value => callback(i, value));
            }
        }

        private Vector3 DrawPositionHandle(
            Vector3 nodePosition,
            Handles.DrawCapFunction capFunction) {

            // Set handle color.
            Handles.color = script.GizmoCurveColor;

            // Get handle size.
            var handleSize = HandleUtility.GetHandleSize(nodePosition);
            var sphereSize = handleSize * MovementHandleSize;

            // In Move All mode..
            if (script.MovementMode == AnimationPathBuilderHandleMode.MoveAll) {
                Handles.color = moveAllModeColor;
                sphereSize = handleSize * MoveAllModeSize;
            }

            // Draw handle.
            var newPos = Handles.FreeMoveHandle(
                nodePosition,
                Quaternion.identity,
                sphereSize,
                Vector3.zero,
                capFunction);
            return newPos;
        }

        #endregion DRAWING METHODS

        #region CALLBACK HANDLERS

        protected virtual void DrawRemoveNodeButtonsCallbackHandles(int nodeIndex) {
            Undo.RecordObject(script.PathData, "Change path");

            script.PathData.RemoveNode(nodeIndex);
            script.PathData.DistributeTimestamps();

            if (script.TangentMode == AnimationPathBuilderTangentMode.Smooth) {
                script.PathData.SmoothAllNodeTangents();
            }
            else if (script.TangentMode == AnimationPathBuilderTangentMode.Linear) {
                script.PathData.SetNodesLinear();
            }
        }

        private void DrawEaseHandlesCallbackHandler(
            int keyIndex,
            float newValue) {
            Undo.RecordObject(script.PathData, "Ease curve changed.");

            if (script.UpdateAllMode) {
                //var keyTime = script.PathData.EaseCurve.keys[keyIndex].time;
                //var keyTime = script.PathData.GetEaseValueAtIndex(keyIndex);
                //var oldValue = script.PathData.EaseCurve.Evaluate(keyTime);
                var oldValue = script.PathData.GetEaseValueAtIndex(keyIndex);
                var delta = newValue - oldValue;
                script.PathData.UpdateEaseValues(delta);
            }
            else {
                script.PathData.UpdateEaseValue(keyIndex, newValue);
            }
        }

        private void DrawRotationHandlesCallbackHandler(
            float timestamp,
            Vector3 newPosition) {
            Undo.RecordObject(script.PathData, "Rotation path changed.");

            script.PathData.ChangeRotationAtTimestamp(timestamp, newPosition);
        }

        private void DrawTiltingHandlesCallbackHandler(
            int keyIndex,
            float newValue) {
            Undo.RecordObject(script.PathData, "Tilting curve changed.");

            script.PathData.UpdateNodeTilting(keyIndex, newValue);
        }

        protected virtual void DrawPositionHandlesCallbackHandler(
            int movedNodeIndex,
            Vector3 position,
            Vector3 moveDelta) {

            Undo.RecordObject(script.PathData, "Change path");

            HandleMoveAllMovementMode(moveDelta);
            HandleMoveSingleHandleMove(movedNodeIndex, position);
        }

        private void JumpBackwardCallbackHandler() {
            // Update animTimeRatio.
            var newAnimationTimeRatio = animTimeRatio.floatValue
                                        - script.ShortJumpValue;
            serializedObject.Update();
            animTimeRatio.floatValue =
                (float) (Math.Round(newAnimationTimeRatio, 3));
            serializedObject.ApplyModifiedProperties();
        }

        private void JumpForwardCallbackHandler() {
            // Update animTimeRatio.
            var newAnimationTimeRatio = animTimeRatio.floatValue
                                        + script.ShortJumpValue;
            serializedObject.Update();
            animTimeRatio.floatValue =
                (float) (Math.Round(newAnimationTimeRatio, 3));
            serializedObject.ApplyModifiedProperties();
        }

        private void JumpToEndCallbackHandler() {
            // Update animTimeRatio.
            serializedObject.Update();
            animTimeRatio.floatValue = 1;
            serializedObject.ApplyModifiedProperties();
        }

        private void JumpToStartCallbackHandler() {
            // Update animTimeRatio.
            serializedObject.Update();
            animTimeRatio.floatValue = 0;
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawAddNodeButtonsCallbackHandler(int nodeIndex) {
            // Make snapshot of the target object.
            Undo.RecordObject(script.PathData, "Change path");

            // Add a new node.
            AddNodeBetween(nodeIndex);

            script.PathData.DistributeTimestamps();

            if (script.TangentMode == AnimationPathBuilderTangentMode.Smooth) {
                script.PathData.SmoothAllNodeTangents();
            }
            else if (script.TangentMode == AnimationPathBuilderTangentMode.Linear) {
                script.PathData.SetNodesLinear();
            }
        }

        #endregion CALLBACK HANDLERS

        #region METHODS

        private void AnyJumpKeyPressedCallbackHandler() {
            if (Application.isPlaying) script.UpdateAnimatedGO();
            if (!Application.isPlaying) script.Animate();
        }

        private void AnyModJumpKeyPressedCallbackHandler() {
            if (Application.isPlaying) script.UpdateAnimatedGO();
            if (!Application.isPlaying) script.Animate();
        }

        /// <summary>
        ///     Change current animation time with arrow keys.
        /// </summary>
        private void ChangeTimeWithArrowKeys() {
            // If a key is pressed..
            if (Event.current.type == EventType.keyDown
                // and modifier key is pressed also..
                && modKeyPressed) {
                HandleModifiedShortcuts(
                    ModJumpForwardCallbackHandler,
                    ModJumpBackwardCallbackHandler,
                    JumpToNextNodeCallbackHandler,
                    JumpToPreviousNodeCallbackHandler,
                    AnyModJumpKeyPressedCallbackHandler);
            }
            // Modifier key not pressed.
            else if (Event.current.type == EventType.keyDown) {
                HandleUnmodifiedShortcuts(
                    JumpBackwardCallbackHandler,
                    JumpForwardCallbackHandler,
                    JumpToStartCallbackHandler,
                    JumpToEndCallbackHandler,
                    AnyJumpKeyPressedCallbackHandler);
            }
        }

        private float ConvertEaseToDegrees(int nodeIndex) {
            // Calculate value to display.
            var easeValue = script.PathData.GetNodeEaseValue(nodeIndex);
            var arcValueMultiplier = 360/maxAnimationSpeed.floatValue;
            var easeValueInDegrees = easeValue*arcValueMultiplier;

            return easeValueInDegrees;
        }

        private float ConvertTiltToDegrees(int nodeIndex) {
            var rotationValue = script.PathData.GetNodeTiltValue(nodeIndex);
            //var arcValue = rotationValue * 2;

            return rotationValue;
        }

        private float GetNearestBackwardNodeTimestamp() {
            var pathTimestamps = script.PathData.GetPathTimestamps();

            for (var i = pathTimestamps.Length - 1; i >= 0; i--) {
                if (pathTimestamps[i] < animTimeRatio.floatValue) {
                    return pathTimestamps[i];
                }
            }

            // Return timestamp of the last node.
            return 0;
        }

        private float GetNearestForwardNodeTimestamp() {
            var pathTimestamps = script.PathData.GetPathTimestamps();

            foreach (var timestamp in pathTimestamps
                .Where(timestamp => timestamp > animTimeRatio.floatValue)) {
                return timestamp;
            }

            // Return timestamp of the last node.
            return 1.0f;
        }

        private void HandlePlayPause() {
            // Pause animation.
            if (script.IsPlaying) {
                script.Pause = true;
                script.IsPlaying = false;
            }
            // Unpause animation.
            else if (script.Pause) {
                script.Pause = false;
                script.IsPlaying = true;
            }
            // Start animation.
            else {
                script.IsPlaying = true;
                // Start animation.
                script.StartEaseTimeCoroutine();
            }
        }

        private void JumpToNextNodeCallbackHandler() {
            // Jump to next node.
            animTimeRatio.floatValue = GetNearestForwardNodeTimestamp();
            serializedObject.ApplyModifiedProperties();
        }

        private void JumpToPreviousNodeCallbackHandler() {
            // Jump to next node.
            animTimeRatio.floatValue = GetNearestBackwardNodeTimestamp();
            serializedObject.ApplyModifiedProperties();
        }

        private void ModJumpBackwardCallbackHandler() {
            // Update animation time.
            animTimeRatio.floatValue -= JumpValue;
            serializedObject.ApplyModifiedProperties();
        }

        private void ModJumpForwardCallbackHandler() {
            // Update animation time.
            animTimeRatio.floatValue += JumpValue;
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawResetPathInspectorButton() {
            if (GUILayout.Button(new GUIContent(
                "Reset Path",
                "Reset path to default."))) {
                // Allow undo this operation.
                Undo.RecordObject(script.PathData, "Change path");

                // Reset curves to its default state.
                script.PathData.ResetPath();
            }
        }

        protected void AddNodeBetween(int nodeIndex) {
            // Timestamp of node on which was taken action.
            var currentKeyTime = script.PathData.GetNodeTimestamp(nodeIndex);
            // Get timestamp of the next node.
            var nextKeyTime = script.PathData.GetNodeTimestamp(nodeIndex + 1);

            // Calculate timestamps for new key. It'll be placed exactly
            // between the two nodes.
            var newKeyTime =
                currentKeyTime +
                ((nextKeyTime - currentKeyTime) / 2);

            // Add node to the animation curves.
            script.PathData.CreateNodeAtTime(newKeyTime);
        }

        #endregion PRIVATE METHODS

        #region SHORTCUT HANDLERS

        private void HandleEaseModeOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != EaseModeShortcut) return;

            script.HandleMode = AnimatorHandleMode.Ease;
        }

        private void HandleModifiedShortcuts(
            Action jumpForwardCallback = null,
            Action jumpBackwardCallback = null,
            Action jumpToNextNodeCallback = null,
            Action jumpToPreviousNodeCallback = null,
            Action anyModJumpKeyPressedCallback = null) {
            serializedObject.Update();

            // Check what key is pressed..
            switch (Event.current.keyCode) {
                // Jump backward.
                case JumpBackward:
                    Event.current.Use();

                    //// Update animation time.
                    //animTimeRatio.floatValue -= JumpValue;
                    //serializedObject.ApplyModifiedProperties();

                    if (jumpBackwardCallback != null) jumpBackwardCallback();
                    if (anyModJumpKeyPressedCallback != null) {
                        anyModJumpKeyPressedCallback();
                    }

                    break;
                // Jump forward.
                case JumpForward:
                    Event.current.Use();

                    //// Update animation time.
                    //animTimeRatio.floatValue += JumpValue;
                    //serializedObject.ApplyModifiedProperties();

                    if (jumpForwardCallback != null) jumpForwardCallback();
                    if (anyModJumpKeyPressedCallback != null) {
                        anyModJumpKeyPressedCallback();
                    }

                    break;

                case JumpToNextNode:
                    Event.current.Use();

                    //// Jump to next node.
                    //animTimeRatio.floatValue = GetNearestForwardNodeTimestamp();
                    //serializedObject.ApplyModifiedProperties();
                    //if (Application.isPlaying) script.UpdateAnimatedGO();

                    if (jumpToNextNodeCallback != null)
                        jumpToNextNodeCallback();
                    if (anyModJumpKeyPressedCallback != null) {
                        anyModJumpKeyPressedCallback();
                    }

                    break;

                case JumpToPreviousNode:
                    Event.current.Use();

                    //// Jump to next node.
                    //animTimeRatio.floatValue = GetNearestBackwardNodeTimestamp();
                    //serializedObject.ApplyModifiedProperties();
                    //if (Application.isPlaying) script.UpdateAnimatedGO();

                    if (jumpToPreviousNodeCallback != null) {
                        jumpToPreviousNodeCallback();
                    }
                    if (anyModJumpKeyPressedCallback != null) {
                        anyModJumpKeyPressedCallback();
                    }

                    break;
            }
        }

        private void HandlePlayPauseShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != PlayPauseShortcut) return;

            HandlePlayPause();
        }

        private void HandleRotationModeOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != RotationModeShortcut) return;

            script.HandleMode = AnimatorHandleMode.Rotation;
        }

        private void HandleTiltingModeOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != TiltingModeShortcut) return;

            script.HandleMode = AnimatorHandleMode.Tilting;
        }

        private void HandleUnmodifiedShortcuts(
            Action jumpBackwardCallback = null,
            Action jumpForwardCallback = null,
            Action jumpToStartCallback = null,
            Action jumpToEndCallback = null,
            Action anyJumpKeyPressedCallback = null) {
            //serializedObject.Update();

            // Helper variable.
            //float newAnimationTimeRatio;

            switch (Event.current.keyCode) {
                // Jump backward.
                case JumpBackward:
                    Event.current.Use();

                    //// Calculate new time ratio.
                    //newAnimationTimeRatio = animTimeRatio.floatValue
                    //                        - AnimationPathAnimator.ShortJumpValue;
                    //// Apply rounded value.
                    //animTimeRatio.floatValue =
                    //    (float)(Math.Round(newAnimationTimeRatio, 3));

                    //serializedObject.ApplyModifiedProperties();

                    if (jumpBackwardCallback != null) jumpBackwardCallback();
                    if (anyJumpKeyPressedCallback != null) {
                        anyJumpKeyPressedCallback();
                    }

                    break;
                // Jump forward.
                case JumpForward:
                    Event.current.Use();

                    //newAnimationTimeRatio = animTimeRatio.floatValue
                    //                        + AnimationPathAnimator.ShortJumpValue;
                    //animTimeRatio.floatValue =
                    //    (float)(Math.Round(newAnimationTimeRatio, 3));

                    //serializedObject.ApplyModifiedProperties();

                    if (jumpForwardCallback != null) jumpForwardCallback();
                    if (anyJumpKeyPressedCallback != null) {
                        anyJumpKeyPressedCallback();
                    }

                    break;
                // Jump to start.
                case JumpToStart:
                    Event.current.Use();

                    //animTimeRatio.floatValue = 0;
                    //serializedObject.ApplyModifiedProperties();

                    if (jumpToStartCallback != null) jumpToStartCallback();
                    if (anyJumpKeyPressedCallback != null) {
                        anyJumpKeyPressedCallback();
                    }

                    // Update camera position, rotation and tilting.
                    //if (Application.isPlaying) script.UpdateAnimatedGO();

                    break;
                // Jump to end.
                case JumpToEnd:
                    Event.current.Use();

                    //animTimeRatio.floatValue = 1;
                    //serializedObject.ApplyModifiedProperties();

                    if (jumpToEndCallback != null) jumpToEndCallback();
                    if (anyJumpKeyPressedCallback != null) {
                        anyJumpKeyPressedCallback();
                    }

                    //if (Application.isPlaying) script.UpdateAnimatedGO();

                    break;
            }
        }

        /// <summary>
        ///     Checked if modifier key is pressed and remember it in a class
        ///     field.
        /// </summary>
        private void UpdateModifierKey() {
            // Check if modifier key is currently pressed.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode == ModKey) {
                // Remember key state.
                modKeyPressed = true;
            }
            // If modifier key was released..
            if (Event.current.type == EventType.keyUp
                && Event.current.keyCode == ModKey) {
                modKeyPressed = false;
            }
        }

        #endregion
    }

}