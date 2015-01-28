using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools {

    [CustomEditor(typeof(TargetAnimationPath))]
    public class TargetAnimationPathEditor : AnimationPathEditor {

        protected override void OnEnable() {
            base.OnEnable();
            script = (TargetAnimationPath)target;
        }

        protected override void DrawAddNodeButtonsCallbackHandler(int nodeIndex) {
            // Make snapshot of the target object.
            script.HandleUndo();

            // Add a new node.
            script.AddNodeAuto(nodeIndex);
        }

        protected override void DrawLinearTangentModeButtonsCallbackHandler(
                    int nodeIndex) {

            // Make snapshot of the target object.
            script.HandleUndo();

            script.SetNodeLinear(nodeIndex);
        }

        protected override void DrawMovementHandlesCallbackHandler(
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
            }
        }

        protected override void DrawRemoveNodeButtonsCallbackHandles(int nodeIndex) {
            // Make snapshot of the target object.
            script.HandleUndo();

            script.RemoveNode(nodeIndex);
        }

        protected override void DrawSmoothTangentButtonsCallbackHandler(int index) {
            // Make snapshot of the target object.
            script.HandleUndo();

            script.SmoothNodeTangents(
                    index,
                    tangentWeight.floatValue);
        }

        protected override void DrawTangentHandlesCallbackHandler(
                    int index,
                    Vector3 inOutTangent) {

            // Make snapshot of the target object.
            script.HandleUndo();

            script.ChangeNodeTangents(index, inOutTangent);
        }
    }
}