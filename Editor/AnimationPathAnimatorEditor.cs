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
        #endregion CONSTANTS

        #region FIELDS
        private readonly GUIStyle targetGizmoStyle = new GUIStyle {
            normal = { textColor = Color.white },
            fontStyle = FontStyle.Bold,
        };

        private readonly Color tiltingHandleColor = Color.green;

        private readonly GUIStyle easeValueLabelStyle = new GUIStyle {
            normal = { textColor = Color.white },
            fontStyle = FontStyle.Bold,
        };

        /// <summary>
        /// If modifier is currently pressed.
        /// </summary>
        private bool modKeyPressed;

        /// <summary>
        /// Reference to target script.
        /// </summary>
        private AnimationPathAnimator script;

        // TODO Make it property.
        private readonly GUIStyle tiltValueLabelStyle = new GUIStyle {
            normal = { textColor = Color.white },
            fontStyle = FontStyle.Bold,
        };

        private readonly GUIStyle forwardPointMarkerStyle = new GUIStyle {
            normal = { textColor = Color.white },
            fontStyle = FontStyle.Bold,
        };
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

        #endregion SERIALIZED PROPERTIES

        #region UNITY MESSAGES

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                pathData,
                new GUIContent(
                    "Path Asset",
                    ""));

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

			EditorGUILayout.PropertyField(
				positionLerpSpeed,
				new GUIContent(
				"Position Lerp Speed",
				""));

			serializedObject.ApplyModifiedProperties();

            //EditorGUILayout.PropertyField(rotationMode);
            script.RotationMode = (AnimatorRotationMode) EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Rotation Mode",
                    ""),
                script.RotationMode);

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

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(
                            "Play/Pause",
                            ""))) {

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

            EditorGUILayout.Space();

            advancedSettingsFoldout.boolValue = EditorGUILayout.Foldout(
                    advancedSettingsFoldout.boolValue,
                    new GUIContent(
                        "Advanced Settings",
                        ""));
            if (advancedSettingsFoldout.boolValue) {
                EditorGUILayout.PropertyField(maxAnimationSpeed);
            }

            serializedObject.ApplyModifiedProperties();
            //if (GUI.changed) EditorUtility.SetDirty(target);
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
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnSceneGUI() {
            if (Event.current.type == EventType.ValidateCommand
                && Event.current.commandName == "UndoRedoPerformed") {
            }
            // Update modifier key state.
            UpdateModifierKey();

            //serializedObject.Update();

            //serializedObject.ApplyModifiedProperties();

            HandleEaseModeOptionShortcut();
            HandleRotationModeOptionShortcut();
            HandleTiltingModeOptionShortcut();
			HandleNoneModeOptionShortcut();
			HandleUpdateAllOptionShortcut();

            // Return if path asset does not exist.
            if (script.AnimationPathBuilder.PathData == null) return;

            // Change current animation time with arrow keys.
            ChangeTimeWithArrowKeys();

			HandleWrapModeDropdown();

            HandleDrawingForwardPointMarker();
            HandleDrawingTargetGizmo();
            HandleDrawingEaseHandles();
            HandleDrawingRotationHandle();
            HandleDrawingTiltingHandles();
            HandleDrawingEaseLabel();
            HandleDrawingTiltLabel();

            //script.UpdateAnimation();
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

            Action<int, float> callbackHandler =
                DrawEaseHandlesCallbackHandler;

            DrawEaseHandles(callbackHandler);
        }

        private void HandleDrawingEaseLabel() {
            if (script.HandleMode != AnimatorHandleMode.Ease) return;

            DrawNodeLabels(
                ConvertEaseToDegrees,
                EaseValueLabelOffsetX,
                EaseValueLabelOffsetY,
                easeValueLabelStyle);
        }

        private void HandleDrawingForwardPointMarker() {
            if (script.RotationMode != AnimatorRotationMode.Forward) return;

            var targetPos = script.GetForwardPoint();
			var globalTargetPos = script.transform.TransformPoint(targetPos);

            Handles.Label(globalTargetPos, "Point", forwardPointMarkerStyle);
        }

        private void HandleDrawingRotationHandle() {
            //if (!drawRotationHandle.boolValue) return;
            if (script.HandleMode != AnimatorHandleMode.Rotation) return;

            // Callback to call when node rotation is changed. TODO Pass func.
            // directly as an argument.
            Action<float, Vector3> callbackHandler =
                DrawRotationHandlesCallbackHandler;

            // Draw handles.
            DrawRotationHandle(callbackHandler);
        }

        private void HandleDrawingTargetGizmo() {
            if (targetGO.objectReferenceValue == null) return;

            var targetPos =
                ((Transform)targetGO.objectReferenceValue).position;

            Handles.Label(targetPos, "Target", targetGizmoStyle);

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
                tiltValueLabelStyle);
        }
        #endregion DRAWING HANDLERS

        #region DRAWING METHODS

        private void DrawEaseHandles(Action<int, float> callback) {
            // Get path node positions.
            var nodePositions = script.AnimationPathBuilder.GetNodeGlobalPositions();

            // Get ease values.
            var easeCurveValues = new float[script.EaseCurve.length];
            for (var i = 0; i < script.EaseCurve.length; i++) {
                easeCurveValues[i] = script.EaseCurve.keys[i].value;
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
            var tiltingCurveValues = new float[script.EaseCurve.length];
            for (var i = 0; i < script.TiltingCurve.length; i++) {
                tiltingCurveValues[i] = script.TiltingCurve.keys[i].value;
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

		// TODO Check if you can pass also timestamp as arg. without adding
		// much overhead.
        private void DrawEaseHandlesCallbackHandler(int keyIndex, float newValue) {
            Undo.RecordObject(script, "Ease curve changed.");

			if (script.UpdateAllMode) {
				var keyTime = script.EaseCurve.keys[keyIndex].time;
				var oldValue = script.EaseCurve.Evaluate(keyTime);
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

            Undo.RecordObject(script.PathData.RotationPath, "Rotation path changed.");

            script.ChangeRotationAtTimestamp(timestamp, newPosition);
        }

        private void DrawTiltingHandlesCallbackHandler(
                    int keyIndex,
                    float newValue) {

            Undo.RecordObject(script, "Tilting curve changed.");

			script.UpdateNodeTilting(keyIndex, newValue);

        }
        #endregion CALLBACK HANDLERS

        #region PRIVATE METHODS

        /// <summary>
        /// Change current animation time with arrow keys.
        /// </summary>
        private void ChangeTimeWithArrowKeys() {
            // If a key is pressed..
            if (Event.current.type == EventType.keyDown
                // and modifier key is pressed also..
                    && modKeyPressed) {

                HandleModifiedShortcuts();
				script.UpdateAnimation();
            }
            // Modifier key not pressed.
            else if (Event.current.type == EventType.keyDown) {
                HandleUnmodifiedShortcuts();
				script.UpdateAnimation();
            }
        }
        private void HandleEaseModeOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != EaseModeShortcut) return;

            script.HandleMode = AnimatorHandleMode.Ease;
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

        private void HandleModifiedShortcuts() {
			serializedObject.Update();

            // Check what key is pressed..
            switch (Event.current.keyCode) {
                // Jump backward.
                case AnimationPathAnimator.JumpBackward:
                    Event.current.Use();

                    // Update animation time.
                    animTimeRatio.floatValue -=
                        AnimationPathAnimator.JumpValue;

                    break;
                // Jump forward.
                case AnimationPathAnimator.JumpForward:
                    Event.current.Use();

                    // Update animation time.
                    animTimeRatio.floatValue +=
                        AnimationPathAnimator.JumpValue;

                    break;

                case AnimationPathAnimator.JumpToStart:
                    Event.current.Use();

                    // Jump to next node.
                    animTimeRatio.floatValue = GetNearestForwardNodeTimestamp();

                    break;

                case AnimationPathAnimator.JumpToEnd:
                    Event.current.Use();

                    // Jump to next node.
                    animTimeRatio.floatValue = GetNearestBackwardNodeTimestamp();

                    break;
            }

			serializedObject.ApplyModifiedProperties();
        }

        private void HandleUnmodifiedShortcuts() {
			serializedObject.Update();

            // Helper variable.
            float newAnimationTimeRatio;
            switch (Event.current.keyCode) {
                // Jump backward.
                case AnimationPathAnimator.JumpBackward:
                    Event.current.Use();

                    // Calculate new time ratio.
                    newAnimationTimeRatio = animTimeRatio.floatValue
                                            - AnimationPathAnimator.ShortJumpValue;
                    // Apply rounded value.
                    animTimeRatio.floatValue =
                        (float)(Math.Round(newAnimationTimeRatio, 3));

                    break;
                // Jump forward.
                case AnimationPathAnimator.JumpForward:
                    Event.current.Use();

                    newAnimationTimeRatio = animTimeRatio.floatValue
                                            + AnimationPathAnimator.ShortJumpValue;
                    animTimeRatio.floatValue =
                        (float)(Math.Round(newAnimationTimeRatio, 3));

                    break;

                case AnimationPathAnimator.JumpToStart:
                    Event.current.Use();

                    animTimeRatio.floatValue = 1;

                    break;

                case AnimationPathAnimator.JumpToEnd:
                    Event.current.Use();

                    animTimeRatio.floatValue = 0;

                    break;
            }

			serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Checked if modifier key is pressed and remember it in a class
        /// field.
        /// </summary>
        private void UpdateModifierKey() {
            // Check if modifier key is currently pressed.
            if (Event.current.type == EventType.keyDown
                    && Event.current.keyCode == AnimationPathAnimator.ModKey) {

                // Remember key state.
                modKeyPressed = true;
            }
            // If modifier key was released..
            if (Event.current.type == EventType.keyUp
                    && Event.current.keyCode == AnimationPathAnimator.ModKey) {

                modKeyPressed = false;
            }
        }
        #endregion PRIVATE METHODS
    }
}	