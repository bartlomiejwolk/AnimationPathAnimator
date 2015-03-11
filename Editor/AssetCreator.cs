using ATP.AnimationPathAnimator.APAnimatorComponent;
using ATP.AnimationPathAnimator.APEventsMessageComponent;
using ATP.AnimationPathAnimator.APEventsReflectionComponent;
using UnityEditor;

namespace ATP.AnimationPathAnimator {

    // TODO Specify name for newly created asset.
    public static class AssetCreator {

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/APAnimator Data")]
        private static void CreatePathAsset() {
            ScriptableObjectUtility.CreateAsset<PathData>();
        }

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/APAnimator Settings")]
        private static void CreateAnimatorSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<APAnimatorSettings>();
        }

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/APEvents Reflection Settings")]
        private static void CreateAPEventsReflectionSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<APEventsReflectionSettings>();
        }
    }

}