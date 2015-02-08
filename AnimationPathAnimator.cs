using System;
using ATP.ReorderableList;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using DemoApplication;
using Fasterflect;
using UnityEngine;

namespace ATP.AnimationPathTools {

    /// <summary>
    /// Component that allows animating transforms position along predefined
    /// Animation Paths and also animate their rotation on x and y axis in
    /// time.
    /// </summary>
    [RequireComponent(typeof(AnimationPath))]
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
        private float duration = 10;

        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private AnimationCurve easeCurve = new AnimationCurve();

        /// <summary>
        /// Transform that the <c>animatedObject</c> will be looking at.
        /// </summary>
        [SerializeField]
#pragma warning disable 649
        // TODO Rename to targetObject.
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

        [SerializeField]
        private AnimationCurve lookForwardCurve = new AnimationCurve();

        [SerializeField]
        private bool lookForwardMode;

        [SerializeField]
        private bool displayEaseHandles;

        [SerializeField]
        private bool drawRotationHandle;

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

        private const float LookForwardGizmoSize = 0.5f;

        /// <summary>
        /// How much look forward point should be positioned away from the
        /// animated object.
        /// </summary>
        /// <remarks>
        /// Value is a time in range from 0 to 1.
        /// </remarks>
        private const float LookForwardTimeDelta = 0.03f;

        /// <summary>
        /// Path used to animate the <c>animatedObject</c> transform.
        /// </summary>
        public AnimationPath AnimatedObjectPath {
            get { return animatedObjectPath; }
        }

        public AnimationCurve EaseCurve {
            get { return easeCurve; }
        }

        public float AnimationTimeRatio {
            get { return animTimeRatio; }
        }

