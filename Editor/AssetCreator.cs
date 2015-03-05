using ATP.SimplePathAnimator.Animator;
using ATP.SimplePathAnimator.Events;
using UnityEditor;

namespace ATP.SimplePathAnimator {

    // TODO Specify name for newly created asset.
    public class AssetCreator {

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/Path Data")]
        private static void CreatePathAsset() {
            ScriptableObjectUtility.CreateAsset<PathData>();
        }

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/Events Data")]
        private static void CreateAnimatorEventsDataAsset() {
            ScriptableObjectUtility.CreateAsset<PathEventsData>();
        }

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/Path Animator Settings")]
        private static void CreateAnimatorSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<AnimatorSettings>();
        }

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/Path Events Settings")]
        private static void CreatePathEventsSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<PathEventsSettings>();
        }
    }

}