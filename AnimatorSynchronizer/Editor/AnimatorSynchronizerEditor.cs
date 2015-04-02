using ATP.AnimationPathTools.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools.AnimatorSynchronizerComponent {

    [CustomEditor(typeof (AnimatorSynchronizer))]
    public class AnimatorSynchronizerEditor : Editor {

        private SerializedProperty targetComponents;

        private AnimatorSynchronizer Script;

        private void OnEnable() {
            Script = (AnimatorSynchronizer) target;

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
