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

        [SerializeField]
        private KeyCode easeModeKey = KeyCode.U;

        // TODO Add setters everywhere.
        public KeyCode EaseModeKey {
            get { return easeModeKey; }
        }

        /// <summary>
        ///     Key shortcut to jump to the end of the animation.
        /// </summary>
        [SerializeField]
        private KeyCode jumpToEndKey = KeyCode.L;

        public KeyCode JumpToEndKey {
            get { return jumpToEndKey; }
        }

        [SerializeField]
        private KeyCode jumpToNextNodeKey = KeyCode.L;

        public KeyCode JumpToNextNodeKey {
            get { return jumpToNextNodeKey; }
        }

        [SerializeField]
        private KeyCode jumpToPreviousNodeKey = KeyCode.H;

        public KeyCode JumpToPreviousNodeKey {
            get { return jumpToPreviousNodeKey; }
        }

        [SerializeField]
        private KeyCode jumpToStartKey = KeyCode.H;

        public KeyCode JumpToStartKey {
            get { return jumpToStartKey; }
        }

        [SerializeField]
        private KeyCode longJumpBackwardKey = KeyCode.J;

        public KeyCode LongJumpBackwardKey {
            get { return longJumpBackwardKey; }
        }

        [SerializeField]
        private KeyCode longJumpForwardKey = KeyCode.K;

        public KeyCode LongJumpForwardKey {
            get { return longJumpForwardKey; }
        }

        [SerializeField]
        private float longJumpValue = 0.01f;

        public float LongJumpValue {
            get { return longJumpValue; }
        }

        [SerializeField]
        private KeyCode modKey = KeyCode.RightAlt;

        public virtual KeyCode ModKey {
            get { return modKey; }
        }

        [SerializeField]
        private Color moveAllModeColor = Color.red;
        public virtual Color MoveAllModeColor {
            get { return moveAllModeColor; }
        }

        [SerializeField]
        private KeyCode moveAllModeKey = KeyCode.P;
        public virtual KeyCode MoveAllModeKey {
            get { return moveAllModeKey; }
        }

        //public virtual KeyCode MoveSingleModeKey {
        //    get { return KeyCode.Y; }
        //}

        [SerializeField]
        private KeyCode noneModeKey = KeyCode.Y;
        public virtual KeyCode NoneModeKey {
            get { return noneModeKey; }
        }

        //public virtual KeyCode PlayPauseKey {
        //    get { return KeyCode.Space; }
        //}

        [SerializeField]
        private KeyCode rotationModeKey = KeyCode.I;
        public virtual KeyCode RotationModeKey {
            get { return rotationModeKey; }
        }

        [SerializeField]
        private KeyCode shortJumpBackwardKey = KeyCode.J;
        public KeyCode ShortJumpBackwardKey {
            get { return shortJumpBackwardKey; }
        }

        [SerializeField]
        private KeyCode shortJumpForwardKey = KeyCode.K;
        public KeyCode ShortJumpForwardKey {
            get { return shortJumpForwardKey; }
        }

        [SerializeField]
        private KeyCode tiltingModeKey = KeyCode.O;
        public virtual KeyCode TiltingModeKey {
            get { return tiltingModeKey; }
        }

        [SerializeField]
        private KeyCode updateAllKey = KeyCode.G;
        public virtual KeyCode UpdateAllKey {
            get { return updateAllKey; }
        }

    }

}
