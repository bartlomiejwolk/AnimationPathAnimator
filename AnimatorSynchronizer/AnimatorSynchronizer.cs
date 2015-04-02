using System.Collections.Generic;
using ATP.LoggingTools;
using UnityEngine;

namespace ATP.AnimationPathTools.AnimatorSynchronizerComponent {

    [RequireComponent(typeof(AnimatorComponent.Animator))]
    public sealed class AnimatorSynchronizer : MonoBehaviour {

        [SerializeField]
        private AnimatorComponent.Animator animator;

        [SerializeField]
        private List<AnimatorComponent.Animator> targetComponents; 

        /// <summary>
        /// Source animator component.
        /// </summary>
        public AnimatorComponent.Animator Animator {
            get { return animator; }
            set { animator = value; }
        }

        public List<AnimatorComponent.Animator> TargetComponents {
            get { return targetComponents; }
        }

        private void OnEnable() {
            Animator.JumpPerformed += Animator_JumpPerformed;
        }

        void Animator_JumpPerformed(object sender, float deltaTime) {
            foreach (var target in TargetComponents) {
                Logger.LogString("deltaTime: {0}", deltaTime);
                Logger.LogString("AnimationTime before: {0}", target.AnimationTime);
                target.AnimationTime += deltaTime;
                Logger.LogString("AnimationTime after : {0}", target.AnimationTime);
            }
        }

        private void OnDisable() {
            Animator.JumpPerformed -= Animator_JumpPerformed;
        }

    }

}
