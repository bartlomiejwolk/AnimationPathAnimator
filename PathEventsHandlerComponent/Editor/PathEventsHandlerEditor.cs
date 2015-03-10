using UnityEditor;

namespace ATP.SimplePathAnimator.PathEventsHandlerComponent {

    [CustomEditor(typeof (PathEventsHandler))]
    public class PathEventsHandlerEditor : Editor {

        private PathEventsHandler Script { get; set; }

        private void OnEnable() {
            Script = target as PathEventsHandler;
        }

    }

}
