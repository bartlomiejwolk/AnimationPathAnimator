using ATP.AnimationPathTools.AnimatorComponent;
using ATP.AnimationPathTools.EventsComponent;
using UnityEditor;

namespace ATP.AnimationPathTools {

    public static class ComponentCreator {

        //[MenuItem("Assets/Create/ATP/AnimationPathAnimator/Animator SettingsAsset")]
        //private static void CreateAnimatorSettingsAsset() {
        //    ScriptableObjectUtility.CreateAsset<AnimatorSettings>("AnimatorSettings");
        //}

        //[MenuItem("Assets/Create/ATP/AnimationPathAnimator/Events Reflection SettingsAsset")]
        //private static void CreateAPEventsReflectionSettingsAsset() {
        //    ScriptableObjectUtility.CreateAsset<APEventsSettings>("APEventsSettings");
        //}

        //[MenuItem("Assets/Create/ATP/AnimationPathAnimator/Animator Data")]
        //private static void CreatePathAsset() {
        //    ScriptableObjectUtility.CreateAsset<PathData>("APAnimatorData");
        //}

        [MenuItem("Component/ATP/AnimationPathAnimator/Animator")]
        private static void AddAPAnimatorComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(Animator));
            }
        }

        [MenuItem("Component/ATP/AnimationPathAnimator/Events")]
        private static void AddAPEventsComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(Events));
            }
        }
    }

}