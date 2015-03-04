using System;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace ATP.SimplePathAnimator.Animator {

    /// <summary>
    /// Class responsible for drawing all on scene handles.
    /// </summary>
    /// <remarks>It has same access to the AnimationPathAnimator class as
    /// the AnimationPathAnimatorEditor does.</remarks>
    // TODO Pass ref. to AnimationPathAnimator in the constructor.
    public class AnimatorHandles {

        private readonly AnimationPathAnimator animator;

        public AnimatorHandles(AnimationPathAnimator animator) {
            this.animator = animator;
        }

        private AnimationPathAnimator Animator {
            get { return animator; }
        }

        private AnimatorSettings AnimatorSettings {
            get { return Animator.Settings; }
        }

        private AnimatorSettings Settings{
            get { return Animator.Settings; }
        }

        public void DrawAddNodeButtons(
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
                    Settings.AddButtonH,
                    Settings.AddButtonV,
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
        public void DrawArcHandle(
            float value,
            Vector3 position,
            float arcValueMultiplier,
            int minDegrees,
            int maxDegrees,
            Color handleColor,
            Action<float> callback) {

            var arcValue = value * arcValueMultiplier;
            var handleSize = HandleUtility.GetHandleSize(position);
            var arcRadius = handleSize * Settings.ArcHandleRadius;

            Handles.color = handleColor;

            Handles.DrawWireArc(
                position,
                Vector3.up,
                Quaternion.AngleAxis(
                    0,
                    Vector3.up) * Vector3.forward,
                arcValue,
                arcRadius);

            Handles.color = handleColor;

            // Set initial arc value to other than zero. If initial value
            // is zero, handle will always return zero.
            arcValue = Math.Abs(arcValue) < GlobalConstants.FloatPrecision
                ? Settings.InitialArcValue : arcValue;

            var scaleHandleSize = handleSize * Settings.ScaleHandleSize;
            var newArcValue = Handles.ScaleValueHandle(
                arcValue,
                position + Vector3.forward * arcRadius
                * 1.3f,
                Quaternion.identity,
                scaleHandleSize,
                Handles.ConeCap,
                1);

            // Limit handle value.
            if (newArcValue > maxDegrees) newArcValue = maxDegrees;
            if (newArcValue < minDegrees) newArcValue = minDegrees;

            if (Math.Abs(newArcValue - arcValue)
                > GlobalConstants.FloatPrecision) {

                callback(newArcValue / arcValueMultiplier);
            }
        }

        public void DrawArcHandleLabels(
            Vector3[] nodeGlobalPositions,
            Func<int, float> calculateValueCallback,
            GUIStyle style) {

            var nodesNo = nodeGlobalPositions.Length;

            // For each path node..
            for (var i = 0; i < nodesNo; i++) {
                // Get value to display.
                var arcValue = String.Format(
                    "{0:0}",
                    calculateValueCallback(i));

                DrawNodeLabel(
                    nodeGlobalPositions[i],
                    arcValue,
                    // TODO These should be taken from method args.
                    Settings.EaseValueLabelOffsetX,
                    Settings.EaseValueLabelOffsetY,
                    style);
            }
        }

        public void DrawEaseHandles(
            Vector3[] nodePositions,
            float[] easeCurveValues,
            float arcValueMultiplier,
            Action<int, float> callback) {

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

        // TODO Remove animator param. from all methods.
        public void DrawMoveAllPositionHandles(
            AnimationPathAnimator animator,
            Action<int, Vector3, Vector3> callback) {

            if (AnimatorSettings.MovementMode !=
                AnimationPathBuilderHandleMode.MoveAll) return;

            // Node global positions.
            var nodes = animator.PathData.GetGlobalNodePositions(
                animator.Transform);

            // Cap function used to draw handle.
            Handles.DrawCapFunction capFunction = Handles.CircleCap;

            // For each node..
            for (var i = 0; i < nodes.Length; i++) {
                var handleColor = Settings.MoveAllModeColor;

                // Draw position handle.
                var newPos = DrawPositionHandle(
                    nodes[i],
                    handleColor,
                    capFunction);

                // If node was moved..
                if (newPos != nodes[i]) {
                    // Calculate node old local position.
                    var oldNodeLocalPosition =
                        animator.Transform.InverseTransformPoint(nodes[i]);

                    // Calculate node new local position.
                    var newNodeLocalPosition =
                        animator.Transform.InverseTransformPoint(newPos);

                    // Calculate movement delta.
                    var moveDelta = newNodeLocalPosition - oldNodeLocalPosition;

                    // Execute callback.
                    callback(i, newNodeLocalPosition, moveDelta);
                }
            }
        }

        public void DrawMoveSinglePositionsHandles(AnimationPathAnimator animator,
            Action<int, Vector3, Vector3> callback) {

            if (AnimatorSettings.MovementMode !=
                AnimationPathBuilderHandleMode.MoveSingle) return;

            // Node global positions.
            var nodes = animator.PathData.GetGlobalNodePositions(
                animator.Transform);

            // Cap function used to draw handle.
            Handles.DrawCapFunction capFunction = Handles.CircleCap;

            // For each node..
            for (var i = 0; i < nodes.Length; i++) {
                var handleColor = animator.Settings.GizmoCurveColor;

                // Draw position handle.
                var newPos = DrawPositionHandle(
                    nodes[i],
                    handleColor,
                    capFunction);

                // TODO Make it into callback.
                // If node was moved..
                if (newPos != nodes[i]) {
                    // Calculate node old local position.
                    var oldNodeLocalPosition = animator.Transform.InverseTransformPoint(nodes[i]);

                    // Calculate node new local position.
                    var newNodeLocalPosition = animator.Transform.InverseTransformPoint(newPos);

                    // Calculate movement delta.
                    var moveDelta = newNodeLocalPosition - oldNodeLocalPosition;

                    // Execute callback.
                    callback(i, newNodeLocalPosition, moveDelta);
                }
            }
        }

        public void DrawNodeLabel(
            Vector3 nodeGlobalPosition,
            string value,
            int offsetX,
            int offsetY,
            GUIStyle style) {

            // Translate node's 3d position into screen coordinates.
            var guiPoint = HandleUtility.WorldToGUIPoint(nodeGlobalPosition);

            // Create rectangle for the label.
            var labelPosition = new Rect(
                guiPoint.x + offsetX,
                guiPoint.y + offsetY,
                Settings.DefaultLabelWidth,
                Settings.DefaultLabelHeight);

            Handles.BeginGUI();

            // Draw label.
            GUI.Label(
                labelPosition,
                value,
                style);

            Handles.EndGUI();
        }

        public void DrawNodeLabels(
            AnimationPathAnimator animator,
            string text,
            int offsetX,
            int offsetY,
            GUIStyle style) {

            var nodeGlobalPositions = animator.PathData.GetGlobalNodePositions(
                animator.Transform);

            foreach (var nodePos in nodeGlobalPositions) {
                DrawNodeLabel(
                    nodePos,
                    text,
                    offsetX,
                    offsetY,
                    style);
            }
        }

        public Vector3 DrawPositionHandle(
            Vector3 nodePosition,
            Color handleColor,
            Handles.DrawCapFunction capFunction) {

            // Set handle color.
            Handles.color = handleColor;

            // Get handle size.
            var handleSize = HandleUtility.GetHandleSize(nodePosition);
            var sphereSize = handleSize * Settings.MovementHandleSize;

            // Draw handle.
            var newPos = Handles.FreeMoveHandle(
                nodePosition,
                Quaternion.identity,
                sphereSize,
                Vector3.zero,
                capFunction);
            return newPos;
        }

        public void DrawRemoveNodeButtons(
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
                    Settings.RemoveButtonH,
                    Settings.RemoveButtonV,
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

        public void DrawRotationHandle(
            AnimationPathAnimator script,
            Vector3 rotationPointPosition,
            Action<float, Vector3> callback) {

            var handleSize = HandleUtility.GetHandleSize(rotationPointPosition);
            var sphereSize = handleSize * Settings.RotationHandleSize;

            var rotationPointGlobalPos =
                script.transform.TransformPoint(rotationPointPosition);

            // Draw node's handle.
            var newGlobalPosition = Handles.FreeMoveHandle(
                rotationPointGlobalPos,
                Quaternion.identity,
                sphereSize,
                Vector3.zero,
                Handles.SphereCap);

            if (newGlobalPosition != rotationPointGlobalPos) {
                var newPointLocalPosition =
                    script.transform.InverseTransformPoint(newGlobalPosition);

                callback(script.AnimationTimeRatio, newPointLocalPosition);
            }
        }

        public void DrawTiltingHandles(
            Vector3[] nodePositions,
            float[] tiltingCurveValues,
            Action<int, float> callback) {

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

        public void DrawUpdateAllLabels(
            AnimationPathAnimator animator,
            GUIStyle style) {

            DrawNodeLabels(
                animator,
                Settings.UpdateAllLabelText,
                Settings.UpdateAllLabelOffsetX,
                Settings.UpdateAllLabelOffsetY,
                style);
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
    }

}