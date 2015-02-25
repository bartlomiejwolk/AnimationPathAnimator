using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools {

    [CustomEditor(typeof(AnimationPathAnimator))]
    public class AnimatorEditor : Editor {

        #region CONSTANTS

        private const float ArcHandleRadius = 0.6f;
        private const int DefaultLabelHeight = 10;
        private const int DefaultLabelWidth = 30;
        private const int EaseValueLabelOffsetX = -20;
        private const int EaseValueLabelOffsetY = -25;
        private const float RotationHandleSize = 0.25f;
        private const int TiltValueLabelOffsetX = -20;
        private const int TiltValueLabelOffsetY = -25;
        public const KeyCode EaseModeShortcut = KeyCode.G;
        public const KeyCode RotationModeShortcut = KeyCode.H;
        public const KeyCode TiltingModeShortcut = KeyCode.J;
		public const KeyCode NoneModeShortcut = KeyCode.K;
		public const KeyCode UpdateAllShortcut = KeyCode.L;
		public const KeyCode PlayPauseShortcut = KeyCode.Space;

        /// <summary>
        /// Key shortcut to jump backward.
        /// </summary>
        public const KeyCode JumpBackward = KeyCode.LeftArrow;

        /// <summary>
        /// Key shortcut to jump forward.
        /// </summary>
        public const KeyCode JumpForward = KeyCode.RightArrow;

        /// <summary>
        /// Key shortcut to jump to the end of the animation.
        /// </summary>
        public const KeyCode JumpToEnd = KeyCode.UpArrow;

        /// <summary>
        /// Key shortcut to jump to the beginning of the animation.
        /// </summary>
        public const KeyCode JumpToStart = KeyCode.DownArrow;
        
        public const KeyCode JumpToNextNode = KeyCode.UpArrow;
        public const KeyCode JumpToPreviousNode = KeyCode.DownArrow;

        public const float JumpValue = 0.01f;
        /// <summary>
        /// Keycode used as a modifier key.
        /// </summary>
        /// <remarks>Modifier key changes how other keys works.</remarks>
        public const KeyCode ModKey = KeyCode.A;
        #endregion CONSTANTS

        #region FIELDS

        // TODO Use gizmo icon instead.
        //private GUIStyle TargetGizmoStyle {
        //    get {
        //        return new GUIStyle {
        //            normal = {textColor = Color.white},
        //            fontStyle = FontStyle.Bold,
        //        };
        //    }
        //}

        //private GUIStyle EaseValueLabelStyle {
        //    get {
        //        return new GUIStyle {
        //            normal = { textColor = Color.white },
        //            fontStyle = FontStyle.Bold,
        //        };
        //    }
        //}

        /// <summary>
        /// If modifier is currently pressed.
        /// </summary>
        private bool modKeyPressed;

        /// <summary>
        /// Reference to target script.
        /// </summary>
        private AnimationPathAnimator script;

        //private GUIStyle TiltValueLabelStyle {
        //    get {
        //        return new GUIStyle {
        //            normal = {textColor = Color.white},
        //            fontStyle = FontStyle.Bold,
        //        };
        //    }
        //}

        // TODO Use gizmo icon instead.
        private GUIStyle ForwardPointMarkerStyle {
            get {
                return new GUIStyle {
                    normal = {textColor = Color.white},
                    fontStyle = FontStyle.Bold,
                };
            }
        }


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

        private SerializedProperty advancedSettingsFoldout;

        private SerializedProperty animatedGO;

        private SerializedProperty animTimeRatio;

        private SerializedProperty targetGO;

        private SerializedProperty forwardPointOffset;

        private SerializedProperty maxAnimationSpeed;

        private SerializedProperty rotationSpeed;
        private const float FloatPrecision = 0.001f;
        private const float ScaleHandleSize = 1.5f;

		private SerializedProperty positionLerpSpeed;
		private SerializedProperty pathData;
		private SerializedProperty enableControlsInPlayMode;
		private SerializedProperty skin;

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
            if (GUILayout.Button(new GUIContent(
                            playPauseBtnText,
                            ""))) {

                HandlePlayPause();
            }

            // Draw Stop button.
            if (GUILayout.Button(new GUIContent(
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

            // Display advanced foldout content.
            if (advancedSettingsFoldout.boolValue) {
                EditorGUILayout.PropertyField(maxAnimationSpeed);
                EditorGUILayout.PropertyField(skin);
            }

            serializedObject.ApplyModifiedProperties();

            //if (GUI.changed) EditorUtility.SetDirty(target);
        }

        private void DrawRotationModeDropdown() {
            // Remember current RotationMode.
            var prevRotationMode = script.RotationMode;
            // Draw RotationMode dropdown.
            script.RotationMode = (AnimatorRotationMode) EditorGUILayout.EnumPopup(
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
            script = (AnimationPathAnimator)target;

            rotationSpeed = serializedObject.FindProperty("rotationSpeed");
            animTimeRatio = serializedObject.FindProperty("animTimeRatio");
            animatedGO = serializedObject.FindProperty("animatedGO");
            targetGO = serializedObject.FindProperty("targetGO");
            forwardPointOffset =
                serializedObject.FindProperty("forwardPointOffset");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");
            maxAnimationSpeed =
                serializedObject.FindProperty("maxAnimationSpeed");
			positionLerpSpeed = serializedObject.FindProperty("positionLerpSpeed");
            pathData = serializedObject.FindProperty("pathData");
            enableControlsInPlayMode =
                serializedObject.FindProperty("enableControlsInPlayMode");
            skin = serializedObject.FindProperty("skin");
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

            HandleDrawingForwardPointMarker();
            //HandleDrawingTargetGizmo();
            HandleDrawingEaseHandles();
            HandleDrawingRotationHandle();
            HandleDrawingTiltingHandles();
            HandleDrawingEaseLabel();
            HandleDrawingTiltLabel();

            //script.UpdateAnimatedGO();
        }

        #endregion UNITY MESSAGES

        #region DRAWING HANDLERS

		void HandleWrapModeDropdown () {
			script.UpdateWrapMode();
		}

		void HandleNoneModeOptionShortcut () {
			if (Event.current.type != EventType.keyUp
			    || Event.current.keyCode != NoneModeShortcut) return;
			
			script.HandleMode = AnimatorHandleMode.None;
		}

		void HandleUpdateAllOptionShortcut () {
			if (Event.current.type != EventType.keyUp
			    || Event.current.keyCode != UpdateAllShortcut) return;
			
			script.UpdateAllMode = !script.UpdateAllMode;
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

        private void HandleDrawingForwardPointMarker() {
            if (script.RotationMode != AnimatorRotationMode.Forward) return;

            var targetPos = script.GetForwardPoint();
			var globalTargetPos = script.transform.TransformPoint(targetPos);

            Handles.Label(globalTargetPos, "Point", ForwardPointMarkerStyle);
        }

        private void HandleDrawingRotationHandle() {
            if (script.HandleMode != AnimatorHandleMode.Rotation) return;

            // Draw handles.
            DrawRotationHandle(DrawRotationHandlesCallbackHandler);
        }

        //private void HandleDrawingTargetGizmo() {
        //    if (targetGO.objectReferenceValue == null) return;

        //    var targetPos =
        //        ((Transform)targetGO.objectReferenceValue).position;

        //    Handles.Label(targetPos, "Target", TargetGizmoStyle);

        //}

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
        #endregion DRAWING HANDLERS

        #region DRAWING METHODS

        private void DrawEaseHandles(Action<int, float> callback) {
            // Get path node positions.
            var nodePositions = script.AnimationPathBuilder.GetNodeGlobalPositions();

            // Get ease values.
            var easeCurveValues = new float[script.PathData.EaseCurve.length];
            for (var i = 0; i < script.PathData.EaseCurve.length; i++) {
                easeCurveValues[i] = script.PathData.EaseCurve.keys[i].value;
            }

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
                    (value) => callback(i, value));
            }
        }

        /// <summary>
        /// Draw arc handle.
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

            var arcValue = value * arcValueMultiplier;
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

            var scaleHandleSize = handleSize * ScaleHandleSize;
            float newArcValue = Handles.ScaleValueHandle(
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

            int nodesNo = script.AnimationPathBuilder.NodesNo;

            // For each path node..
            for (int i = 0; i < nodesNo; i++) {
                // Get value to display.
                var arcValue = String.Format(
                    "{0:0}",
                    calculateValueCallback(i));

                DrawNodeLabel(i, arcValue, offsetX, offsetY, style);
            }
        }

        private void DrawRotationHandle(Action<float, Vector3> callback) {
            var currentAnimationTime = script.AnimationTimeRatio;
            var rotationPointPosition = script.GetRotationAtTime(currentAnimationTime);
            var nodeTimestamps = script.AnimationPathBuilder.GetNodeTimestamps();

            // Return if current animation time is not equal to any node
            // timestamp.
            var index = Array.FindIndex(
                nodeTimestamps, x => Math.Abs(x - currentAnimationTime) < FloatPrecision);
            if (index < 0) return;

            Handles.color = Color.magenta;
            var handleSize = HandleUtility.GetHandleSize(rotationPointPosition);
            var sphereSize = handleSize * RotationHandleSize;

			var rotationPointGlobalPos = script.transform.TransformPoint(rotationPointPosition);

            // draw node's handle.
            var newGlobalPosition = Handles.FreeMoveHandle(
                //rotationPointPosition,
				rotationPointGlobalPos,
                Quaternion.identity,
                sphereSize,
                Vector3.zero,
                Handles.SphereCap);

            if (newGlobalPosition != rotationPointGlobalPos) {
				var newPointLocalPosition = script.transform.InverseTransformPoint(newGlobalPosition);
                // Execute callback.
                callback(currentAnimationTime, newPointLocalPosition);
            }
        }

        private void DrawTiltingHandles(Action<int, float> callback) {
            // Get path node positions.
            //var nodePositions = script.AnimationPathBuilder.GetNodePositions();
			var nodePositions = script.AnimationPathBuilder.GetNodeGlobalPositions();

            // Get tilting curve values.
            var tiltingCurveValues = new float[script.PathData.EaseCurve.length];
            for (var i = 0; i < script.PathData.TiltingCurve.length; i++) {
                tiltingCurveValues[i] = script.PathData.TiltingCurve.keys[i].value;
            }

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
                    (value) => callback(i, value));
            }
        }
        #endregion DRAWING METHODS

        #region CALLBACK HANDLERS
        private void JumpToEndCallbackHandler() {
            // Update animTimeRatio.
            serializedObject.Update();
            animTimeRatio.floatValue = 1;
            serializedObject.ApplyModifiedProperties();
        }

        private void JumpForwardCallbackHandler() {
            // Update animTimeRatio.
            var newAnimationTimeRatio = animTimeRatio.floatValue
                + AnimationPathAnimator.ShortJumpValue;
            serializedObject.Update();
            animTimeRatio.floatValue =
                (float)(Math.Round(newAnimationTimeRatio, 3));
            serializedObject.ApplyModifiedProperties();
        }

        private void JumpToStartCallbackHandler() {
            // Update animTimeRatio.
            serializedObject.Update();
            animTimeRatio.floatValue = 0;
            serializedObject.ApplyModifiedProperties();
        }

        private void JumpBackwardCallbackHandler() {
            // Update animTimeRatio.
            var newAnimationTimeRatio = animTimeRatio.floatValue
                - AnimationPathAnimator.ShortJumpValue;
            serializedObject.Update();
            animTimeRatio.floatValue =
                (float)(Math.Round(newAnimationTimeRatio, 3));
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEaseHandlesCallbackHandler(
            int keyIndex,
            float newValue) {

            Undo.RecordObject(script.PathData, "Ease curve changed.");

			if (script.UpdateAllMode) {
				var keyTime = script.PathData.EaseCurve.keys[keyIndex].time;
				var oldValue = script.PathData.EaseCurve.Evaluate(keyTime);
				var delta = newValue - oldValue;
				script.UpdateEaseValues(delta);
			}
			else {
				script.UpdateEaseValue(keyIndex, newValue);
			}
        }

        private void DrawRotationHandlesCallbackHandler(
                            float timestamp,
                            Vector3 newPosition) {

            Undo.RecordObject(script.PathData, "Rotation path changed.");

            script.ChangeRotationAtTimestamp(timestamp, newPosition);
        }

        private void DrawTiltingHandlesCallbackHandler(
                    int keyIndex,
                    float newValue) {

            Undo.RecordObject(script.PathData, "Tilting curve changed.");

			script.UpdateNodeTilting(keyIndex, newValue);

        }
        #endregion CALLBACK HANDLERS

        #region PRIVATE METHODS
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


        /// <summary>
        /// Change current animation time with arrow keys.
        /// </summary>
        private void ChangeTimeWithArrowKeys() {
            // If a key is pressed..
            if (Event.current.type == EventType.keyDown
                // and modifier key is pressed also..
                    && modKeyPressed) {

                HandleModifiedShortcuts(
                    modJumpForwardCallbackHandler,
                    modJumpBackwardCallbackHandler,
                    jumpToNextNodeCallbackHandler,
                    jumpToPreviousNodeCallbackHandler,
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

        private void AnyModJumpKeyPressedCallbackHandler() {
            if (Application.isPlaying) script.UpdateAnimatedGO();
            if (!Application.isPlaying) script.Animate();
        }

        private void AnyJumpKeyPressedCallbackHandler() {
            if (Application.isPlaying) script.UpdateAnimatedGO();
            if (!Application.isPlaying) script.Animate();
        }

        private void jumpToPreviousNodeCallbackHandler() {
            // Jump to next node.
            animTimeRatio.floatValue = GetNearestBackwardNodeTimestamp();
            serializedObject.ApplyModifiedProperties();
        }

        private void jumpToNextNodeCallbackHandler() {
            // Jump to next node.
            animTimeRatio.floatValue = GetNearestForwardNodeTimestamp();
            serializedObject.ApplyModifiedProperties();
        }

        private void modJumpBackwardCallbackHandler() {
            // Update animation time.
            animTimeRatio.floatValue -= JumpValue;
            serializedObject.ApplyModifiedProperties();
        }

        private void modJumpForwardCallbackHandler() {
            // Update animation time.
            animTimeRatio.floatValue += JumpValue;
            serializedObject.ApplyModifiedProperties();
        }

        private float ConvertEaseToDegrees(int nodeIndex) {
            // Calculate value to display.
            var easeValue = script.GetNodeEaseValue(nodeIndex);
            var arcValueMultiplier = 360 / maxAnimationSpeed.floatValue;
            var easeValueInDegrees = easeValue * arcValueMultiplier;

            return easeValueInDegrees;
        }

        private float ConvertTiltToDegrees(int nodeIndex) {
            var rotationValue = script.GetNodeTiltValue(nodeIndex);
            //var arcValue = rotationValue * 2;

            return rotationValue;
        }

        private float GetNearestBackwardNodeTimestamp() {
            var pathTimestamps = script.GetPathTimestamps();

            for (var i = pathTimestamps.Length - 1; i >= 0; i--) {
                if (pathTimestamps[i] < animTimeRatio.floatValue) {
                    return pathTimestamps[i];
                }
            }

            // Return timestamp of the last node.
            return 0;
        }

        private float GetNearestForwardNodeTimestamp() {
            var pathTimestamps = script.GetPathTimestamps();

            foreach (var timestamp in pathTimestamps
                .Where(timestamp => timestamp > animTimeRatio.floatValue)) {

                return timestamp;
            }

            // Return timestamp of the last node.
            return 1.0f;
        }
        #endregion PRIVATE METHODS

        #region SHORTCUT HANDLERS
        /// <summary>
        /// Checked if modifier key is pressed and remember it in a class
        /// field.
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

                    if (jumpBackwardCallback!= null) jumpBackwardCallback();
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

                    if (jumpToNextNodeCallback != null) jumpToNextNodeCallback();
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

        private void HandleTiltingModeOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != TiltingModeShortcut) return;

            script.HandleMode = AnimatorHandleMode.Tilting;
        }

        private void HandleRotationModeOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != RotationModeShortcut) return;

            script.HandleMode = AnimatorHandleMode.Rotation;
        }

        private void HandleEaseModeOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != EaseModeShortcut) return;

            script.HandleMode = AnimatorHandleMode.Ease;
        }

        private void HandlePlayPauseShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != PlayPauseShortcut) return;

            HandlePlayPause();
        }
        #endregion
    }
}	