using OneDayGame.LoggingTools;
using System;
using UnityEditor;
using UnityEngine;

namespace OneDayGame.AnimationPathTools {

    /// It's responsible for drawing on-scene handles and other info for an
    /// AnimationPath class.
    public class AnimationPathHandles : ScriptableObject {
        #region Drawing methods

        public void DrawAddNodeButtons(
            Vector3[] nodePositions,
            Action<int> callback,
            GUIStyle buttonStyle) {

            Handles.BeginGUI();

            bool buttonPressed = false;

            // Draw add buttons for each node. Execute callback on button
            // press.
            for (int i = 0; i < nodePositions.Length; i++) {
                // Translate node's 3d position into screen coordinates.
                Vector2 guiPoint = HandleUtility.WorldToGUIPoint(
                        nodePositions[i]);

                // Draw button.
                buttonPressed = DrawButton(
                    guiPoint,
                    82,
                    25,
                    15,
                    15,
                    buttonStyle);

                // Execute callback.
                if (buttonPressed == true) {
                    callback(i);
                }
            }

            Handles.EndGUI();
        }

        public void DrawRemoveNodeButtons(
            Vector3[] nodePositions,
            Action<int> callback,
            GUIStyle buttonStyle) {

            Handles.BeginGUI();

            bool buttonPressed = false;

            // Draw add buttons for each node. Execute callback on button
            // press.
            for (int i = 0; i < nodePositions.Length; i++) {
                // Translate node's 3d position into screen coordinates.
                Vector2 guiPoint = HandleUtility.WorldToGUIPoint(
                        nodePositions[i]);

                // Draw button.
                buttonPressed = DrawButton(
                    guiPoint,
                    64,
                    25,
                    15,
                    15,
                    buttonStyle);

                // Execute callback.
                if (buttonPressed == true) {
                    callback(i);
                }
            }

            Handles.EndGUI();
        }

        public void DrawMovementHandles(
            Vector3[] nodes,
            Action<int, Vector3, Vector3> callback) {

            Vector3 newPos;

            // For each node..
            for (int i = 0; i < nodes.Length; i++) {
                // draw node's handle.
                newPos = Handles.PositionHandle(
                        nodes[i],
                        Quaternion.identity);

                // If node was moved..
                if (newPos != nodes[i]) {
                    // Calculate movement delta.
                    Vector3 moveDelta = newPos - nodes[i];

                    // Execute callback.
                    callback(i, newPos, moveDelta);
                }
            }
        }

        /// <summary>
        /// Draw smooth tangent button for each node on the scene.
        /// </summary>
        /// <param name="curves">Animation curves.</param>
        /// <param name="smoothWeight">
        /// Weight parameter for the Unity's
        /// <c>AnimationCurve.SmoothTangent</c> method.
        /// </param>
        /// <param name="smoothButtonStyle">Style of the button.</param>
        /// <returns>If any button was pressed.</returns>
        public void DrawSmoothTangentButtons(
            Vector3[] nodePositions,
            GUIStyle smoothButtonStyle,
            Action<int> callback) {

            Handles.BeginGUI();

            bool buttonPressed;

            // For each key..
            for (int i = 0; i < nodePositions.Length; i++) {
                // Translate node's 3d position into screen coordinates.
                Vector2 guiPoint = HandleUtility.WorldToGUIPoint(
                        nodePositions[i]);

                // Create rectangle for the "+" button.
                Rect rect = new Rect(
                        guiPoint.x + 100,
                        guiPoint.y + 25,
                        15,
                        15);

                // Draw button.
                buttonPressed = GUI.Button(rect, "", smoothButtonStyle);

                // If button pressed..
                if (buttonPressed == true) {
                    // Execute callback.
                    callback(i);
                }
            }

            Handles.EndGUI();
        }

        /// <summary>
        /// Draw linear tangent mode button for each node on the scene.
        /// </summary>
        /// <param name="curves">Animation curves.</param>
        /// <param name="buttonStyle">Style of the button.</param>
        /// <returns>If any button was pressed.</returns>
        public void DrawLinearTangentModeButtons(
            Vector3[] nodePositions,
            GUIStyle buttonStyle,
            Action<int> callback) {

            Handles.BeginGUI();

            bool buttonPressed;

            // For each key..
            for (int i = 0; i < nodePositions.Length; i++) {
                // Translate node's 3d position into screen coordinates.
                Vector2 guiPoint = HandleUtility.WorldToGUIPoint(
                        nodePositions[i]);

                // Create rectangle for the button.
                Rect rect = new Rect(
                        guiPoint.x + 120,
                        guiPoint.y + 25,
                        15,
                        15);

                // Draw button.
                buttonPressed = GUI.Button(rect, "X", buttonStyle);

                // If button pressed..
                if (buttonPressed == true) {
                    // Execute callback.
                    callback(i);
                }
            }
            Handles.EndGUI();
        }

