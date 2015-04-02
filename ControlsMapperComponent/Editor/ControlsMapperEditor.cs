using ATP.AnimationPathTools.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools.ControlsMapperComponent {

    [CustomEditor(typeof (ControlsMapper))]
    public class ControlsMapperEditor : Editor {

        private SerializedProperty targetComponents;

        private ControlsMapper Script;

        private void OnEnable() {
            Script = (ControlsMapper) target;

            targetComponents = serializedObject.FindProperty("targetComponents");
        }

        public override void OnInspectorGUI() {
            DrawSourceAnimatorField();
            DrawTargetAnimatorComponentList();
        }

        private void DrawTargetAnimatorComponentList() {
            serializedObject.Update();

            ReorderableListGUI.Title("Target Animator Components");
            ReorderableListGUI.ListField(targetComponents);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSourceAnimatorField() {
            Script.Animator = (AnimatorComponent.Animator)EditorGUILayout.ObjectField(
                new GUIContent(
                    "Animator",
                    ""),
                Script.Animator,
                typeof (AnimatorComponent.Animator));
        }

    }

}
