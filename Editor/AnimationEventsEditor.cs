using UnityEditor;
using ATP.SimplePathAnimator.ReorderableList;

namespace ATP.SimplePathAnimator {

    [CustomEditor(typeof (AnimationEvents))]
    public class AnimationEventsEditor : Editor {

        private AnimationEvents Script { get; set; }

        private SerializedProperty events;
        private SerializedProperty animator;

        private void OnEnable() {
            Script = (AnimationEvents) target;

            events = serializedObject.FindProperty("events");
            animator = serializedObject.FindProperty("animator");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(animator);

            ReorderableListGUI.Title("Events");
            ReorderableListGUI.ListField(events);

            serializedObject.ApplyModifiedProperties();
        }

    }

}