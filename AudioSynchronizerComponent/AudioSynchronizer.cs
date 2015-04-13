// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com).
//  
// This file is part of the AnimationPath Animator Unity extension.
// Licensed under the MIT license. See LICENSE file in the project root folder.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using ATP.AnimationPathTools.AnimatorComponent;
using UnityEngine;

namespace ATP.AnimationPathTools.AudioSynchronizerComponent {

    /// <summary>
    ///     Allows controlling <c>AudioSource</c> component from inspector
    ///     and with keyboard shortcuts.
    /// </summary>
    [RequireComponent(typeof (AnimationPathAnimator))]
    [RequireComponent(typeof (AudioSource))]
    public sealed class AudioSynchronizer : MonoBehaviour {
        #region FIELDS

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private AnimationPathAnimator animator;

        /// <summary>
        ///     If to start audio playback on play mode enter.
        /// </summary>
        [SerializeField]
        private bool autoPlay;

        /// <summary>
        ///     If auto play is enabled, delay playback by this value.
        /// </summary>
        [SerializeField]
        private float autoPlayDelay;

        private Dictionary<int, float> audioNodeTimestamps;

        /// <summary>
        ///     Shortcut for play/pause.
        /// </summary>
        public const KeyCode PlayPauseKey = KeyCode.Space;

        #endregion

        #region PROPERTIES

        /// <summary>
        ///     Reference to audio source component.
        /// </summary>
        public AudioSource AudioSource {
            get { return audioSource; }
            set { audioSource = value; }
        }

        /// <summary>
        ///     Reference to animator component.
        /// </summary>
        public AnimationPathAnimator Animator {
            get { return animator; }
            set { animator = value; }
        }

        /// <summary>
        ///     Collection of node indexes and corresponding audio timestamps.
        /// </summary>
        public Dictionary<int, float> AudioNodeTimestamps {
            get { return audioNodeTimestamps; }
            set { audioNodeTimestamps = value; }
        }

        public bool AutoPlay {
            get { return autoPlay; }
            set { autoPlay = value; }
        }

        /// <summary>
        ///     If auto play is enabled, delay playback by this value.
        /// </summary>
        public float AutoPlayDelay {
            get { return autoPlayDelay; }
            set { autoPlayDelay = value; }
        }

        #endregion

        #region UNITY MESSAGES

        private void Reset() {
            AudioSource = GetComponent<AudioSource>();
            Animator = GetComponent<AnimationPathAnimator>();
        }

        private void OnDisable() {
            UnsubscribeFromEvents();
        }

        private void Awake() {
            AudioNodeTimestamps = new Dictionary<int, float>();
        }

        private void OnEnable() {
            UnsubscribeFromEvents();
            SubscribeToEvents();
        }

        private void OnValidate() {
            UnsubscribeFromEvents();
            SubscribeToEvents();
        }

        #endregion

        #region EVENT HANDLERS

        private void Animator_NodeReached(object sender, NodeReachedEventArgs e) {
            if (!Application.isPlaying) return;

            // If audio is playing, record timestamp.
            if (AudioSource.isPlaying) {
                AudioNodeTimestamps[e.NodeIndex] = AudioSource.time;
            }
        }

        private void Animator_JumpedToNode(
            object sender,
            NodeReachedEventArgs e) {
            if (!Application.isPlaying) return;

            // Return if audio timestamp for this node was not recorded.
            if (!AudioNodeTimestamps.ContainsKey(e.NodeIndex)) return;

            AudioSource.time = AudioNodeTimestamps[e.NodeIndex];
        }

        private void Animator_AnimationStarted(object sender, EventArgs e) {
            if (!AutoPlay) return;

            if (AutoPlayDelay != 0) {
                AudioSource.PlayDelayed(AutoPlayDelay);
            }
            else {
                AudioSource.Play();
            }
        }

        private void Animator_AnimationPaused(object sender, EventArgs e) {
            AudioSource.Pause();
        }

        private void Animator_AnimationResumed(object sender, EventArgs e) {
            AudioSource.UnPause();
        }

        #endregion

        #region DO METHODS

        private void UnsubscribeFromEvents() {

            Animator.NodeReached -= Animator_NodeReached;
            Animator.JumpedToNode -= Animator_JumpedToNode;
            Animator.AnimationStarted -= Animator_AnimationStarted;
            Animator.AnimationPaused -= Animator_AnimationPaused;
            Animator.AnimationResumed -= Animator_AnimationResumed;
        }

        private void SubscribeToEvents() {
            Animator.NodeReached += Animator_NodeReached;
            Animator.JumpedToNode += Animator_JumpedToNode;
            Animator.AnimationStarted += Animator_AnimationStarted;
            Animator.AnimationPaused += Animator_AnimationPaused;
            Animator.AnimationResumed += Animator_AnimationResumed;
        }

        #endregion
    }

}

#endif