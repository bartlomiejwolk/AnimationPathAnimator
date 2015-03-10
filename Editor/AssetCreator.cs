using ATP.AnimationPathAnimator.APAnimatorComponent;
using ATP.AnimationPathAnimator.APEventsMessageComponent;
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
            ScriptableObjectUtility.CreateAsset<APEventsMessageData>();
        }

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/Path APAnimator messageSettings")]
        private static void CreateAnimatorSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<APAnimatorSettings>();
        }

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/Path Events messageSettings")]
        private static void CreatePathEventsSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<APEventsMessageSettings>();
        }
    }

}