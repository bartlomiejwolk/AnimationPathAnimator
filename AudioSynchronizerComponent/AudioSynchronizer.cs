using System.Collections.Generic;
using UnityEngine;
using ATP.AnimationPathTools.AnimatorComponent;

namespace ATP.AnimationPathTools.AudioSynchronizerComponent {

    /// <summary>
    /// Allows controlling <c>AudioSource</c> component from inspector
    /// and with keyboard shortcuts.
    /// </summary>
    // todo add menu option to create component
    [RequireComponent(typeof(AnimatorComponent.Animator))]
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioSynchronizer : MonoBehaviour {

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private AnimatorComponent.Animator animator;

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
        public AnimatorComponent.Animator Animator {
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

        private void Awake() {
            AudioNodeTimestamps = new Dictionary<int, float>();
        }

        private void OnEnable() {
            AudioSource = GetComponent<AudioSource>();
            Animator = GetComponent<AnimatorComponent.Animator>();

            Animator.NodeReached += Animator_NodeReached;
            Animator.JumpedToNode += Animator_JumpedToNode;
        }

        void Animator_JumpedToNode(object sender, NodeReachedEventArgs e) {
            // Return if audio timestamp for this node was not recorded.
            if (!AudioNodeTimestamps.ContainsKey(e.NodeIndex)) return;

            AudioSource.time = AudioNodeTimestamps[e.NodeIndex];
        }

        void Animator_NodeReached(object sender, NodeReachedEventArgs e) {
            // If audio is playing, record timestamps.
            if (AudioSource.isPlaying) {
                AudioNodeTimestamps[e.NodeIndex] = AudioSource.time;
            }
        }

        private void OnDisable() {
            // todo unsubscribe from events.
        }

        private void Update() {
            HandleShortcuts();
            RecordTimestamps();
        }

        private void RecordTimestamps() {
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
            if (Animator.IsInvoking("StartAnimation")) return;

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
