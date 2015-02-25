using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ATP.ReorderableList;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace ATP.AnimationPathTools {
    public enum AnimatorHandleMode {
        None,
        Ease,
        Rotation,
        Tilting
    }

    public enum AnimatorRotationMode {
        Forward,
        Custom,
        Target
    }


    /// <summary>
    ///     Component that allows animating transforms position along predefined
    ///     Animation Paths and also animate their rotation on x and y axis in
    ///     time.
    /// </summary>
    [RequireComponent(typeof (AnimationPathBuilder))]
    [ExecuteInEditMode]
    public class AnimationPathAnimator : GameComponent {
    
        #region CONSTANTS

        /// <summary>
        ///     Value of the jump when modifier key is pressed.
        /// </summary>
        public const float ShortJumpValue = 0.002f;

        private const string CurrentRotationPointGizmoIcon = "rec_16x16-yellow";
        private const string ForwardPointIcon = "target_22x22-pink";
        private const int RotationCurveSampling = 20;
        private const string RotationPointGizmoIcon = "rec_16x16";
        private const string TargetGizmoIcon = "target_22x22-blue";

        #endregion CONSTANTS

        #region EVENTS

        public event EventHandler NodeTiltChanged;

        public event EventHandler RotationPointPositionChanged;

        #endregion EVENTS

        #region FIELDS
        private readonly Color rotationCurveColor = Color.gray;
        [SerializeField]
        private WrapMode wrapMode = WrapMode.Clamp;

        /// <summary>
        ///     Path used to animate the <c>animatedGO</c> transform.
        /// </summary>
        [SerializeField] private AnimationPathBuilder animationPathBuilder;

        [SerializeField] private bool autoPlay = true;

        [SerializeField] private AnimatorHandleMode handleMode =
            AnimatorHandleMode.None;

        /// <summary>
        ///     If animation is currently enabled (may be paused).
        /// </summary>
        /// <remarks>Used in play mode.</remarks>
        private bool isPlaying;

        [SerializeField] private AnimatorRotationMode rotationMode =
            AnimatorRotationMode.Forward;


        [SerializeField] private bool updateAllMode;

        #endregion FIELDS

        #region SERIALIZED FIELDS

        [SerializeField]
#pragma warning disable 169
            private bool advancedSettingsFoldout;
#pragma warning restore 169

        /// <summary>
        ///     Transform to be animated.
        /// </summary>
        [SerializeField] private Transform animatedGO;

        /// Current play time represented as a number between 0 and 1.
        [SerializeField] private float animTimeRatio;

        [SerializeField]
#pragma warning disable 169
            private bool enableControlsInPlayMode = true;
#pragma warning restore 169

        /// <summary>
        ///     How much look forward point should be positioned away from the
        ///     animated object.
        /// </summary>
        /// <remarks>Value is a time in range from 0 to 1.</remarks>
        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private float forwardPointOffset = 0.05f;

        [SerializeField]
#pragma warning disable 169
            private float maxAnimationSpeed = 0.3f;
#pragma warning restore 169

        [SerializeField] private PathData pathData;

        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private float positionLerpSpeed = 0.1f;

        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private float rotationSpeed = 3.0f;

        [SerializeField] private GUISkin skin;

        /// <summary>
        ///     Transform that the <c>animatedGO</c> will be looking at.
        /// </summary>
        [SerializeField]
#pragma warning disable 649
            private Transform targetGO;
#pragma warning restore 649

        #endregion SERIALIZED FIELDS

        #region PUBLIC PROPERTIES
        public float FloatPrecision {
            get { return 0.001f; }
        }

        public WrapMode WrapMode {
            get { return wrapMode; }
            set { wrapMode = value; }
        }


        /// <summary>
        ///     Path used to animate the <c>animatedGO</c> transform.
        /// </summary>
        public AnimationPathBuilder AnimationPathBuilder {
            get { return animationPathBuilder; }
        }

        public float AnimationTimeRatio {
            get { return animTimeRatio; }
        }

        public bool AutoPlay {
            get { return autoPlay; }
            set { autoPlay = value; }
        }

        public AnimatorHandleMode HandleMode {
            get { return handleMode; }
            set { handleMode = value; }
        }

        /// <summary>
        ///     If animation is currently enabled.
        /// </summary>
        /// <remarks>
        ///     Used in play mode. You can use it to stop animation.
        /// </remarks>
        public bool IsPlaying {
            get { return isPlaying; }
            set { isPlaying = value; }
        }

        public PathData PathData {
            get { return pathData; }
            set { pathData = value; }
        }

        public bool Pause { get; set; }

        public AnimatorRotationMode RotationMode {
            get { return rotationMode; }
            set { rotationMode = value; }
        }

        public GUISkin Skin {
            get { return skin; }
            set { skin = value; }
        }

        public bool UpdateAllMode {
            get { return updateAllMode; }
            set { updateAllMode = value; }
        }

        //public AnimationCurve EaseCurve {
        //    get { return easeCurve; }
        //}

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
            // Return if path asset file is not assigned.
            if (PathData == null) return;

            if (rotationMode == AnimatorRotationMode.Target) {
                DrawTargetIcon();
            }

            if (rotationMode == AnimatorRotationMode.Forward) {
                DrawForwardPointIcon();
            }

            // Return if handle mode is not rotation mode.
            if (handleMode == AnimatorHandleMode.Rotation) {
                DrawRotationGizmoCurve();
                DrawCurrentRotationPointGizmo();
                DrawRotationPointGizmos();
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnEnable() {
            // Subscribe to events.
            animationPathBuilder.PathReset += animationPathBuilder_PathReset;
            animationPathBuilder.NodeAdded += animationPathBuilder_NodeAdded;
            animationPathBuilder.NodeRemoved += animationPathBuilder_NodeRemoved;
            animationPathBuilder.NodeTimeChanged +=
                animationPathBuilder_NodeTimeChanged;
            animationPathBuilder.NodePositionChanged +=
                animationPathBuilder_NodePositionChanged;
            RotationPointPositionChanged += this_RotationPointPositionChanged;
            NodeTiltChanged += this_NodeTiltChanged;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnValidate() {
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Start() {
            if (Application.isPlaying && autoPlay) {
                isPlaying = true;

                StartEaseTimeCoroutine();
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Update() {
            // In play mode, update animation time with delta time.
            if (Application.isPlaying && isPlaying && !Pause) {
                Animate();
            }
        }

        #endregion UNITY MESSAGES

        #region EVENT INVOCATORS

        protected virtual void OnNodeTiltChanged() {
            var handler = NodeTiltChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnRotationPointPositionChanged() {
            var handler = RotationPointPositionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion EVENT INVOCATORS

        #region EVENT HANDLERS

        private void animationPathBuilder_NodeAdded(object sender,
            EventArgs eventArgs) {
            UpdateCurveWithAddedKeys(PathData.EaseCurve);
            UpdateCurveWithAddedKeys(PathData.TiltingCurve);
            UpdateRotationCurvesWithAddedKeys();
        }

        private void animationPathBuilder_NodePositionChanged(
            object sender,
            EventArgs e) {
            if (!Application.isPlaying) Animate();
            if (Application.isPlaying) UpdateAnimatedGO();
        }

        private void animationPathBuilder_NodeRemoved(
            object sender,
            EventArgs e) {
            UpdateCurveWithRemovedKeys(PathData.EaseCurve);
            UpdateCurveWithRemovedKeys(PathData.TiltingCurve);
            UpdateRotationCurvesWithRemovedKeys();
        }

        private void animationPathBuilder_NodeTimeChanged(
            object sender,
            EventArgs e) {
            UpdateRotationCurvesTimestamps();
            UpdateCurveTimestamps(PathData.EaseCurve);
            UpdateCurveTimestamps(PathData.TiltingCurve);
        }

        private void animationPathBuilder_PathReset(
            object sender,
            EventArgs eventArgs) {
            PathData.Reset();

            // Change handle mode to None.
            handleMode = AnimatorHandleMode.None;
            // Change rotation mode to None.
            rotationMode = AnimatorRotationMode.Forward;
        }

        private void this_NodeTiltChanged(object sender, EventArgs e) {
            if (!Application.isPlaying) Animate();
            if (Application.isPlaying) UpdateAnimatedGO();
        }

        private void this_RotationPointPositionChanged(object sender,
            EventArgs e) {
            if (!Application.isPlaying) Animate();
        }

        #endregion EVENT HANDLERS

        #region PUBLIC METHODS

        public void ChangeRotationAtTimestamp(
            float timestamp,
            Vector3 newPosition) {
            // Get node timestamps.
            var timestamps = PathData.RotationPath.GetTimestamps();
            // If matching timestamp in the path was found.
            var foundMatch = false;
            // For each timestamp..
            for (var i = 0; i < PathData.RotationPath.KeysNo; i++) {
                // Check if it is the timestamp to remove..
                if (Math.Abs(timestamps[i] - timestamp) < FloatPrecision) {
                    // Remove node.
                    PathData.RotationPath.RemoveNode(i);

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
            PathData.RotationPath.CreateNewNode(timestamp, newPosition);
            // Smooth all nodes.
            PathData.RotationPath.SmoothAllNodes();

            OnRotationPointPositionChanged();
            //UpdateAnimatedGO();
        }

        //public static void RemoveAllCurveKeys(AnimationCurve curve) {
        //    var keysToRemoveNo = curve.length;
        //    for (var i = 0; i < keysToRemoveNo; i++) {
        //        curve.RemoveKey(0);
        //    }
        //}
        public Vector3 GetForwardPoint(bool globalPosition) {
            // Timestamp offset of the forward point.
            var forwardPointDelta = forwardPointOffset;
            // Forward point timestamp.
            var forwardPointTimestamp = animTimeRatio + forwardPointDelta;
            var localPosition =
                animationPathBuilder.GetVectorAtTime(forwardPointTimestamp);

            // Return global position.
            if (globalPosition) {
                return transform.TransformPoint(localPosition);
            }

            return localPosition;
        }

        public Vector3 GetGlobalNodePosition(int nodeIndex) {
            var localNodePosition =
                animationPathBuilder.GetNodePosition(nodeIndex);
            var globalNodePosition = transform.TransformPoint(localNodePosition);

            return globalNodePosition;
        }

        public float GetNodeEaseValue(int i) {
            return PathData.EaseCurve.keys[i].value;
        }

        public Vector3 GetNodePosition(int i) {
            return animationPathBuilder.GetNodePosition(i);
        }

        public float GetNodeTiltValue(int nodeIndex) {
            return PathData.TiltingCurve.keys[nodeIndex].value;
        }

        public float[] GetPathTimestamps() {
            return animationPathBuilder.GetNodeTimestamps();
        }

        public Vector3 GetRotationAtTime(float timestamp) {
            return PathData.RotationPath.GetVectorAtTime(timestamp);
        }

        public Vector3 GetRotationPointPosition(int nodeIndex) {
            return PathData.RotationPath.GetVectorAtKey(nodeIndex);
        }

        public void ResetRotation() {
            // Remove all nodes.
            for (var i = 0; i < PathData.RotationPath.KeysNo; i++) {
                // NOTE After each removal, next node gets index 0.
                PathData.RotationPath.RemoveNode(0);
            }
        }

        public void SmoothCurve(AnimationCurve curve) {
            for (var i = 0; i < curve.length; i++) {
                curve.SmoothTangents(i, 0);
            }
        }

        public void StartEaseTimeCoroutine() {
            // Check for play mode.
            StartCoroutine("EaseTime");
        }

        public void StopEaseTimeCoroutine() {
            StopCoroutine("EaseTime");

            // Reset animation.
            isPlaying = false;
            Pause = false;
            animTimeRatio = 0;
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

        public void UpdateEaseValue(int keyIndex, float newValue) {
            // Copy keyframe.
            var keyframeCopy = PathData.EaseCurve.keys[keyIndex];
            // Update keyframe value.
            keyframeCopy.value = newValue;

            // Replace old key with updated one.
            PathData.EaseCurve.RemoveKey(keyIndex);
            PathData.EaseCurve.AddKey(keyframeCopy);

            SmoothCurve(PathData.EaseCurve);
        }

        public void UpdateEaseValues(float delta) {
            for (var i = 0; i < PathData.EaseCurve.length; i++) {
                // Copy key.
                var keyCopy = PathData.EaseCurve[i];
                // Update key value.
                keyCopy.value += delta;

                // Remove old key.
                PathData.EaseCurve.RemoveKey(i);

                // Add key.
                PathData.EaseCurve.AddKey(keyCopy);

                // Smooth all tangents.
                SmoothCurve(PathData.EaseCurve);
            }
        }

        public void UpdateNodeTilting(int keyIndex, float newValue) {
            // Copy keyframe.
            var keyframeCopy = PathData.TiltingCurve.keys[keyIndex];
            // Update keyframe value.
            keyframeCopy.value = newValue;

            // Replace old key with updated one.
            PathData.TiltingCurve.RemoveKey(keyIndex);
            PathData.TiltingCurve.AddKey(keyframeCopy);
            SmoothCurve(PathData.TiltingCurve);
            EaseCurveExtremeNodes(PathData.TiltingCurve);

            OnNodeTiltChanged();
        }

        public void UpdateWrapMode() {
            animationPathBuilder.SetWrapMode(wrapMode);
        }

        #endregion PUBLIC METHODS

        #region PRIVATE METHODS

        public void Animate() {
            AnimateObject();
            HandleAnimatedGORotation();
            TiltObject();
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

        /// <summary>
        ///     Update animatedGO position, rotation and tilting based on current
        ///     animTimeRatio.
        /// </summary>
        /// <remarks>
        ///     Used to update animatedGO with keys, in
        ///     play mode.
        /// </remarks>
        public void UpdateAnimatedGO() {
            UpdateAnimatedGOPosition();
            UpdateAnimatedGORotation();
            // Update animatedGO tilting.
            TiltObject();
        }

        private void AddKeyToCurve(
            AnimationCurve curve,
            float timestamp) {
            var value = curve.Evaluate(timestamp);

            curve.AddKey(timestamp, value);
        }


        private void AnimateObject() {
            if (animatedGO == null
                || animationPathBuilder == null) {
                return;
            }

            var positionAtTimestamp =
                animationPathBuilder.GetVectorAtTime(animTimeRatio);

            var globalPositionAtTimestamp =
                transform.TransformPoint(positionAtTimestamp);

            if (Application.isPlaying) {
                // Update position.
                animatedGO.position = Vector3.Lerp(
                    animatedGO.position,
                    globalPositionAtTimestamp,
                    positionLerpSpeed);
            }
            else {
                animatedGO.position = globalPositionAtTimestamp;
            }
        }

        private void DrawCurrentRotationPointGizmo() {
            // Get current animation time.
            var currentAnimationTime = AnimationTimeRatio;

            // Node path node timestamps.
            var nodeTimestamps = AnimationPathBuilder.GetNodeTimestamps();

            // Return if current animation time is the same as any node time.
            if (nodeTimestamps.Any(nodeTimestamp =>
                Math.Abs(nodeTimestamp - currentAnimationTime)
                < FloatPrecision)) {

                return;
            }

            // Get rotation point position.
            var rotationPointPosition = GetRotationAtTime(currentAnimationTime);

            // Convert position to global coordinate.
            rotationPointPosition =
                transform.TransformPoint(rotationPointPosition);

            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                rotationPointPosition,
                CurrentRotationPointGizmoIcon,
                false);
        }

        private void DrawForwardPointIcon() {
            var forwardPointPosition = GetForwardPoint(true);

            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                forwardPointPosition,
                ForwardPointIcon,
                false);
        }

        private void DrawRotationGizmoCurve() {
            var points = PathData.RotationPath.SamplePathForPoints(
                RotationCurveSampling);

            for (var i = 0; i < points.Count; i++) {
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

            var rotationPointPositions = GetRotationPointPositions(true);

            for (var i = 0; i < rotationPointPositions.Length; i++) {
                // Return if current animation time is the same as any node
                // time.
                if (Math.Abs(nodeTimestamps[i] - currentAnimationTime) <
                    FloatPrecision) {
                    continue;
                }

                //Draw rotation point gizmo.
                Gizmos.DrawIcon(
                    rotationPointPositions[i],
                    RotationPointGizmoIcon,
                    false);
            }
        }

        private void DrawTargetIcon() {
            if (targetGO == null) return;

            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                targetGO.position,
                TargetGizmoIcon,
                false);
        }

        // ReSharper disable once UnusedMember.Local
        private IEnumerator EaseTime() {
            while (true) {
                // If animation is not paused..
                if (!Pause) {
                    // Ease time.
                    var timeStep = PathData.EaseCurve.Evaluate(animTimeRatio);
                    animTimeRatio += timeStep*Time.deltaTime;
                }

                yield return null;
            }
            // ReSharper disable once FunctionNeverReturns
        }

        // ReSharper disable once UnusedMember.Local
        private Vector3 GetRotationPointPosition(float nodeTimestamp) {
            return PathData.RotationPath.GetVectorAtTime(nodeTimestamp);
        }

        private Vector3[] GetRotationPointPositions(bool globalPositions) {
            // Get number of existing rotation points.
            var rotationPointsNo = pathData.RotationPath.KeysNo;
            // Result array.
            var rotationPointPositions = new Vector3[rotationPointsNo];

            // For each rotation point..
            for (var i = 0; i < rotationPointsNo; i++) {
                // Get rotation point local position.
                var localPos = GetRotationPointPosition(i);

                // If global position arg. is true..
                if (globalPositions) {
                    // Convert position to global coordinate.
                    rotationPointPositions[i] =
                        transform.TransformPoint(localPos);
                }
                else {
                    // Add local position.
                    rotationPointPositions[i] = localPos;
                }
            }

            return rotationPointPositions;
        }


        private void HandleAnimatedGORotation() {
            if (animatedGO == null) return;

            // Look at target.
            if (targetGO != null
                && rotationMode == AnimatorRotationMode.Target) {
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
            if (rotationMode == AnimatorRotationMode.Custom) {
                RotateObjectWithAnimationCurves();
            }
            // Look forward.
            else if (rotationMode == AnimatorRotationMode.Forward) {
                var globalForwardPoint = GetForwardPoint(true);

                // In play mode..
                if (Application.isPlaying) {
                    RotateObjectWithSlerp(globalForwardPoint);
                }
                else {
                    RotateObjectWithLookAt(globalForwardPoint);
                }
            }
        }

        private void InitializeEaseCurve() {
            var firstKey = new Keyframe(0, 0, 0, 0);
            var lastKey = new Keyframe(1, 1, 0, 0);

            PathData.EaseCurve.AddKey(firstKey);
            PathData.EaseCurve.AddKey(lastKey);
        }

        private void InitializeRotationCurve() {
            var firstKey = new Keyframe(0, 0, 0, 0);
            var lastKey = new Keyframe(1, 0, 0, 0);

            PathData.TiltingCurve.AddKey(firstKey);
            PathData.TiltingCurve.AddKey(lastKey);
        }


        private void RotateObjectWithAnimationCurves() {
            var lookAtTarget =
                PathData.RotationPath.GetVectorAtTime(animTimeRatio);
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
            // Return when point to look at is at the same position as the
            // animated object.
            if (targetPosition == animatedGO.position) return;

            // Calculate direction to target.
            var targetDirection = targetPosition - animatedGO.position;
            // Calculate rotation to target.
            var rotation = Quaternion.LookRotation(targetDirection);
            // Calculate rotation speed.
            var speed = Time.deltaTime*rotationSpeed;

            // Lerp rotation.
            animatedGO.rotation = Quaternion.Slerp(
                animatedGO.rotation,
                rotation,
                speed);
        }

        private void TiltObject() {
            if (animatedGO == null) return;

            // Get current animatedGO rotation.
            var eulerAngles = animatedGO.rotation.eulerAngles;
            // Get rotation from tiltingCurve.
            var zRotation = PathData.TiltingCurve.Evaluate(animTimeRatio);
            // Update value on Z axis.
            eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, zRotation);
            // Update animatedGO rotation.
            animatedGO.rotation = Quaternion.Euler(eulerAngles);
        }

        private void UpdateAnimatedGOPosition() {
            // Get animatedGO position at current animation time.
            var positionAtTimestamp =
                animationPathBuilder.GetVectorAtTime(animTimeRatio);
            var globalPositionAtTimestamp =
                transform.TransformPoint(positionAtTimestamp);

            // Update animatedGO position.
            animatedGO.position = globalPositionAtTimestamp;
        }

        private void UpdateAnimatedGORotation() {
            if (animatedGO == null) return;

            switch (rotationMode) {
                case AnimatorRotationMode.Forward:
                    var globalForwardPoint = GetForwardPoint(true);

                    RotateObjectWithLookAt(globalForwardPoint);

                    break;

                case AnimatorRotationMode.Custom:
                    // Get rotation point position.
                    var rotationPointPos =
                        PathData.RotationPath.GetVectorAtTime(animTimeRatio);

                    // Convert target position to global coordinates.
                    var rotationPointGlobalPos =
                        transform.TransformPoint(rotationPointPos);

                    // Update animatedGO rotation.
                    RotateObjectWithLookAt(rotationPointGlobalPos);

                    break;

                case AnimatorRotationMode.Target:
                    if (targetGO == null) return;

                    RotateObjectWithLookAt(targetGO.position);
                    break;
            }
        }

        private void UpdateCurveTimestamps(AnimationCurve curve) {
            // Get node timestamps.
            var pathNodeTimestamps = animationPathBuilder.GetNodeTimestamps();
            // For each key in easeCurve..
            for (var i = 1; i < curve.length - 1; i++) {
                // If resp. node timestamp is different from easeCurve
                // timestamp..
                if (Math.Abs(pathNodeTimestamps[i] - curve.keys[i].value) >
                    FloatPrecision) {
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
        ///     Update AnimationCurve with keys added to the path.
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
            foreach (var nodeTimestamp in nodeTimestamps) {
                var valueExists = curveTimestamps.Any(t =>
                    Math.Abs(nodeTimestamp - t) < FloatPrecision);

                // Add missing key.
                if (valueExists) continue;

                AddKeyToCurve(curve, nodeTimestamp);
                SmoothCurve(curve);

                break;
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
                // Check if key at this timestamp exists..
                var keyExists = nodeTimestamps.Any(t =>
                    Math.Abs(curveTimestamps[i] - t) < FloatPrecision);

                if (keyExists) continue;

                curve.RemoveKey(i);
                SmoothCurve(curve);

                break;
            }
        }

        private void UpdateRotationCurvesTimestamps() {
            // Get node timestamps.
            var nodeTimestamps = animationPathBuilder.GetNodeTimestamps();
            // Get rotation point timestamps.
            var rotationCurvesTimestamps =
                PathData.RotationPath.GetTimestamps();

            // For each node in rotationPath..
            for (var i = 1; i < PathData.RotationPath.KeysNo - 1; i++) {
                // If resp. path node timestamp is different from rotation
                // point timestamp..
                if (Math.Abs(nodeTimestamps[i] - rotationCurvesTimestamps[i])
                    > FloatPrecision) {
                    // Update rotation point timestamp.
                    PathData.RotationPath.ChangeNodeTimestamp(
                        i,
                        nodeTimestamps[i]);
                }
            }
        }
        private void UpdateRotationCurvesWithAddedKeys() {
            // AnimationPathBuilder node timestamps.
            var animationCurvesTimestamps =
                animationPathBuilder.GetNodeTimestamps();
            // Get values from rotationPath.
            var rotationCurvesTimestamps = PathData.RotationPath.GetTimestamps();
            var rotationCurvesKeysNo = rotationCurvesTimestamps.Length;

            // For each timestamp in rotationPath..
            for (var i = 0; i < animationCurvesTimestamps.Length; i++) {
                var keyExists = false;
                for (var j = 0; j < rotationCurvesKeysNo; j++) {
                    if (Math.Abs(rotationCurvesTimestamps[j]
                                 - animationCurvesTimestamps[i]) <
                        FloatPrecision) {
                        keyExists = true;
                        break;
                    }
                }

                if (!keyExists) {
                    var addedKeyTimestamp =
                        animationPathBuilder.GetNodeTimestamp(i);
                    var defaultRotation =
                        PathData.RotationPath.GetVectorAtTime(addedKeyTimestamp);

                    PathData.RotationPath.CreateNewNode(
                        animationCurvesTimestamps[i],
                        defaultRotation);
                }
            }
        }

        private void UpdateRotationCurvesWithRemovedKeys() {
            // AnimationPathBuilder node timestamps.
            var pathTimestamps = animationPathBuilder.GetNodeTimestamps();
            // Get values from rotationPath.
            var rotationCurvesTimestamps = PathData.RotationPath.GetTimestamps();

            // For each timestamp in rotationPath..
            for (var i = 0; i < rotationCurvesTimestamps.Length; i++) {
                // Check if same timestamp exist in rotationPath.
                var keyExists = pathTimestamps.Any(nodeTimestamp =>
                    Math.Abs(rotationCurvesTimestamps[i] - nodeTimestamp)
                    < FloatPrecision);

                // If key exists check next timestamp.
                if (keyExists) continue;

                // Remove node from rotationPath.
                PathData.RotationPath.RemoveNode(i);

                break;
            }
        }

        #endregion PRIVATE METHODS
    }
}