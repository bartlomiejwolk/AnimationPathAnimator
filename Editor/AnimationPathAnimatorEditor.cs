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
        #endregion CONSTANTS

        #region FIELDS

        private readonly Color tiltingHandleColor = Color.green;
        private GUIStyle easeValueLabelStyle;
        /// <summary>
        /// If modifier is currently pressed.
        /// </summary>
        private bool modKeyPressed;

        /// <summary>
        /// Reference to target script.
        /// </summary>
        private AnimationPathAnimator script;

        private GUIStyle tiltValueLabelStyle;

        private GUIStyle forwardPointMarkerStyle;
        #endregion FIELDS

        #region SERIALIZED PROPERTIES

        private SerializedProperty advancedSettingsFoldout;

        // TODO Change this value directly.
        private SerializedProperty animatedGO;

        private SerializedProperty animTimeRatio;

        private SerializedProperty targetGO;

        private SerializedProperty forwardPointOffset;

        private SerializedProperty maxAnimationSpeed;

        private SerializedProperty rotationSpeed;
        #endregion SERIALIZED PROPERTIES

        #region UNITY MESSAGES

        public override void OnInspectorGUI() {
            serializedObject.Update();

            animTimeRatio.floatValue = EditorGUILayout.Slider(
                new GUIContent(
                    "Animation Time",
                    "Current animation time."),
                    animTimeRatio.floatValue,
                    0,
                    1);

            script.HandleMode = (AnimatorHandleMode) EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Handle Mode",
                    ""),
                script.HandleMode);

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

            easeValueLabelStyle = new GUIStyle {
                normal = { textColor = Color.white },
                fontStyle = FontStyle.Bold,
            };
            tiltValueLabelStyle = easeValueLabelStyle;
            forwardPointMarkerStyle = easeValueLabelStyle;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnSceneGUI() {
            if (Event.current.type == EventType.ValidateCommand
                && Event.current.commandName == "UndoRedoPerformed") {
            }

            // TODO Is this update needed?
            serializedObject.Update();

            // Update modifier key state.
            UpdateModifierKey();

            // Change current animation time with arrow keys.
            ChangeTimeWithArrowKeys();

            // Save changes
            serializedObject.ApplyModifiedProperties();

            HandleDrawingForwardPointMarker();
            HandleDrawingTargetGizmo();
            HandleDrawingEaseHandles();
            HandleDrawingRotationHandle();
            HandleDrawingTiltingHandles();
            HandleDrawingEaseLabel();
            HandleDrawingTiltLabel();

            script.UpdateAnimation();
        }

        #endregion UNITY MESSAGES

        #region DRAWING HANDLERS

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

            Handles.Label(targetPos, "Point", forwardPointMarkerStyle);
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
            // TODO Create class field with this style.
            var style = new GUIStyle {
                normal = { textColor = Color.white },
                fontStyle = FontStyle.Bold,
            };

            Handles.Label(targetPos, "Target", style);

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

        // TODO Refactor.
        private void DrawEaseHandles(Action<int, float> callback) {
            // Get AnimationPath node positions.
            var nodePositions = script.AnimatedObjectPath.GetNodePositions();

            // Get ease curve timestamps.
            var easeTimestamps = new float[script.EaseCurve.length];
            for (var i = 0; i < script.EaseCurve.length; i++) {
                easeTimestamps[i] = script.EaseCurve.keys[i].time;
            }

            var easeCurveValues = new float[script.EaseCurve.length];
            for (var i = 0; i < script.EaseCurve.length; i++) {
                easeCurveValues[i] = script.EaseCurve.keys[i].value;
            }

            // For each path node..
            for (var i = 0; i < nodePositions.Length; i++) {
                var easeValue = easeCurveValues[i];
                //var arcValue = easeValue * 3600f;
                var arcValueMultiplier = 360 / maxAnimationSpeed.floatValue;
                var arcValue = easeValue * arcValueMultiplier;
                var handleSize = HandleUtility.GetHandleSize(nodePositions[i]);
                var arcRadius = handleSize * ArcHandleRadius;

                // TODO Create const.
                Handles.color = Color.red;

                Handles.DrawWireArc(
                    nodePositions[i],
                    Vector3.up,
                    // Make the arc simetrical on the left and right side of
                    // the object.
                    Quaternion.AngleAxis(
                    //-arcValue / 2,
                        0,
                        Vector3.up) * Vector3.forward,
                    arcValue,
                    arcRadius);

                // TODO Create const.
                Handles.color = Color.red;

                // Set initial arc value to other than zero. If initial value
                // is zero, handle will always return zero.
                arcValue = Math.Abs(arcValue) < 0.001f ? 10f : arcValue;

                // TODO Create constant.
                var scaleHandleSize = handleSize * 1.5f;
                float newArcValue = Handles.ScaleValueHandle(
                    arcValue,
                    nodePositions[i] + Vector3.forward * arcRadius
                        * 1.3f,
                    Quaternion.identity,
                    scaleHandleSize,
                    Handles.ConeCap,
                    1);

                // Limit handle value.
                if (newArcValue > 360) newArcValue = 360;
                if (newArcValue < 0) newArcValue = 0;

                // TODO Create float precision const.
                if (Math.Abs(newArcValue - arcValue) > 0.001f) {
                    // Execute callback.
                    callback(i, newArcValue / arcValueMultiplier);
                }
            }
        }

        private void DrawNodeLabel(
                    int nodeIndex,
                    string value,
                    int offsetX,
                    int offsetY,
                    GUIStyle style) {

            // Get node position.
            var nodePosition = script.GetNodePosition(nodeIndex);

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

            int nodesNo = script.AnimatedObjectPath.NodesNo;

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
            var currentObjectPosition = script.GetRotationAtTime(currentAnimationTime);
            var nodeTimestamps = script.AnimatedObjectPath.GetNodeTimestamps();

            // Return if current animation time is not equal to any node
            // timestamp.
            var index = Array.FindIndex(
                nodeTimestamps, x => Math.Abs(x - currentAnimationTime) < 0.001f);
            if (index < 0) return;

            Handles.color = Color.magenta;
            var handleSize = HandleUtility.GetHandleSize(currentObjectPosition);
            var sphereSize = handleSize * RotationHandleSize;

            // draw node's handle.
            var newPosition = Handles.FreeMoveHandle(
                currentObjectPosition,
                Quaternion.identity,
                sphereSize,
                Vector3.zero,
                Handles.SphereCap);

            if (newPosition != currentObjectPosition) {
                // Execute callback.
                callback(currentAnimationTime, newPosition);
            }
        }

        // TODO Extract methods. Do the same to ease curve drawing method.
        private void DrawTiltingHandles(Action<int, float> callback) {
            // Get AnimationPath node positions.
            var nodePositions = script.AnimatedObjectPath.GetNodePositions();

            // Get rotation curve timestamps.
            //var easeTimestamps = new float[script.EaseCurve.length];
            //for (var i = 0; i < script.EaseCurve.length; i++) {
            //    easeTimestamps[i] = script.EaseCurve.keys[i].time;
            //}

            // Get rotation curve values.
            var tiltingCurveValues = new float[script.EaseCurve.length];
            for (var i = 0; i < script.TiltingCurve.length; i++) {
                tiltingCurveValues[i] = script.TiltingCurve.keys[i].value;
            }

            // For each path node..
            for (var i = 0; i < nodePositions.Length; i++) {
                var rotationValue = tiltingCurveValues[i];
                //var arcValue = rotationValue * 2;
                var arcValue = rotationValue;
                var handleSize = HandleUtility.GetHandleSize(nodePositions[i]);
                var arcHandleSize = handleSize * ArcHandleRadius;

                // TODO Create const.
                Handles.color = tiltingHandleColor;

                Handles.DrawWireArc(
                    nodePositions[i],
                    Vector3.up,
                    // Make the arc simetrical on the left and right side of
                    // the object.
                    Quaternion.AngleAxis(
                    //-arcValue / 2,
                        0,
                        Vector3.up) * Vector3.forward,
                    arcValue,
                    arcHandleSize);

                // TODO Create const.
                Handles.color = tiltingHandleColor;

                // TODO Create constant.
                var scaleHandleSize = handleSize * 1.5f;

                // Set initial arc value to other than zero. If initial value
                // is zero, handle will always return zero.
                arcValue = Math.Abs(arcValue) < 0.001f ? 15f : arcValue;

                float newArcValue = Handles.ScaleValueHandle(
                    arcValue,
                    nodePositions[i] + Vector3.forward * arcHandleSize
                        * 1.3f,
                    Quaternion.identity,
                    scaleHandleSize,
                    Handles.ConeCap,
                    1);

                // Limit handle value.
                if (newArcValue > 90) newArcValue = 90;
                if (newArcValue < -90) newArcValue = -90;

                // TODO Create float precision const.
                if (Math.Abs(newArcValue - arcValue) > 0.001f) {
                    // Execute callback.
                    callback(i, newArcValue);
                }
            }
        }
        #endregion DRAWING METHODS

        #region CALLBACK HANDLERS

        // TODO Refactor.
        private void DrawEaseHandlesCallbackHandler(int keyIndex, float newValue) {
            RecordTargetObject();

            // Copy keyframe.
            var keyframeCopy = script.EaseCurve.keys[keyIndex];
            // Update keyframe timestamp.
            //keyframeCopy.time = newValue;
            // Update keyframe value.
            keyframeCopy.value = newValue;
            //var oldTimestamp = script.EaseCurve.keys[keyIndex].time;

            // Replace old key with updated one.
            script.EaseCurve.RemoveKey(keyIndex);
            script.EaseCurve.AddKey(keyframeCopy);
            script.SmoothCurve(script.EaseCurve);
            //script.EaseCurveExtremeNodes(script.EaseCurve);

            // If new timestamp is bigger than old timestamp..
            //if (newValue > oldTimestamp) {
            //    // Get timestamp of the node to the right.
            //    var rightNeighbourTimestamp = script.EaseCurve.keys[keyIndex + 1].time;
            //    // If new timestamp is bigger or equal to the neighbors's..
            //    if (newValue >= rightNeighbourTimestamp) return;

            //    // Move key in the easeCurve.
            //    script.EaseCurve.MoveKey(keyIndex, keyframeCopy);
            //}
            //else {
            //    // Get timestamp of the node to the left.
            //    var leftNeighbourTimestamp = script.EaseCurve.keys[keyIndex - 1].time;
            //    // If new timestamp is smaller or equal to the neighbors's..
            //    if (newValue <= leftNeighbourTimestamp) return;

            //    // Move key in the easeCurve.
            //    script.EaseCurve.MoveKey(keyIndex, keyframeCopy);
            //}
        }

        private void DrawRotationHandlesCallbackHandler(
                            float timestamp,
                            Vector3 newPosition) {

            RecordRotationObject();

            script.ChangeRotationForTimestamp(timestamp, newPosition);
        }

        private void DrawTiltingHandlesCallbackHandler(
                    int keyIndex,
                    float newValue) {

            RecordTargetObject();

            // Copy keyframe.
            var keyframeCopy = script.TiltingCurve.keys[keyIndex];
            // Update keyframe value.
            keyframeCopy.value = newValue;
            //var oldTimestamp = script.EaseCurve.keys[keyIndex].time;

            // Replace old key with updated one.
            script.TiltingCurve.RemoveKey(keyIndex);
            script.TiltingCurve.AddKey(keyframeCopy);
            script.SmoothCurve(script.TiltingCurve);
            script.EaseCurveExtremeNodes(script.TiltingCurve);
        }
        #endregion CALLBACK HANDLERS

        #region PRIVATE METHODS

        protected void RecordRotationObject() {
            Undo.RecordObject(script.RotationCurves, "Ease curve changed.");
        }

        /// <summary>
        /// Record target object state for undo.
        /// </summary>
        // TODO Remove these methods and use record directly.
        protected void RecordTargetObject() {
            Undo.RecordObject(script, "Ease curve changed.");
        }

        /// <summary>
        /// Change current animation time with arrow keys.
        /// </summary>
        private void ChangeTimeWithArrowKeys() {
            // If a key is pressed..
            if (Event.current.type == EventType.keyDown
                // and modifier key is pressed also..
                    && modKeyPressed) {

                HandleModifiedShortcuts();
            }
            // Modifier key not pressed.
            else if (Event.current.type == EventType.keyDown) {
                HandleUnmodifiedShortcuts();
            }

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
        // TODO Rename to GetNearestBackwardNodeTimestamp().
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

        // TODO Rename to GetNearestForwardNodeTimestamp().
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
        }

        private void HandleUnmodifiedShortcuts() {
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