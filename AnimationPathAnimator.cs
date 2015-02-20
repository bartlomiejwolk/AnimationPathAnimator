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

		#region EVENTS
		public event EventHandler RotationPointPositionChanged;
		public event EventHandler NodeTiltChanged;
		#endregion

        #region FIELDS
        [SerializeField]
        private AnimatorRotationMode rotationMode = AnimatorRotationMode.Forward;

        [SerializeField]
        private bool autoPlay = true;

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
        /// If animation is currently enabled (may be paused).
        /// </summary>
        /// <remarks>
        /// Used in play mode.
        /// </remarks>
        private bool isPlaying;

        private bool pause;

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

        /// <summary>
        /// If animation is currently enabled.
        /// </summary>
        /// <remarks>
        /// Used in play mode. You can use it to stop animation.
        /// </remarks>
        public bool IsPlaying {
            get { return isPlaying; }
            set { isPlaying = value; }
        }

        public bool Pause {
            get { return pause; }
            set { pause = value; }
        }

        public bool AutoPlay {
            get { return autoPlay; }
            set { autoPlay = value; }
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
            animationPathBuilder.PathReset -= animationPathBuilder_PathReset;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnDrawGizmosSelected() {
            // Return if handle mode is not rotation mode.
            if (handleMode != AnimatorHandleMode.Rotation) return;

            DrawRotationGizmoCurve();
            DrawCurrentRotationPointGizmo();
            DrawRotationPointGizmos();
        }
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnEnable() {
            // Subscribe to events.
            animationPathBuilder.PathReset += animationPathBuilder_PathReset;
            animationPathBuilder.NodeAdded += animationPathBuilder_NodeAdded;
            animationPathBuilder.NodeRemoved += animationPathBuilder_NodeRemoved;
            animationPathBuilder.NodeTimeChanged += animationPathBuilder_NodeTimeChanged;
			animationPathBuilder.NodePositionChanged += animationPathBuilder_NodePositionChanged;
			RotationPointPositionChanged += this_RotationPointPositionChanged;
			NodeTiltChanged += this_NodeTiltChanged;

            // Instantiate rotationPath.
            if (rotationPath == null) {
                rotationPath =
                    ScriptableObject.CreateInstance<AnimationPath>();
            }
        }

		void this_RotationPointPositionChanged (object sender, EventArgs e) {
			UpdateAnimation();
		}

		void animationPathBuilder_NodePositionChanged (object sender, EventArgs e) {
			UpdateAnimation();
		}

		void this_NodeTiltChanged(object sender, EventArgs e) {
			UpdateAnimation();
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
            //isPlaying = true;

            // Start animation from time ratio specified in the inspector.
            //currentAnimTime = animTimeRatio * duration;

            if (Application.isPlaying && autoPlay) {
                isPlaying = true;

                StartEaseTimeCoroutine();
            }
        }
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Update() {
            // In play mode, update animation time with delta time.
            if (Application.isPlaying && isPlaying && !pause) {
                Animate();
            }
        }
        #endregion UNITY MESSAGES
		
		#region EVENT INVOCATORS
		protected virtual void OnRotationPointPositionChanged() {
			var handler = RotationPointPositionChanged;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		protected virtual void OnNodeTiltChanged() {
			var handler = NodeTiltChanged;
			if (handler != null) handler(this, EventArgs.Empty);
		}
		#endregion

        #region EVENT HANDLERS
        void animationPathBuilder_NodeRemoved(object sender, EventArgs e) {
            UpdateCurveWithRemovedKeys(easeCurve);
            UpdateCurveWithRemovedKeys(tiltingCurve);
            UpdateRotationCurvesWithRemovedKeys();
        }

        void animationPathBuilder_NodeTimeChanged(object sender, EventArgs e) {
            UpdateCurveTimestamps(easeCurve);
            UpdateCurveTimestamps(tiltingCurve);
            UpdateRotationCurvesTimestamps();
        }

        private void animationPathBuilder_NodeAdded(object sender, EventArgs eventArgs) {
            UpdateCurveWithAddedKeys(easeCurve);
            UpdateCurveWithAddedKeys(tiltingCurve);
            UpdateRotationCurvesWithAddedKeys();
        }


        private void animationPathBuilder_PathReset(object sender, EventArgs eventArgs) {
            ResetRotationPath();
            ResetEaseCurve();
            ResetTiltingCurve();

            // Change handle mode to None.
            handleMode = AnimatorHandleMode.None;
            // Change rotation mode to None.
            rotationMode = AnimatorRotationMode.Forward;
        }
        #endregion EVENT HANDLERS

        #region PUBLIC METHODS

		public void UpdateNodeTilting (int keyIndex, float newValue) {
			// Copy keyframe.
			var keyframeCopy = TiltingCurve.keys[keyIndex];
			// Update keyframe value.
			keyframeCopy.value = newValue;

			// Replace old key with updated one.
			TiltingCurve.RemoveKey(keyIndex);
			TiltingCurve.AddKey(keyframeCopy);
			SmoothCurve(TiltingCurve);
			EaseCurveExtremeNodes(TiltingCurve);

			OnNodeTiltChanged();
		}
        public void StartEaseTimeCoroutine() {
            // Check for play mode.
            StartCoroutine("EaseTime");
        }

        public void StopEaseTimeCoroutine() {
            StopCoroutine("EaseTime");

            // Reset animation.
            isPlaying = false;
            pause = false;
            animTimeRatio = 0;
        }

        public float GetNodeTiltValue(int nodeIndex) {
            return tiltingCurve.keys[nodeIndex].value;
        }

        public Vector3 GetNodePosition(int i) {
            return animationPathBuilder.GetNodePosition(i);
        }

		public Vector3 GetGlobalNodePosition(int nodeIndex) {
			var localNodePosition = animationPathBuilder.GetNodePosition(nodeIndex);
			var globalNodePosition = transform.TransformPoint(localNodePosition);

			return globalNodePosition;
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

        public void ChangeRotationAtTimestamp(
            float timestamp,
            Vector3 newPosition) {

            // Get node timestamps.
            var timestamps = rotationPath.GetTimestamps();
            // If matching timestamp in the path was found.
            var foundMatch = false;
            // For each timestamp..
            for (var i = 0; i < rotationPath.KeysNo; i++) {
                // Check if it is the timestamp to remove..
                if (Math.Abs(timestamps[i] - timestamp) < 0.001f) {
                    // Remove node.
                    rotationPath.RemoveNode(i);

                    foundMatch = true;
                }
            }

            // If timestamp was not found..
            if (!foundMatch) {
                Debug.Log("You're trying to change rotation for nonexistent " +
                    "node.");

                return;
            }

            // Create new node.
            rotationPath.CreateNewNode(timestamp, newPosition);
            // Smooth all nodes.
            rotationPath.SmoothAllNodes();

			OnRotationPointPositionChanged();
			//UpdateAnimation();
        }

        public Vector3 GetForwardPoint() {
            // Timestamp offset of the forward point.
            var forwardPointDelta = forwardPointOffset;
            // Forward point timestamp.
            var forwardPointTimestamp = animTimeRatio + forwardPointDelta;

            return animationPathBuilder.GetVectorAtTime(forwardPointTimestamp);
        }

        public Vector3 GetNodeRotationPointPosition(int nodeIndex) {
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
                rotationPath.RemoveNode(0);
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
        #region PRIVATE METHODS
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
			// Convert position to global coordinate.
			rotationPointPosition = transform.TransformPoint(rotationPointPosition);

            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                rotationPointPosition,
                "rec_16x16-yellow",
                false);
        }

        private Vector3 GetNodeRotationPointPosition(float nodeTimestamp) {
            return rotationPath.GetVectorAtTime(nodeTimestamp);
        }

        private void DrawRotationGizmoCurve() {
            var points = rotationPath.SamplePathForPoints(RotationCurveSampling);

			for (var i = 0; i < points.Count; i++){
				// Convert point positions to global coordinates.
				points[i] = transform.TransformPoint(points[i]);
			}

            if (points.Count < 2) return;

            Gizmos.color = rotationCurveColor;

            // Draw curve.
            for (var i = 0; i < points.Count - 1; i++) {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }
        }

        private void DrawRotationPointGizmos() {
            // Get current animation time.
            var currentAnimationTime = AnimationTimeRatio;

            // Path node timestamps.
            var nodeTimestamps = AnimationPathBuilder.GetNodeTimestamps();

            var nodesNo = animationPathBuilder.NodesNo;
            var rotationPointPositions = new Vector3[nodesNo];
			// TODO Create GetRotationPathPositions() and GetRotationPathGlobalPositions().
            for (int i = 0; i < nodesNo; i++) {
                rotationPointPositions[i] = GetNodeRotationPointPosition(i);
				// Convert position to global coordinate.
				rotationPointPositions[i] = transform.TransformPoint(rotationPointPositions[i]);
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

        private void ResetTiltingCurve() {
            RemoveAllCurveKeys(tiltingCurve);

            tiltingCurve.AddKey(0, 0);
            //tiltingCurve.AddKey(0.5f, 0);
            tiltingCurve.AddKey(1, 0);
        }

        private void ResetRotationPath() {
            var pathNodePositions = animationPathBuilder.GetNodePositions();

            rotationPath.RemoveAllKeys();

            var firstRotationPointPosition =
                pathNodePositions[0] + defaultRotationPointOffset;
            //var secondRotationPointPosition =
            //    pathNodePositions[1] + defaultRotationPointOffset;
            var lastRotationPointPosition =
                pathNodePositions[1] + defaultRotationPointOffset;

            rotationPath.CreateNewNode(0, firstRotationPointPosition);
            //rotationPath.CreateNewNode(0.5f, secondRotationPointPosition);
            rotationPath.CreateNewNode(1, lastRotationPointPosition);
        }

        private void ResetEaseCurve() {
            RemoveAllCurveKeys(easeCurve);

            easeCurve.AddKey(0, DefaultStartEaseValue);
            //easeCurve.AddKey(0.5f, DefaultSecondEaseValue);
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
            // Return if AnimationPathBuilder is not initialized.
            if (!animationPathBuilder.IsInitialized) return;

            AnimateObject();
            HandleAnimatedGORotation();
            TiltObject();
        }

        private void AnimateObject() {
            if (animatedGO == null
                || animationPathBuilder == null
                || !animationPathBuilder.IsInitialized) {

                return;
            }

			var positionAtTimestamp = animationPathBuilder.GetVectorAtTime(animTimeRatio);
			var globalPositionAtTimestamp = transform.TransformPoint(positionAtTimestamp);

            // Update position.
			animatedGO.position = globalPositionAtTimestamp;
			//animatedGO.localPosition =
			//animationPathBuilder.GetVectorAtTime(animTimeRatio);
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

        private IEnumerator EaseTime() {
            while (true) {
				// If animation is not paused..
                if (!pause) {
                    // Ease time.
                    var timeStep = easeCurve.Evaluate(animTimeRatio);
					Debug.Log (timeStep);
                    animTimeRatio += timeStep * Time.deltaTime;
					Debug.Log (animTimeRatio);
                }

                yield return null;
            }

            // Reset animation.
            isPlaying = false;
            animTimeRatio = 0;
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

        private void HandleAnimatedGORotation() {
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
            // Use rotation path.
            if (animatedGO != null
                && targetGO == null
                && rotationMode != AnimatorRotationMode.Forward) {

                RotateObjectWithAnimationCurves();
            }
            // Look forward.
            else if (animatedGO != null
                && rotationMode == AnimatorRotationMode.Forward) {

                Vector3 forwardPoint = GetForwardPoint();
				var globalForwardPoint = transform.TransformPoint(forwardPoint);

                // In play mode..
                if (Application.isPlaying) {
                    RotateObjectWithSlerp(globalForwardPoint);
                }
                else {
                    RotateObjectWithLookAt(globalForwardPoint);
                }
            }
        }

        private void RotateObjectWithAnimationCurves() {
            var lookAtTarget = rotationPath.GetVectorAtTime(animTimeRatio);
			// Convert target position to global coordinates.
			var lookAtTargetGlobal = transform.TransformPoint(lookAtTarget);

            // In play mode use Quaternion.Slerp();
            if (Application.isPlaying) {
                RotateObjectWithSlerp(lookAtTargetGlobal);
            }
            // In editor mode use Transform.LookAt().
            else {
                RotateObjectWithLookAt(lookAtTargetGlobal);
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

            var eulerAngles = animatedGO.rotation.eulerAngles;
            // Get rotation from AnimationCurve.
            var zRotation = tiltingCurve.Evaluate(animTimeRatio);
            eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, zRotation);
            animatedGO.rotation = Quaternion.Euler(eulerAngles);

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

                    SmoothCurve(curve);
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
                    SmoothCurve(curve);

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
                    SmoothCurve(curve);

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

                    rotationPath.CreateNewNode(
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
                    rotationPath.RemoveNode(i);

                    break;
                }
            }
        }
        #endregion PRIVATE METHODS
    }
}