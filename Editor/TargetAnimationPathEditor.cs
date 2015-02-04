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
            // TODO Move to separate method and make those private again.
            // TODO Use base.OnEnable() again.
            // Initialize serialized properties.
            GizmoCurveColor = serializedObject.FindProperty("gizmoCurveColor");
            skin = serializedObject.FindProperty("skin");
            exportSamplingFrequency =
                serializedObject.FindProperty("exportSamplingFrequency");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");

            Script = (TargetAnimationPath)target;

            // TODO Move to separate method.
            // Remember active scene tool.
            if (Tools.current != Tool.None) {
                LastTool = Tools.current;
                Tools.current = Tool.None;
            }

            FirstNodeOffset = new Vector3(1, -1, 0);
            LastNodeOffset = new Vector3(2, 0, 1);

            if (!Script.IsInitialized) {
                ResetPath(FirstNodeOffset, LastNodeOffset);
            }

            // Set default gizmo curve color.
            Script.GizmoCurveColor = Color.magenta;
        }
    }
}