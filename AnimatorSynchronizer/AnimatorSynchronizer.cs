using System.Collections.Generic;
using ATP.AnimationPathTools.AnimatorComponent;
using ATP.LoggingTools;
using UnityEngine;

#if UNITY_EDITOR

namespace ATP.AnimationPathTools.AnimatorSynchronizerComponent {

    [RequireComponent(typeof(AnimatorComponent.AnimationPathAnimator))]
    public sealed class AnimatorSynchronizer : MonoBehaviour {

        [SerializeField]
        private AnimatorComponent.AnimationPathAnimator animator;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Assigned in editor.</remarks>
        [SerializeField]
        private List<AnimatorComponent.AnimationPathAnimator> targetComponents; 

        private List<Dictionary<int, float>> nodeTimestamps;

        /// <summary>
        /// Source animator component.
        /// </summary>
        public AnimatorComponent.AnimationPathAnimator Animator {
            get { return animator; }
            set { animator = value; }
        }

        public List<AnimatorComponent.AnimationPathAnimator> TargetComponents {
            get { return targetComponents; }
        }

        private List<Dictionary<int, float>> NodeTimestamps {
            get { return nodeTimestamps; }
            set { nodeTimestamps = value; }
        }

        private void Awake() {
            // Instantiate list.
            NodeTimestamps = new List<Dictionary<int, float>>();
            // Instantiate list elements.
            for (int i = 0; i < TargetComponents.Count; i++) {
                NodeTimestamps.Add(new Dictionary<int, float>());
            }
        }

        private void OnEnable() {
            UnsubscribeFromEvents();
            SubscribeToEvents();
        }

        private void SubscribeToEvents() {
            Animator.NodeReached += Animator_NodeReached;
            Animator.JumpedToNode += Animator_JumpedToNode;
            Animator.AnimationPaused += Animator_AnimationPaused;
            Animator.AnimationResumed += Animator_AnimationResumed;
        }

        void Animator_AnimationResumed(object sender, System.EventArgs e) {
            foreach (var target in TargetComponents) {
                target.UnpauseAnimation();
            }
        }

        void Animator_AnimationPaused(object sender, System.EventArgs e) {
            foreach (var target in TargetComponents) {
                target.PauseAnimation();
            }
        }

        private void OnValidate() {
            SubscribeToEvents();
        }

        // todo it should record also for the node where car timestamp is 0
        void Animator_NodeReached(
            object sender,
            NodeReachedEventArgs e) {

            // For each target animator component..
            for (int i = 0; i < TargetComponents.Count; i++) {
                // Don't record when source component is not playing.
                if (!TargetComponents[i].IsPlaying) return;

                // Remember current timestamp.
                NodeTimestamps[i][e.NodeIndex] =
                    TargetComponents[i].AnimationTime;
            }

            //Logger.LogString("Record: [{0}] {1}",
            //    e.NodeIndex,
            //    TargetComponents[0].AnimationTime);
        }

        private void OnDisable() {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents() {
            Animator.NodeReached -= Animator_NodeReached;
            Animator.JumpedToNode -= Animator_JumpedToNode;
            Animator.AnimationPaused -= Animator_AnimationPaused;
            Animator.AnimationResumed -= Animator_AnimationResumed;
        }

        private void Animator_JumpedToNode(object sender, NodeReachedEventArgs e) {
            if (!Application.isPlaying) return;

            // For each target animator component..
            for (int i = 0; i < TargetComponents.Count; i++) {
                // Return if timestamp for this node was not recorded.
                if (!NodeTimestamps[i].ContainsKey(e.NodeIndex)) continue;

                // Update animation time.
                TargetComponents[i].AnimationTime =
                    NodeTimestamps[i][e.NodeIndex];
            }
        }
    }

}

#endif