        [SerializeField]
        private AnimationPathCurves rotationCurves;

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
        private void Awake() {
            InitializeEaseCurve();
            InitializeRotationCurve();
            InitializeLookForwardCurve();

            // Initialize animatedObject field.
            if (animatedObject == null && Camera.main.transform != null) {
                animatedObject = Camera.main.transform;
            }
            // Initialize animatedObjectPath field.
            animatedObjectPath = GetComponent<AnimationPath>();
            // Initialize followedObjectPath field.
            followedObjectPath = GetComponent<TargetAnimationPath>();

            //CreateTargetGO();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnEnable() {
            // TODO Move it to Awake() and OnDestroy().
            animatedObjectPath.PathChanged += AnimatedObjectPathOnPathChanged;

            // Instantiate rotationCurves.
            if (rotationCurves == null) {
                rotationCurves =
                    ScriptableObject.CreateInstance<AnimationPathCurves>();
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnDisable() {
            animatedObjectPath.PathChanged -= AnimatedObjectPathOnPathChanged;
        }

        private void AnimatedObjectPathOnPathChanged(object sender, EventArgs eventArgs) {
            UpdateEaseCurve();
            UpdateRotationCurves();

            // If there's not, create a new key with this value and the corresponding
            // timestamp in the ease curve.
        }

        public void UpdateEaseCurve() {
            if (animatedObjectPath.NodesNo > easeCurve.length) {
                UpdateEaseCurveWithAddedKeys();
            }
            else if (animatedObjectPath.NodesNo < easeCurve.length) {
                UpdateEaseCurveWithRemovedKeys();
            }
            // Update easeCurve values.
            else {
                UpdateEaseCurveValues();
            }
        }

        private void UpdateEaseCurveValues() {
            // Get node timestamps.
            var nodeTimestamps = animatedObjectPath.GetNodeTimestamps();
            // For each key in easeCurve..
            for (var i = 1; i < nodeTimestamps.Length - 1; i++) {
                // If resp. node timestamp is different from key value..
                if (Math.Abs(nodeTimestamps[i] - easeCurve.keys[i].value) > 0.001f) {
                    // Copy key
                    var keyCopy = easeCurve.keys[i];
                    // Update timestamp
                    keyCopy.value = nodeTimestamps[i];
                    // Move key to new value.
                    easeCurve.MoveKey(i, keyCopy);
                }
            }
        }

        private void UpdateEaseCurveWithRemovedKeys() {
            // AnimationPath node timestamps.
            var nodeTimestamps = animatedObjectPath.GetNodeTimestamps();
            // Get values from easeCurve.
            var easeCurveValues = new float[easeCurve.length];
            for (var i = 0; i < easeCurveValues.Length; i++) {
                easeCurveValues[i] = easeCurve.keys[i].value;
            }

            // For each value in easeCurve..
            for (var i = 0; i < easeCurveValues.Length; i++) {
                var keyExists = false;
                for (var j = 0; j < nodeTimestamps.Length; j++) {
                    if (Math.Abs(easeCurveValues[i] - nodeTimestamps[j]) < 0.001f) {
                        keyExists = true;
                        break;
                    }
                }

                if (!keyExists) {
                    easeCurve.RemoveKey(i);
                    break;
                }
            }
        }

        private void UpdateEaseCurveWithAddedKeys() {
            var nodeTimestamps = animatedObjectPath.GetNodeTimestamps();
            // Get values from easeCurve.
            var easeCurveValues = new float[easeCurve.length];
            for (var i = 0; i < easeCurveValues.Length; i++) {
                easeCurveValues[i] = easeCurve.keys[i].value;
            }

            // For each AnimationPath node timestamp..
            for (var i = 0; i < nodeTimestamps.Length; i++) {
                bool valueExists = false;
                // For each value in easeCurve..
                for (var j = 0; j < easeCurveValues.Length; j++) {
                    if (Math.Abs(nodeTimestamps[i] - easeCurveValues[j]) < 0.001f) {
                        valueExists = true;
                        break;
                    }
                }

                // Add missing key.
                if (!valueExists) {
                    AddKeyToEaseCurve(nodeTimestamps[i]);
                    // Only one node could be added to the path in one frame.
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">Value for the new key in <c>easeCurve</c>.</param>
        private void AddKeyToEaseCurve(float value) {
            // TODO Make it a class field.
            const float precision = 0.001f;
            float time = FindTimestampForValue(easeCurve, value, precision);
            easeCurve.AddKey(time, value);

            // Ease first node.
            var firstKeyCopy = easeCurve.keys[0];
            firstKeyCopy.outTangent = 0;
            easeCurve.RemoveKey(0);
            easeCurve.AddKey(firstKeyCopy);

            // Ease last node.
            var lastKeyIndex = easeCurve.length - 1;
            var lastKeyCopy = easeCurve.keys[lastKeyIndex];
            lastKeyCopy.inTangent = 0;
            easeCurve.RemoveKey(lastKeyIndex);
            easeCurve.AddKey(lastKeyCopy);
        }

        private float FindTimestampForValue(
            AnimationCurve curve,
            float value,
            float precision) {

            var timestamp = 0f;
            bool timestampFound = false;

            // Search for timestamp.
            while (timestampFound == false) {
                var easeCurveValue = curve.Evaluate(timestamp);
                // Check the given timestamp generates expected value;
                if (Math.Abs(easeCurveValue - value) < precision) {
                    timestampFound = true;
                }

                timestamp = CalculateNewTestTimestamp(curve, timestamp, value);
            }

            return timestamp;
        }

        private float CalculateNewTestTimestamp(
            AnimationCurve curve,
            float currentTimestamp,
            float desiredValue) {

            var newTimestamp = RootFinding.Brent(
                EvaluateTimestamp, 
                0,
                1,
                1e-10,
                desiredValue);

            return (float)newTimestamp;
        }

        private double EvaluateTimestamp(double x) {
            return easeCurve.Evaluate((float)x);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Start() {

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

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnDrawGizmosSelected() {
            //if (followedObject != null) return;

            //Vector3 forwardPoint = GetForwardPoint();
            //Vector3 size = new Vector3(
            //    LookForwardGizmoSize,
            //    LookForwardGizmoSize,
            //    LookForwardGizmoSize);
            //Gizmos.DrawWireCube(forwardPoint, size);
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

        // TODO Rename to GetRotationDirection().
        public Vector3 GetNodeRotation(int nodeIndex) {
            return rotationCurves.GetVectorAtKey(nodeIndex);
        }

        public void ChangeRotationForTimestamp(
            float timestamp,
            Vector3 newPosition) {

            // TODO Extract.
            // Remove old key.
            var timestamps = rotationCurves.GetTimestamps();
            for (var i = 0; i < rotationCurves.KeysNo; i++) {
                if (Math.Abs(timestamps[i] - timestamp) < 0.001f) {
                    rotationCurves.RemovePoint(i);
                }
            }
            rotationCurves.CreateNewPoint(timestamp, newPosition);
            rotationCurves.SmoothAllNodes();
        }

        public Vector3 GetRotationAtTime(float timestamp) {
            return rotationCurves.GetVectorAtTime(timestamp);
        }

        public void ResetRotation() {
            // Remove all nodes.
            for (var i = 0; i < rotationCurves.KeysNo; i++) {
                // NOTE After each removal, next node gets index 0.
                rotationCurves.RemovePoint(0);
            }
        }
        private void InitializeLookForwardCurve() {
            var firstKey = new Keyframe(0, LookForwardTimeDelta, 0, 0);
            var lastKey = new Keyframe(1, LookForwardTimeDelta, 0, 0);

            lookForwardCurve.AddKey(firstKey);
            lookForwardCurve.AddKey(lastKey);
        }

        private void CreateTargetGO() {
            string followedGOName = name + "-target";
            GameObject followedGO = GameObject.Find(followedGOName);
            // If nothing was found, create a new one.
            if (followedGO == null) {
                followedObject = new GameObject(followedGOName).transform;
                //followedObject.parent = gameObject.transform;
            }
            else {
                followedObject = followedGO.transform;
            }
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

        private void AnimateObject() {
            if (animatedObject == null
                || animatedObjectPath == null
                || !animatedObjectPath.IsInitialized) {

                return;
            }

            // Update position.
            animatedObject.position =
                animatedObjectPath.GetVectorAtTime(animTimeRatio);
        }

        private void AnimateTarget() {
            if (followedObject == null
               || followedObjectPath == null
               || !followedObjectPath.IsInitialized) {

                return;
            }

            // Update position.
            followedObject.position =
                followedObjectPath.GetVectorAtTime(animTimeRatio);
        }
      
        // TODO Add possibility to stop when isPlaying is disabled.
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
        // TODO Rename to HandleObjectRotation().
        // TODO Refactor.
        private void RotateObject() {
            // TODO Move this condition to Animate().
            if (!animatedObjectPath.IsInitialized) return;

            // Look at target.
            if (animatedObject != null
                && followedObject != null
                && !lookForwardMode) {

                // In play mode use Quaternion.Slerp();
                if (Application.isPlaying) {
                    RotateObjectWithSlerp(followedObject.position);
                }
                // In editor mode use Transform.LookAt().
                else {
                    RotateObjectWithLookAt(followedObject.position);
                }
            }
            if (animatedObject != null
                && followedObject == null
                && !lookForwardMode) {

                RotateObjectWithPathRotation();
            }
            // Look forward.
            else if (animatedObject != null && lookForwardMode) {
                Vector3 forwardPoint = GetForwardPoint();

                if (Application.isPlaying) {
                    RotateObjectWithSlerp(forwardPoint);
                }
                else {
                    RotateObjectWithLookAt(forwardPoint);
                }
            }
        }

        private void RotateObjectWithPathRotation() {
            var rotation = GetRotationAtTime(animTimeRatio);
            animatedObject.rotation = Quaternion.Euler(rotation);
        }

        private void RotateObjectWithLookAt(Vector3 targetPos) {
            animatedObject.LookAt(targetPos);
        }

        public Vector3 GetForwardPoint() {
            // Timestamp offset of the forward point.
            var forwardPointDelta = lookForwardCurve.Evaluate(animTimeRatio);
            // Forward point timestamp.
            var forwardPointTimestamp = animTimeRatio + forwardPointDelta;

            return animatedObjectPath.GetVectorAtTime(forwardPointTimestamp);
        }

        private void RotateObjectWithSlerp(Vector3 targetPosition) {
            // There's no more points to look at.
            if (targetPosition == animatedObject.position) return;

            // Calculate direction to target.
            var targetDirection = targetPosition - animatedObject.position;
            // Calculate rotation to target.
            var rotation = Quaternion.LookRotation(targetDirection);
            // Calculate rotation speed.
            var speed = Time.deltaTime * rotationSpeed;

            // Lerp rotation.
            animatedObject.rotation = Quaternion.Slerp(
                animatedObject.rotation,
                rotation,
                speed);
        }

        private void TiltObject() {
            if (animatedObject == null
                || followedObject == null
                || !animatedObjectPath.IsInitialized) {

                return;
            }

            var eulerAngles = transform.rotation.eulerAngles;
            // Get rotation from AnimationCurve.
            var zRotation = tiltingCurve.Evaluate(animTimeRatio);
            eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, zRotation);
            transform.rotation = Quaternion.Euler(eulerAngles);

        }

        private void UpdateRotationCurves() {
            if (animatedObjectPath.NodesNo > rotationCurves.KeysNo) {
                UpdateRotationCurvesWithAddedKeys();
            }
            else if (animatedObjectPath.NodesNo < rotationCurves.KeysNo) {
                UpdateRotationCurvesWithRemovedKeys();
            }
            // Update rotationCurves timestamps.
            else {
                UpdateRotationCurvesTimestamps();
            }
        }

        private void UpdateRotationCurvesWithRemovedKeys() {
            // AnimationPath node timestamps.
            var animationCurvesTimestamps = animatedObjectPath.GetNodeTimestamps();
            // Get values from rotationCurves.
            var rotationCurvesTimestamps = rotationCurves.GetTimestamps();

            // For each timestamp in rotationCurves..
            for (var i = 0; i < rotationCurvesTimestamps.Length; i++) {
                var keyExists = false;
                for (var j = 0; j < animationCurvesTimestamps.Length; j++) {
                    if (Math.Abs(rotationCurvesTimestamps[i]
                        - animationCurvesTimestamps[j]) < 0.001f) {

                        keyExists = true;
                        break;
                    }
                }

                if (!keyExists) {
                    rotationCurves.RemovePoint(i);
                    break;
                }
            }
        }
        private void UpdateRotationCurvesTimestamps() {
            // Get node timestamps.
            var nodeTimestamps = animatedObjectPath.GetNodeTimestamps();
            var rotationCurvesTimestamps = rotationCurves.GetTimestamps();
            // For each node in rotationCurves..
            for (var i = 1; i < rotationCurves.KeysNo - 1; i++) {
                // If resp. node timestamp is different from key value..
                if (Math.Abs(nodeTimestamps[i] - rotationCurvesTimestamps[i]) > 0.001f) {
                    rotationCurves.ChangePointTimestamp(i, nodeTimestamps[i]);
                }
            }
        }
        private void UpdateRotationCurvesWithAddedKeys() {
            // AnimationPath node timestamps.
            var animationCurvesTimestamps = animatedObjectPath.GetNodeTimestamps();
            // Get values from rotationCurves.
            var rotationCurvesTimestamps = rotationCurves.GetTimestamps();
            var rotationCurvesKeysNo = rotationCurvesTimestamps.Length;

            // For each timestamp in rotationCurves..
            for (var i = 0; i < animationCurvesTimestamps.Length; i++) {
                var keyExists = false;
                for (var j = 0; j < rotationCurvesKeysNo; j++) {
                    if (Math.Abs(rotationCurvesTimestamps[j]
                        - animationCurvesTimestamps[i]) < 0.001f) {

                        keyExists = true;
                        break;
                    }
                }

                if (!keyExists) {
                    var defaultRotation = new Vector3(0, 0, 0);

                    rotationCurves.CreateNewPoint(
                        animationCurvesTimestamps[i],
                        defaultRotation);
                }
            }
        }
        #endregion PRIVATE METHODS
    }
}