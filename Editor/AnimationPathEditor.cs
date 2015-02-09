using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        #region CONSTANS

        private const int AddButtonH = 44;
        private const int AddButtonV = 10;
        //private const float FirstNodeSize = 0.12f;
        private const float MoveAllModeSize = 0.15f;
        private const float MovementHandleSize = 0.12f;
        private const int RemoveButtonH = 63;
        private const int RemoveButtonV = 10;
        private const int SmoothButtonH = 25;
        private const int SmoothButtonV = 10;
        private const float TangentHandleSize = 0.25f;

        #endregion CONSTANS

        #region FIELDS
        private readonly Color moveAllModeColor = Color.red;

        /// <summary>
        /// Scene tool that was selected when game object was first selected in
        /// the hierarchy view.
        /// </summary>
        public static Tool LastTool = Tool.None;

        /// <summary>
        /// Reference to serialized class.
        /// </summary>
        public AnimationPath Script { get; protected set; }

        #endregion FIELDS

        #region SERIALIZED PROPERTIES

        protected SerializedProperty GizmoCurveColor;
        protected SerializedProperty advancedSettingsFoldout;
        protected SerializedProperty exportSamplingFrequency;
        protected SerializedProperty skin;
        //protected SerializedProperty rotationCurves;

        public Vector3 FirstNodeOffset { get; protected set; }
        public Vector3 LastNodeOffset { get; protected set; }
        #endregion SERIALIZED PROPERTIES

        #region UNITY MESSAGES

        public override void OnInspectorGUI() {
            // Draw inspector GUI elements.
            DrawInspector();

            // Update scene.
            SceneView.RepaintAll();
        }

        protected virtual void OnEnable() {
            // Initialize serialized properties.
            GizmoCurveColor = serializedObject.FindProperty("gizmoCurveColor");
            skin = serializedObject.FindProperty("skin");
            exportSamplingFrequency =
                serializedObject.FindProperty("exportSamplingFrequency");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");
            //rotationCurves = serializedObject.FindProperty("rotationCurves");

            Script = (AnimationPath)target;

            // Remember active scene tool.
            if (Tools.current != Tool.None) {
                LastTool = Tools.current;
                Tools.current = Tool.None;
            }

            // Initialize public properties.
            FirstNodeOffset = new Vector3(0, 0, 0);
            LastNodeOffset = new Vector3(1, 1, 1);

            if (!Script.IsInitialized) {
                ResetPath(FirstNodeOffset, LastNodeOffset);
            }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        protected void OnSceneGUI() {
            //Debug.Log(drawRotationHandle.boolValue);
            // Log error if inspector GUISkin filed is empty.
            if (Script.Skin == null) {
                Script.MissingReferenceError(
                        "Skin",
                        "Skin field cannot be empty. You will find default " +
                        "GUISkin in the Animation PathTools/GUISkin folder");
            }

            // Disable scene tool.
            Tools.current = Tool.None;

            // Return if curves are not initialized or scene controls are
            // disabled from the inspector.
            if (Script.NodesNo < 2 || !Script.SceneControls) {
                return;
            }

            // Change handles mode. Each on-scene node has a handle to update
            // node's attribute. Based on inspector option/key pressed, this
            // will change handles mode. See HandlesMode enum for available
            // tools.
            HandleTangentModeOptionShortcut();

            // Update "Move All" inspector option with keyboard shortcut.
            HandleMoveAllOptionShortcut();

            // Handle drawing movement handles.
            HandleDrawingMovementHandles();

            // Handle displaying tangent handles. Tangent handles allows
            // changing nodes' in/out tangents.
            HandleDrawingTangentHandles();

            // Draw add node buttons.
            HandleDrawingAddButtons();

            // Draw remove node buttons.
            HandleDrawingRemoveButtons();

            // Handle drawing smooth tangent button for each node.
            HandleDrawingSmoothTangentButton();

        }
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnDisable() {
            Tools.current = LastTool;
        }

        #endregion UNITY MESSAGES

        #region DRAWING HANDLERS


        private void HandleDrawingAddButtons() {
            // Get positions at which to draw movement handles.
            var nodePositions = Script.GetNodePositions();

            // Get style for add button.
            var addButtonStyle = Script.Skin.GetStyle(
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
            if (Script.TangentMode) return;

            // Positions at which to draw movement handles.
            // TODO Move to DrawmovementHandles().
            var nodes = Script.GetNodePositions();

            // Callback to call when a node is moved on the scene.
            Action<int, Vector3, Vector3> handlerCallback =
                DrawMovementHandlesCallbackHandler;

            // Draw handles.
            DrawMovementHandles(nodes, handlerCallback);
        }

        private void HandleDrawingRemoveButtons() {
            // Positions at which to draw movement handles.
            var nodes = Script.GetNodePositions();

            // Get style for add button.
            var removeButtonStyle = Script.Skin.GetStyle(
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
            var smoothButtonStyle = Script.Skin.GetStyle(
                        "SmoothButton");

            // Positions at which to draw movement handles.
            var nodePositions = Script.GetNodePositions();

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
            if (!Script.TangentMode) return;

            // Positions at which to draw tangent handles.
            var nodes = Script.GetNodePositions();

            // Callback: After handle is moved, update animation curves.
            Action<int, Vector3> updateTangentsCallback =
                DrawTangentHandlesCallbackHandler;

            // Draw tangent handles.
            DrawTangentHandles(
                nodes,
                updateTangentsCallback);
        }

        #endregion DRAWING HANDLERS

        #region DRAWING METHODS


        private void DrawAddNodeButtons(
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

        // TODO Rename to DrawPositionHandles().
        private void DrawMovementHandles(
            Vector3[] nodes,
            Action<int, Vector3, Vector3> callback) {
            // For each node..
            for (var i = 0; i < nodes.Length; i++) {
                // Set handle color.
                Handles.color = Script.GizmoCurveColor;

                // Set node color for Move All mode.
                //if (Script.MoveAllMode) {
                //    Handles.color = moveAllModeColor;
                //}

                // Get handle size.
                var handleSize = HandleUtility.GetHandleSize(nodes[i]);
                //var sphereSize = handleSize * MovementHandleSize;
                var sphereSize = handleSize * MovementHandleSize;

                // Cap function used to draw handle.
                Handles.DrawCapFunction capFunction = Handles.CircleCap;

                // In Move All mode..
                if (Script.MoveAllMode) {
                    //capFunction = Handles.DotCap;
                    //capFunction = Handles.SphereCap;
                    Handles.color = moveAllModeColor;
                    sphereSize = handleSize * MoveAllModeSize;
                }

                // Set first node handle properties.
                //if (i == 0) {
                //    capFunction = Handles.CircleCap;
                //    sphereSize = handleSize * MoveAllModeSize;

                //}

                // Draw handle.
                var newPos = Handles.FreeMoveHandle(
                    nodes[i],
                    Quaternion.identity,
                    sphereSize,
                    Vector3.zero,
                    capFunction);

                // If node was moved..
                if (newPos != nodes[i]) {
                    // Calculate movement delta.
                    var moveDelta = newPos - nodes[i];

                    // Execute callback.
                    callback(i, newPos, moveDelta);
                }
            }
        }

        private void DrawRemoveNodeButtons(
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

        /// <summary>
        /// Draw on-scene tangent button for each path node.
        /// </summary>
        /// <param name="nodePositions">Node positions.</param>
        /// <param name="smoothButtonStyle">Button style.</param>
        /// <param name="callback">
        /// Callback called when a button is pressed.
        /// </param>
        private void DrawSmoothTangentButtons(
            Vector3[] nodePositions,
            GUIStyle smoothButtonStyle,
            Action<int> callback) {

            Handles.BeginGUI();

            // For each key..
            for (var i = 0; i < nodePositions.Length; i++) {
                // Translate node's 3d position into screen coordinates.
                var guiPoint = HandleUtility.WorldToGUIPoint(
                        nodePositions[i]);

                // Create rectangle for the "+" button.
                var rect = new Rect(
                        guiPoint.x + SmoothButtonH,
                        guiPoint.y + SmoothButtonV,
                        15,
                        15);

                // Draw button.
                var buttonPressed = GUI.Button(rect, "", smoothButtonStyle);

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

            Handles.color = Script.GizmoCurveColor;

            // For each node..
            for (var i = 0; i < nodes.Length; i++) {
                var handleSize = HandleUtility.GetHandleSize(nodes[i]);
                var sphereSize = handleSize * TangentHandleSize;

                // draw node's handle.
                var newHandleValue = Handles.FreeMoveHandle(
                    nodes[i],
                    Quaternion.identity,
                    sphereSize,
                    Vector3.zero,
                    Handles.CircleCap);

                // How much tangent's value changed in this frame.
                var tangentDelta = newHandleValue - nodes[i];

                // Remember if handle was moved.
                if (tangentDelta != Vector3.zero) {
                    // Execute callback.
                    callback(i, tangentDelta);
                }
            }
        }

        #endregion Drawing methods

        #region CALLBACK HANDLERS


        protected virtual void DrawAddNodeButtonsCallbackHandler(int nodeIndex) {
            // Make snapshot of the target object.
            HandleUndo();

            // Add a new node.
            AddNodeBetween(nodeIndex);

            Script.DistributeTimestamps();

            Script.OnPathChanged();
        }

        protected virtual void DrawMovementHandlesCallbackHandler(
                    int movedNodeIndex,
                    Vector3 position,
                    Vector3 moveDelta) {

            // Make snapshot of the target object.
            HandleUndo();

            // If Move All mode enabled, move all nodes.
            if (Script.MoveAllMode) {
                Script.MoveAllNodes(moveDelta);
            }
            // Move single node.
            else {
                Script.MoveNodeToPosition(movedNodeIndex, position);
                Script.DistributeTimestamps();
            }

            Script.OnPathChanged();
        }

        protected virtual void DrawRemoveNodeButtonsCallbackHandles(int nodeIndex) {
            // Make snapshot of the target object.
            HandleUndo();

            Script.RemoveNode(nodeIndex);
            Script.DistributeTimestamps();

            Script.OnPathChanged();
        }

        protected virtual void DrawSmoothTangentButtonsCallbackHandler(int index) {
            // Make snapshot of the target object.
            HandleUndo();

            Script.SmoothNodeTangents(index);

            Script.DistributeTimestamps();

            Script.OnPathChanged();
        }

        protected virtual void DrawTangentHandlesCallbackHandler(
                    int index,
                    Vector3 inOutTangent) {

            // Make snapshot of the target object.
            HandleUndo();

            Script.ChangeNodeTangents(index, inOutTangent);
            Script.DistributeTimestamps();

            Script.OnPathChanged();
        }

        #endregion CALLBACK HANDLERS

        #region INSPECTOR

        protected virtual void DrawSmoothInspectorButton() {
            if (GUILayout.Button(new GUIContent(
                "Smooth",
                "Use AnimationCurve.SmoothNodesTangents on every node in the path."))) {
                HandleUndo();
                Script.SmoothNodesTangents();
                Script.DistributeTimestamps();
            }
        }

        private void DrawCreateInspectorButton() {
            if (GUILayout.Button(new GUIContent(
                "Create",
                "Create a new default Animation Path or reset to default."))) {
                // Allow undo this operation.
                HandleUndo();
                // Reset curves to its default state.
                ResetPath(FirstNodeOffset, LastNodeOffset);
            }
        }

        private void DrawExportNodesInspectorButton() {
            if (GUILayout.Button("Export Nodes")) {
                ExportNodes(exportSamplingFrequency.intValue);
            }
        }

        private void DrawLinearInspectorButton() {
            if (GUILayout.Button(new GUIContent(
                "Linear",
                "Set tangent mode to linear for all nodePositions."))) {
                // Allow undo this operation.
                HandleUndo();

                Script.SetNodesLinear();
                Script.DistributeTimestamps();
            }
        }

        #endregion INSPECTOR

        #region PRIVATE

        protected void AddNodeBetween(int nodeIndex) {
            // Timestamp of node on which was taken action.
            var currentKeyTime = Script.GetNodeTimestamp(nodeIndex);
            // Get timestamp of the next node.
            var nextKeyTime = Script.GetNodeTimestamp(nodeIndex + 1);

            // Calculate timestamps for new key. It'll be placed exactly
            // between the two nodes.
            var newKeyTime =
                currentKeyTime +
                ((nextKeyTime - currentKeyTime) / 2);

            // Add node to the animation curves.
            Script.CreateNodeAtTime(newKeyTime);
        }

        /// <summary>
        /// Record target object state for undo.
        /// </summary>
        protected void HandleUndo() {
            Undo.RecordObject(Script.AnimationCurves, "Change path");
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
            var moveAllModeTooltip = String.Format(
                "Move all nodes at once. " +
                "Toggle it with {0} key.",
                AnimationPath.MoveAllKey);

            // Disable this control if Tangent Mode is enabled.
            GUI.enabled = !Script.TangentMode;
            Script.MoveAllMode = GUILayout.Toggle(
                Script.MoveAllMode,
                new GUIContent(
                    "Move All Mode",
                    moveAllModeTooltip));
            GUI.enabled = true;

            // Tooltip for handlesMode property.
            var tangentModeTooltip = String.Format(
                "Display handles that allow changing node tangents. " +
                "Toggle it with {0} key.",
                AnimationPath.HandlesModeKey);

            GUI.enabled = !Script.MoveAllMode;
            Script.TangentMode = GUILayout.Toggle(
                Script.TangentMode,
                new GUIContent(
                    "Tangent Mode",
                    tangentModeTooltip));
            GUI.enabled = true;

            Script.SceneControls = GUILayout.Toggle(
                Script.SceneControls,
                new GUIContent(
                    "Scene Controls",
                    "Toggle on-scene node controls."));

            //EditorGUILayout.PropertyField(rotationCurves);

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
        /// Export Animation Path nodes as transforms.
        /// </summary>
        /// <param name="exportSampling">
        /// Amount of result transforms for one meter of Animation Path.
        /// </param>
        private void ExportNodes(int exportSampling) {
            // Points to be exported.
            List<Vector3> points;

            // If exportSampling arg. is zero then export one transform for
            // each Animation Path node.
            if (exportSampling == 0) {
                // Initialize points.
                points = new List<Vector3>(Script.NodesNo);

                // For each node in the path..
                for (var i = 0; i < Script.NodesNo; i++) {
                    // Get it 3d position.
                    points[i] = Script.GetNodePosition(i);
                }
            }
            // exportSampling not zero..
            else {
                // Initialize points array with nodes to export.
                points = Script.SamplePathForPoints(exportSampling);
            }

            // Create parent GO.
            var exportedPath = new GameObject("exported_path");

            // Create child GOs.
            for (var i = 0; i < points.Count; i++) {
                // Create child GO.
                var node = new GameObject("Node " + i);

                // Move node under the path GO.
                node.transform.parent = exportedPath.transform;

                // Assign node local position.
                node.transform.localPosition = points[i];
            }
        }

        /// <summary>
        /// Update <c>moveAllMode</c> option with keyboard shortcut.
        /// </summary>
        private void HandleMoveAllOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != AnimationPath.MoveAllKey) return;

            // Make sure Tangent mode is disabled.
            Script.TangentMode = false;

            // Toggle Move All mode.
            Script.MoveAllMode = !Script.MoveAllMode;
        }

        /// <summary>
        /// Toggle handles mode with key shortcut.
        /// </summary>
        private void HandleTangentModeOptionShortcut() {
            // Return if Tangent Mode shortcut wasn't released.
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != AnimationPath.HandlesModeKey) return;

            // Make sure Move All mode is disabled.
            Script.MoveAllMode = false;

            // Toggle Move All mode.
            Script.TangentMode = !Script.TangentMode;
        }

        /// <summary>
        /// Remove all keys in animation curves and create new, default ones.
        /// </summary>
        // TODO Refactor.
        protected void ResetPath(
            Vector3 firstNodeOffset,
            Vector3 lastNodeOffset) {

            // Get scene view camera.
            var sceneCamera = SceneView.lastActiveSceneView.camera;
            // Get world point to place the Animation Path.
            // TODO Create constant field.
            var worldPoint = sceneCamera.transform.position
                + sceneCamera.transform.forward * 7;
            // Number of nodes to remove.
            var noOfNodesToRemove = Script.NodesNo;

            // TODO Move to AnimationCurves class.
            // Remove all nodes.
            for (var i = 0; i < noOfNodesToRemove; i++) {
                // NOTE After each removal, next node gets index 0.
                Script.RemoveNode(0);
            }

            // Calculate end point.
            var endPoint = worldPoint + lastNodeOffset;

            // Add beginning and end points.
            Script.CreateNode(0, worldPoint + firstNodeOffset);
            Script.CreateNode(1, endPoint);
            
            // Raise event.
            Script.OnPathChanged();
            Script.OnPathReset();
        }

        #endregion PRIVATE
    }
}