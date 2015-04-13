/* 
 * Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com).
 *
 * This file is part of the AnimationPath Animator Unity extension.
 * Licensed under the MIT license. See LICENSE file in the project root folder.
 */

using System.Collections.Generic;
using UnityEngine;
using ATP.AnimationPathTools.AnimatorComponent;

namespace ATP.AnimationPathTools.AudioSynchronizerComponent {

    /// <summary>
    /// Allows controlling <c>AudioSource</c> component from inspector
    /// and with keyboard shortcuts.
    /// </summary>
    [RequireComponent(typeof(AnimatorComponent.AnimationPathAnimator))]
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioSynchronizer : MonoBehaviour {

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private AnimatorComponent.AnimationPathAnimator animator;

        /// <summary>
        /// If to start audio playback on play mode enter.
        /// </summary>
        [SerializeField]
        private bool autoPlay;

        /// <summary>
        /// If auto play is enabled, delay playback by this value.
        /// </summary>
        [SerializeField]
        private float autoPlayDelay;

        private Dictionary<int, float> audioNodeTimestamps;

        /// <summary>
        /// Shortcut for play/pause.
        /// </summary>
        public const KeyCode PlayPauseKey = KeyCode.Space;

        /// <summary>
        /// Reference to audio source component.
        /// </summary>
        public AudioSource AudioSource {
            get { return audioSource; }
            set { audioSource = value; }
        }

        /// <summary>
        /// Reference to animator component.
        /// </summary>
        public AnimatorComponent.AnimationPathAnimator Animator {
            get { return animator; }
            set { animator = value; }
        }

        /// <summary>
        /// Collection of node indexes and corresponding audio timestamps.
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
        /// If auto play is enabled, delay playback by this value.
        /// </summary>
        public float AutoPlayDelay {
            get { return autoPlayDelay; }
            set { autoPlayDelay = value; }
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

        private void SubscribeToEvents() {
            Animator.NodeReached += Animator_NodeReached;
            Animator.JumpedToNode += Animator_JumpedToNode;
            Animator.AnimationStarted += Animator_AnimationStarted;
            Animator.AnimationPaused += Animator_AnimationPaused;
            Animator.AnimationResumed += Animator_AnimationResumed;
        }

        void Animator_AnimationResumed(object sender, System.EventArgs e) {
            AudioSource.UnPause();
        }

        void Animator_AnimationPaused(object sender, System.EventArgs e) {
            AudioSource.Pause();
        }

        void Animator_AnimationStarted(object sender, System.EventArgs e) {
            if (!AutoPlay) return;

            if (AutoPlayDelay != 0) {
                AudioSource.PlayDelayed(AutoPlayDelay);
            }
            else {
                AudioSource.Play();
            }
        }

        private void Reset() {
            AudioSource = GetComponent<AudioSource>();
            Animator = GetComponent<AnimatorComponent.AnimationPathAnimator>();
        }

        private void Start() {
            HandleAutoPlay();
        }

        /// <summary>
        /// Handle auto play inspector option.
        /// </summary>
        private void HandleAutoPlay() {

        }

        void Animator_JumpedToNode(object sender, NodeReachedEventArgs e) {
            if (!Application.isPlaying) return;

            // Return if audio timestamp for this node was not recorded.
            if (!AudioNodeTimestamps.ContainsKey(e.NodeIndex)) return;

            AudioSource.time = AudioNodeTimestamps[e.NodeIndex];
        }

        void Animator_NodeReached(object sender, NodeReachedEventArgs e) {
            if (!Application.isPlaying) return;

            // If audio is playing, record timestamp.
            if (AudioSource.isPlaying) {
                AudioNodeTimestamps[e.NodeIndex] = AudioSource.time;
            }
        }

        private void OnDisable() {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents() {

            Animator.NodeReached -= Animator_NodeReached;
            Animator.JumpedToNode -= Animator_JumpedToNode;
            Animator.AnimationStarted -= Animator_AnimationStarted;
            Animator.AnimationPaused -= Animator_AnimationPaused;
            Animator.AnimationResumed -= Animator_AnimationResumed;
        }

        private void Update() {
            //HandleShortcuts();
        }

        /// <summary>
        /// Handle space shortcut.
        /// </summary>
        private void HandleShortcuts() {
            // If space pressed..
            if (Input.GetKeyDown(KeyCode.Space)) {
                HandlePlayPause();
            }
        }

        private void HandlePlayPause() {
            // Disable shortcut while animator awaits animation start.
            if (Animator.IsInvoking("Play")) return;

            // Pause
            if (AudioSource.isPlaying) {
                AudioSource.Pause();
            }
            // Play
            else {
                AudioSource.Play();
            }
        }

    }

}
