using ATP.ReorderableList;
using System.Collections.Generic;
using UnityEngine;

namespace ATP.AnimationPathTools {

    /// <summary>
    /// Component that allows animating transforms position along predefined
    /// Animation Paths and also animate their rotation on x and y axis in
    /// time.
    /// </summary>
    [ExecuteInEditMode]
    public class AnimationPathAnimator : GameComponent {

        #region CONSTANTS

        /// <summary>
        /// Key shortcut to jump backward.
        /// </summary>
        public const KeyCode JumpBackward = KeyCode.LeftArrow;

        /// <summary>
        /// Key shortcut to jump forward.
        /// </summary>
        public const KeyCode JumpForward = KeyCode.RightArrow;

        /// <summary>
        /// Key shortcut to jump to the end of the animation.
        /// </summary>
        public const KeyCode JumpToEnd = KeyCode.DownArrow;

        /// <summary>
        /// Key shortcut to jump to the beginning of the animation.
        /// </summary>
        public const KeyCode JumpToStart = KeyCode.UpArrow;

        public const float JumpValue = 0.01f;

        /// <summary>
        /// Keycode used as a modifier key.
        /// </summary>
        /// <remarks>Modifier key changes how other keys works.</remarks>
        public const KeyCode ModKey = KeyCode.A;

        /// <summary>
        /// Value of the jump when modifier key is pressed.
        /// </summary>
        public const float ShortJumpValue = 0.002f;

        #endregion CONSTANTS

        #region FIELDS

        /// <summary>
        /// List of animations to by played by the animator.
        /// </summary>
        [SerializeField]
        private List<Animation> animations = new List<Animation>();

        /// Current play time represented as a number between 0 and 1.
        [SerializeField]
        private float animTimeRatio;

        /// <summary>
        /// Current animation time in seconds.
        /// </summary>
        private float currentAnimTime;

        /// <summary>
        /// Animation duration in seconds.
        /// </summary>
        [SerializeField]
        private float duration = 10;

        /// <summary>
        /// If animation is currently enabled.
        /// </summary>
        /// <remarks>
        /// Used in play mode. You can use it to stop animation.
        /// </remarks>
        private bool isPlaying;

        #endregion FIELDS

        #region UNITY MESSAGES

        private void OnValidate() {
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

        private void Start() {
            // Start playing animation on Start().
            isPlaying = true;

            // Start animation from time ratio specified in the inspector.
            currentAnimTime = animTimeRatio * duration;
        }

        private void Update() {
            // In play mode, update animation time with delta time.
            if (Application.isPlaying && isPlaying) {
                // Increase animation time.
                currentAnimTime += Time.deltaTime;

                // Convert animation time to <0; 1> ratio.
                animTimeRatio = currentAnimTime / duration;
            }

            Animate();
        }
        #endregion UNITY MESSAGES

        #region PUBLIC METHODS

        /// <summary>
        /// Call in edit mode to update animation.
        /// </summary>
        public void UpdateAnimation() {
            if (!Application.isPlaying) {
                Animate();
            }
        }

        #endregion PUBLIC METHODS

        #region PRIVATE METHODS

        private void Animate() {
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

                // If target and look at target inspector fields are not
                // empty..
                if (anim.Target != null && anim.LookAtTarget != null) {
                    // rotate target.
                    anim.Target.LookAt(anim.LookAtTarget);
                }
            }
        }

        #endregion PRIVATE METHODS
    }
}