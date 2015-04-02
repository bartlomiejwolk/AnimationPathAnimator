using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools.ControlsMapperComponent {

    [CustomEditor(typeof (ControlsMapper))]
    public class ControlsMapperEditor : Editor {

        private ControlsMapper Script;

        private void OnEnable() {
            Script = (ControlsMapper) target;
        }

        public override void OnInspectorGUI() {
            DrawAnimatorField();
        }

        private void DrawAnimatorField() {
            Script.Animator = (AnimatorComponent.Animator)EditorGUILayout.ObjectField(
                new GUIContent(
                    "Animator",
                    ""),
                Script.Animator,
                typeof (AnimatorComponent.Animator));
        }

    }

}
