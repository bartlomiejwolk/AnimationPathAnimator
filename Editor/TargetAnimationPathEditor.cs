using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools {

    [CustomEditor(typeof(TargetAnimationPath))]
    public class TargetAnimationPathEditor : AnimationPathEditor {

        protected override void OnEnable() {
            base.OnEnable();

            script = (TargetAnimationPath)target;

            // Set default gizmo curve color.
            script.GizmoCurveColor = Color.magenta;
        }

        protected override void DrawAddNodeButtonsCallbackHandler(int nodeIndex) {
            // Make snapshot of the target object.
            HandleUndo();

            // Add a new node.
            AddNodeBetween(nodeIndex);
        }

        //protected override void DrawLinearTangentModeButtonsCallbackHandler(
        //            int nodeIndex) {

        //    // Make snapshot of the target object.
        //    HandleUndo();

        //    script.SetNodeLinear(nodeIndex);
        //}

        protected override void DrawMovementHandlesCallbackHandler(
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
            }
        }

        protected override void DrawRemoveNodeButtonsCallbackHandles(int nodeIndex) {
            // Make snapshot of the target object.
            HandleUndo();

            script.RemoveNode(nodeIndex);
        }

        protected override void DrawSmoothTangentButtonsCallbackHandler(int index) {
            // Make snapshot of the target object.
            HandleUndo();

            script.SmoothNodeTangents(index);
        }

        protected override void DrawSmoothInspectorButton() {
            if (GUILayout.Button(new GUIContent(
                "Smooth",
                "Use AnimationCurve.SmoothNodesTangents on every node in the path."))) {
                HandleUndo();
                script.SmoothNodesTangents();
            }
        }
        protected override void DrawTangentHandlesCallbackHandler(
                    int index,
                    Vector3 inOutTangent) {

            // Make snapshot of the target object.
            HandleUndo();

            script.ChangeNodeTangents(index, inOutTangent);
        }
    }
}