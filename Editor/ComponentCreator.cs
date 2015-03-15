using ATP.AnimationPathAnimator.APAnimatorComponent;
using ATP.AnimationPathAnimator.APEventsComponent;
using UnityEditor;

namespace ATP.AnimationPathAnimator {

    public static class ComponentCreator {

        //[MenuItem("Assets/Create/ATP/AnimationPathAnimator/APAnimator SettingsAsset")]
        //private static void CreateAnimatorSettingsAsset() {
        //    ScriptableObjectUtility.CreateAsset<APAnimatorSettings>("APAnimatorSettings");
        //}

        //[MenuItem("Assets/Create/ATP/AnimationPathAnimator/APEvents Reflection SettingsAsset")]
        //private static void CreateAPEventsReflectionSettingsAsset() {
        //    ScriptableObjectUtility.CreateAsset<APEventsSettings>("APEventsSettings");
        //}

        //[MenuItem("Assets/Create/ATP/AnimationPathAnimator/APAnimator Data")]
        //private static void CreatePathAsset() {
        //    ScriptableObjectUtility.CreateAsset<PathData>("APAnimatorData");
        //}

        [MenuItem("Component/ATP/AnimationPathAnimator/APAnimator")]
        private static void AddAPAnimatorComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(APAnimator));
            }
        }

        [MenuItem("Component/ATP/AnimationPathAnimator/APEvents")]
        private static void AddAPEventsComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(APEvents));
            }
        }
    }

}