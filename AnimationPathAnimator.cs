using ATP.ReorderableList;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace ATP.AnimationPathTools {

    /// <summary>
    /// Component that allows animating transforms position along predefined
    /// Animation Paths and also animate their rotation on x and y axis in
    /// time.
    /// </summary>
    [RequireComponent (typeof (AnimationPath))]
    [RequireComponent (typeof (TargetAnimationPath))]
    [ExecuteInEditMode]
    public class AnimationPathAnimator : GameComponent {

        #region CONSTANTS
        // TODO Add in to the inspector.
        private const float RotationDamping = 3.0f;

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

        private float _rotationDuration = 3.0f;
        private const int DOTweenSamplingFrequency = 5;

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

            //foreach (Animation anim in animations) {
            //    if (anim.LookAtTarget != null && anim.LookAtPath != null) {
            //        List<Vector3> waypoints =
            //            anim.LookAtPath.SamplePathForPoints(DOTweenSamplingFrequency);
            //        anim.LookAtTarget.transform.DOPath(waypoints.ToArray(), duration);
            //    }
            //    if (anim.Target != null && anim.Path != null) {
            //        List<Vector3> waypoints =
            //            anim.Path.SamplePathForPoints(DOTweenSamplingFrequency);
            //        anim.Target.transform.DOPath(waypoints.ToArray(), duration);
            //        //.SetLookAt(anim.LookAtTarget).SetEase(Ease.InCirc);

            //        //anim.Target.DOLookAt(
            //        //    anim.LookAtTarget.position,
            //        //    2.0f);
            //    }
            //}

            DOTween.To(() => animTimeRatio, x => animTimeRatio = x, 1, duration)
                .SetEase(Ease.InOutSine);
        }

        private void Update() {
            // In play mode, update animation time with delta time.
            if (Application.isPlaying && isPlaying) {
                // Increase animation time.
                //currentAnimTime += Time.deltaTime;

                // Convert animation time to <0; 1> ratio.
                //animTimeRatio = currentAnimTime / duration;
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

        // TODO Rename target object to objectTransform.
        private void Animate() {
            // Animate targets selected in the inspector.
            foreach (Animation anim in animations) {
                // If target and target path inspector fields are not empty..
                if (anim.Target != null && anim.Path != null) {
                    // animate target.
                    anim.Target.position =
                        anim.Path.GetVectorAtTime(animTimeRatio);

                    //List<Vector3> waypoints = anim.Path.SamplePathForPoints(DOTweenSamplingFrequency);
                    //anim.Target.transform.DOPath(waypoints.ToArray(), duration);
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
                    Vector3 targetDirection =
                        anim.LookAtTarget.position - anim.Target.position;
                    Quaternion rotation = Quaternion.LookRotation(
                        targetDirection);
                    float speed = Time.deltaTime * RotationDamping;
                    anim.Target.rotation = Quaternion.Slerp(
                        anim.Target.rotation,
                        rotation,
                        speed);

                    // In play mode, rotate using tween.
                    //if (Application.isPlaying) {
                    //    anim.Target.DOLookAt(
                    //        anim.LookAtTarget.position,
                    //        _rotationDuration);
                    //}
                    //// In editor mode, rotate using Unity LookAt().
                    //else {
                    //    transform.LookAt(anim.LookAtTarget.position);
                    //}

                    // rotate target.
                    //anim.Target.LookAt(anim.LookAtTarget);
                }
            }
        }

        #endregion PRIVATE METHODS
    }
}