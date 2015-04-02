using System.Collections.Generic;
using UnityEngine;

namespace ATP.AnimationPathTools.ControlsMapperComponent {

    [RequireComponent(typeof(AnimatorComponent.Animator))]
    public sealed class ControlsMapper : MonoBehaviour {

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
            Animator.AnimationTimeChanged += Animator_AnimationTimeChanged;
        }

        private void OnDisable() {
            Animator.AnimationTimeChanged -= Animator_AnimationTimeChanged;
        }

        void Animator_AnimationTimeChanged(object sender, AnimatorComponent.AnimationTimeChangedEventArgs e) {
            foreach (var target in TargetComponents) {
                target.AnimationTime += e.TimeDelta;
            }
        }

    }

}