        /// <summary>Draw speed label for each node on the scene.</summary>
        /// <remarks>
        /// Each node's speed value is the speed of a transform animated along
        /// that path.
        /// </remarks>
        /// <param name="curves"></param>
        /// <param name="labelStyle"></param>
        public void DrawSpeedLabels(
                Vector3[] nodePositions,
                float[] speedValues,
                GUIStyle labelStyle) {

            Handles.BeginGUI();

            // For each node.. First node has no speed label.
            for (int i = 1; i < nodePositions.Length; i++) {
                // Translate node's 3d position into screen coordinates.
                Vector2 guiPoint = HandleUtility.WorldToGUIPoint(
                        nodePositions[i]);

                // Create rectangle for the label.
                Rect rect = new Rect(guiPoint.x, guiPoint.y + 60, 40, 20);

                // Label content.
                string label = string.Format("{0:0.0} m/s", speedValues[i]);

                // Draw label on the scene.
                GUI.Box(
                        rect,
                        label,
                        labelStyle);
            }

            Handles.EndGUI();
        }

        /// <summary>
        /// For each node in the scene draw handle that allow manipulating
        /// tangents for each of the animation curves separately.
        /// </summary>
        /// <returns>True if any handle was moved.</returns>
        public void DrawTangentHandles(
            Vector3[] nodes,
            Action<int, Vector3> callback) {

            // For each node..
            for (int i = 0; i < nodes.Length; i++) {
                // draw node's handle.
                Vector3 newHandleValue = Handles.PositionHandle(
                        nodes[i],
                        Quaternion.identity);

                // How much tangent's value changed in this frame.
                Vector3 tangentDelta = newHandleValue - nodes[i];

                // Remember if handle was moved.
                if (tangentDelta != Vector3.zero) {
                    // Execute callback.
                    callback(i, tangentDelta);
                }
            }
        }

        /// <summary>
        /// This method allows displaying a string next to each node on the
        /// scene.
        /// </summary>
        /// <param name="labelStyle">Label style.</param>
        /// <param name="labelText">Label text.</param>
        /// <param name="relativeXPos">
        /// Horizontal label position relative to the node's position.
        /// </param>
        /// <param name="relativeYPos">
        /// Vertical label position relative to the node's position.
        /// </param>
        /// <param name="width">Label width.</param>
        /// <param name="height">Label height.</param>
        public void DrawLabelForEachNode(
            Vector3[] positions,
            GUIStyle labelStyle,
            string labelText,
            int relativeXPos,
            int relativeYPos,
            int width,
            int height) {

            Handles.BeginGUI();

            // For each node..
            for (int i = 0; i < positions.Length; i++) {
                // Translate node's 3d position into screen coordinates.
                Vector2 guiPoint = HandleUtility.WorldToGUIPoint(
                        positions[i]);

                // Create rectangle for the "+" button.
                Rect rect = new Rect(
                        guiPoint.x + relativeXPos,
                        guiPoint.y + relativeYPos,
                        width,
                        height);

                // Draw label.
                GUI.Label(rect, labelText, labelStyle);
            }

            Handles.EndGUI();
        }

        /// <summary>Draw information labels for each node.</summary>
        /// <param name="curves">
        /// Animation curves from which animation path is created.
        /// </param>
        /// <param name="labelStyle">
        /// Style of the label displayed in the scene view.
        /// </param>
        public void DrawTimestampLabels(
                Vector3[] nodes,
                float[] timestamps,
                GUIStyle labelStyle) {

            // Draw GUI elements inside scene.
            Handles.BeginGUI();

            // For each node..
            for (int i = 0; i < nodes.Length; i++) {
                // Translate node's 3d position into screen coordinates.
                Vector2 guiPoint = HandleUtility.WorldToGUIPoint(
                        nodes[i]);

                // Create rectangle for the label.
                Rect rect = new Rect(guiPoint.x, guiPoint.y + 20, 40, 20);

                // Label content.
                string label = string.Format("{0:0.000}", timestamps[i]);

                // Draw label on the scene.
                GUI.Box(
                        rect,
                        label,
                        labelStyle);
            }

            Handles.EndGUI();
        }

        public bool DrawButton(
            Vector2 position,
            int relativeXPos,
            int relativeYPos,
            int width,
            int height,
            GUIStyle style,
            string buttonText = "") {

            // Create rectangle for the "+" button.
            Rect rectAdd = new Rect(
                    position.x + relativeXPos,
                    position.y + relativeYPos,
                    width,
                    height);

            // Draw the "+" button.
            bool addButtonPressed = GUI.Button(rectAdd, buttonText, style);

            return addButtonPressed;
        }

        #endregion Drawing methods
    }
}