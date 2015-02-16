using ATP.ReorderableList;
using DemoApplication;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace ATP.AnimationPathTools {

    public enum AnimatorHandleMode { None, Ease, Rotation, Tilting }
    public enum AnimatorRotationMode { Forward, Custom, Target }

    /// <summary>
    /// Component that allows animating transforms position along predefined
    /// Animation Paths and also animate their rotation on x and y axis in
    /// time.
    /// </summary>
    [RequireComponent(typeof(AnimationPathBuilder))]
    [ExecuteInEditMode]
    public class AnimationPathAnimator : GameComponent {
        #region CONSTANTS
        private const int RotationCurveSampling = 20;
        private const float DefaultEndEaseValue = 0.01f;
        private const float DefaultSecondEaseValue = 0.08f;
        private const float DefaultStartEaseValue = 0.01f;

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

        #region READ-ONLY

        private readonly Color rotationCurveColor = Color.gray;
        private readonly Vector3 defaultRotationPointOffset = new Vector3(0, 0, 0);
        #endregion
        #region FIELDS
        [SerializeField]
        private AnimatorRotationMode rotationMode = AnimatorRotationMode.Forward;

        [SerializeField]
        private AnimatorHandleMode handleMode = AnimatorHandleMode.None;

        /// <summary>
        /// Path used to animate the <c>animatedGO</c> transform.
        /// </summary>
        [SerializeField]
        private AnimationPathBuilder animationPathBuilder;

        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private AnimationCurve tiltingCurve = new AnimationCurve();

        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private AnimationCurve easeCurve = new AnimationCurve();


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

        [SerializeField]
        private AnimationPath rotationPath;

        //private float timeStep;

        #endregion FIELDS

        #region EDITOR
        /// <summary>
        /// Transform to be animated.
        /// </summary>
        [SerializeField]
#pragma warning disable 649
        private Transform animatedGO;
        /// Current play time represented as a number between 0 and 1.
        [SerializeField]
        private float animTimeRatio;

        [SerializeField]
        private bool advancedSettingsFoldout;

        [SerializeField]
        private float maxAnimationSpeed = 0.3f;

        /// <summary>
        /// Transform that the <c>animatedGO</c> will be looking at.
        /// </summary>
        [SerializeField]
#pragma warning disable 649
        private Transform targetGO;

        /// <summary>
        /// How much look forward point should be positioned away from the
        /// animated object.
        /// </summary>
        /// <remarks>Value is a time in range from 0 to 1.</remarks>
        [SerializeField]
        private float forwardPointOffset = 0.05f;


        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local ReSharper
        // disable once ConvertToConstant.Local
        private float rotationSpeed = 3.0f;
#pragma warning restore 649
#pragma warning restore 649
        #endregion EDITOR
        #region PUBLIC PROPERTIES

        /// <summary>
        /// Path used to animate the <c>animatedGO</c> transform.
        /// </summary>
        public AnimationPathBuilder AnimationPathBuilder {
            get { return animationPathBuilder; }
        }

        public float AnimationTimeRatio {
            get { return animTimeRatio; }
        }

        public AnimationCurve EaseCurve {
            get { return easeCurve; }
        }

        public AnimationPath RotationPath {
            get { return rotationPath; }
        }

        public AnimationCurve TiltingCurve {
            get { return tiltingCurve; }
        }

        public AnimatorHandleMode HandleMode {
            get { return handleMode; }
            set { handleMode = value; }
        }

        public AnimatorRotationMode RotationMode {
            get { return rotationMode; }
            set { rotationMode = value; }
        }

        #endregion PUBLIC PROPERTIES

        #region UNITY MESSAGES

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Awake() {
            InitializeEaseCurve();
            InitializeRotationCurve();

            // Initialize animatedGO field.
            if (animatedGO == null && Camera.main.transform != null) {
                animatedGO = Camera.main.transform;
            }
            // Initialize AnimationPathBuilder field.
            animationPathBuilder = GetComponent<AnimationPathBuilder>();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnDisable() {
            animationPathBuilder.PathReset -= AnimationPathBuilderOnPathReset;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnDrawGizmosSelected() {
            // Return if handle mode is not rotation mode.
            if (handleMode != AnimatorHandleMode.Rotation) return;

            DrawRotationGizmoCurve();
            DrawCurrentRotationPointGizmo();
            DrawRotationPointGizmos();
        }

        private void DrawRotationPointGizmos() {
            // Get current animation time.
            var currentAnimationTime = AnimationTimeRatio;

            // Path node timestamps.
            var nodeTimestamps = AnimationPathBuilder.GetNodeTimestamps();

            var nodesNo = animationPathBuilder.NodesNo;
            var rotationPointPositions = new Vector3[nodesNo];
            for (int i = 0; i < nodesNo; i++) {
                rotationPointPositions[i] = GetNodeRotation(i);
            }

            //foreach (var rotationPointPosition in rotationPointPositions) {
            for (int i = 0; i < rotationPointPositions.Length; i++) {
                // Return if current animation time is the same as any node time.
                if (Math.Abs(nodeTimestamps[i] - currentAnimationTime) < 0.001f) {
                    continue;
                }

                //Draw rotation point gizmo.
                Gizmos.DrawIcon(
                    rotationPointPositions[i],
                    "rec_16x16",
                    false);
            }
        }

        private void DrawRotationGizmoCurve() {
            var points = rotationPath.SamplePathForPoints(RotationCurveSampling);

            if (points.Count < 2) return;

            Gizmos.color = rotationCurveColor;

            // Draw curve.
            for (var i = 0; i < points.Count - 1; i++) {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }
        }

        private void DrawCurrentRotationPointGizmo() {
            // Get current animation time.
            var currentAnimationTime = AnimationTimeRatio;

            // Node path node timestamps.
            var nodeTimestamps = AnimationPathBuilder.GetNodeTimestamps();

            // Return if current animation time is the same as any node time.
            foreach (var nodeTimestamp in nodeTimestamps) {
                if (Math.Abs(nodeTimestamp - currentAnimationTime) < 0.001f) return;
            }

            // Get rotation point position.
            var rotationPointPosition = GetRotationAtTime(currentAnimationTime);

            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                rotationPointPosition,
                "rec_16x16-yellow",
                false);
        }

        private Vector3 GetNodeRotation(float nodeTimestamp) {
            return rotationPath.GetVectorAtTime(nodeTimestamp);
        }

        private List<float> GetSampledTimestamps(float samplingRate) {
            var result = new List<float>();

            float time = 0;

            var timestep = 1f / samplingRate;

            for (var i = 0; i < samplingRate + 1; i++) {
                result.Add(time);

                // Time goes towards 1.
                time += timestep;
            }

            return result;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnEnable() {
            // Subscribe to events.
            animationPathBuilder.PathReset += AnimationPathBuilderOnPathReset;
            animationPathBuilder.NodeAdded += AnimationPathBuilderOnNodeAdded;
            animationPathBuilder.NodeRemoved += AnimationPathBuilderOnNodeRemoved;
            animationPathBuilder.NodeTimeChanged += AnimationPathBuilderOnNodeTimeChanged;

            // Instantiate rotationPath.
            if (rotationPath == null) {
                rotationPath =
                    ScriptableObject.CreateInstance<AnimationPath>();
            }
        }

        void AnimationPathBuilderOnNodeTimeChanged(object sender, EventArgs e) {
            UpdateCurveTimestamps(easeCurve);
            UpdateCurveTimestamps(tiltingCurve);
            UpdateRotationCurvesTimestamps();
        }

        void AnimationPathBuilderOnNodeRemoved(object sender, EventArgs e) {
            UpdateCurveWithRemovedKeys(easeCurve);
            UpdateCurveWithRemovedKeys(tiltingCurve);
            UpdateRotationCurvesWithRemovedKeys();
        }

        private void AnimationPathBuilderOnNodeAdded(object sender, EventArgs eventArgs) {
            UpdateCurveWithAddedKeys(easeCurve);
            UpdateCurveWithAddedKeys(tiltingCurve);
            UpdateRotationCurvesWithAddedKeys();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnValidate() {
            // Limit duration value.
            //if (duration < 1) {
            //    duration = 1;
            //}

            // Limit animation time ratio to <0; 1>.
            //if (animTimeRatio < 0) {
            //    animTimeRatio = 0;
            //}
            //else if (animTimeRatio > 1) {
            //    animTimeRatio = 1;
            //}
        }
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Start() {

            // Start playing animation on Start().
            isPlaying = true;

            // Start animation from time ratio specified in the inspector.
            //currentAnimTime = animTimeRatio * duration;

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
        public float GetNodeTiltValue(int nodeIndex) {
            return tiltingCurve.keys[nodeIndex].value;
        }

        public Vector3 GetNodePosition(int i) {
            return animationPathBuilder.GetNodePosition(i);
        }

        public float GetNodeEaseValue(int i) {
            return easeCurve.keys[i].value;
        }

        public void SmoothCurve(AnimationCurve curve) {
            for (var i = 0; i < curve.length; i++) {
                curve.SmoothTangents(i, 0);
            }
        }

        public static void RemoveAllCurveKeys(AnimationCurve curve) {
            var keysToRemoveNo = curve.length;
            for (var i = 0; i < keysToRemoveNo; i++) {
                curve.RemoveKey(0);
            }
        }

        public void ChangeRotationForTimestamp(
                    float timestamp,
                    Vector3 newPosition) {

            // TODO Extract. Remove old key.
            var timestamps = rotationPath.GetTimestamps();
            for (var i = 0; i < rotationPath.KeysNo; i++) {
                if (Math.Abs(timestamps[i] - timestamp) < 0.001f) {
                    rotationPath.RemovePoint(i);
                }
            }
            rotationPath.CreateNewPoint(timestamp, newPosition);
            rotationPath.SmoothAllNodes();
        }

        public Vector3 GetForwardPoint() {
            // Timestamp offset of the forward point.
            //var forwardPointDelta = lookForwardCurve.Evaluate(animTimeRatio);
            var forwardPointDelta = forwardPointOffset;
            // Forward point timestamp.
            var forwardPointTimestamp = animTimeRatio + forwardPointDelta;

            return animationPathBuilder.GetVectorAtTime(forwardPointTimestamp);
        }

        // TODO Rename to GetRotationDirection().
        public Vector3 GetNodeRotation(int nodeIndex) {
            return rotationPath.GetVectorAtKey(nodeIndex);
        }

        public float[] GetPathTimestamps() {
            return animationPathBuilder.GetNodeTimestamps();
        }

        public Vector3 GetRotationAtTime(float timestamp) {
            return rotationPath.GetVectorAtTime(timestamp);
        }

        public void ResetRotation() {
            // Remove all nodes.
            for (var i = 0; i < rotationPath.KeysNo; i++) {
                // NOTE After each removal, next node gets index 0.
                rotationPath.RemovePoint(0);
            }
        }
        /// <summary>
        /// Call in edit mode to update animation.
        /// </summary>
        public void UpdateAnimation() {
            if (!Application.isPlaying) {
                Animate();
            }
        }

        public void SyncCurveWithPath(AnimationCurve curve) {
            if (animationPathBuilder.NodesNo > curve.length) {
                UpdateCurveWithAddedKeys(curve);
            }
            else if (animationPathBuilder.NodesNo < curve.length) {
                UpdateCurveWithRemovedKeys(curve);
            }
            // Update curve timestamps.
            else {
                UpdateCurveTimestamps(curve);
            }
        }
        #endregion PUBLIC METHODS

        #region EVENT HANDLERS

        private void AnimationPathBuilderOnPathReset(object sender, EventArgs eventArgs) {
            ResetRotationPath();
            ResetEaseCurve();
            ResetTiltingCurve();

            // Change handle mode to None.
            handleMode = AnimatorHandleMode.None;
            // Change rotation mode to None.
            rotationMode = AnimatorRotationMode.Forward;
        }
        #endregion EVENT HANDLERS

        #region PRIVATE METHODS
        private void ResetTiltingCurve() {
            RemoveAllCurveKeys(tiltingCurve);

            tiltingCurve.AddKey(0, 0);
            tiltingCurve.AddKey(0.5f, 0);
            tiltingCurve.AddKey(1, 0);
        }

        private void ResetRotationPath() {
            var pathNodePositions = animationPathBuilder.GetNodePositions();

            rotationPath.RemoveAllKeys();

            var firstRotationPointPosition =
                pathNodePositions[0] + defaultRotationPointOffset;
            var secondRotationPointPosition =
                pathNodePositions[1] + defaultRotationPointOffset;
            var lastRotationPointPosition =
                pathNodePositions[2] + defaultRotationPointOffset;

            rotationPath.CreateNewPoint(0, firstRotationPointPosition);
            rotationPath.CreateNewPoint(0.5f, secondRotationPointPosition);
            rotationPath.CreateNewPoint(1, lastRotationPointPosition);
        }

        private void ResetEaseCurve() {
            RemoveAllCurveKeys(easeCurve);

            easeCurve.AddKey(0, DefaultStartEaseValue);
            easeCurve.AddKey(0.5f, DefaultSecondEaseValue);
            easeCurve.AddKey(1, DefaultEndEaseValue);
        }

        private void AddKeyToCurve(
            AnimationCurve curve,
            float timestamp) {

            var value = curve.Evaluate(timestamp);

            curve.AddKey(timestamp, value);
        }

        public void EaseCurveExtremeNodes(AnimationCurve curve) {
            // Ease first node.
            var firstKeyCopy = curve.keys[0];
            firstKeyCopy.outTangent = 0;
            curve.RemoveKey(0);
            curve.AddKey(firstKeyCopy);

            // Ease last node.
            var lastKeyIndex = curve.length - 1;
            var lastKeyCopy = curve.keys[lastKeyIndex];
            lastKeyCopy.inTangent = 0;
            curve.RemoveKey(lastKeyIndex);
            curve.AddKey(lastKeyCopy);
        }

        private void Animate() {
            AnimateObject();
            RotateObject();
            TiltObject();
        }

        private void AnimateObject() {
            if (animatedGO == null
                || animationPathBuilder == null
                || !animationPathBuilder.IsInitialized) {

                return;
            }

            // Update position.
            animatedGO.position =
                animationPathBuilder.GetVectorAtTime(animTimeRatio);
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

        // TODO Add possibility to stop when isPlaying is disabled.
        private IEnumerator EaseTime() {
            do {
                var timeStep = easeCurve.Evaluate(animTimeRatio);
                animTimeRatio += timeStep * Time.deltaTime;

                yield return null;
            } while (animTimeRatio < 1.0f);
        }

        private double EvaluateTimestamp(double x) {
            return easeCurve.Evaluate((float)x);
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

        // TODO Rename to HandleObjectRotation(). TODO Refactor.
        private void RotateObject() {
            // TODO Move this condition to Animate().
            if (!animationPathBuilder.IsInitialized) return;

            // Look at target.
            if (animatedGO != null
                && targetGO != null
                && rotationMode != AnimatorRotationMode.Forward) {

                // In play mode use Quaternion.Slerp();
                if (Application.isPlaying) {
                    RotateObjectWithSlerp(targetGO.position);
                }
                // In editor mode use Transform.LookAt().
                else {
                    RotateObjectWithLookAt(targetGO.position);
                }
            }
            // Use objectPath.
            if (animatedGO != null
                && targetGO == null
                //&& !lookForwardMode) {
                && rotationMode != AnimatorRotationMode.Forward) {

                RotateObjectWithAnimationCurves();
            }
            // Look forward.
            else if (animatedGO != null
                && rotationMode == AnimatorRotationMode.Forward) {

                Vector3 forwardPoint = GetForwardPoint();

                if (Application.isPlaying) {
                    RotateObjectWithSlerp(forwardPoint);
                }
                else {
                    RotateObjectWithLookAt(forwardPoint);
                }
            }
        }

        private void RotateObjectWithAnimationCurves() {
            var lookAtTarget = rotationPath.GetVectorAtTime(animTimeRatio);

            // In play mode use Quaternion.Slerp();
            if (Application.isPlaying) {
                RotateObjectWithSlerp(lookAtTarget);
            }
            // In editor mode use Transform.LookAt().
            else {
                RotateObjectWithLookAt(lookAtTarget);
            }
        }

        private void RotateObjectWithLookAt(Vector3 targetPos) {
            animatedGO.LookAt(targetPos);
        }

        private void RotateObjectWithSlerp(Vector3 targetPosition) {
            // There's no more points to look at.
            if (targetPosition == animatedGO.position) return;

            // Calculate direction to target.
            var targetDirection = targetPosition - animatedGO.position;
            // Calculate rotation to target.
            var rotation = Quaternion.LookRotation(targetDirection);
            // Calculate rotation speed.
            var speed = Time.deltaTime * rotationSpeed;

            // Lerp rotation.
            animatedGO.rotation = Quaternion.Slerp(
                animatedGO.rotation,
                rotation,
                speed);
        }

        private void TiltObject() {
            if (animatedGO == null
                || !animationPathBuilder.IsInitialized) {

                return;
            }

            var eulerAngles = transform.rotation.eulerAngles;
            // Get rotation from AnimationCurve.
            var zRotation = tiltingCurve.Evaluate(animTimeRatio);
            eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, zRotation);
            transform.rotation = Quaternion.Euler(eulerAngles);

        }

        private void UpdateCurveTimestamps(AnimationCurve curve) {
            // Get node timestamps.
            var pathNodeTimestamps = animationPathBuilder.GetNodeTimestamps();
            // For each key in easeCurve..
            for (var i = 1; i < curve.length - 1; i++) {
                // If resp. node timestamp is different from easeCurve timestamp.. 
                if (Math.Abs(pathNodeTimestamps[i] - curve.keys[i].value) > 0.001f) {
                    // Copy key
                    var keyCopy = curve.keys[i];
                    // Update timestamp
                    keyCopy.time = pathNodeTimestamps[i];
                    // Move key to new value.
                    curve.MoveKey(i, keyCopy);
                }
            }
        }

        /// <summary>
        /// Update AnimationCurve with keys added to the path.
        /// </summary>
        /// <param name="curve"></param>
        private void UpdateCurveWithAddedKeys(AnimationCurve curve) {
            var nodeTimestamps = animationPathBuilder.GetNodeTimestamps();
            // Get curve value.
            var curveTimestamps = new float[curve.length];
            for (var i = 0; i < curve.length; i++) {
                curveTimestamps[i] = curve.keys[i].time;
            }

            // For each path timestamp..
            for (var i = 0; i < nodeTimestamps.Length; i++) {
                bool valueExists = false;
                // For each curve timestamp..
                for (var j = 0; j < curveTimestamps.Length; j++) {
                    if (Math.Abs(nodeTimestamps[i] - curveTimestamps[j]) < 0.001f) {
                        valueExists = true;
                        break;
                    }
                }

                // Add missing key.
                if (!valueExists) {
                    AddKeyToCurve(curve, nodeTimestamps[i]);
                    // Only one node could be added to the path in one frame.
                    break;
                }
            }
        }
        private void UpdateCurveWithRemovedKeys(AnimationCurve curve) {
            // AnimationPathBuilder node timestamps.
            var nodeTimestamps = animationPathBuilder.GetNodeTimestamps();
            // Get values from curve.
            var curveTimestamps = new float[curve.length];
            for (var i = 0; i < curveTimestamps.Length; i++) {
                curveTimestamps[i] = curve.keys[i].time;
            }

            // For each curve timestamp..
            for (var i = 0; i < curveTimestamps.Length; i++) {
                var keyExists = false;
                // For each path node timestamp..
                for (var j = 0; j < nodeTimestamps.Length; j++) {
                    if (Math.Abs(curveTimestamps[i] - nodeTimestamps[j]) < 0.001f) {
                        keyExists = true;
                        break;
                    }
                }

                if (!keyExists) {
                    curve.RemoveKey(i);
                    break;
                }
            }
        }
        private void UpdateRotationPath() {
            if (animationPathBuilder.NodesNo > rotationPath.KeysNo) {
                UpdateRotationCurvesWithAddedKeys();
            }
            else if (animationPathBuilder.NodesNo < rotationPath.KeysNo) {
                UpdateRotationCurvesWithRemovedKeys();
            }
            // Update rotationPath timestamps.
            else {
                UpdateRotationCurvesTimestamps();
            }
        }

        private void UpdateRotationCurvesTimestamps() {
            // Get node timestamps.
            var nodeTimestamps = animationPathBuilder.GetNodeTimestamps();
            var rotationCurvesTimestamps = rotationPath.GetTimestamps();
            // For each node in rotationPath..
            for (var i = 1; i < rotationPath.KeysNo - 1; i++) {
                // If resp. node timestamp is different from key value..
                if (Math.Abs(nodeTimestamps[i] - rotationCurvesTimestamps[i]) > 0.001f) {
                    rotationPath.ChangeNodeTimestamp(i, nodeTimestamps[i]);
                }
            }
        }

        private void UpdateRotationCurvesWithAddedKeys() {
            // AnimationPathBuilder node timestamps.
            var animationCurvesTimestamps = animationPathBuilder.GetNodeTimestamps();
            // Get values from rotationPath.
            var rotationCurvesTimestamps = rotationPath.GetTimestamps();
            var rotationCurvesKeysNo = rotationCurvesTimestamps.Length;

            // For each timestamp in rotationPath..
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
                    var addedKeyTimestamp =
                        animationPathBuilder.GetNodeTimestamp(i);
                    var defaultRotation =
                        rotationPath.GetVectorAtTime(addedKeyTimestamp);

                    rotationPath.CreateNewPoint(
                        animationCurvesTimestamps[i],
                        defaultRotation);
                }
            }
        }

        private void UpdateRotationCurvesWithRemovedKeys() {
            // AnimationPathBuilder node timestamps.
            var pathTimestamps = animationPathBuilder.GetNodeTimestamps();
            // Get values from rotationPath.
            var rotationCurvesTimestamps = rotationPath.GetTimestamps();

            // For each timestamp in rotationPath..
            for (var i = 0; i < rotationCurvesTimestamps.Length; i++) {
                var keyExists = false;
                // For each timestamp in AnimationPathBuilder..
                for (var j = 0; j < pathTimestamps.Length; j++) {
                    // If both timestamps are equal..
                    if (Math.Abs(rotationCurvesTimestamps[i]
                        - pathTimestamps[j]) < 0.001f) {

                        keyExists = true;

                        break;
                    }
                }

                // Remove node from rotationPath.
                if (!keyExists) {
                    rotationPath.RemovePoint(i);

                    break;
                }
            }
        }
        #endregion PRIVATE METHODS
    }
}