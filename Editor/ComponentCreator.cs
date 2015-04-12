using ATP.AnimationPathTools.AnimatorComponent;
using ATP.AnimationPathTools.AnimatorEventsComponent;
using ATP.AnimationPathTools.AnimatorSynchronizerComponent;
using ATP.AnimationPathTools.AudioSynchronizerComponent;
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

        [MenuItem("Component/ATP/AnimationPathTools/AnimationPathAnimator")]
        private static void AddAnimationPathAnimatorComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(AnimationPathAnimator));
            }
        }

        [MenuItem("Component/ATP/AnimationPathTools/AnimatorEvents")]
        private static void AddAnimatorEventsComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(AnimatorEvents));
            }
        }

        [MenuItem("Component/ATP/AnimationPathTools/AnimatorSynchronizer")]
        private static void AddAnimatorSynchronizerComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(AnimatorSynchronizer));
            }
        }

        [MenuItem("Component/ATP/AnimationPathTools/AudioSynchronizer")]
        private static void AddAudioSynchronizerComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(AudioSynchronizer));
            }
        }

    }

}