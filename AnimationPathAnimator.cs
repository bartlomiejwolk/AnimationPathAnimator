using System.Collections;
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
        //[SerializeField]
        //private List<Animation> animations = new List<Animation>();

        /// <summary>
        ///     Transform to be animated.
        /// </summary>
        [SerializeField]
        private Transform _target;

        /// <summary>
        ///     Path used to animate the <c>_target</c> transform.
        /// </summary>
        [SerializeField]
        private AnimationPath _path;

        /// <summary>
        ///     Transform that the <c>_target</c> will be looking at.
        /// </summary>
        [SerializeField]
        private Transform _lookAtTarget;

        /// <summary>
        ///     Path used to animate the <c>lookAtTarget</c>.
        /// </summary>
        [SerializeField]
        private TargetAnimationPath _lookAtPath;

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

        //private float _rotationDuration = 3.0f;

        [SerializeField]
        private AnimationCurve _easeAnimationCurve;

        //private const int dotweensamplingfrequency = 5;

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

            //DOTween.To(() => animTimeRatio, x => animTimeRatio = x, 1, duration)
            //    .SetEase(_easeAnimationCurve);

            if (Application.isPlaying) {
                StartCoroutine(EaseTime());
            }
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

        public float[] GetTargetPathTimestamps() {
            return _lookAtPath.GetNodeTimestamps();
        }

        #endregion PUBLIC METHODS

        #region PRIVATE METHODS
        private IEnumerator EaseTime() {
            do {
                // Increase animation time.
                currentAnimTime += Time.deltaTime;

                // Convert animation time to <0; 1> ratio.
                //animTimeRatio = currentAnimTime / duration;
                float timeRatio = currentAnimTime/duration;

                animTimeRatio = _easeAnimationCurve.Evaluate(timeRatio);

                yield return null;
            } while (animTimeRatio < 1.0f);
        }


        // TODO Rename target object to objectTransform.
        private void Animate() {
            // Animate target.
            if (_lookAtTarget != null && _lookAtPath != null) {
                // Update position.
                _lookAtTarget.position =
                    _lookAtPath.GetVectorAtTime(animTimeRatio);
            }

            // Animate transform.
            if (_target != null && _path != null) {
                // Update position.
                _target.position =
                    _path.GetVectorAtTime(animTimeRatio);

                //List<Vector3> waypoints = anim.Path.SamplePathForPoints(DOTweenSamplingFrequency);
                //anim.Target.transform.DOPath(waypoints.ToArray(), duration);
            }

            // Rotate transform.
            if (_target != null && _lookAtTarget != null) {
                // Calculate direction to target.
                Vector3 targetDirection =
                    _lookAtTarget.position - _target.position;
                // Calculate rotation to target.
                Quaternion rotation = Quaternion.LookRotation(
                    targetDirection);
                // Calculate rotation speed.
                float speed = Time.deltaTime * RotationDamping;
                // Lerp rotation.
                _target.rotation = Quaternion.Slerp(
                    _target.rotation,
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

        #endregion PRIVATE METHODS
    }
}