/* 
 * Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com).
 *
 * This file is part of the AnimationPath Animator Unity extension.
 * Licensed under the MIT license. See LICENSE file in the project root folder.
 */

using AnimationPathTools.AnimatorComponent;
using AnimationPathTools.AnimatorEventsComponent;
using AnimationPathTools.AnimatorSynchronizerComponent;
using AnimationPathTools.AudioSynchronizerComponent;
using UnityEditor;

namespace AnimationPathTools {

    public static class ComponentCreator {

        [MenuItem("Component/AirTime Productions/AnimationPath Animator/AnimationPathAnimator")]
        private static void AddAnimationPathAnimatorComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(AnimationPathAnimator));
            }
        }

        [MenuItem("Component/AirTime Productions/AnimationPath Animator/AnimatorEvents")]
        private static void AddAnimatorEventsComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(AnimatorEvents));
            }
        }

        [MenuItem("Component/AirTime Productions/AnimationPath Animator/AnimatorSynchronizer")]
        private static void AddAnimatorSynchronizerComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(AnimatorSynchronizer));
            }
        }

        [MenuItem("Component/AirTime Productions/AnimationPath Animator/AudioSynchronizer")]
        private static void AddAudioSynchronizerComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(AudioSynchronizer));
            }
        }

        [MenuItem("Assets/Create/AirTime Productions/AnimationPath Animator/Animator Settings")]
        private static void CreateAnimatorSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<AnimatorSettings>("AnimatorSettings");
        }

        [MenuItem("Assets/Create/AirTime Productions/AnimationPath Animator/AnimatorEvents Settings")]
        private static void CreateAPEventsReflectionSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<AnimatorEventsSettings>("AnimatorEventsSettings");
        }

        [MenuItem("Assets/Create/AirTime Productions/AnimationPath Animator/Animation Path")]
        private static void CreatePathAsset() {
            ScriptableObjectUtility.CreateAsset<PathData>("AnimationPath");
        }
    }

}