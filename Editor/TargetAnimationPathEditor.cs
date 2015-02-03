using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools {

    [CustomEditor(typeof(TargetAnimationPath))]
    public class TargetAnimationPathEditor : AnimationPathEditor {

        protected override void DrawAddNodeButtonsCallbackHandler(int nodeIndex) {
            // Make snapshot of the target object.
            HandleUndo();

            // Add a new node.
            AddNodeBetween(nodeIndex);
        }

        protected override void DrawMovementHandlesCallbackHandler(
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
            }
        }

        protected override void DrawRemoveNodeButtonsCallbackHandles(int nodeIndex) {
            // Make snapshot of the target object.
            HandleUndo();

            Script.RemoveNode(nodeIndex);
        }

        protected override void DrawSmoothInspectorButton() {
            if (GUILayout.Button(new GUIContent(
                "Smooth",
                "Use AnimationCurve.SmoothNodesTangents on every node in the path."))) {
                HandleUndo();
                Script.SmoothNodesTangents();
            }
        }

        protected override void DrawSmoothTangentButtonsCallbackHandler(int index) {
            // Make snapshot of the target object.
            HandleUndo();

            Script.SmoothNodeTangents(index);
        }

        protected override void DrawTangentHandlesCallbackHandler(
                    int index,
                    Vector3 inOutTangent) {

            // Make snapshot of the target object.
            HandleUndo();

            Script.ChangeNodeTangents(index, inOutTangent);
        }

        protected override void OnEnable() {
            base.OnEnable();

            Script = (TargetAnimationPath)target;

            // Set default gizmo curve color.
            Script.GizmoCurveColor = Color.magenta;
        }
    }
}