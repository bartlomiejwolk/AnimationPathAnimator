// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com).
//  
// This file is part of the AnimationPath Animator Unity extension.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using System;
using System.Collections.Generic;
using AnimationPathTools.AnimatorComponent;
using UnityEngine;

namespace AnimationPathTools.AnimatorSynchronizerComponent {

    [RequireComponent(typeof (AnimationPathAnimator))]
    public sealed class AnimatorSynchronizer : MonoBehaviour {
        #region FIELDS

        [SerializeField]
        private AnimationPathAnimator animator;

        /// <summary>
        /// </summary>
        /// <remarks>Assigned in editor.</remarks>
        [SerializeField]
        private List<AnimationPathAnimator> targetComponents;

        private List<Dictionary<int, float>> nodeTimestamps;

        #endregion

        #region PROPERTIES

        /// <summary>
        ///     Source animator component.
        /// </summary>
        public AnimationPathAnimator Animator {
            get { return animator; }
            set { animator = value; }
        }

        public List<AnimationPathAnimator> TargetComponents {
            get { return targetComponents; }
        }

        private List<Dictionary<int, float>> NodeTimestamps {
            get { return nodeTimestamps; }
            set { nodeTimestamps = value; }
        }

        #endregion

        #region UNITY MESSAGES

        private void OnEnable() {
            UnsubscribeFromEvents();
            SubscribeToEvents();
        }

        private void Awake() {
            // Instantiate list.
            NodeTimestamps = new List<Dictionary<int, float>>();
            // Instantiate list elements.
            for (var i = 0; i < TargetComponents.Count; i++) {
                NodeTimestamps.Add(new Dictionary<int, float>());
            }
        }

        private void OnDisable() {
            UnsubscribeFromEvents();
        }

        private void OnValidate() {
            SubscribeToEvents();
        }

        #endregion

        #region EVENT HANDLERS

        private void Animator_JumpedToNode(
            object sender,
            NodeReachedEventArgs e) {
            if (!Application.isPlaying) return;

            // For each target animator component..
            for (var i = 0; i < TargetComponents.Count; i++) {
                // Return if timestamp for this node was not recorded.
                if (!NodeTimestamps[i].ContainsKey(e.NodeIndex)) continue;

                // Update animation time.
                TargetComponents[i].AnimationTime =
                    NodeTimestamps[i][e.NodeIndex];
            }
        }

        private void Animator_NodeReached(
            object sender,
            NodeReachedEventArgs e) {

            if (!Application.isPlaying) return;

            // For each target animator component..
            for (var i = 0; i < TargetComponents.Count; i++) {
                // Don't record when source component is not playing.
                if (!TargetComponents[i].IsPlaying) return;

                // Remember current timestamp.
                NodeTimestamps[i][e.NodeIndex] =
                    TargetComponents[i].AnimationTime;
            }
        }

        private void Animator_AnimationPaused(object sender, EventArgs e) {
            foreach (var target in TargetComponents) {
                target.Pause();
            }
        }

        private void Animator_AnimationResumed(object sender, EventArgs e) {
            foreach (var target in TargetComponents) {
                target.Play();
            }
        }

        private void Animator_AnimationStopped(object sender, EventArgs e) {
            foreach (var target in TargetComponents) {
                target.Stop();
            }
        }

        #endregion

        #region DO METHODS

        private void UnsubscribeFromEvents() {
            Animator.NodeReached -= Animator_NodeReached;
            Animator.JumpedToNode -= Animator_JumpedToNode;
            Animator.AnimationPaused -= Animator_AnimationPaused;
            Animator.AnimationResumed -= Animator_AnimationResumed;
        }

        private void SubscribeToEvents() {
            if (Animator == null) return;

            Animator.NodeReached += Animator_NodeReached;
            Animator.JumpedToNode += Animator_JumpedToNode;
            Animator.AnimationPaused += Animator_AnimationPaused;
            Animator.AnimationResumed += Animator_AnimationResumed;
            Animator.AnimationStopped += Animator_AnimationStopped;
        }

        #endregion
    }

}