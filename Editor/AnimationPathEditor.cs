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

        #region Composite Object

        /// <summary>
        /// Object responsible for drawing on-scene handles.
        /// </summary>
        private AnimationPathHandles animationPathHandles =
            ScriptableObject.CreateInstance<AnimationPathHandles>();

        #endregion Composite Object

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

        void OnEnable() {
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

        void OnSceneGUI() {
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
                script.HandleUndo();

                script.SetNodesLinear();
                script.DistributeNodeSpeedValues();
            }
            if (GUILayout.Button(new GUIContent(
                           "Smooth",
                "Use AnimationCurve.SmoothNodesTangents on every node in the path."))) {

                script.HandleUndo();
                script.SmoothNodesTangents(tangentWeight.floatValue);
                script.DistributeNodeSpeedValues();
            }
            if (GUILayout.Button(new GUIContent(
                "Reset",
                "Create a new default Animation Path or reset to default."))) {

                // Allow undo this operation. TODO Check if this works.
                script.HandleUndo();

                // Reset curves to its default state.
                script.ResetPath();
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
            animationPathHandles.DrawAddNodeButtons(
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
                animationPathHandles.DrawLabelForEachNode(
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
                animationPathHandles.DrawLabelForEachNode(
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
            animationPathHandles.DrawLinearTangentModeButtons(
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
            animationPathHandles.DrawMovementHandles(nodes, handlerCallback);
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
                animationPathHandles.DrawLabelForEachNode(
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
                animationPathHandles.DrawLabelForEachNode(
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
            animationPathHandles.DrawRemoveNodeButtons(
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
            animationPathHandles.DrawSmoothTangentButtons(
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
            animationPathHandles.DrawSpeedLabels(
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
            animationPathHandles.DrawTangentHandles(
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
            animationPathHandles.DrawTimestampLabels(
                    nodes,
                    timestamps,
                    style);
        }
        #endregion DRAWING

        #region CALLBACK HANDLERS

        protected virtual void DrawAddNodeButtonsCallbackHandler(int nodeIndex) {
            // Make snapshot of the target object.
            script.HandleUndo();

            // Add a new node.
            script.AddNodeAuto(nodeIndex);

            script.DistributeNodeSpeedValues();
        }

        protected virtual void DrawLinearTangentModeButtonsCallbackHandler(
                    int nodeIndex) {

            // Make snapshot of the target object.
            script.HandleUndo();

            script.SetNodeLinear(nodeIndex);
            script.DistributeNodeSpeedValues();
        }

        protected virtual void DrawMovementHandlesCallbackHandler(
                    int movedNodeIndex,
                    Vector3 position,
                    Vector3 moveDelta) {

            // Make snapshot of the target object.
            script.HandleUndo();

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
            script.HandleUndo();

            script.RemoveNode(nodeIndex);
            script.DistributeNodeSpeedValues();
        }

        protected virtual void DrawSmoothTangentButtonsCallbackHandler(int index) {
            // Make snapshot of the target object.
            script.HandleUndo();

            script.SmoothNodeTangents(
                    index,
                    tangentWeight.floatValue);

            script.DistributeNodeSpeedValues();
        }
        protected virtual void DrawTangentHandlesCallbackHandler(
                    int index,
                    Vector3 inOutTangent) {

            // Make snapshot of the target object.
            script.HandleUndo();

            script.ChangeNodeTangents(index, inOutTangent);
            script.DistributeNodeSpeedValues();
        }
        #endregion CALLBACK HANDLERS
    }
}