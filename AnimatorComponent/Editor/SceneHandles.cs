using System;
using ATP.LoggingTools;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools.AnimatorComponent {

    /// <summary>
    ///     Class responsible for drawing all on scene handles.
    /// </summary>
    public static class SceneHandles {

        /// <summary>
        /// Minimum value below which arc handle drawer method will set the 
        /// value back to default.
        /// </summary>
        private static float MinValueThreshold = 0.05f;
        #region METHDOS

        public static void DrawAddNodeButtons(
            Vector3[] nodePositions,
            int buttonHoffset,
            int buttonVoffset,
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
                    buttonHoffset,
                    buttonVoffset,
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

        public static void DrawArcHandleLabels(
            Vector3[] nodeGlobalPositions,
            int offsetX,
            int offsetY,
            int labelWidth,
            int labelHeight,
            Func<int, float> calculateValueCallback,
            GUIStyle style) {

            var nodesNo = nodeGlobalPositions.Length;

            // For each path node..
            for (var i = 0; i < nodesNo; i++) {
                // Original value.
                var value = calculateValueCallback(i);
                // Value to display.
                var displayedValue = (Mathf.Abs(value) % 360) * Mathf.Sign(value);
                // Calculate number of full 360 deg. cycles.
                var cycles = Mathf.Floor(Mathf.Abs(value) / 360);

                // Get value to display.
                var labelText = String.Format(
                    "{1} {0:0.0}",
                    //value,
                    displayedValue,
                    cycles);

                DrawNodeLabel(
                    nodeGlobalPositions[i],
                    labelText,
                    offsetX,
                    offsetY,
                    labelWidth,
                    labelHeight,
                    style);
            }
        }

        public static void DrawEaseHandles(
            Vector3[] nodePositions,
            float[] easeCurveValues,
            float arcValueMultiplier,
            float arcHandleRadius,
            float initialArcValue,
            float scaleHandleSize,
            Action<int, float> callback) {

            // For each path node..
            for (var i = 0; i < nodePositions.Length; i++) {
                var iTemp = i;
                DrawArcHandle(
                    easeCurveValues[i],
                    nodePositions[i],
                    arcValueMultiplier,
                    0,
                    360,
                    arcHandleRadius,
                    initialArcValue,
                    scaleHandleSize,
                    Color.red,
                    value => callback(iTemp, value));
            }
        }

        public static void DrawNodeLabels(
            Vector3[] nodeGlobalPositions,
            string[] text,
            int offsetX,
            int offsetY,
            int labelWidth,
            int labelHeight,
            GUIStyle style) {

            for (var i = 0; i < nodeGlobalPositions.Length; i++) {
                DrawNodeLabel(
                    nodeGlobalPositions[i],
                    text[i],
                    offsetX,
                    offsetY,
                    labelWidth,
                    labelHeight,
                    style);
            }
        }

        public static void DrawCustomPositionHandles(
            Vector3[] nodeGlobalPositions,
            float handleSize,
            Color curveColor,
            Action<int, Vector3> callback) {

            // Cap function used to draw handle.
            Handles.DrawCapFunction capFunction = Handles.CircleCap;

            // For each node..
            for (var i = 0; i < nodeGlobalPositions.Length; i++) {
                var handleColor = curveColor;

                // Draw position handle.
                var newGlobalPos = DrawCustomPositionHandle(
                    nodeGlobalPositions[i],
                    handleSize,
                    handleColor,
                    capFunction);

                // If node was moved..
                if (newGlobalPos != nodeGlobalPositions[i]) {
                    // Execute callback.
                    callback(i, newGlobalPos);
                }
            }
        }

        public static void DrawRemoveNodeButtons(
            Vector3[] nodePositions,
            int offsetH,
            int offsetV,
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
                    offsetH,
                    offsetV,
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

        public static void DrawRotationHandle(
            Vector3 rotationPointGlobalPosition,
            float rotationHandleSize,
            Color rotationHandleColor,
            Action<Vector3> callback) {

            var handleSize =
                HandleUtility.GetHandleSize(rotationPointGlobalPosition);
            var sphereSize = handleSize * rotationHandleSize;

            // Set handle color.
            Handles.color = rotationHandleColor;

            // Draw node's handle.
            var newGlobalPosition = Handles.FreeMoveHandle(
                rotationPointGlobalPosition,
                Quaternion.identity,
                sphereSize,
                Vector3.zero,
                Handles.SphereCap);

            if (newGlobalPosition != rotationPointGlobalPosition) {
                callback(newGlobalPosition);
            }
        }

        public static void DrawTiltingHandles(
            Vector3[] nodePositions,
            float[] tiltingCurveValues,
            float arcHandleRadius,
            float initialArcValue,
            float scaleHandleSize,
            Action<int, float> callback) {

            // Set arc value multiplier.
            // todo make a field.
            const int arcValueMultiplier = 1;

            // For each path node..
            // todo revert.
            //for (var i = 0; i < nodePositions.Length; i++) {
            for (var i = 0; i < 1; i++) {
                var iTemp = i;
                DrawArcHandle(
                    tiltingCurveValues[i],
                    nodePositions[i],
                    arcValueMultiplier,
                    -359,
                    359,
                    arcHandleRadius,
                    initialArcValue,
                    scaleHandleSize,
                    Color.green,
                    value => callback(iTemp, value));
            }
        }

        public static void DrawUpdateAllLabels(
            Vector3[] nodeGlobalPositions,
            string[] labelText,
            int offsetX,
            int offsetY,
            int labelWidth,
            int labelHeight,
            GUIStyle style) {

            DrawNodeLabels(
                nodeGlobalPositions,
                labelText,
                offsetX,
                offsetY,
                labelWidth,
                labelHeight,
                style);
        }

        /// <summary>
        ///     Draw arc handle.
        /// </summary>
        /// <param name="value">Arc value.</param>
        /// <param name="position">Arc position.</param>
        /// <param name="arcValueMultiplier">If set to 1, values will be converted to degrees in relation 1 to 1.</param>
        /// <param name="minDegrees">Lower boundary for amount of degrees that will be drawn.</param>
        /// <param name="maxDegrees">Higher boundary for amount of degrees that will be drawn.</param>
        /// <param name="scaleHandleSize"></param>
        /// <param name="handleColor">Handle color.</param>
        /// <param name="callback">Callback that will be executed when arc value changes. It takes changed value as an argument.</param>
        /// <param name="arcHandleRadius">Radius of the arc.</param>
        /// <param name="initialArcValue">When handle is close to zero and user moves the handle, this value will be set as start value.</param>
        // todo rename to DrawTiltingTool().
        private static void DrawArcHandle(
            float value,
            Vector3 position,
            float arcValueMultiplier,
            int minDegrees,
            int maxDegrees,
            float arcHandleRadius,
            float initialArcValue,
            float scaleHandleSize,
            Color handleColor,
            Action<float> callback) {

            // Original arc value.
            var arcValue = value * arcValueMultiplier;
            // Value to be drawn with arc.
            var drawedValue = arcValue % 360;
            var handleSize = HandleUtility.GetHandleSize(position);
            var arcRadius = handleSize * arcHandleRadius;

            DrawTiltingArcHandle(
                position,
                handleColor,
                drawedValue,
                arcRadius);

            var newArcValue = DrawTiltingValueHandle(arcValue,
                position, handleSize, scaleHandleSize, arcRadius, handleColor);

            SaveTiltValue(arcValueMultiplier, callback, newArcValue, arcValue);
        }

        private static void SaveTiltValue(
            float arcValueMultiplier,
            Action<float> callback,
            float newArcValue,
            float arcValue) {

            if (!Utilities.FloatsEqual(
                newArcValue,
                arcValue,
                GlobalConstants.FloatPrecision)) {

                // Convert value from degrees to value on animation curve.
                var convertedValue = newArcValue / arcValueMultiplier;

                callback(convertedValue);
            }
        }

        private static float DrawTiltingValueHandle(float arcValue, Vector3 position, float handleSize, float scaleHandleSize, float arcRadius, Color handleColor) {
            Logger.LogString("arcValue: {0}", arcValue);

            Handles.color = handleColor;

            // Set initial arc value to other than zero. If initial value
            // is zero, handle will always return zero.
            //arcValue = Math.Abs(arcValue) < MinValueThreshold
            //    ? initialArcValue
            //    : arcValue;

            var scaleSize = handleSize * scaleHandleSize;
            var newArcValue = Handles.ScaleValueHandle(
                arcValue,
                position + Vector3.forward * arcRadius
                * 1.3f,
                Quaternion.identity,
                scaleSize,
                Handles.ConeCap,
                1);
            return newArcValue;
        }

        private static void DrawTiltingArcHandle(
            Vector3 position,
            Color handleColor,
            float displayedValue,
            float arcRadius) {

            Handles.color = handleColor;

            Handles.DrawWireArc(
                position,
                Vector3.up,
                Quaternion.AngleAxis(
                    0,
                    Vector3.up) * Vector3.forward,
                //arcValue,
                displayedValue,
                arcRadius);
        }

        private static bool DrawButton(
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

        private static void DrawNodeLabel(
            Vector3 nodeGlobalPosition,
            string value,
            int offsetX,
            int offsetY,
            int labelWidth,
            int labelHeight,
            GUIStyle style) {

            // Translate node's 3d position into screen coordinates.
            var guiPoint = HandleUtility.WorldToGUIPoint(nodeGlobalPosition);

            // Create rectangle for the label.
            var labelPosition = new Rect(
                guiPoint.x + offsetX,
                guiPoint.y + offsetY,
                labelWidth,
                labelHeight);

            Handles.BeginGUI();

            // Draw label.
            GUI.Label(
                labelPosition,
                value,
                style);

            Handles.EndGUI();
        }

        private static Vector3 DrawCustomPositionHandle(
            Vector3 nodePosition,
            float handleSize,
            Color handleColor,
            Handles.DrawCapFunction capFunction) {

            // Set handle color.
            Handles.color = handleColor;

            // Get handle size.
            var movementHandleSize = HandleUtility.GetHandleSize(nodePosition);
            var sphereSize = movementHandleSize * handleSize;

            // Draw handle.
            var newPos = Handles.FreeMoveHandle(
                nodePosition,
                Quaternion.identity,
                sphereSize,
                Vector3.zero,
                capFunction);
            return newPos;
        }

        #endregion

        public static void DrawPositionHandles(
            Vector3[] nodeGlobalPositions,
            Action<int, Vector3> callback) {

            // For each node..
            for (var i = 0; i < nodeGlobalPositions.Length; i++) {
                // Draw position handle.
                var newGlobalPos = Handles.PositionHandle(
                    nodeGlobalPositions[i],
                    Quaternion.identity);

                // If node was moved..
                if (newGlobalPos != nodeGlobalPositions[i]) {
                    // Execute callback.
                    callback(i, newGlobalPos);
                }
            } 
        }

    }

}