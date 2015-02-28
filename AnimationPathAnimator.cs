using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ATP.ReorderableList;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace ATP.AnimationPathTools {

    /// <summary>
    ///     Component that allows animating transforms position along predefined
    ///     Animation Paths and also animate their rotation on x and y axis in
    ///     time.
    /// </summary>
    [ExecuteInEditMode]
    public class AnimationPathAnimator : GameComponent {
        #region EVENT HANDLERS

        private void PathData_RotationPointPositionChanged(
            object sender,
            EventArgs e) {
            UpdateAnimatedGO();
        }

        #endregion EVENT HANDLERS

        #region FIELDS

        public const int GizmoCurveSamplingFrequency = 20;

        [SerializeField]
#pragma warning disable 169
            private bool advancedSettingsFoldout;

        /// <summary>
        ///     Transform to be animated.
        /// </summary>
        [SerializeField]
        private Transform animatedGO;

        /// Current play time represented as a number between 0 and 1.
        [SerializeField]
        private float animTimeRatio;

        [SerializeField]
        private int exportSamplingFrequency = 5;

        [SerializeField]
        private AnimatorGizmos animatorGizmos;

#pragma warning restore 169

        [SerializeField]
        private PathData pathData;

        [SerializeField]
        private GUISkin skin;

        /// <summary>
        ///     Transform that the <c>animatedGO</c> will be looking at.
        /// </summary>
        [SerializeField]
        private Transform targetGO;

        private Transform Transform { get; set; }

        #region OPTIONS

        [SerializeField]
        protected bool EnableControlsInPlayMode = true;

        [SerializeField]
        protected float MaxAnimationSpeed = 0.3f;

        /// <summary>
        ///     Value of the jump when modifier key is pressed.
        /// </summary>
        [SerializeField]
        private readonly float shortJumpValue = 0.002f;

        [SerializeField]
        private bool autoPlay = true;

        /// <summary>
        ///     How much look forward point should be positioned away from the
        ///     animated object.
        /// </summary>
        /// <remarks>Value is a time in range from 0 to 1.</remarks>
        [SerializeField]
        private float forwardPointOffset = 0.05f;

        /// <summary>
        ///     Color of the gizmo curve.
        /// </summary>
        [SerializeField]
        private Color gizmoCurveColor = Color.yellow;

        [SerializeField]
        private AnimatorHandleMode handleMode =
            AnimatorHandleMode.None;

        [SerializeField]
        private AnimationPathBuilderHandleMode movementMode =
            AnimationPathBuilderHandleMode.MoveAll;

        [SerializeField]
        private float positionLerpSpeed = 0.1f;

        [SerializeField]
        private AnimatorRotationMode rotationMode =
            AnimatorRotationMode.Forward;

        [SerializeField]
        private float rotationSpeed = 3.0f;

        [SerializeField]
        private AnimationPathBuilderTangentMode tangentMode =
            AnimationPathBuilderTangentMode.Smooth;

        [SerializeField]
        private bool updateAllMode;

        [SerializeField]
        private WrapMode wrapMode = WrapMode.Clamp;
#pragma warning disable 0414
#pragma warning restore 0414

        #endregion OPTIONS

        #endregion FIELDS

        #region PROPERTIES

        public float AnimationTimeRatio {
            get { return animTimeRatio; }
            set {
                animTimeRatio = value;

                if (Application.isPlaying) UpdateAnimatedGO();
                if (!Application.isPlaying) Animate();
            }
        }

        public bool AutoPlay {
            get { return autoPlay; }
            set { autoPlay = value; }
        }

        public virtual float FloatPrecision {
            get { return 0.001f; }
        }

        /// <summary>
        ///     Color of the gizmo curve.
        /// </summary>
        public Color GizmoCurveColor {
            get { return gizmoCurveColor; }
            set { gizmoCurveColor = value; }
        }

        public AnimatorGizmos AnimatorGizmos {
            get { return animatorGizmos; }
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
        public bool IsPlaying { get; set; }

        public AnimationPathBuilderHandleMode MovementMode {
            get { return movementMode; }
            set { movementMode = value; }
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

        /// <summary>
        ///     Value of the jump when modifier key is pressed.
        /// </summary>
        public virtual float ShortJumpValue {
            get { return shortJumpValue; }
        }

        public GUISkin Skin {
            get { return skin; }
            set { skin = value; }
        }

        public AnimationPathBuilderTangentMode TangentMode {
            get { return tangentMode; }
            set { tangentMode = value; }
        }

        public bool UpdateAllMode {
            get { return updateAllMode; }
            set { updateAllMode = value; }
        }

        public WrapMode WrapMode {
            get { return wrapMode; }
            set { wrapMode = value; }
        }

        protected virtual int RotationCurveSampling {
            get { return 20; }
        }

        #endregion PROPERTIES

        #region UNITY MESSAGES

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        public virtual void OnEnable() {
            Transform = GetComponent<Transform>();

            if (pathData != null) {
                pathData.RotationPointPositionChanged +=
                    PathData_RotationPointPositionChanged;

                PathData.NodePositionChanged += PathData_NodePositionChanged;

                pathData.NodeTiltChanged += pathData_NodeTiltChanged;
            }

            if (animatorGizmos == null) {
                animatorGizmos = ScriptableObject.CreateInstance<AnimatorGizmos>();
            }
        }

        void pathData_NodeTiltChanged(object sender, EventArgs e) {
            if (Application.isPlaying) UpdateAnimatedGO();
            if (!Application.isPlaying) Animate();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Awake() {
            skin = Resources.Load("GUISkin/default") as GUISkin;

            // Initialize animatedGO field.
            if (animatedGO == null && Camera.main.transform != null) {
                animatedGO = Camera.main.transform;
            }

            animatorGizmos = ScriptableObject.CreateInstance<AnimatorGizmos>();
        }

        void PathData_NodePositionChanged(object sender, EventArgs e) {
            if (Application.isPlaying) UpdateAnimatedGO();
            if (!Application.isPlaying) Animate();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        // TODO Refactor.
        private void OnDrawGizmosSelected() {
            // Return if path asset file is not assigned.
            if (PathData == null) return;

            if (rotationMode == AnimatorRotationMode.Target
                && targetGO != null) {
                AnimatorGizmos.DrawTargetIcon(targetGO.position);
            }

            if (rotationMode == AnimatorRotationMode.Forward) {
                var globalForwardPointPosition = GetForwardPoint(true);
                AnimatorGizmos.DrawForwardPointIcon(globalForwardPointPosition);
            }

            if (handleMode == AnimatorHandleMode.Rotation) {
                var localPointPositions =
                    PathData.SampleRotationPathForPoints(
                        RotationCurveSampling);

                var globalPointPositions =
                    new Vector3[localPointPositions.Count];

                for (var i = 0; i < localPointPositions.Count; i++) {
                    globalPointPositions[i] =
                        Transform.TransformPoint(localPointPositions[i]);
                }
                AnimatorGizmos.DrawRotationGizmoCurve(globalPointPositions);

                HandleDrawingCurrentRotationPointGizmo();

                DrawRotationPointGizmos();
            }

            HandleDrawingGizmoCurve();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Start() {
            if (Application.isPlaying && autoPlay) {
                IsPlaying = true;

                StartEaseTimeCoroutine();
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Update() {
            if (Application.isPlaying && IsPlaying && !Pause) {
                Animate();
            }
        }

        #endregion UNITY MESSAGES

        #region HANDLERS
        private void HandleDrawingCurrentRotationPointGizmo() {
            // Get current animation time.
            var currentAnimationTime = AnimationTimeRatio;

            // Node path node timestamps.
            var nodeTimestamps = PathData.GetPathTimestamps();

            // Return if current animation time is the same as any node time.
            if (nodeTimestamps.Any(
                nodeTimestamp =>
                    Math.Abs(nodeTimestamp - currentAnimationTime)
                    < FloatPrecision)) {
                return;
            }

            // Get rotation point position.
            var localRotationPointPosition =
                PathData.GetRotationAtTime(currentAnimationTime);
            var globalRotationPointPosition =
                Transform.TransformPoint(localRotationPointPosition);
            AnimatorGizmos.DrawCurrentRotationPointGizmo(
                globalRotationPointPosition);
        }

        // Move to AnimatorGizmos class.
        private void HandleDrawingGizmoCurve() {
            // Return if path asset is not assigned.
            if (pathData == null) return;

            // Get transform component.
            //var transform = GetComponent<Transform>();

            // Get path points.
            var points = pathData.SampleAnimationPathForPoints(
                GizmoCurveSamplingFrequency);

            // Convert points to global coordinates.
            var globalPoints = new Vector3[points.Count];
            for (var i = 0; i < points.Count; i++) {
                globalPoints[i] = Transform.TransformPoint(points[i]);
            }

            // There must be at least 3 points to draw a line.
            if (points.Count < 3) return;

            Gizmos.color = gizmoCurveColor;

            // Draw curve.
            for (var i = 0; i < points.Count - 1; i++) {
                Gizmos.DrawLine(globalPoints[i], globalPoints[i + 1]);
            }
        }

        private void HandleUpdateAnimatedGORotation() {
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

        #endregion

        #region METHODS

        public void Animate() {
            AnimateObjectPosition();
            HandleUpdateAnimatedGORotation();
            TiltObject();
        }

        // TODO Remove the globalPosition arg. and create separate method.
        public Vector3 GetForwardPoint(bool globalPosition) {
            // Timestamp offset of the forward point.
            var forwardPointDelta = forwardPointOffset;
            // Forward point timestamp.
            var forwardPointTimestamp = animTimeRatio + forwardPointDelta;
            var localPosition = PathData.GetVectorAtTime(forwardPointTimestamp);

            // Return global position.
            if (globalPosition) {
                return Transform.TransformPoint(localPosition);
            }

            return localPosition;
        }

        public Vector3 GetGlobalNodePosition(int nodeIndex) {
            var localNodePosition = PathData.GetNodePosition(nodeIndex);
            var globalNodePosition = Transform.TransformPoint(localNodePosition);

            return globalNodePosition;
        }

        public Vector3[] GetGlobalNodePositions() {
            var nodePositions = PathData.GetNodePositions();

            for (var i = 0; i < nodePositions.Length; i++) {
                // Convert each position to global coordinate.
                nodePositions[i] = Transform.TransformPoint(nodePositions[i]);
            }

            return nodePositions;
        }

        public void StartEaseTimeCoroutine() {
            // Check for play mode.
            StartCoroutine("EaseTime");
        }

        public void StopEaseTimeCoroutine() {
            StopCoroutine("EaseTime");

            // Reset animation.
            IsPlaying = false;
            Pause = false;
            animTimeRatio = 0;
        }

        /// <summary>
        ///     Update animatedGO position, rotation and tilting based on current
        ///     animTimeRatio.
        /// </summary>
        /// <remarks>
        ///     Used to update animatedGO with keys, in play mode.
        /// </remarks>
        public void UpdateAnimatedGO() {
            UpdateAnimatedGOPosition();
            UpdateAnimatedGORotation();
            // Update animatedGO tilting.
            TiltObject();
        }

        public void UpdateWrapMode() {
            PathData.SetWrapMode(wrapMode);
        }

        private void AnimateObjectPosition() {
            if (animatedGO == null) return;

            var positionAtTimestamp = PathData.GetVectorAtTime(animTimeRatio);

            var globalPositionAtTimestamp =
                Transform.TransformPoint(positionAtTimestamp);

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
        private void DrawRotationPointGizmos() {
            var rotationPointPositions = GetGlobalRotationPointPositions();

            // Path node timestamps.
            var nodeTimestamps = PathData.GetPathTimestamps();

            for (var i = 0; i < rotationPointPositions.Length; i++) {
                // Return if current animation time is the same as any node
                // time.
                if (Math.Abs(nodeTimestamps[i] - AnimationTimeRatio) <
                    FloatPrecision) {
                    continue;
                }

                AnimatorGizmos.DrawRotationPointGizmo(rotationPointPositions[i]);
            }
        }

        private IEnumerator EaseTime() {
            while (true) {
                // If animation is not paused..
                if (!Pause) {
                    // Ease time.
                    var timeStep = PathData.GetEaseValueAtTime(animTimeRatio);
                    animTimeRatio += timeStep * Time.deltaTime;
                }

                yield return null;
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private Vector3[] GetGlobalRotationPointPositions() {
            var localPositions = PathData.GetRotationPointPositions();
            var globalPositions = new Vector3[localPositions.Length];

            for (var i = 0; i < localPositions.Length; i++) {
                globalPositions[i] = Transform.TransformPoint(localPositions[i]);
            }

            return globalPositions;
        }
        private void RotateObjectWithAnimationCurves() {
            var lookAtTarget =
                PathData.GetRotationAtTime(animTimeRatio);
            // Convert target position to global coordinates.
            var lookAtTargetGlobal = Transform.TransformPoint(lookAtTarget);

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
            var speed = Time.deltaTime * rotationSpeed;

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
            var zRotation = PathData.GetTiltingValueAtTime(animTimeRatio);
            // Update value on Z axis.
            eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, zRotation);
            // Update animatedGO rotation.
            animatedGO.rotation = Quaternion.Euler(eulerAngles);
        }

        private void UpdateAnimatedGOPosition() {
            // Get animatedGO position at current animation time.
            var positionAtTimestamp = PathData.GetVectorAtTime(animTimeRatio);
            var globalPositionAtTimestamp =
                Transform.TransformPoint(positionAtTimestamp);

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
                        PathData.GetRotationValueAtTime(animTimeRatio);

                    // Convert target position to global coordinates.
                    var rotationPointGlobalPos =
                        Transform.TransformPoint(rotationPointPos);

                    // Update animatedGO rotation.
                    RotateObjectWithLookAt(rotationPointGlobalPos);

                    break;

                case AnimatorRotationMode.Target:
                    if (targetGO == null) return;

                    RotateObjectWithLookAt(targetGO.position);
                    break;
            }
        }

        #endregion METHODS
    }

}