using UnityEngine;
using Animator = ATP.AnimationPathTools.AnimatorComponent.Animator;

namespace ATP.AnimationPathTools.AudioSourceControllerComponent {

    /// <summary>
    /// Allows controlling <c>AudioSource</c> component from inspector
    /// and with keyboard shortcuts.
    /// </summary>
    // todo add menu option to create component
    public sealed class AudioSourceController : MonoBehaviour {

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private Animator animator;

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
        public Animator Animator {
            get { return animator; }
            set { animator = value; }
        }

        private void Reset() {
            AudioSource = GetComponent<AudioSource>();
            Animator = GetComponent<Animator>();
        }

        private void Update() {
            HandleSpaceShortcut();
        }

        /// <summary>
        /// Handle space shortcut.
        /// </summary>
        private void HandleSpaceShortcut() {
            // Disable shortcut while animator awaits animation start.
            if (Animator.IsInvoking("StartAnimation")) return;

            // If space pressed..
            if (Input.GetKeyDown(KeyCode.Space)) {
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

}
