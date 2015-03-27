using ATP.AnimationPathTools.AnimatorComponent;
using ATP.AnimationPathTools.EventsComponent;
using UnityEditor;

namespace ATP.AnimationPathTools {

    public static class ComponentCreator {

        [MenuItem("Assets/Create/ATP/AnimationPathTools/Animation Path")]
        private static void CreatePathAsset() {
            ScriptableObjectUtility.CreateAsset<PathData>("AnimationPath");
        }

        [MenuItem("Assets/Create/ATP/AnimationPathTools/Animator Settings")]
        private static void CreateAnimatorSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<AnimatorSettings>("AnimatorSettings");
        }

        [MenuItem("Assets/Create/ATP/AnimationPathTools/Events Settings")]
        private static void CreateAPEventsReflectionSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<EventsSettings>("EventsSettings");
        }

        [MenuItem("Component/ATP/AnimationPathTools/Animator")]
        private static void AddAPAnimatorComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(Animator));
            }
        }

        [MenuItem("Component/ATP/AnimationPathTools/Events")]
        private static void AddAPEventsComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(Events));
            }
        }
    }

}