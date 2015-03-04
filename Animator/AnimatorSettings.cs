using UnityEngine;
using System.Collections;

namespace ATP.SimplePathAnimator.Animator {

    public class AnimatorSettings : ScriptableObject {

        [SerializeField]
        private KeyCode playPauseKey = KeyCode.Space;

        public KeyCode PlayPauseKey {
            get { return playPauseKey; }
            set { playPauseKey = value; }
        }

    }

}
