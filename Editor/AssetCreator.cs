using ATP.AnimationPathAnimator.APAnimatorComponent;
using ATP.AnimationPathAnimator.EventsMessageComponent;
using UnityEditor;

namespace ATP.AnimationPathAnimator {

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

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/Path APAnimator Settings")]
        private static void CreateAnimatorSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<AnimatorSettings>();
        }

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/Path Events Settings")]
        private static void CreatePathEventsSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<PathEventsSettings>();
        }
    }

}