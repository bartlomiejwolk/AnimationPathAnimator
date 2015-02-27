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
    [RequireComponent(typeof (AnimationPathBuilder))]
    [ExecuteInEditMode]
    public class AnimationPathAnimator : GameComponent {
        #region FIELDS

        [SerializeField]
#pragma warning disable 169
            private bool advancedSettingsFoldout;

        /// <summary>
        ///     Transform to be animated.
        /// </summary>
        [SerializeField]
        private Transform animatedGO;

        /// <summary>
        ///     Path used to animate the <c>animatedGO</c> transform.
        /// </summary>
        [SerializeField]
        private AnimationPathBuilder animationPathBuilder;

        /// Current play time represented as a number between 0 and 1.
        [SerializeField]
        private float animTimeRatio;

        [SerializeField]
        private GizmoDrawer gizmoDrawer;

#pragma warning restore 169

        [SerializeField]
        private PathData pathData;

        [SerializeField]
        private GUISkin skin;

        /// <summary>
        ///     Transform that the <c>animatedGO</c> will be looking at.
        /// </summary>
        [SerializeField]
#pragma warning disable 649
            private Transform targetGO;

#pragma warning restore 649

        #region OPTIONS

        [SerializeField]
#pragma warning disable 169
            protected bool EnableControlsInPlayMode = true;

        [SerializeField]
#pragma warning disable 169
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
        // ReSharper disable once FieldCanBeMadeReadOnly.Local ReSharper
        // disable once ConvertToConstant.Local
        private float forwardPointOffset = 0.05f;

        [SerializeField]
        private AnimatorHandleMode handleMode =
            AnimatorHandleMode.None;

        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local ReSharper
        // disable once ConvertToConstant.Local
        private float positionLerpSpeed = 0.1f;

        [SerializeField]
        private AnimatorRotationMode rotationMode =
            AnimatorRotationMode.Forward;

        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local ReSharper
        // disable once ConvertToConstant.Local
        private float rotationSpeed = 3.0f;

        [SerializeField]
        private bool updateAllMode;

        [SerializeField]
        private WrapMode wrapMode = WrapMode.Clamp;

#pragma warning restore 169
#pragma warning restore 169

        #endregion OPTIONS

        #endregion FIELDS

        #region PROPERTIES

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

        public virtual float FloatPrecision {
            get { return 0.001f; }
        }

        public GizmoDrawer GizmoDrawer {
            get { return gizmoDrawer; }
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
            // Subscribe to events.
            //animationPathBuilder.PathReset += animationPathBuilder_PathReset;
            pathData.RotationPointPositionChanged += pathData_RotationPointPositionChanged;

            // TODO First unsubscribe from events. Make separate method.
            //if (pathData != null) {
            //    PathData.NodeTimeChanged +=
            //        animationPathBuilder_NodeTimeChanged;
            //    PathData.NodeRemoved += animationPathBuilder_NodeRemoved;
            //    PathData.NodePositionChanged +=
            //        animationPathBuilder_NodePositionChanged;
            //    PathData.NodeAdded += animationPathBuilder_NodeAdded;
            //    pathData.NodeTiltChanged += this_NodeTiltChanged;
            //}
        }
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Awake() {
            //InitializeEaseCurve();
            //InitializeRotationCurve();

            // Initialize animatedGO field.
            if (animatedGO == null && Camera.main.transform != null) {
                animatedGO = Camera.main.transform;
            }

            gizmoDrawer = ScriptableObject.CreateInstance<GizmoDrawer>();
            // Initialize AnimationPathBuilder field.
            animationPathBuilder = GetComponent<AnimationPathBuilder>();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnDisable() {
            //animationPathBuilder.PathReset -= animationPathBuilder_PathReset;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        // TODO Refactor.
        private void OnDrawGizmosSelected() {
            // Return if path asset file is not assigned.
            if (PathData == null) return;

            if (rotationMode == AnimatorRotationMode.Target
                && targetGO != null) {
                GizmoDrawer.DrawTargetIcon(targetGO.position);
            }

            if (rotationMode == AnimatorRotationMode.Forward) {
                var globalForwardPointPosition = GetForwardPoint(true);
                GizmoDrawer.DrawForwardPointIcon(globalForwardPointPosition);
            }

            // Return if handle mode is not rotation mode.
            if (handleMode == AnimatorHandleMode.Rotation) {
                var localPointPositions =
                    PathData.SampleRotationPathForPoints(
                        RotationCurveSampling);

                var globalPointPositions =
                    new Vector3[localPointPositions.Count];

                for (var i = 0; i < localPointPositions.Count; i++) {
                    globalPointPositions[i] =
                        transform.TransformPoint(localPointPositions[i]);
                }
                GizmoDrawer.DrawRotationGizmoCurve(globalPointPositions);

                //GizmoDrawer.DrawCurrentRotationPointGizmo();
                HandleDrawingCurrentRotationPointGizmo();

                DrawRotationPointGizmos();
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnValidate() {
            // Subscribe to event.
            //if (pathData != null) {
            //    pathData.NodeTiltChanged -= this_NodeTiltChanged;
            //    pathData.NodeTiltChanged += this_NodeTiltChanged;
            //}
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
            // In play mode, update animation time with delta time.
            if (Application.isPlaying && IsPlaying && !Pause) {
                Animate();
            }
        }

        #endregion UNITY MESSAGES

        #region EVENT HANDLERS
        void pathData_RotationPointPositionChanged(object sender, EventArgs e) {
            UpdateAnimatedGO();
        }

        #endregion EVENT HANDLERS

        #region METHODS

        public void Animate() {
            AnimateObject();
            HandleAnimatedGORotation();
            TiltObject();
        }

        // TODO Remove the globalPosition arg. and create separate method.
        public Vector3 GetForwardPoint(bool globalPosition) {
            // Timestamp offset of the forward point.
            var forwardPointDelta = forwardPointOffset;
            // Forward point timestamp.
            var forwardPointTimestamp = animTimeRatio + forwardPointDelta;
            var localPosition =
                animationPathBuilder.PathData.GetVectorAtTime(forwardPointTimestamp);

            // Return global position.
            if (globalPosition) {
                return transform.TransformPoint(localPosition);
            }

            return localPosition;
        }

        public Vector3 GetGlobalNodePosition(int nodeIndex) {
            var localNodePosition =
                animationPathBuilder.PathData.GetNodePosition(nodeIndex);
            var globalNodePosition = transform.TransformPoint(localNodePosition);

            return globalNodePosition;
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

        private void AnimateObject() {
            if (animatedGO == null
                || animationPathBuilder == null) {
                return;
            }

            var positionAtTimestamp =
                animationPathBuilder.PathData.GetVectorAtTime(animTimeRatio);

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

                GizmoDrawer.DrawRotationPointGizmo(rotationPointPositions[i]);
            }
        }

        // ReSharper disable once UnusedMember.Local
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
                globalPositions[i] = transform.TransformPoint(localPositions[i]);
            }

            return globalPositions;
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
                transform.TransformPoint(localRotationPointPosition);
            GizmoDrawer.DrawCurrentRotationPointGizmo(
                globalRotationPointPosition);
        }

        private void RotateObjectWithAnimationCurves() {
            var lookAtTarget =
                PathData.GetRotationAtTime(animTimeRatio);
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
            //var zRotation = PathData.TiltingCurve.Evaluate(animTimeRatio);
            var zRotation = PathData.GetTiltingValueAtTime(animTimeRatio);
            // Update value on Z axis.
            eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, zRotation);
            // Update animatedGO rotation.
            animatedGO.rotation = Quaternion.Euler(eulerAngles);
        }

        private void UpdateAnimatedGOPosition() {
            // Get animatedGO position at current animation time.
            var positionAtTimestamp =
                animationPathBuilder.PathData.GetVectorAtTime(animTimeRatio);
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
                        PathData.GetRotationValueAtTime(animTimeRatio);

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

        #endregion METHODS
    }

}