using System;
using System.Collections.Generic;
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

        #region BUTTON OFFSETS

        private const int SmoothButtonH = 25;
        private const int SmoothButtonV = 10;
        private const int AddButtonH = 44;
        private const int AddButtonV = 10;
        private const int RemoveButtonH = 63;
        private const int RemoveButtonV = 10;
        #endregion

        #region SERIALIZED PROPERTIES

        private SerializedProperty advancedSettingsFoldout;
        private SerializedProperty exportSamplingFrequency;
        protected SerializedProperty GizmoCurveColor;
        private SerializedProperty skin;

        #endregion SERIALIZED PROPERTIES

        #region Helper Variables

        /// <summary>
        /// Scene tool that was selected when game object was first selected in
        /// the hierarchy view.
        /// </summary>
        public static Tool LastTool = Tool.None;

        /// <summary>
        /// Reference to serialized class.
        /// </summary>
        private AnimationPath script;
        private const float MovementHandleSize = 0.25f;
        private const float FirstNodeSize = 0.12f;
        private readonly Color moveAllModeColor = Color.gray;

        #endregion Helper Variables

        #region UNITY MESSAGES

        void OnDisable() {
            Tools.current = LastTool;
        }

        protected virtual void OnEnable() {
            // Initialize serialized properties.
            GizmoCurveColor = serializedObject.FindProperty("gizmoCurveColor");
            skin = serializedObject.FindProperty("skin");
            exportSamplingFrequency =
                serializedObject.FindProperty("exportSamplingFrequency");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");

            script = (AnimationPath)target;

            // Remember active scene tool.
            if (Tools.current != Tool.None) {
                LastTool = Tools.current;
                Tools.current = Tool.None;
            }
        }

        public override void OnInspectorGUI() {
            // Draw inspector GUI elements.
            DrawInspector();

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

            // Draw add node buttons.
            HandleDrawingAddButtons();

            // Draw remove node buttons.
            HandleDrawingRemoveButtons();

            // Handle drawing smooth tangent button for each node.
            HandleDrawingSmoothTangentButton();

        }

        #endregion UNITY MESSAGES

        #region DRAWING HANDLERS

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
        /// Handle drawing movement handles.
        /// </summary>
        private void HandleDrawingMovementHandles() {
            if (script.TangentMode) return;

            // Positions at which to draw movement handles.
            Vector3[] nodes = script.GetNodePositions();

            // Callback to call when a node is moved on the scene.
            Action<int, Vector3, Vector3> handlerCallback =
                DrawMovementHandlesCallbackHandler;

            // Draw handles.
            DrawMovementHandles(nodes, handlerCallback);
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

        #endregion DRAWING
        #region Drawing methods

        private void DrawAddNodeButtons(
            Vector3[] nodePositions,
            Action<int> callback,
            GUIStyle buttonStyle) {

            Handles.BeginGUI();

            // Draw add buttons for each node (except the last one).
            // Execute callback on button press.
            for (var i = 0; i < nodePositions.Length - 1; i++) {
                // Translate node's 3d position into screen coordinates.
                Vector2 guiPoint = HandleUtility.WorldToGUIPoint(
                        nodePositions[i]);

                // Draw button.
                bool buttonPressed = DrawButton(
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

        private void DrawRemoveNodeButtons(
            Vector3[] nodePositions,
            Action<int> callback,
            GUIStyle buttonStyle) {

            Handles.BeginGUI();

            // Draw remove buttons for each node except for the first and the
            // last one.
            // Execute callback on button press.
            for (int i = 1; i < nodePositions.Length - 1; i++) {
                // Translate node's 3d position into screen coordinates.
                Vector2 guiPoint = HandleUtility.WorldToGUIPoint(
                        nodePositions[i]);

                // Draw button.
                bool buttonPressed = DrawButton(
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

        private void DrawMovementHandles(
            Vector3[] nodes,
            Action<int, Vector3, Vector3> callback) {
            // For each node..
            for (int i = 0; i < nodes.Length; i++) {
                // Set handle color.
                Handles.color = script.GizmoCurveColor;
                // Set node color for Move All mode.
                if (script.MoveAllMode) {
                    Handles.color = moveAllModeColor;
                }
                // Get handle size.
                float handleSize = HandleUtility.GetHandleSize(nodes[i]);
                float sphereSize = handleSize * MovementHandleSize;
                // Decide on cap function used to draw handle.
                Handles.DrawCapFunction capFunction = Handles.SphereCap;

                // Set first node handle properties.
                if (i == 0) {
                    capFunction = Handles.DotCap;
                    sphereSize = handleSize * FirstNodeSize;
                 
                }

                // Draw handle.
                Vector3 newPos = Handles.FreeMoveHandle(
                    nodes[i],
                    Quaternion.identity,
                    sphereSize,
                    Vector3.zero,
                    capFunction);

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
        /// Draw on-scene tangent button for each path node.
        /// </summary>
        /// <param name="nodePositions">Node positions.</param>
        /// <param name="smoothButtonStyle">Button style.</param>
        /// <param name="callback">Callback called when a button is pressed.</param>
        private void DrawSmoothTangentButtons(
            Vector3[] nodePositions,
            GUIStyle smoothButtonStyle,
            Action<int> callback) {

            Handles.BeginGUI();

            // For each key..
            for (int i = 0; i < nodePositions.Length; i++) {
                // Translate node's 3d position into screen coordinates.
                Vector2 guiPoint = HandleUtility.WorldToGUIPoint(
                        nodePositions[i]);

                // Create rectangle for the "+" button.
                Rect rect = new Rect(
                        guiPoint.x + SmoothButtonH,
                        guiPoint.y + SmoothButtonV,
                        15,
                        15);

                // Draw button.
                bool buttonPressed = GUI.Button(rect, "", smoothButtonStyle);

                // If button pressed..
                if (buttonPressed) {
                    // Execute callback.
                    callback(i);
                }
            }

            Handles.EndGUI();
        }
        /// <summary>
        /// For each node in the scene draw handle that allow manipulating
        /// tangents for each of the animation curves separately.
        /// </summary>
        /// <returns>True if any handle was moved.</returns>
        private void DrawTangentHandles(
            Vector3[] nodes,
            Action<int, Vector3> callback) {

            Handles.color = script.GizmoCurveColor;

            // For each node..
            for (int i = 0; i < nodes.Length; i++) {
                float handleSize = HandleUtility.GetHandleSize(nodes[i]);
                float sphereSize = handleSize * MovementHandleSize;

                // draw node's handle.
                Vector3 newHandleValue = Handles.FreeMoveHandle(
                    nodes[i],
                    Quaternion.identity,
                    sphereSize,
                    Vector3.zero,
                    Handles.CircleCap);

                // How much tangent's value changed in this frame.
                Vector3 tangentDelta = newHandleValue - nodes[i];

                // Remember if handle was moved.
                if (tangentDelta != Vector3.zero) {
                    // Execute callback.
                    callback(i, tangentDelta);
                }
            }
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
            AddNodeBetween(nodeIndex);

            script.DistributeTimestamps();
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
                script.DistributeTimestamps();
            }
        }

        protected virtual void DrawRemoveNodeButtonsCallbackHandles(int nodeIndex) {
            // Make snapshot of the target object.
            HandleUndo();

            script.RemoveNode(nodeIndex);
            script.DistributeTimestamps();
        }

        protected virtual void DrawSmoothTangentButtonsCallbackHandler(int index) {
            // Make snapshot of the target object.
            HandleUndo();

            script.SmoothNodeTangents(index);

            script.DistributeTimestamps();
        }
        protected virtual void DrawTangentHandlesCallbackHandler(
                    int index,
                    Vector3 inOutTangent) {

            // Make snapshot of the target object.
            HandleUndo();

            script.ChangeNodeTangents(index, inOutTangent);
            script.DistributeTimestamps();
        }
        #endregion CALLBACK HANDLERS
        #region INSPECTOR
        private void DrawCreateInspectorButton() {
            if (GUILayout.Button(new GUIContent(
                "Create",
                "Create a new default Animation Path or reset to default."))) {
                // Allow undo this operation.
                HandleUndo();
                // Reset curves to its default state.
                ResetPath();
            }
        }

        private void DrawLinearInspectorButton() {
            if (GUILayout.Button(new GUIContent(
                "Linear",
                "Set tangent mode to linear for all nodePositions."))) {
                // Allow undo this operation.
                HandleUndo();

                script.SetNodesLinear();
                script.DistributeTimestamps();
            }
        }

        protected virtual void DrawSmoothInspectorButton() {
            if (GUILayout.Button(new GUIContent(
                "Smooth",
                "Use AnimationCurve.SmoothNodesTangents on every node in the path."))) {
                HandleUndo();
                script.SmoothNodesTangents();
                script.DistributeTimestamps();
            }
        }

        private void DrawExportNodesInspectorButton() {
            if (GUILayout.Button("Export Nodes")) {
                ExportNodes(exportSamplingFrequency.intValue);
            }
        }
        #endregion

        #region PRIVATE
        /// <summary>
        /// Update <c>moveAllMode</c> option with keyboard shortcut.
        /// </summary>
        private void HandleMoveAllOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != AnimationPath.MoveAllKey) return;

            // Make sure Tangent mode is disabled.
            script.TangentMode = false;

            // Toggle Move All mode.
            script.MoveAllMode = !script.MoveAllMode;
        }

        /// <summary>
        /// Toggle handles mode with key shortcut.
        /// </summary>
        private void HandleTangentModeOptionShortcut() {
                       // Return if Tangent Mode shortcut wasn't released.
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != AnimationPath.HandlesModeKey) return;

            // Make sure Move All mode is disabled.
            script.MoveAllMode = false;

            // Toggle Move All mode.
            script.TangentMode = !script.TangentMode;
        }

        private void DrawInspector() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(
                    GizmoCurveColor,
                    new GUIContent("Curve Color", ""));
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.BeginHorizontal();
            serializedObject.Update();
            DrawSmoothInspectorButton();
            DrawLinearInspectorButton();
            DrawCreateInspectorButton();
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

            DrawExportNodesInspectorButton();

            EditorGUILayout.Space();

            serializedObject.Update();
            advancedSettingsFoldout.boolValue = EditorGUILayout.Foldout(
                    advancedSettingsFoldout.boolValue,
                    new GUIContent(
                        "Advanced Settings",
                        ""));
            if (advancedSettingsFoldout.boolValue) {
                
                EditorGUILayout.PropertyField(
                        skin,
                        new GUIContent(
                            "Skin",
                            "Styles used by on-scene GUI elements."));
            }
            serializedObject.ApplyModifiedProperties();
        }
        /// <summary>
        /// Record target object state for undo.
        /// </summary>
        protected void HandleUndo() {
            Undo.RecordObject(script.AnimationCurves, "Change path");
        }

     

        protected void AddNodeBetween(int nodeIndex) {
            // Timestamp of node on which was taken action.
            float currentKeyTime = script.GetNodeTimestamp(nodeIndex);
            // Get timestamp of the next node.
            float nextKeyTime = script.GetNodeTimestamp(nodeIndex + 1);

            // Calculate timestamps for new key. It'll be placed exactly
            // between the two nodes.
            float newKeyTime =
                currentKeyTime +
                ((nextKeyTime - currentKeyTime) / 2);

            // Add node to the animation curves.
            script.CreateNodeAtTime(newKeyTime);
        }



        /// <summary>
        /// Export Animation Path nodes as transforms.
        /// </summary>
        /// <param name="exportSampling">
        /// Amount of result transforms for one meter of Animation Path.
        /// </param>
        private void ExportNodes(int exportSampling) {
            // Points to be exported.
            List<Vector3> points;

            // If exportSampling arg. is zero then export one transform for each
            // Animation Path node.
            if (exportSampling == 0) {
                // Initialize points.
                points = new List<Vector3>(script.NodesNo);

                // For each node in the path..
                for (int i = 0; i < script.NodesNo; i++) {
                    // Get it 3d position.
                    points[i] = script.GetNodePosition(i);
                }
            }
            // exportSampling not zero..
            else {
                // Initialize points array with nodes to export.
                points = script.SamplePathForPoints(exportSampling);
            }

            // Create parent GO.
            GameObject exportedPath = new GameObject("exported_path");

            // Create child GOs.
            for (int i = 0; i < points.Count; i++) {
                // Create child GO.
                GameObject node = new GameObject("Node " + i);

                // Move node under the path GO.
                node.transform.parent = exportedPath.transform;

                // Assign node local position.
                node.transform.localPosition = points[i];
            }
        }

        /// <summary>
        /// Remove all keys in animation curves and create new, default ones.
        /// </summary>
        private void ResetPath() {
            // Get scene view camera.
            Camera sceneCamera = SceneView.lastActiveSceneView.camera;
            // Get world point to place the Animation Path.
            Vector3 worldPoint = sceneCamera.transform.position
                + sceneCamera.transform.forward * 7;
            // Number of nodes to remove.
            int noOfNodesToRemove = script.NodesNo;

            // Remove all nodes.
            for (var i = 0; i < noOfNodesToRemove; i++) {
                // NOTE After each removal, next node gets index 0.
                script.RemoveNode(0);
            }

            // Calculate end point.
            Vector3 endPoint = worldPoint + new Vector3(1, 1, 1);

            // Add beginning and end points.
            script.CreateNode(0, worldPoint);
            script.CreateNode(1, endPoint);
        }
        #endregion
    }
}