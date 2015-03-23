using UnityEngine;
using System.Collections;

namespace ATP.AnimationPathTools.AudioSourceControllerComponent {

    /// <summary>
    /// Allows controlling <c>AudioSource</c> component from inspector
    /// and with keyboard shortcuts.
    /// </summary>
    // todo add menu option to create component
    public sealed class AudioSourceController : MonoBehaviour {

        [SerializeField]
        private AudioSource audioSource;

        public AudioSource AudioSource {
            get { return audioSource; }
        }

        private void Start() {

        }

        private void Update() {
            // Play/Pause.
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
