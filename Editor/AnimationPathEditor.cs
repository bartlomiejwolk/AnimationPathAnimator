using System;
using ATP.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools {

    /// <summary>
    /// Editor for AnimationPath class.
    /// </summary>
    /// <remarks>
    /// It is composed of AnimationPathHandles class that is responsible for
    /// drawing handles, buttons and labels.
    /// </remarks>
    [CustomEditor(typeof(AnimationPath))]
    public class AnimationPathEditor : Editor {

        #region Keyboard Keys

        /// <summary>
        /// If handles mode key is pressed in this frame.
        /// </summary>
        private bool handlesModeKeyPressed = false;

        /// <summary>
        /// If move all mode key is pressed in this frame.
        /// </summary>
        private bool moveAllKeyPressed = false;

        /// <summary>
        /// If handles mode key was pressed in the previous frame.
        /// </summary>
        private bool prevHandlesModeKeyValue = false;

        /// <summary>
        /// If move all mode key was pressed in the previous frame.
        /// </summary>
        private bool prevMoveAllKeyValue = false;

        #endregion Keyboard Keys

        #region SERIALIZED PROPERTIES

        private SerializedProperty advancedSettingsFoldout;
        private SerializedProperty exportSamplingFrequency;
        private SerializedProperty gizmoCurveColor;
        // TODO Rename to curveSamplingFrequency
        private SerializedProperty samplingFrequency;
        private SerializedProperty skin;
        // TODO Make it a const field.
        protected SerializedProperty tangentWeight;

        #endregion SERIALIZED PROPERTIES

        #region Helper Variables

        /// <summary>
        /// Scene tool that was selected when game object was first selected in
        /// the hierarchy view.
        /// </summary>
        private Tool lastTool = Tool.None;

        /// <summary>
        /// Reference to serialized class.
        /// </summary>
        protected AnimationPath script;

        #endregion Helper Variables

        #region UNITY MESSAGES

        void OnDisable() {
            Tools.current = lastTool;
        }

        protected virtual void OnEnable() {
            // Initialize serialized properties.
            gizmoCurveColor = serializedObject.FindProperty("gizmoCurveColor");
            skin = serializedObject.FindProperty("skin");
            samplingFrequency =
                serializedObject.FindProperty("gizmoCurveSamplingFrequency");
            exportSamplingFrequency =
                serializedObject.FindProperty("exportSamplingFrequency");
            tangentWeight = serializedObject.FindProperty("tangentWeight");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");

            script = (AnimationPath)target;
            lastTool = Tools.current;
            Tools.current = Tool.None;
        }

        public override void OnInspectorGUI() {
            // Draw inspector GUI elements.
            DrawInspectorGUI();

            // Update scene.
            SceneView.RepaintAll();
        }

        protected void OnSceneGUI() {
            // Log error if inspector GUISkin filed is empty.
            if (script.Skin == null) {
                script.MissingReferenceError(
                        "Skin",
                        "Skin field cannot be empty. You will find default " +
                        "GUISkin in the Animation PathTools/GUISkin folder");
            }

            // Return if curves are not initialized or scene controls are
            // disabled from the inspector.
            if (script.NodesNo < 2 || !script.SceneControls) {
                return;
            }

            // Update shortcut keys state for this frame. TODO Move to a
            // separate method.
            AnimationPathUtilities.UpdateKeyboardKey(
                AnimationPath.MoveAllKey,
                ref moveAllKeyPressed,
                ref prevMoveAllKeyValue);

            AnimationPathUtilities.UpdateKeyboardKey(
                AnimationPath.HandlesModeKey,
                ref handlesModeKeyPressed,
                ref prevHandlesModeKeyValue);

            // Change handles mode.
            // Each on-scene node has a handle to update node's attribute.
            // Based on inspector option/key pressed, this will change handles
            // mode. See HandlesMode enum for available tools.
            HandleTangentModeOptionShortcut();

            // Update "Move All" inspector option with keyboard shortcut.
            HandleMoveAllOptionShortcut();

            // Handle drawing movement handles.
            HandleDrawingMovementHandles();

            // Handle displaying tangent handles.
            // Tangent handles allows changing nodes' in/out tangents.
            HandleDrawingTangentHandles();

            // Handle drawing for each node an indicator of currently active
            // handles mode.
            HandleDrawingHandlesModeIndicator();

            // Handle drawing for each node an indicator of currently active
            // movement mode.
            HandleDrawingMovementModeIndicator();

            // Handle drawing for each node timestamp label.
            //HandleDrawingTimestampLabels();

            // Handle drawing speed label for each node.
            //HandleDrawingSpeedLabels();

            // Draw add node buttons.
            HandleDrawingAddButtons();

            // Draw remove node buttons.
            HandleDrawingRemoveButtons();

            // Handle drawing smooth tangent button for each node.
            HandleDrawingSmoothTangentButton();

            // Handle drawing button that changes node's tangent mode to
            // linear.
            HandleDrawingLinearTangentModeButtons();
        }

        #endregion UNITY MESSAGES

        #region PRIVATE METHODS

        private void DrawInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(
                    gizmoCurveColor,
                    new GUIContent("Curve Color", ""));
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.BeginHorizontal();
            serializedObject.Update();
            if (GUILayout.Button(new GUIContent(
                "Linear",
                "Set tangent mode to linear for all nodePositions."))) {

                // Allow undo this operation.
                HandleUndo();

                script.SetNodesLinear();
                script.DistributeNodeSpeedValues();
            }
            if (GUILayout.Button(new GUIContent(
                           "Smooth",
                "Use AnimationCurve.SmoothNodesTangents on every node in the path."))) {

                HandleUndo();
                script.SmoothNodesTangents(tangentWeight.floatValue);
                script.DistributeNodeSpeedValues();
            }
            if (GUILayout.Button(new GUIContent(
                "Reset",
                "Create a new default Animation Path or reset to default."))) {

                // Allow undo this operation. TODO Check if this works.
                HandleUndo();

                // Get scene view camera.
                Camera sceneCamera = SceneView.lastActiveSceneView.camera;
                // Get world point to place the Animation Path.
                Vector3 worldPoint = sceneCamera.transform.position
                    + sceneCamera.transform.forward * 7;

                // Reset curves to its default state.
                script.ResetPath(worldPoint);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Tooltip for moveAllMode property.
            string moveAllModeTooltip = String.Format(
                "If enabled, you can move with mouse all nodePositions at once. " +
                "Toggle it with {0} key.",
                AnimationPath.MoveAllKey);

            // Disable this control if Tangent Mode is enabled.
            GUI.enabled = !script.TangentMode;
            script.MoveAllMode = GUILayout.Toggle(
                script.MoveAllMode,
                new GUIContent(
                    "Move All Mode",
                    moveAllModeTooltip));
            GUI.enabled = true;

            // Tooltip for handlesMode property.
            string tangentModeTooltip = String.Format(
                "If enabled, the on-scene handles will change node's tangents." +
                "Enable it temporarily with {0} key.",
                AnimationPath.HandlesModeKey);

            GUI.enabled = !script.MoveAllMode;
            script.TangentMode = GUILayout.Toggle(
                script.TangentMode,
                new GUIContent(
                    "Tangent Mode",
                    tangentModeTooltip));
            GUI.enabled = true;

            script.SceneControls = GUILayout.Toggle(
                script.SceneControls,
                new GUIContent(
                    "Scene Controls",
                    "Toggle displaying on-scene node controls."));

            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(
                    exportSamplingFrequency,
                    new GUIContent(
                        "Export Sampling",
                        "Number of points to export for 1 m of the curve. " +
                        "If set to 0, it'll export only keys defined in " +
                        "the curve."));
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Export Nodes")) {
                script.ExportNodes(exportSamplingFrequency.intValue);
            }

            EditorGUILayout.Space();

            serializedObject.Update();
            advancedSettingsFoldout.boolValue = EditorGUILayout.Foldout(
                    advancedSettingsFoldout.boolValue,
                    new GUIContent(
                        "Advanced Settings",
                        ""));
            if (advancedSettingsFoldout.boolValue) {
                EditorGUILayout.PropertyField(
                        samplingFrequency,
                        new GUIContent(
                            "Curve Sampling",
                            "Number of points to draw 1 m of gizmo curve."));
                EditorGUILayout.PropertyField(
                        skin,
                        new GUIContent(
                            "Skin",
                            "Styles used by on-scene GUI elements."));
            }
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Update <c>moveAllMode</c> option with keyboard shortcut.
        /// </summary>
        private void HandleMoveAllOptionShortcut() {
            // Don't allow changing Movement mode when Tangent mode is already
            // enabled.
            if (script.TangentMode) return;

            // If was pressed..
            if (moveAllKeyPressed && !prevMoveAllKeyValue) {
                // Toggle move all mode.
                script.MoveAllMode = !script.MoveAllMode;
            }

            // If key was released..
            if (!moveAllKeyPressed && prevMoveAllKeyValue) {
                // Toggle move all mode.
                script.MoveAllMode = !script.MoveAllMode;
            }
        }

        /// <summary>
        /// Toggle handles mode with key shortcut.
        /// </summary>
        private void HandleTangentModeOptionShortcut() {
            // Don't allow changing Handles mode when Move All mode is already
            // enabled.
            if (script.MoveAllMode) return;

            // If modifier key was pressed, toggle handles mode.
            if (handlesModeKeyPressed && !prevHandlesModeKeyValue) {
                // Enable Tangent Mode.
                script.TangentMode = !script.TangentMode;
            }

            // If key was released..
            if (!handlesModeKeyPressed && prevHandlesModeKeyValue) {
                // Disable Tangent Mode.
                script.TangentMode = !script.TangentMode;
            }
        }

        /// <summary>
        /// Record target object state for undo.
        /// </summary>
        // Remove this method. TODO Move undo implementation to AnimationPath
        // class.
        public void HandleUndo() {
            Undo.RecordObject(script.AnimationCurves, "Change path");
        }

        #endregion PRIVATE METHODS

        #region DRAWING

        private void HandleDrawingAddButtons() {
            // Get positions at which to draw movement handles.
            Vector3[] nodePositions = script.GetNodePositions();

            // Get style for add button.
            GUIStyle addButtonStyle = script.Skin.GetStyle(
                        "AddButton");

            // Callback executed after add button was pressed.
            Action<int> callbackHandler = DrawAddNodeButtonsCallbackHandler;

            // Draw add node buttons.
            DrawAddNodeButtons(
                nodePositions,
                callbackHandler,
                addButtonStyle);
        }

        /// <summary>
        /// Handle drawing handles mode indicator.
        /// </summary>
        private void HandleDrawingHandlesModeIndicator() {
            // Tangent mode indicator style;
            GUIStyle style = script.Skin.GetStyle(
                        "TangentModeIndicator");

            // Positions at which to draw movement handles.
            Vector3[] nodes = script.GetNodePositions();

            // Draw label for Movement mode.
            if (!script.TangentMode) {
                DrawLabelForEachNode(
                    nodes,
                    style,
                    "M",
                    30,
                    6,
                    25,
                    25);
            }
            // Draw label for Tangent mode.
            else {
                DrawLabelForEachNode(
                    nodes,
                    style,
                    "T",
                    30,
                    6,
                    25,
                    25);
            }
        }

        /// <summary>
        /// Handle drawing linear tangent mode button.
        /// </summary>
        private void HandleDrawingLinearTangentModeButtons() {
            // Get button style.
            GUIStyle buttonStyle = script.Skin.GetStyle(
                        "LinearTangentModeButton");

            // Positions at which to draw movement handles.
            Vector3[] nodePositions = script.GetNodePositions();

            // Callback to smooth a node after smooth node button was pressed.
            Action<int> setNodeLinearCallback =
                DrawLinearTangentModeButtonsCallbackHandler;

            // Draw button.
            DrawLinearTangentModeButtons(
                nodePositions,
                buttonStyle,
                setNodeLinearCallback);
        }

        /// <summary>
        /// Handle drawing movement handles.
        /// </summary>
        private void HandleDrawingMovementHandles() {
            if (script.TangentMode) return;

            // Positions at which to draw movement handles.
            Vector3[] nodes = script.GetNodePositions();

            // Callback to call when a node is moved on the scene.
            Action<int, Vector3, Vector3> handlerCallback =
                (index, newPosition, moveDelta) =>
                    DrawMovementHandlesCallbackHandler(
                        index,
                        newPosition,
                        moveDelta);

            // Draw handles.
            DrawMovementHandles(nodes, handlerCallback);
        }

        /// <summary>
        /// Handle drawing Move mode indicator.
        /// </summary>
        private void HandleDrawingMovementModeIndicator() {
            // Draw label for Move All mode. TODO Add this condition to one
            // above.
            if (script.MoveAllMode) {
                GUIStyle style = script.Skin.GetStyle("MoveAllModeIndicator");

                // Positions at which to draw movement handles.
                Vector3[] nodes = script.GetNodePositions();

                // Draw labels for all nodes.
                DrawLabelForEachNode(
                    nodes,
                    style,
                    "A",
                    20,
                    6,
                    25,
                    25);
            }
            // Draw label for Move Single mode.
            else {
                GUIStyle style = script.Skin.GetStyle("MoveSingleModeIndicator");

                // Positions at which to draw movement handles.
                Vector3[] nodes = script.GetNodePositions();

                // Draw labels for all nodes.
                DrawLabelForEachNode(
                    nodes,
                    style,
                    "S",
                    20,
                    6,
                    25,
                    25);
            }
        }

        private void HandleDrawingRemoveButtons() {
            // Positions at which to draw movement handles.
            Vector3[] nodes = script.GetNodePositions();

            // Get style for add button.
            GUIStyle removeButtonStyle = script.Skin.GetStyle(
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

        /// <summary>
        /// Handle drawing smooth tangent button.
        /// </summary>
        private void HandleDrawingSmoothTangentButton() {
            // Get button style.
            GUIStyle smoothButtonStyle = script.Skin.GetStyle(
                        "SmoothButton");

            // Positions at which to draw movement handles.
            Vector3[] nodePositions = script.GetNodePositions();

            // Callback to smooth a node after smooth node button was pressed.
            Action<int> smoothNodeCallback =
                DrawSmoothTangentButtonsCallbackHandler;

            // Draw button.
            DrawSmoothTangentButtons(
                nodePositions,
                smoothButtonStyle,
                smoothNodeCallback);
        }

        /// <summary>
        /// Handle all actions related to drawing speed labels.
        /// </summary>
        // TODO Remove before release.
        private void HandleDrawingSpeedLabels() {
            // Return if animation curves are not initialized or if there's not
            // enough nodes to calculate speed. TODO Replace with a call to
            // method in AnimationPath.
            if (script.NodesNo < 2) {
                return;
            }

            // Timestamp label style.
            GUIStyle style = script.Skin.GetStyle("SpeedLabel");

            // Positions at which to draw movement handles.
            Vector3[] nodePositions = script.GetNodePositions();

            // Get speed values.
            float[] speedValues = script.GetSpeedValues();

            // Draw speed labels.
            DrawSpeedLabels(
                    nodePositions,
                    speedValues,
                    style);
        }

        /// <summary>
        /// Handle drawing tangent handles.
        /// </summary>
        private void HandleDrawingTangentHandles() {
            if (!script.TangentMode) return;

            // Positions at which to draw tangent handles.
            Vector3[] nodes = script.GetNodePositions();

            // Callback: After handle is moved, update animation curves.
            Action<int, Vector3> updateTangentsCallback =
                DrawTangentHandlesCallbackHandler;

            // Draw tangent handles.
            DrawTangentHandles(
                nodes,
                updateTangentsCallback);
        }

        /// <summary>
        /// Handle drawing timestamp labels.
        /// </summary>
        // TODO Remove before release.
        private void HandleDrawingTimestampLabels() {
            // Timestamp label style.
            GUIStyle style = script.Skin.GetStyle("TimestampLabel");

            // Positions at which to draw movement handles.
            Vector3[] nodes = script.GetNodePositions();

            // Nodes' timestamps.
            float[] timestamps = script.GetNodeTimestamps();

            // Draw timestamp labels.
            DrawTimestampLabels(
                    nodes,
                    timestamps,
                    style);
        }
        #endregion DRAWING
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
        #region CALLBACK HANDLERS

        protected virtual void DrawAddNodeButtonsCallbackHandler(int nodeIndex) {
            // Make snapshot of the target object.
            HandleUndo();

            // Add a new node.
            script.AddNodeAuto(nodeIndex);

            script.DistributeNodeSpeedValues();
        }

        protected virtual void DrawLinearTangentModeButtonsCallbackHandler(
                    int nodeIndex) {

            // Make snapshot of the target object.
            HandleUndo();

            script.SetNodeLinear(nodeIndex);
            script.DistributeNodeSpeedValues();
        }

        protected virtual void DrawMovementHandlesCallbackHandler(
                    int movedNodeIndex,
                    Vector3 position,
                    Vector3 moveDelta) {

            // Make snapshot of the target object.
            HandleUndo();

            // If Move All mode enabled, move all nodes.
            if (script.MoveAllMode) {
                script.MoveAllNodes(moveDelta);
            }
            // Move single node.
            else {
                script.MoveNodeToPosition(movedNodeIndex, position);
                script.DistributeNodeSpeedValues();
            }
        }

        protected virtual void DrawRemoveNodeButtonsCallbackHandles(int nodeIndex) {
            // Make snapshot of the target object.
            HandleUndo();

            script.RemoveNode(nodeIndex);
            script.DistributeNodeSpeedValues();
        }

        protected virtual void DrawSmoothTangentButtonsCallbackHandler(int index) {
            // Make snapshot of the target object.
            HandleUndo();

            script.SmoothNodeTangents(
                    index,
                    tangentWeight.floatValue);

            script.DistributeNodeSpeedValues();
        }
        protected virtual void DrawTangentHandlesCallbackHandler(
                    int index,
                    Vector3 inOutTangent) {

            // Make snapshot of the target object.
            HandleUndo();

            script.ChangeNodeTangents(index, inOutTangent);
            script.DistributeNodeSpeedValues();
        }
        #endregion CALLBACK HANDLERS
    }
}