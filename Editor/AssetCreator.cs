using ATP.SimplePathAnimator.Animator;
using UnityEditor;

namespace ATP.SimplePathAnimator {

    public class AssetCreator {

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/Path")]
        private static void CreatePathAsset() {
            ScriptableObjectUtility.CreateAsset<PathData>();
        }

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/AnimatorSettings")]
        private static void CreateAnimatorSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<AnimatorSettings>();
        }

    }

}