using System;
using System.Globalization;
using ATP.LoggingTools;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools.AnimatorComponent {

    /// <summary>
    ///     Class responsible for drawing all on scene handles.
    /// </summary>
    public static class SceneHandles {

        /// <summary>
        ///     Minimum value below which arc handle drawer method will set the
        ///     value back to default.
        /// </summary>
        private static float MinValueThreshold = 0.01f;

        /// <summary>
        /// It means that one unit of tilt will be represented as one degree
        /// in the arc handle.
        /// </summary>
        private const int ArcValueMultiplier = 1;

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
                var displayedValue = (Mathf.Abs(value) % 360)
                                     * Mathf.Sign(value);
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
                DrawArcTool(
                    easeCurveValues[i],
                    nodePositions[i],
                    arcValueMultiplier,
                    arcHandleRadius,
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

            // For each path node..
            for (var i = 0; i < nodePositions.Length; i++) {
                var iTemp = i;
                DrawArcTool(
                    tiltingCurveValues[i],
                    nodePositions[i],
                        ArcValueMultiplier,
                    arcHandleRadius,
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

        #region DrawArcTool()

        /// <summary>
        ///     Draw arc handle.
        /// </summary>
        /// <param name="arcValue">Value passed to the tool.</param>
        /// <param name="position">Arc position.</param>
        /// <param name="scaleHandleSize"></param>
        /// <param name="handleColor">Handle color.</param>
        /// <param name="callback">Callback that will be executed when arc value changes. It takes changed value as an argument.</param>
        /// <param name="arcHandleRadius">Radius of the arc.</param>
        private static void DrawArcTool(
            float value,
            Vector3 position,
            float arcValueMultiplier,
            float arcHandleRadius,
            float scaleHandleSize,
            Color handleColor,
            Action<float> callback) {

            // Calculate value to display.
            var arcValue = value * arcValueMultiplier;
            // Limit value to 360.
            var displayedValue = arcValue % 360;

            var handleSize = HandleUtility.GetHandleSize(position);
            var arcRadius = handleSize * arcHandleRadius;

            DrawArcHandle(
                position,
                handleColor,
                displayedValue,
                arcRadius);

            var newArcValue = DrawArcScaleHandle(
                arcValue,
                position,
                scaleHandleSize,
                arcRadius,
                handleColor);

            SaveArcValue(arcValueMultiplier, callback, newArcValue, arcValue);
        }

        // todo reorganize args.
        // todo remove arcValueMultiplier arg.
        /// <summary>
        /// Save new tilting value to animation curve.
        /// </summary>
        /// <param name="callback">Pass updated value here.</param>
        /// <param name="newArcValue"></param>
        /// <param name="arcValue"></param>
        private static void SaveArcValue(
            float arcValueMultiplier,
            Action<float> callback,
            float newArcValue,
            float arcValue) {

            // todo this might be not necessary.
            if (Utilities.FloatsEqual(
                newArcValue,
                arcValue,
                GlobalConstants.FloatPrecision)) return;

            var modArcValue = arcValue % 360;
            var diff = Utilities.CalculateDifferenceBetweenAngles(modArcValue, newArcValue);
            var resultValue = arcValue + diff;

            // If diff is not zero..
            if (!Utilities.FloatsEqual(diff, 0, GlobalConstants.FloatPrecision)) {
                callback(resultValue);
            }
        }

        /// <summary>
        /// Draw handle for changing tilting value.
        /// </summary>
        /// <param name="value">Handle value.</param>
        /// <param name="position">Position to display handle.</param>
        /// <param name="scaleHandleSize">Handle size.</param>
        /// <param name="arcRadius">Position offset.</param>
        /// <param name="handleColor">Handle color.</param>
        /// <returns></returns>
        private static float DrawArcScaleHandle(
            float value,
            Vector3 position,
            float scaleHandleSize,
            float arcRadius,
            Color handleColor) {

            Handles.color = handleColor;

            // Calculate size of the scale handle.
            var handleSize = HandleUtility.GetHandleSize(position);
            var scaleSize = handleSize * scaleHandleSize;

            // Calculate displayed value.
            var handleValue = value % 360;

            // Set initial handle value. Without it, after reseting handle value,
            // the value would change really slow.
            handleValue = Math.Abs(handleValue) < MinValueThreshold
                // todo pass through arg.
                ? 5
                : handleValue;

            // Draw handle.
            var newArcValue = Handles.ScaleValueHandle(
                handleValue,
                position + Vector3.forward * arcRadius * 1.3f,
                Quaternion.identity,
                scaleSize,
                Handles.ConeCap,
                1);

            // If value was changed with handle..
            if (!Utilities.FloatsEqual(
                handleValue,
                newArcValue,
                GlobalConstants.FloatPrecision)) {
                
                // Calculate modulo from value.
                var modValue = newArcValue % 360;

                return modValue;
            }

            // Return same value.
            return value;
        }

        private static void DrawArcHandle(
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
                displayedValue,
                arcRadius);
        }

        #endregion

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
    }

}