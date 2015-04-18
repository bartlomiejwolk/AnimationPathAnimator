/* 
 * Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com)
 *
 * This file is part of the AnimationPath Animator Unity extension.
 * Licensed under the MIT license. See LICENSE file in the project root folder.
 */

using AnimationPathAnimator.AnimatorComponent;
using AnimationPathAnimator.AnimatorEventsComponent;
using AnimationPathAnimator.AnimatorSynchronizerComponent;
using AnimationPathAnimator.AudioSynchronizerComponent;
using UnityEditor;

namespace AnimationPathAnimator {

    public static class ComponentCreator {

        [MenuItem("Component/AnimationPath Animator/PathAnimator")]
        private static void AddPathAnimatorComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(PathAnimator));
            }
        }

        [MenuItem("Component/AnimationPath Animator/AnimatorEvents")]
        private static void AddAnimatorEventsComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(AnimatorEvents));
            }
        }

        [MenuItem("Component/AnimationPath Animator/AnimatorSynchronizer")]
        private static void AddAnimatorSynchronizerComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(AnimatorSynchronizer));
            }
        }

        [MenuItem("Component/AnimationPath Animator/AudioSynchronizer")]
        private static void AddAudioSynchronizerComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(AudioSynchronizer));
            }
        }

        [MenuItem("Assets/Create/AnimationPath Animator/Animator Settings")]
        private static void CreateAnimatorSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<AnimatorSettings>("AnimatorSettings");
        }

        [MenuItem("Assets/Create/AnimationPath Animator/AnimatorEvents Settings")]
        private static void CreateAPEventsReflectionSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<AnimatorEventsSettings>("AnimatorEventsSettings");
        }

        [MenuItem("Assets/Create/AnimationPath Animator/Animation Path")]
        private static void CreatePathAsset() {
            ScriptableObjectUtility.CreateAsset<PathData>("AnimationPath");
        }
    }

}