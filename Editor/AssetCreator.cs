using ATP.AnimationPathAnimator.APAnimatorComponent;
using ATP.AnimationPathAnimator.APEventsComponent;
using UnityEditor;

namespace ATP.AnimationPathAnimator {

    public static class AssetCreator {

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/APAnimator SettingsAsset")]
        private static void CreateAnimatorSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<APAnimatorSettings>("APAnimatorSettings");
        }

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/APEvents Reflection SettingsAsset")]
        private static void CreateAPEventsReflectionSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<APEventsSettings>("APEventsSettings");
        }

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/APAnimator Data")]
        private static void CreatePathAsset() {
            ScriptableObjectUtility.CreateAsset<PathData>("APAnimatorData");
        }
    }

}