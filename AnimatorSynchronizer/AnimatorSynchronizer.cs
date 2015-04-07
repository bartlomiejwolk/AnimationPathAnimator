using System.Collections.Generic;
using ATP.AnimationPathTools.AnimatorComponent;
using ATP.LoggingTools;
using UnityEngine;

namespace ATP.AnimationPathTools.AnimatorSynchronizerComponent {

    [RequireComponent(typeof(AnimatorComponent.Animator))]
    public sealed class AnimatorSynchronizer : MonoBehaviour {

        [SerializeField]
        private AnimatorComponent.Animator animator;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Assigned in editor.</remarks>
        [SerializeField]
        private List<AnimatorComponent.Animator> targetComponents; 

        private List<Dictionary<int, float>> nodeTimestamps;

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
            Animator.NodeReached += Animator_NodeReached;
            Animator.JumpedToNode += Animator_JumpedToNode;
            Animator.PlayPause += Animator_PlayPause;
        }

        void Animator_PlayPause(object sender, System.EventArgs e) {
            foreach (var target in TargetComponents) {
                target.PlayPauseAnimation();
            }
        }

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
        }

        private void OnDisable() {
            Animator.NodeReached -= Animator_NodeReached;
            Animator.JumpedToNode -= Animator_JumpedToNode;
            Animator.PlayPause -= Animator_PlayPause;
        }

        private void Animator_JumpedToNode(object sender, NodeReachedEventArgs e) {
            // For each target animator component..
            for (int i = 0; i < TargetComponents.Count; i++) {
                // Return if audio timestamp for this node was not recorded.
                if (!nodeTimestamps[i].ContainsKey(e.NodeIndex)) continue;

                TargetComponents[i].AnimationTime =
                    NodeTimestamps[i][e.NodeIndex];
            }
        }
    }

}
