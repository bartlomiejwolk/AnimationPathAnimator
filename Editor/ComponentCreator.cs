using ATP.AnimationPathTools.AnimatorComponent;
using ATP.AnimationPathTools.AnimatorEventsComponent;
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

        [MenuItem("Assets/Create/ATP/AnimationPathTools/AnimatorEvents Settings")]
        private static void CreateAPEventsReflectionSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<AnimatorEventsSettings>("AnimatorEventsSettings");
        }

        [MenuItem("Component/ATP/AnimationPathTools/Animator")]
        private static void AddAPAnimatorComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(Animator));
            }
        }

        [MenuItem("Component/ATP/AnimationPathTools/AnimatorEvents")]
        private static void AddAPEventsComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(AnimatorEvents));
            }
        }
    }

}