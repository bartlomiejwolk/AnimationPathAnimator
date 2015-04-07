using System.Collections.Generic;
using ATP.AnimationPathTools.AnimatorComponent;
using ATP.LoggingTools;
using UnityEngine;
using Animator = ATP.AnimationPathTools.AnimatorComponent.Animator;

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

        void Animator_PlayPause(object sender, float timestamp) {
            foreach (var target in TargetComponents) {
                target.PlayPauseAnimation();
            }
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

            Logger.LogString("Record: [{0}] {1}",
                e.NodeIndex,
                TargetComponents[0].AnimationTime);
        }

        private void OnDisable() {
            Animator.NodeReached -= Animator_NodeReached;
            Animator.JumpedToNode -= Animator_JumpedToNode;
            Animator.PlayPause -= Animator_PlayPause;
        }

        private void Animator_JumpedToNode(object sender, NodeReachedEventArgs e) {
            // For each target animator component..
            for (int i = 0; i < TargetComponents.Count; i++) {
                // Return if timestamp for this node was not recorded.
                if (!NodeTimestamps[i].ContainsKey(e.NodeIndex)) continue;

                TargetComponents[i].AnimationTime =
                    NodeTimestamps[i][e.NodeIndex];

                if (i == 0) {
                    Logger.LogString("Jump to: [{0}] {1}",
                        e.NodeIndex,
                        NodeTimestamps[i][e.NodeIndex]);
                }
            }
        }
    }

}
