using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools {

    /// <summary>
    /// Editor for AnimationPathBuilder class.
    /// </summary>
    /// <remarks>
    /// It is composed of AnimationPathHandles class that is responsible for
    /// drawing handles, buttons and labels.
    /// </remarks>
    [CustomEditor(typeof(AnimationPathBuilder))]
    public class AnimationPathBuilderEditor : Editor {

        #region CONSTANS
        //private const float ResetPathCameraDistance = 20f;

        private const int AddButtonH = 25;
        private const int AddButtonV = 10;
        //private const float FirstNodeSize = 0.12f;
        private const float MoveAllModeSize = 0.15f;
        private const float MovementHandleSize = 0.12f;
        private const int RemoveButtonH = 44;
        private const int RemoveButtonV = 10;
        //private const int SmoothButtonH = 25;
        //private const int SmoothButtonV = 10;
        //private const float TangentHandleSize = 0.25f;

        /// <summary>
        /// Key shortcut to enable handles mode.
        /// </summary>
        /// <remarks>
        /// Handles mode will change only while key is pressed.
        /// </remarks>
        //public const KeyCode TangentModeKey = KeyCode.J;

        /// <summary>
        /// Key shortcut to toggle movement mode.
        /// </summary>
        /// <remarks>
        /// Movement mode will change only while key is pressed.
        /// </remarks>
        public const KeyCode MoveAllKey = KeyCode.U;
        public const KeyCode MoveSingleModeKey = KeyCode.Y;
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
        public AnimationPathBuilder Script { get; protected set; }

        #endregion FIELDS

        #region SERIALIZED PROPERTIES

        private SerializedProperty GizmoCurveColor;
        private SerializedProperty advancedSettingsFoldout;
        private SerializedProperty exportSamplingFrequency;
        private SerializedProperty skin;
        private SerializedProperty pathData;

        //private Vector3 firstNodeOffset = new Vector3(0, 0, 0);
        //private Vector3 secondNodeOffset = new Vector3(1, -2, 0.5f);
        private readonly Vector3 lastNodeOffset = new Vector3(0, 0, 1);
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
            pathData = serializedObject.FindProperty("pathData");
            //handlesMode = serializedObject.FindProperty("handlesMode");
            //rotationCurves = serializedObject.FindProperty("rotationCurves");
            //tangentMode = serializedObject.FindProperty("tangentMode");

            Script = (AnimationPathBuilder)target;

            // Remember active scene tool.
            if (Tools.current != Tool.None) {
                LastTool = Tools.current;
                Tools.current = Tool.None;
            }

            //if (!Script.IsInitialized) {
            //    ResetPath();
            //}
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        protected void OnSceneGUI() {
            //Debug.Log(handlesMode.enumValueIndex);
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
            //if (Script.NodesNo < 2 || !Script.SceneControls) {
            //    return;
            //}

            // Change handles mode. Each on-scene node has a handle to update
            // node's attribute. Based on inspector option/key pressed, this
            // will change handles mode. See HandleMode enum for available
            // tools.
            //HandleTangentModeOptionShortcut();

            // Update "Move All" inspector option with keyboard shortcut.
            HandleMoveAllOptionShortcut();

            HandleMoveSingleModeShortcut();

            // Return if path asset does not exist.
            if (Script.PathData == null) return;

            // Handle drawing movement handles.
            HandleDrawingPositionHandles();

            // Handle displaying tangent handles. Tangent handles allows
            // changing nodes' in/out tangents.
            //HandleDrawingTangentHandles();

            // Draw add node buttons.
            HandleDrawingAddButtons();

            // Draw remove node buttons.
            HandleDrawingRemoveButtons();

            // Handle drawing smooth tangent button for each node.
            //HandleDrawingSmoothTangentButton();

        }
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnDisable() {
            Tools.current = LastTool;
        }

        #endregion UNITY MESSAGES
        #region DRAWING HANDLERS
        private void HandleDrawingAddButtons() {
            // Get positions at which to draw movement handles.
            var nodePositions = Script.GetNodeGlobalPositions();

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
        private void HandleDrawingPositionHandles() {
            // Callback to call when a node is moved on the scene.
            Action<int, Vector3, Vector3> handlerCallback =
                DrawPositionHandlesCallbackHandler;

            // Draw handles.
            DrawPositionHandles(handlerCallback);
        }

        private void HandleDrawingRemoveButtons() {
            // Positions at which to draw movement handles.
            var nodes = Script.GetNodeGlobalPositions();

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
        //private void HandleDrawingSmoothTangentButton() {
        //    // Get button style.
        //    var smoothButtonStyle = Script.Skin.GetStyle(
        //                "SmoothButton");

        //    // Positions at which to draw movement handles.
        //    var nodePositions = Script.GetNodePositions();

        //    // Callback to smooth a node after smooth node button was pressed.
        //    Action<int> smoothNodeCallback =
        //        DrawSmoothTangentButtonsCallbackHandler;

        //    // Draw button.
        //    DrawSmoothTangentButtons(
        //        nodePositions,
        //        smoothButtonStyle,
        //        smoothNodeCallback);
        //}

        /// <summary>
        /// Handle drawing tangent handles.
        /// </summary>
        //private void HandleDrawingTangentHandles() {
        //    if (handlesMode.enumValueIndex !=
        //        (int)AnimationPathBuilderHandleMode.Tangent) return;

        //    // Positions at which to draw tangent handles.
        //    var nodes = Script.GetNodePositions();

        //    // Callback: After handle is moved, update animation curves.
        //    Action<int, Vector3> updateTangentsCallback =
        //        DrawTangentHandlesCallbackHandler;

        //    // Draw tangent handles.
        //    DrawTangentHandles(
        //        nodes,
        //        updateTangentsCallback);
        //}

        #endregion DRAWING HANDLERS

        #region OTHER HANDLERS
        private void HandleMoveSingleModeShortcut() {
            // Return if Tangent Mode shortcut wasn't released.
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != MoveSingleModeKey) return;

            Script.HandleMode = AnimationPathBuilderHandleMode.MoveSingle;
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Update <c>moveAllMode</c> option with keyboard shortcut.
        /// </summary>
        private void HandleMoveAllOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != MoveAllKey) return;

            Script.HandleMode = AnimationPathBuilderHandleMode.MoveAll;
            serializedObject.ApplyModifiedProperties();
        }

        private void HandleTangentModeChange() {
            // Update path node tangents.
            if (Script.TangentMode == AnimationPathBuilderTangentMode.Smooth) {
                Script.SmoothAllNodeTangents();
            }
            else if (Script.TangentMode == AnimationPathBuilderTangentMode.Linear) {
                Script.SetNodesLinear();
            }
        }

        private void HandleSmoothTangentMode() {
            if (Script.TangentMode == AnimationPathBuilderTangentMode.Smooth) {
                Script.SmoothAllNodeTangents();
            }
        }

        private void HandleLinearTangentMode() {
            if (Script.TangentMode == AnimationPathBuilderTangentMode.Linear) {
                Script.SetNodesLinear();
            }
        }

        private void HandleMoveAllHandleMove(Vector3 moveDelta) {
            if (Script.HandleMode == AnimationPathBuilderHandleMode.MoveAll) {
                Script.OffsetNodePositions(moveDelta);
            }
        }

        private void HandleMoveSingleHandleMove(int movedNodeIndex, Vector3 position) {
            if (Script.HandleMode == AnimationPathBuilderHandleMode.MoveSingle) {

                Script.MoveNodeToPosition(movedNodeIndex, position);
                Script.PathData.DistributeTimestamps();

                HandleSmoothTangentMode();
                HandleLinearTangentMode();
            }
        }
        #endregion

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

        private void DrawPositionHandles(
            Action<int, Vector3, Vector3> callback) {

            // Node global positions.
            var nodes = Script.GetNodePositions(true);

            // Cap function used to draw handle.
            Handles.DrawCapFunction capFunction = Handles.CircleCap;

            // For each node..
            for (var i = 0; i < nodes.Length; i++) {
                // Draw position handle.
                var newPos = DrawPositionHandle(nodes[i], capFunction);

                // If node was moved..
				if (newPos != nodes[i]) {
                    // Calculate node old local position.
                    var oldNodeLocalPosition =
                        Script.transform.InverseTransformPoint(nodes[i]);

                    // Calculate node new local position.
                    var newNodeLocalPosition =
                        Script.transform.InverseTransformPoint(newPos);

                    // Calculate movement delta.
                    var moveDelta = newNodeLocalPosition - oldNodeLocalPosition;

                    // Execute callback.
					callback(i, newNodeLocalPosition, moveDelta);
                }
            }
        }

        private Vector3 DrawPositionHandle(
            Vector3 nodePosition,
            Handles.DrawCapFunction capFunction) {

            // Set handle color.
            Handles.color = Script.GizmoCurveColor;

            // Get handle size.
            var handleSize = HandleUtility.GetHandleSize(nodePosition);
            var sphereSize = handleSize*MovementHandleSize;

            // In Move All mode..
            if (Script.HandleMode == AnimationPathBuilderHandleMode.MoveAll) {
                Handles.color = moveAllModeColor;
                sphereSize = handleSize*MoveAllModeSize;
            }

            // Draw handle.
            var newPos = Handles.FreeMoveHandle(
                nodePosition,
                Quaternion.identity,
                sphereSize,
                Vector3.zero,
                capFunction);
            return newPos;
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
        //private void DrawSmoothTangentButtons(
        //    Vector3[] nodePositions,
        //    GUIStyle smoothButtonStyle,
        //    Action<int> callback) {

        //    Handles.BeginGUI();

        //    // For each key..
        //    for (var i = 0; i < nodePositions.Length; i++) {
        //        // Translate node's 3d position into screen coordinates.
        //        var guiPoint = HandleUtility.WorldToGUIPoint(
        //                nodePositions[i]);

        //        // Create rectangle for the "+" button.
        //        var rect = new Rect(
        //                guiPoint.x + SmoothButtonH,
        //                guiPoint.y + SmoothButtonV,
        //                15,
        //                15);

        //        // Draw button.
        //        var buttonPressed = GUI.Button(rect, "", smoothButtonStyle);

        //        // If button pressed..
        //        if (buttonPressed) {
        //            // Execute callback.
        //            callback(i);
        //        }
        //    }

        //    Handles.EndGUI();
        //}

        /// <summary>
        /// For each node in the scene draw handle that allow manipulating
        /// tangents for each of the animation curves separately.
        /// </summary>
        /// <returns>True if any handle was moved.</returns>
        //private void DrawTangentHandles(
        //    Vector3[] nodes,
        //    Action<int, Vector3> callback) {

        //    Handles.color = Script.GizmoCurveColor;

        //    // For each node..
        //    for (var i = 0; i < nodes.Length; i++) {
        //        var handleSize = HandleUtility.GetHandleSize(nodes[i]);
        //        var sphereSize = handleSize * TangentHandleSize;

        //        // draw node's handle.
        //        var newHandleValue = Handles.FreeMoveHandle(
        //            nodes[i],
        //            Quaternion.identity,
        //            sphereSize,
        //            Vector3.zero,
        //            Handles.CircleCap);

        //        // How much tangent's value changed in this frame.
        //        var tangentDelta = newHandleValue - nodes[i];

        //        // Remember if handle was moved.
        //        if (tangentDelta != Vector3.zero) {
        //            // Execute callback.
        //            callback(i, tangentDelta);
        //        }
        //    }
        //}

        #endregion Drawing methods

        #region CALLBACK HANDLERS


        protected virtual void DrawAddNodeButtonsCallbackHandler(int nodeIndex) {
            // Make snapshot of the target object.
            Undo.RecordObject(Script.PathData, "Change path");

            // Add a new node.
            AddNodeBetween(nodeIndex);

            Script.PathData.DistributeTimestamps();

            if (Script.TangentMode == AnimationPathBuilderTangentMode.Smooth) {
                Script.SmoothAllNodeTangents();
            }
            else if (Script.TangentMode == AnimationPathBuilderTangentMode.Linear) {
                Script.SetNodesLinear();
            }
        }

        protected virtual void DrawPositionHandlesCallbackHandler(
            int movedNodeIndex,
            Vector3 position,
            Vector3 moveDelta) {

            Undo.RecordObject(Script.PathData, "Change path");

            HandleMoveAllHandleMove(moveDelta);
            HandleMoveSingleHandleMove(movedNodeIndex, position);
        }
        protected virtual void DrawRemoveNodeButtonsCallbackHandles(int nodeIndex) {
            Undo.RecordObject(Script.PathData, "Change path");

            Script.RemoveNode(nodeIndex);
            Script.PathData.DistributeTimestamps();

            if (Script.TangentMode == AnimationPathBuilderTangentMode.Smooth) {
                Script.SmoothAllNodeTangents();
            }
            else if (Script.TangentMode == AnimationPathBuilderTangentMode.Linear) {
                Script.SetNodesLinear();
            }
        }

        protected virtual void DrawSmoothTangentButtonsCallbackHandler(int index) {
            Undo.RecordObject(Script.PathData, "Change path");

            Script.SmoothSingleNodeTangents(index);
            Script.PathData.DistributeTimestamps();
        }

        #endregion CALLBACK HANDLERS

        #region INSPECTOR

        protected virtual void DrawSmoothInspectorButton() {
            if (GUILayout.Button(new GUIContent(
                "Smooth",
                "Use AnimationCurve.SmoothAllNodeTangents on every node in the path."))) {

                Undo.RecordObject(Script.PathData, "Change path");
                Script.SmoothAllNodeTangents();
                Script.PathData.DistributeTimestamps();
            }
        }

        private void DrawResetPathInspectorButton() {
            if (GUILayout.Button(new GUIContent(
                "Reset Path",
                "Reset path to default."))) {
                // Allow undo this operation.
                Undo.RecordObject(Script.PathData, "Change path");

                // Reset curves to its default state.
                ResetPath();
            }
        }

        private void DrawExportControls() {
            EditorGUILayout.BeginHorizontal();
            serializedObject.Update();
            EditorGUILayout.PropertyField(
                exportSamplingFrequency,
                new GUIContent(
                    "Export Sampling",
                    "Number of points to export for 1 m of the curve. " +
                    "If set to 0, it'll export only keys defined in " +
                    "the curve."));
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Export")) {
                ExportNodes(exportSamplingFrequency.intValue);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawLinearInspectorButton() {
            if (GUILayout.Button(new GUIContent(
                "Linear",
                "Set tangent mode to linear for all nodePositions."))) {
                // Allow undo this operation.
                Undo.RecordObject(Script.PathData, "Change path");

                Script.SetNodesLinear();
                Script.PathData.DistributeTimestamps();
            }
        }

        #endregion INSPECTOR

        #region PRIVATE

        protected void AddNodeBetween(int nodeIndex) {
            // Timestamp of node on which was taken action.
            var currentKeyTime = Script.PathData.GetNodeTimestamp(nodeIndex);
            // Get timestamp of the next node.
            var nextKeyTime = Script.PathData.GetNodeTimestamp(nodeIndex + 1);

            // Calculate timestamps for new key. It'll be placed exactly
            // between the two nodes.
            var newKeyTime =
                currentKeyTime +
                ((nextKeyTime - currentKeyTime) / 2);

            // Add node to the animation curves.
            Script.PathData.CreateNodeAtTime(newKeyTime);
        }
        private void DrawInspector() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(
                pathData,
                new GUIContent(
                    "Path Asset",
                    ""));
            serializedObject.ApplyModifiedProperties();

            serializedObject.Update();
            EditorGUILayout.PropertyField(
                    GizmoCurveColor,
                    new GUIContent("Curve Color", ""));
            serializedObject.ApplyModifiedProperties();

            Script.HandleMode =
                (AnimationPathBuilderHandleMode)EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Handle Mode",
                    ""),
                Script.HandleMode);

            // Remember current tangent mode.
            var prevTangentMode = Script.TangentMode;
            // Draw tangent mode dropdown.
            Script.TangentMode =
                (AnimationPathBuilderTangentMode) EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Tangent Mode",
                    ""),
                    Script.TangentMode);
            // Update gizmo curve is tangent mode changed.
            if (Script.TangentMode != prevTangentMode) HandleTangentModeChange();

            DrawResetPathInspectorButton();

            EditorGUILayout.Space();

            // Tooltip for moveAllMode property.
            var moveAllModeTooltip = String.Format(
                "Move all nodes at once. " +
                "Toggle it with {0} key.",
                MoveAllKey);

            EditorGUILayout.Space();

            DrawExportControls();

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

            if (GUI.changed) EditorUtility.SetDirty(Script);
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
                    points[i] = Script.PathData.GetNodePosition(i);
                }
            }
            // exportSampling not zero..
            else {
                // Initialize points array with nodes to export.
                points = Script.PathData.AnimatedObjectPath.SamplePathForPoints(
                    exportSampling);
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
        /// Remove all keys in animation curves and create new, default ones.
        /// </summary>
        protected void ResetPath() {
            // Get scene view camera.
            //var sceneCamera = SceneView.lastActiveSceneView.camera;
            // Get world point to place the Animation Path.
            //var worldPoint = sceneCamera.transform.position;
                //+ sceneCamera.transform.forward * ResetPathCameraDistance;

			//Script.transform.localPosition = Vector3.zero;

            // First node position.
            var firstNodePos = Script.transform.localPosition;
            // Set y to 0.
            firstNodePos = new Vector3(firstNodePos.x, 0, firstNodePos.z);

            // Last node position.
            var lastNodePos = firstNodePos + lastNodeOffset;
            // Set y to 0.
            lastNodePos = new Vector3(lastNodePos.x, 0, lastNodePos.z);

            Script.RemoveAllNodes();

            // Create beginning and end nodes.
            Script.PathData.AnimatedObjectPath.CreateNewNode(0, firstNodePos);
            Script.PathData.CreateNode(0, lastNodePos);

            Script.this_PathReset();
        }

        #endregion PRIVATE
    }
}