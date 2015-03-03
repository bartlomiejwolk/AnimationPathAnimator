using UnityEditor;
using ATP.SimplePathAnimator.ReorderableList;

namespace ATP.SimplePathAnimator.PathEvents {

    [CustomEditor(typeof (AnimationEvents))]
    public class AnimationEventsEditor : Editor {

        private AnimationEvents Script { get; set; }

        private SerializedProperty nodeEvents;
        private SerializedProperty animator;

        private void OnEnable() {
            Script = (AnimationEvents) target;

            nodeEvents = serializedObject.FindProperty("nodeEvents");
            animator = serializedObject.FindProperty("animator");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(animator);

            ReorderableListGUI.Title("Events");
            ReorderableListGUI.ListField(nodeEvents);

            serializedObject.ApplyModifiedProperties();
        }

    }

}