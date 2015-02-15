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
    [RequireComponent(typeof(AnimationPath))]
    [ExecuteInEditMode]
    public class AnimationPathAnimator : GameComponent {
        #region CONSTANTS
        private Vector3 defaultRotationPointOffset = new Vector3(0, 0, 0);
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

        /// <summary>
        /// How much look forward point should be positioned away from the
        /// animated object.
        /// </summary>
        /// <remarks>Value is a time in range from 0 to 1.</remarks>
        // TODO Rename to LookForwardTimeOffset.
        private const float LookForwardTimeDelta = 0.03f;
        #endregion CONSTANTS

        #region FIELDS
        /// <summary>
        /// Path used to animate the <c>animatedObject</c> transform.
        /// </summary>
        [SerializeField]
        private AnimationPath animatedObjectPath;

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
        private AnimationPathCurves rotationCurves;

        //private float timeStep;

        #endregion FIELDS

        #region EDITOR
        [SerializeField]
        private AnimatorHandleMode handleMode = AnimatorHandleMode.None;

        [SerializeField]
        private AnimatorRotationMode rotationMode = AnimatorRotationMode.Forward;


        /// <summary>
        /// Transform to be animated.
        /// </summary>
        [SerializeField]
#pragma warning disable 649
        private Transform animatedObject;
        /// Current play time represented as a number between 0 and 1.
        [SerializeField]
        private float animTimeRatio;

        [SerializeField]
        private bool advancedSettingsFoldout;

        [SerializeField]
        private float maxAnimationSpeed = 0.3f;

        //[SerializeField]
        //private bool displayEaseHandles;

        //[SerializeField]
        //private bool drawRotationHandle;

        /// <summary>
        /// Animation duration in seconds.
        /// </summary>
        //[SerializeField]
        //private float duration = 10;
        /// <summary>
        /// Transform that the <c>animatedObject</c> will be looking at.
        /// </summary>
        [SerializeField]
#pragma warning disable 649
        // TODO Rename to targetObject.
        private Transform followedObject;

        //[SerializeField]
        //// TODO Replace with float value.
        //private AnimationCurve lookForwardCurve = new AnimationCurve();

        [SerializeField]
        private float forwardPointOffset = 0.05f;

        //[SerializeField]
        //private bool lookForwardMode;

        //[SerializeField]
        //private bool tiltingMode;


        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local ReSharper
        // disable once ConvertToConstant.Local
        private float rotationSpeed = 3.0f;

#pragma warning restore 649
#pragma warning restore 649
        #endregion EDITOR
        #region PUBLIC PROPERTIES

        /// <summary>
        /// Path used to animate the <c>animatedObject</c> transform.
        /// </summary>
        public AnimationPath AnimatedObjectPath {
            get { return animatedObjectPath; }
        }

        public float AnimationTimeRatio {
            get { return animTimeRatio; }
        }

        public AnimationCurve EaseCurve {
            get { return easeCurve; }
        }

        public AnimationPathCurves RotationCurves {
            get { return rotationCurves; }
        }

        public AnimationCurve TiltingCurve {
            get { return tiltingCurve; }
        }

        #endregion PUBLIC PROPERTIES

        #region UNITY MESSAGES

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Awake() {
            InitializeEaseCurve();
            InitializeRotationCurve();
            //InitializeLookForwardCurve();

            // Initialize animatedObject field.
            if (animatedObject == null && Camera.main.transform != null) {
                animatedObject = Camera.main.transform;
            }
            // Initialize animatedObjectPath field.
            animatedObjectPath = GetComponent<AnimationPath>();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnDisable() {
            animatedObjectPath.PathChanged -= AnimatedObjectPathOnPathChanged;
            animatedObjectPath.PathReset -= AnimatedObjectPathOnPathReset;
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
            var nodeTimestamps = AnimatedObjectPath.GetNodeTimestamps();

            var nodesNo = animatedObjectPath.NodesNo;
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
            // TODO Calculate samplingRate using rotatio path length.
            var points = SampleForRotationPointPositions(100);

            if (points.Count < 2) return;

            // TODO Create const.
            Gizmos.color = Color.grey;

            // Draw curve.
            for (var i = 0; i < points.Count - 1; i++) {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }
        }

        private void DrawCurrentRotationPointGizmo() {
            // Get current animation time.
            var currentAnimationTime = AnimationTimeRatio;

            // Node path node timestamps.
            var nodeTimestamps = AnimatedObjectPath.GetNodeTimestamps();

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

        private List<Vector3> SampleForRotationPointPositions(float samplingRate) {
            var result = new List<Vector3>();
            var timestamps = GetSampledTimestamps(samplingRate);
            foreach (var timestamp in timestamps) {
                var point = GetNodeRotation(timestamp);
                result.Add(point);
            }

            return result;
        }

        private Vector3 GetNodeRotation(float nodeTimestamp) {
            return rotationCurves.GetVectorAtTime(nodeTimestamp);
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
            // TODO Rename event handlers to use underscore.
            animatedObjectPath.PathChanged += AnimatedObjectPathOnPathChanged;
            animatedObjectPath.PathReset += AnimatedObjectPathOnPathReset;
            animatedObjectPath.NodeAdded += AnimatedObjectPathOnNodeAdded;
            animatedObjectPath.NodeRemoved += animatedObjectPath_NodeRemoved;
            animatedObjectPath.NodeTimeChanged += animatedObjectPath_NodeTimeChanged;

            // Instantiate rotationCurves.
            if (rotationCurves == null) {
                rotationCurves =
                    ScriptableObject.CreateInstance<AnimationPathCurves>();
            }
        }

        void animatedObjectPath_NodeTimeChanged(object sender, EventArgs e) {
            //throw new NotImplementedException();
        }

        void animatedObjectPath_NodeRemoved(object sender, EventArgs e) {
            //throw new NotImplementedException();
        }

        private void AnimatedObjectPathOnNodeAdded(object sender, EventArgs eventArgs) {
            //throw new NotImplementedException();
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
            return animatedObjectPath.GetNodePosition(i);
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
            var timestamps = rotationCurves.GetTimestamps();
            for (var i = 0; i < rotationCurves.KeysNo; i++) {
                if (Math.Abs(timestamps[i] - timestamp) < 0.001f) {
                    rotationCurves.RemovePoint(i);
                }
            }
            rotationCurves.CreateNewPoint(timestamp, newPosition);
            rotationCurves.SmoothAllNodes();
        }

        public Vector3 GetForwardPoint() {
            // Timestamp offset of the forward point.
            //var forwardPointDelta = lookForwardCurve.Evaluate(animTimeRatio);
            var forwardPointDelta = forwardPointOffset;
            // Forward point timestamp.
            var forwardPointTimestamp = animTimeRatio + forwardPointDelta;

            return animatedObjectPath.GetVectorAtTime(forwardPointTimestamp);
        }

        // TODO Rename to GetRotationDirection().
        public Vector3 GetNodeRotation(int nodeIndex) {
            return rotationCurves.GetVectorAtKey(nodeIndex);
        }

        public float[] GetPathTimestamps() {
            return animatedObjectPath.GetNodeTimestamps();
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
        /// <summary>
        /// Call in edit mode to update animation.
        /// </summary>
        public void UpdateAnimation() {
            if (!Application.isPlaying) {
                Animate();
            }
        }

        public void SyncCurveWithPath(AnimationCurve curve) {
            if (animatedObjectPath.NodesNo > curve.length) {
                UpdateCurveWithAddedKeys(curve);
            }
            else if (animatedObjectPath.NodesNo < curve.length) {
                UpdateCurveWithRemovedKeys(curve);
            }
            // Update curve timestamps.
            else {
                UpdateCurveTimestamps(curve);
            }
        }
        #endregion PUBLIC METHODS

        #region EVENT HANDLERS

        private void AnimatedObjectPathOnPathChanged(
                    object sender,
                    EventArgs eventArgs) {

            SyncCurveWithPath(easeCurve);

            SyncCurveWithPath(tiltingCurve);
            EaseCurveExtremeNodes(easeCurve);

            UpdateRotationCurves();

            // If there's not, create a new key with this value and the
            // corresponding timestamp in the ease curve.
        }

        private void AnimatedObjectPathOnPathReset(object sender, EventArgs eventArgs) {
            ResetRotationData();
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

        private void ResetRotationData() {
            var pathNodePositions = animatedObjectPath.GetNodePositions();

            rotationCurves.RemoveAllKeys();

            var firstRotationPointPosition =
                pathNodePositions[0] + defaultRotationPointOffset;
            var secondRotationPointPosition =
                pathNodePositions[1] + defaultRotationPointOffset;
            var lastRotationPointPosition =
                pathNodePositions[2] + defaultRotationPointOffset;

            rotationCurves.CreateNewPoint(0, firstRotationPointPosition);
            rotationCurves.CreateNewPoint(0.5f, secondRotationPointPosition);
            rotationCurves.CreateNewPoint(1, lastRotationPointPosition);
        }

        private void ResetEaseCurve() {
            RemoveAllCurveKeys(easeCurve);

            easeCurve.AddKey(0, DefaultStartEaseValue);
            easeCurve.AddKey(0.5f, DefaultSecondEaseValue);
            easeCurve.AddKey(1, DefaultEndEaseValue);

            //EaseCurveExtremeNodes(easeCurve);
        }



        /// <summary>
        /// </summary>
        /// <param name="value">
        /// Value for the new key in <c>easeCurve</c>.
        /// </param>
        //private void AddKeyToEaseCurve(float value) {
        //     TODO Make it a class field.
        //    const float precision = 0.001f;
        //    float time = FindTimestampForValue(easeCurve, value, precision);
        //    easeCurve.AddKey(time, value);

        //    EaseCurveExtremeNodes(easeCurve);
        //}
        private void AddKeyToCurve(
            AnimationCurve curve,
            float timestamp) {

            var value = curve.Evaluate(timestamp);

            curve.AddKey(timestamp, value);
            //EaseCurveExtremeNodes(curve);
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
            // Animate target.
            //AnimateTarget();

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

        //private void CreateTargetGO() {
        //    string followedGOName = name + "-target";
        //    GameObject followedGO = GameObject.Find(followedGOName);
        //    // If nothing was found, create a new one.
        //    if (followedGO == null) {
        //        followedObject = new GameObject(followedGOName).transform;
        //        //followedObject.parent = gameObject.transform;
        //    }
        //    else {
        //        followedObject = followedGO.transform;
        //    }
        //}

        // TODO Add possibility to stop when isPlaying is disabled.
        private IEnumerator EaseTime() {
            do {
                // Increase animation time.
                //currentAnimTime += Time.deltaTime;
                //currentAnimTime = 0;

                // Convert animation time to <0; 1> ratio.
                //var timeStep = Time.deltaTime;
                //var timeStep = easeCurve.Evaluate(currentAnimTime);
                var timeStep = easeCurve.Evaluate(animTimeRatio);
                //Debug.Log("timeStep: " + timeStep);

                //animTimeRatio = easeCurve.Evaluate(timeStep);
                animTimeRatio += timeStep * Time.deltaTime;
                //Debug.Log("animTimeRatio: " + animTimeRatio);

                //currentAnimTime += timeStep * Time.deltaTime;

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

        //private void InitializeLookForwardCurve() {
        //    var firstKey = new Keyframe(0, LookForwardTimeDelta, 0, 0);
        //    var lastKey = new Keyframe(1, LookForwardTimeDelta, 0, 0);

        //    lookForwardCurve.AddKey(firstKey);
        //    lookForwardCurve.AddKey(lastKey);
        //}

        private void InitializeRotationCurve() {
            var firstKey = new Keyframe(0, 0, 0, 0);
            var lastKey = new Keyframe(1, 0, 0, 0);

            tiltingCurve.AddKey(firstKey);
            tiltingCurve.AddKey(lastKey);
        }

        // TODO Rename to HandleObjectRotation(). TODO Refactor.
        private void RotateObject() {
            // TODO Move this condition to Animate().
            if (!animatedObjectPath.IsInitialized) return;

            // Look at target.
            if (animatedObject != null
                && followedObject != null
                //&& !lookForwardMode) {
                && rotationMode != AnimatorRotationMode.Forward) {

                // In play mode use Quaternion.Slerp();
                if (Application.isPlaying) {
                    RotateObjectWithSlerp(followedObject.position);
                }
                // In editor mode use Transform.LookAt().
                else {
                    RotateObjectWithLookAt(followedObject.position);
                }
            }
            // Use AnimationCurves.
            if (animatedObject != null
                && followedObject == null
                //&& !lookForwardMode) {
                && rotationMode != AnimatorRotationMode.Forward) {

                RotateObjectWithAnimationCurves();
            }
            // Look forward.
            else if (animatedObject != null
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
            //var rotation = GetRotationAtTime(animTimeRatio);
            //animatedObject.rotation = Quaternion.Euler(rotation);

            var lookAtTarget = rotationCurves.GetVectorAtTime(animTimeRatio);

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
            animatedObject.LookAt(targetPos);
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
                || !animatedObjectPath.IsInitialized) {

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
            var pathNodeTimestamps = animatedObjectPath.GetNodeTimestamps();
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
            var nodeTimestamps = animatedObjectPath.GetNodeTimestamps();
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
            // AnimationPath node timestamps.
            var nodeTimestamps = animatedObjectPath.GetNodeTimestamps();
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
                    var addedKeyTimestamp =
                        animatedObjectPath.GetNodeTimestamp(i);
                    var defaultRotation =
                        rotationCurves.GetVectorAtTime(addedKeyTimestamp);

                    rotationCurves.CreateNewPoint(
                        animationCurvesTimestamps[i],
                        defaultRotation);
                }
            }
        }

        private void UpdateRotationCurvesWithRemovedKeys() {
            // AnimationPath node timestamps.
            var pathTimestamps = animatedObjectPath.GetNodeTimestamps();
            // Get values from rotationCurves.
            var rotationCurvesTimestamps = rotationCurves.GetTimestamps();

            // For each timestamp in rotationCurves..
            for (var i = 0; i < rotationCurvesTimestamps.Length; i++) {
                var keyExists = false;
                // For each timestamp in animatedObjectPath..
                for (var j = 0; j < pathTimestamps.Length; j++) {
                    // If both timestamps are equal..
                    if (Math.Abs(rotationCurvesTimestamps[i]
                        - pathTimestamps[j]) < 0.001f) {

                        keyExists = true;

                        break;
                    }
                }

                // Remove node from rotationCurves.
                if (!keyExists) {
                    rotationCurves.RemovePoint(i);

                    break;
                }
            }
        }
        #endregion PRIVATE METHODS
    }
}