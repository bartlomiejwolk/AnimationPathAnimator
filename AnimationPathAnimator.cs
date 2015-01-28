using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ATP.ReorderableList;

namespace ATP.AnimationPathTools {

    /// <summary>
    ///     Component that allows animating transforms position along
    ///     predefined Animation Paths and also animate their rotation on x and
    ///     y axis in time.
    /// </summary>
    [ExecuteInEditMode]
	public class AnimationPathAnimator : GameComponent {

        /// <summary>
        ///     Animation duration in seconds.
        /// </summary>
        [SerializeField]
        private float duration = 10;

        /// <summary>
        ///     List of animations to by played by the animator.
        /// </summary>
        [SerializeField]
        private List<Animation> animations = new List<Animation>();

        /// <summary>
        ///     Current animation time in seconds.
        /// </summary>
        private float currentAnimTime;

		/// Current play time represented as a number between 0 and 1.
		[SerializeField]
		private float animTimeRatio;

        /// <summary>
        ///     If animation is currently enabled.
        /// </summary>
        private bool isPlaying;

        /// <summary>
        ///     Keycode used as a modifier key.
        /// </summary>
        /// <remarks>
        ///     Modifier key changes how other keys works.
        /// </remarks>
        public const KeyCode ModKey = KeyCode.A;

        /// <summary>
        ///     Key shortcut to jump to the beginning of the animation.
        /// </summary>
        public const KeyCode JumpToStart = KeyCode.UpArrow;

        /// <summary>
        ///     Key shortcut to jump to the end of the animation.
        /// </summary>
        public const KeyCode JumpToEnd = KeyCode.DownArrow;

        /// <summary>
        ///     Key shortcut to jump forward.
        /// </summary>
        public const KeyCode JumpForward = KeyCode.RightArrow;

        /// <summary>
        ///     Key shortcut to jump backward.
        /// </summary>
        public const KeyCode JumpBackward = KeyCode.LeftArrow;

        public const float JumpValue = 0.01f;

        /// <summary>
        ///     Value of the jump when modifier key is pressed.
        /// </summary>
        public const float ShortJumpValue = 0.002f;

		void Start () {
            // Start playing animation on Start().
            isPlaying = true;

            // Start animation from time ratio specified in the inspector.
            currentAnimTime = animTimeRatio * duration;
		}

		void OnValidate() {
            // Limit duration value.
            if (duration < 1) {
                duration = 1;
            }

			// Limit animation time ratio to <0; 1>.
			if (animTimeRatio < 0) {
				animTimeRatio = 0;
			}
			else if (animTimeRatio > 1) {
				animTimeRatio = 1;
			}
		}

		public void Update () {
            // In play mode, update animation time with delta time.
            if (Application.isPlaying && isPlaying) {
                // Increase animation time.
                currentAnimTime += Time.deltaTime;

				// Convert animation time to <0; 1> ratio.
				animTimeRatio = currentAnimTime / duration;
            }

            // Animate targets selected in the inspector.
            foreach (Animation anim in animations) {
                // If target and target path inspector fields are not empty..
                if (anim.Target != null && anim.Path != null) {
                    // animate target.
                    anim.Target.position =
                        anim.Path.GetVectorAtTime(animTimeRatio);
                }

                // If look at target and look at target path inspector options
                // are not empty..
                if (anim.LookAtTarget != null && anim.LookAtPath != null) {
                    // animate look at target.
                    anim.LookAtTarget.position =
                        anim.LookAtPath.GetVectorAtTime(animTimeRatio);
                }

                // If target and look at target inspector fields are not empty..
                if (anim.Target != null && anim.LookAtTarget != null) {
                    // rotate target.
                    anim.Target.LookAt(anim.LookAtTarget);
                }
            }
		}
	}
}
