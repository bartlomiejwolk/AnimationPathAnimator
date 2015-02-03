using ATP.ReorderableList;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace ATP.AnimationPathTools {

    /// <summary>
    /// Component that allows animating transforms position along predefined
    /// Animation Paths and also animate their rotation on x and y axis in
    /// time.
    /// </summary>
    [RequireComponent(typeof(AnimationPath))]
    [RequireComponent(typeof(TargetAnimationPath))]
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

        #region EDITOR

        /// <summary>
        /// Transform to be animated.
        /// </summary>
        [SerializeField]
#pragma warning disable 649
        private Transform animatedObject;

        /// <summary>
        /// Path used to animate the <c>animatedObject</c> transform.
        /// </summary>
        [SerializeField]
        private AnimationPath animatedObjectPath;

        /// Current play time represented as a number between 0 and 1.
        [SerializeField]
        private float animTimeRatio;

        /// <summary>
        /// Animation duration in seconds.
        /// </summary>
        [SerializeField]
        private float duration = 20;

        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private AnimationCurve easeCurve = new AnimationCurve();

        /// <summary>
        /// Transform that the <c>animatedObject</c> will be looking at.
        /// </summary>
        [SerializeField]
#pragma warning disable 649
        private Transform followedObject;

        /// <summary>
        /// Path used to animate the <c>lookAtTarget</c>.
        /// </summary>
        [SerializeField]
        private TargetAnimationPath followedObjectPath;

        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local ReSharper
        // disable once ConvertToConstant.Local
        private float rotationSpeed = 3.0f;
#pragma warning restore 649
#pragma warning restore 649
        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private AnimationCurve tiltingCurve = new AnimationCurve();

        #endregion EDITOR

        #region FIELDS

        /// <summary>
        /// Current animation time in seconds.
        /// </summary>
        private float currentAnimTime;

        /// <summary>
        /// If animation is currently enabled.
        /// </summary>
        /// <remarks>
        /// Used in play mode. You can use it to stop animation.
        /// </remarks>
        private bool isPlaying;
        #endregion FIELDS

        #region UNITY MESSAGES

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
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

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Start() {
            animatedObjectPath = GetComponent<AnimationPath>();
            followedObjectPath = GetComponent<TargetAnimationPath>();

            InitializeEaseCurve();
            InitializeRotationCurve();

            // Start playing animation on Start().
            isPlaying = true;

            // Start animation from time ratio specified in the inspector.
            currentAnimTime = animTimeRatio * duration;

            if (Application.isPlaying) {
                StartCoroutine(EaseTime());
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Update() {
            // In play mode, update animation time with delta time.
            if (Application.isPlaying && isPlaying) {
                Animate();
            }
        }

        #endregion UNITY MESSAGES

        #region PUBLIC METHODS

        public float[] GetTargetPathTimestamps() {
            return followedObjectPath.GetNodeTimestamps();
        }

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
            // Animate target.
            AnimateTarget();

            // Animate transform.
            AnimateObject();

            // Rotate transform.
            RotateObject();

            TiltObject();
        }

        private void AnimateObject() {
            if (animatedObject != null && animatedObjectPath != null) {
                // Update position.
                animatedObject.position =
                    animatedObjectPath.GetVectorAtTime(animTimeRatio);
            }
        }

        private void AnimateTarget() {
            if (followedObject != null && followedObjectPath != null) {
                // Update position.
                followedObject.position =
                    followedObjectPath.GetVectorAtTime(animTimeRatio);
            }
        }

        private IEnumerator EaseTime() {
            do {
                // Increase animation time.
                currentAnimTime += Time.deltaTime;

                // Convert animation time to <0; 1> ratio.
                var timeRatio = currentAnimTime / duration;

                animTimeRatio = easeCurve.Evaluate(timeRatio);

                yield return null;
            } while (animTimeRatio < 1.0f);
        }

        private void InitializeEaseCurve() {
            var firstKey = new Keyframe(0, 0, 0, 0);
            var lastKey = new Keyframe(1, 1, 0, 0);

            easeCurve.AddKey(firstKey);
            easeCurve.AddKey(lastKey);
        }

        private void InitializeRotationCurve() {
            var firstKey = new Keyframe(0, 0, 0, 0);
            var lastKey = new Keyframe(1, 0, 0, 0);

            tiltingCurve.AddKey(firstKey);
            tiltingCurve.AddKey(lastKey);
        }
        private void RotateObject() {
            if (animatedObject != null && followedObject != null) {
                // Calculate direction to target.
                var targetDirection =
                    followedObject.position - animatedObject.position;
                // Calculate rotation to target.
                var rotation = Quaternion.LookRotation(
                    targetDirection);
                // Calculate rotation speed.
                var speed = Time.deltaTime * rotationSpeed;
                // Lerp rotation.
                animatedObject.rotation = Quaternion.Slerp(
                    animatedObject.rotation,
                    rotation,
                    speed);
            }
        }

        private void TiltObject() {
            if (animatedObject != null && followedObject != null) {
                var eulerAngles = transform.rotation.eulerAngles;
                // Get rotation from AnimationCurve.
                var zRotation = tiltingCurve.Evaluate(animTimeRatio);
                eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, zRotation);
                transform.rotation = Quaternion.Euler(eulerAngles);

            }
        }
        #endregion PRIVATE METHODS
    }
}