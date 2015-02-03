using System.Collections;
using ATP.ReorderableList;
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
        [SerializeField]
        private float rotationSpeed = 3.0f;

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
        ///     Transform to be animated.
        /// </summary>
        [SerializeField]
        private Transform animatedObject;

        /// <summary>
        ///     Path used to animate the <c>animatedObject</c> transform.
        /// </summary>
        [SerializeField]
        private AnimationPath animatedObjectPath;

        /// <summary>
        ///     Transform that the <c>animatedObject</c> will be looking at.
        /// </summary>
        [SerializeField]
        private Transform followedObject;

        /// <summary>
        ///     Path used to animate the <c>lookAtTarget</c>.
        /// </summary>
        [SerializeField]
        private TargetAnimationPath followedObjectPath;

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
        private float duration = 20;

        /// <summary>
        /// If animation is currently enabled.
        /// </summary>
        /// <remarks>
        /// Used in play mode. You can use it to stop animation.
        /// </remarks>
        private bool isPlaying;


        [SerializeField]
        private AnimationCurve easeCurve = new AnimationCurve();

        [SerializeField]
        private AnimationCurve zAxisRotationCurve = new AnimationCurve();


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
        private void Update() {
            // In play mode, update animation time with delta time.
            if (Application.isPlaying && isPlaying) {
                Animate();
            }
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
            return followedObjectPath.GetNodeTimestamps();
        }

        #endregion PUBLIC METHODS

        #region PRIVATE METHODS
        private void InitializeEaseCurve() {
            Keyframe firstKey = new Keyframe(0, 0, 0, 0);
            Keyframe lastKey = new Keyframe(1, 1, 0, 0);

            easeCurve.AddKey(firstKey);
            easeCurve.AddKey(lastKey);
        }

        private void InitializeRotationCurve() {
            Keyframe firstKey = new Keyframe(0, 0, 0, 0);
            Keyframe lastKey = new Keyframe(1, 0, 0, 0);

            zAxisRotationCurve.AddKey(firstKey);
            zAxisRotationCurve.AddKey(lastKey);
        }

        private IEnumerator EaseTime() {
            do {
                // Increase animation time.
                currentAnimTime += Time.deltaTime;

                // Convert animation time to <0; 1> ratio.
                float timeRatio = currentAnimTime/duration;

                animTimeRatio = easeCurve.Evaluate(timeRatio);

                yield return null;
            } while (animTimeRatio < 1.0f);
        }


        private void Animate() {
            // Animate target.
            AnimateTarget();

            // Animate transform.
            AnimateObject();

            // Rotate transform.
            RotateObject();

            TiltObject();
        }

        private void TiltObject() {
            if (animatedObject != null && followedObject != null) {
                Vector3 eulerAngles = transform.rotation.eulerAngles;
                // Get rotation from AnimationCurve.
                float zRotation = zAxisRotationCurve.Evaluate(animTimeRatio);
                eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, zRotation);
                transform.rotation = Quaternion.Euler(eulerAngles);
              
            }
        }

        private void RotateObject() {
            if (animatedObject != null && followedObject != null) {
                // Calculate direction to target.
                Vector3 targetDirection =
                    followedObject.position - animatedObject.position;
                // Calculate rotation to target.
                Quaternion rotation = Quaternion.LookRotation(
                    targetDirection);
                // Calculate rotation speed.
                float speed = Time.deltaTime*rotationSpeed;
                // Lerp rotation.
                animatedObject.rotation = Quaternion.Slerp(
                    animatedObject.rotation,
                    rotation,
                    speed);
            }
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

        #endregion PRIVATE METHODS
    }
}