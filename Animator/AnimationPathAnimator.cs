using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace ATP.SimplePathAnimator.Animator {

    /// <summary>
    ///     Component that allows animating transforms position along predefined
    ///     Animation Paths and also animate their rotation on x and y axis in
    ///     time.
    /// </summary>
    [ExecuteInEditMode]
    public class AnimationPathAnimator : GameComponent {
        #region EVENTS

        public event EventHandler<NodeReachedEventArgs> NodeReached;

        #endregion

        protected virtual void OnNodeReached(NodeReachedEventArgs eventArgs) {
            var handler = NodeReached;
            if (handler != null) handler(this, eventArgs);
        }

        #region FIELDS

        [SerializeField]
        private bool advancedSettingsFoldout;

        /// <summary>
        ///     Transform to be animated.
        /// </summary>
        [SerializeField]
        private Transform animatedGO;

        /// Current play time represented as a number between 0 and 1.
        [SerializeField]
        private float animationTimeRatio;

        [SerializeField]
        private AnimatorGizmos animatorGizmos;

        [SerializeField]
        private PathData pathData;

        [SerializeField]
        private AnimatorSettings settings;

        [SerializeField]
        private GUISkin skin;

        /// <summary>
        ///     Transform that the <c>animatedGO</c> will be looking at.
        /// </summary>
        [SerializeField]
        private Transform targetGO;

        #endregion OPTIONS

        #region PROPERTIES

        public float AnimationTimeRatio {
            get { return animationTimeRatio; }
            set {
                animationTimeRatio = value;

                // Limit animationTimeRatio in edit mode.
                if (!Application.isPlaying && animationTimeRatio > 1) {
                    animationTimeRatio = 1;
                }

                // Animate while animation is running.
                if (Application.isPlaying && IsPlaying && !Pause) {
                    Animate();
                    HandleFireNodeReachedEvent();
                }
                // Update animation with keys while animation is stopped or paused.
                else {
                    UpdateAnimation();
                }
            }
        }

        public AnimatorGizmos AnimatorGizmos {
            get { return animatorGizmos; }
        }

        /// <summary>
        ///     If animation is currently enabled.
        /// </summary>
        /// <remarks>When it's true, it means that the EaseTime coroutine is running.</remarks>
        public bool IsPlaying { get; set; }

        public PathData PathData {
            get { return pathData; }
            set { pathData = value; }
        }

        public bool Pause { get; set; }

        public AnimatorSettings Settings {
            get { return settings; }
            set { settings = value; }
        }

        public GUISkin Skin {
            get { return skin; }
            set { skin = value; }
        }

        [SerializeField]
        private Transform thisTransform;

        public Transform ThisTransform {
            get { return thisTransform; }
        }

        private void HandleFireNodeReachedEvent() {
            // Get path timestamps.
            var nodeTimestamps = PathData.GetPathTimestamps();

            // Compare current AnimationTimeRatio to node timestamps.
            var index = Array.FindIndex(
                nodeTimestamps,
                x => Math.Abs(x - AnimationTimeRatio)
                     < GlobalConstants.FloatPrecision);

            // Return if current AnimationTimeRatio is not equal to any node
            // timestamp.
            if (index < 0) return;

            // Create event args.
            var args = new NodeReachedEventArgs(index, AnimationTimeRatio);

            // Fire event.
            OnNodeReached(args);
        }

        #endregion PROPERTIES

        #region UNITY MESSAGES

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        public virtual void OnEnable() {
            thisTransform = GetComponent<Transform>();
            Settings = Resources.Load("Settings/Animator") as AnimatorSettings;
            skin = Resources.Load("GUISkin/Animator") as GUISkin;

            // Initialize animatedGO field.
            if (animatedGO == null && Camera.main != null) {
                animatedGO = Camera.main.transform;
            }

            animatorGizmos = ScriptableObject.CreateInstance<AnimatorGizmos>();
            animatorGizmos.Init(Settings);

            if (pathData != null) {
                PathData.RotationPointPositionChanged +=
                    PathData_RotationPointPositionChanged;

                PathData.NodePositionChanged += PathData_NodePositionChanged;
                PathData.NodeTiltChanged += PathData_NodeTiltChanged;
                PathData.PathReset += PathData_PathReset;
                PathData.RotationPathReset += PathData_RotationPathReset;
            }

            if (animatorGizmos == null) {
                animatorGizmos =
                    ScriptableObject.CreateInstance<AnimatorGizmos>();
            }
        }

        private void OnDisable() {
            PathData.NodePositionChanged -= PathData_NodePositionChanged;
            PathData.NodeTiltChanged -= PathData_NodeTiltChanged;
            PathData.PathReset -= PathData_PathReset;
            PathData.RotationPathReset -= PathData_RotationPathReset;
        }

        void PathData_RotationPathReset(object sender, EventArgs e) {
            Settings.RotationMode = AnimatorRotationMode.Custom;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Awake() {

        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        // TODO Refactor.
        private void OnDrawGizmosSelected() {
            // Return if path asset file is not assigned.
            if (PathData == null) return;

            if (Settings.RotationMode == AnimatorRotationMode.Target
                && targetGO != null) {
                AnimatorGizmos.DrawTargetIcon(targetGO.position);
            }

            if (Settings.RotationMode == AnimatorRotationMode.Forward) {
                var globalForwardPointPosition = GetGlobalForwardPoint();
                AnimatorGizmos.DrawForwardPointIcon(globalForwardPointPosition);
            }

            if (Settings.HandleMode == AnimatorHandleMode.Rotation) {
                AnimatorGizmos.DrawRotationPathCurve(PathData, transform);

                AnimatorGizmos.DrawCurrentRotationPointGizmo(
                    PathData,
                    transform,
                    AnimationTimeRatio);

                AnimatorGizmos.DrawRotationPointGizmos(
                    PathData,
                    transform,
                    AnimationTimeRatio);
            }

            AnimatorGizmos.DrawAnimationCurve(PathData, transform);
        }

        private void OnValidate() {

        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Start() {
            if (Application.isPlaying && Settings.AutoPlay) {
                StartEaseTimeCoroutine();
                IsPlaying = true;
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Update() {
        }

        #endregion UNITY MESSAGES

        #region EVENT HANDLERS

        private void PathData_NodePositionChanged(object sender, EventArgs e) {
            UpdateAnimation();
        }

        private void PathData_NodeTiltChanged(object sender, EventArgs e) {
            UpdateAnimation();
        }

        private void PathData_PathReset(object sender, EventArgs e) {
            AnimationTimeRatio = 0;
            Settings.MovementMode = Settings.MovementModeAfterReset;
            UpdateAnimation();
        }

        private void PathData_RotationPointPositionChanged(
            object sender,
            EventArgs e) {

            UpdateAnimation();
        }

        #endregion

        #region ANIMATION

        public void Animate() {
            AnimateAnimatedGOPosition();
            AnimateAnimatedGORotation();
            AnimateAnimatedGOTilting();
        }

        public void HandlePlayPause() {
            if (!Application.isPlaying) return;

            // Pause animation.
            if (IsPlaying && !Pause) {
                Pause = true;
                //Script.IsPlaying = false;
            }
            // Unpause animation.
            else if (IsPlaying && Pause) {
                Pause = false;
                //Script.IsPlaying = true;
            }
            // Start animation.
            else {
                IsPlaying = true;
                // Start animation.
                StartEaseTimeCoroutine();
            }
        }

        public void StartEaseTimeCoroutine() {
            if (PathData == null) {
                Debug.LogWarning("Assign Path Asset in the inspector.");
                return;
            }
            // Starting node was reached.
            if (AnimationTimeRatio == 0) {
                var args = new NodeReachedEventArgs(0, 0);
                OnNodeReached(args);
            }

            // Check for play mode.
            StartCoroutine("EaseTime");
        }

        public void StopEaseTimeCoroutine() {
            StopCoroutine("EaseTime");

            // Reset animation.
            IsPlaying = false;
            Pause = false;
            AnimationTimeRatio = 0;
        }

        /// <summary>
        ///     Update animatedGO position, rotation and tilting based on current
        ///     AnimationTimeRatio.
        /// </summary>
        /// <remarks>
        ///     Used to update animatedGO with keys, in play mode.
        /// </remarks>
        public void UpdateAnimation() {
            if (PathData == null) return;

            UpdateAnimatedGOPosition();
            UpdateAnimatedGORotation();
            AnimateAnimatedGOTilting();
        }

        private void AnimateAnimatedGOPosition() {
            if (animatedGO == null) return;

            var localPosAtTime =
                PathData.GetVectorAtTime(AnimationTimeRatio);

            var globalPosAtTime =
                ThisTransform.TransformPoint(localPosAtTime);

            // Update position.
            animatedGO.position = Vector3.Lerp(
                animatedGO.position,
                globalPosAtTime,
                Settings.PositionLerpSpeed);

        }

        private void AnimateAnimatedGORotation() {
            if (animatedGO == null) return;

            // Look at target.
            if (targetGO != null
                && Settings.RotationMode == AnimatorRotationMode.Target) {

                RotateObjectWithSlerp(targetGO.position);
            }
            // Use rotation path.
            if (Settings.RotationMode == AnimatorRotationMode.Custom) {
                RotateObjectWithAnimationCurves();
            }
            // Look forward.
            else if (Settings.RotationMode == AnimatorRotationMode.Forward) {
                var globalForwardPoint = GetGlobalForwardPoint();

                RotateObjectWithSlerp(globalForwardPoint);
            }
        }

        private void AnimateAnimatedGOTilting() {
            if (animatedGO == null) return;

            // Get current animatedGO rotation.
            var eulerAngles = animatedGO.rotation.eulerAngles;
            // Get rotation from tiltingCurve.
            var zRotation = PathData.GetTiltingValueAtTime(AnimationTimeRatio);
            // Update value on Z axis.
            eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, zRotation);
            // Update animatedGO rotation.
            animatedGO.rotation = Quaternion.Euler(eulerAngles);
        }

        private IEnumerator EaseTime() {
            while (true) {
                // If animation is enabled and not paused..
                if (!Pause) {
                    // Ease time.
                    var timeStep =
                        PathData.GetEaseValueAtTime(AnimationTimeRatio);
                    AnimationTimeRatio += timeStep * Time.deltaTime;

                    if (AnimationTimeRatio > 1
                        && Settings.WrapMode == WrapMode.Once) {

                        AnimationTimeRatio = 1;
                        Pause = true;
                    }
                }

                yield return null;
            }
        }

        private void RotateObjectWithAnimationCurves() {
            var lookAtTarget =
                PathData.GetRotationAtTime(AnimationTimeRatio);
            // Convert target position to global coordinates.
            var lookAtTargetGlobal = ThisTransform.TransformPoint(lookAtTarget);

            if (Application.isPlaying) {
                RotateObjectWithSlerp(lookAtTargetGlobal);
            }
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
            var speed = Time.deltaTime * Settings.RotationSpeed;

            // Lerp rotation.
            animatedGO.rotation = Quaternion.Slerp(
                animatedGO.rotation,
                rotation,
                speed);
        }

        private void UpdateAnimatedGOPosition() {
            // Get animatedGO position at current animation time.
            var positionAtTimestamp =
                PathData.GetVectorAtTime(AnimationTimeRatio);
            var globalPositionAtTimestamp =
                ThisTransform.TransformPoint(positionAtTimestamp);

            // Update animatedGO position.
            animatedGO.position = globalPositionAtTimestamp;
        }

        private void UpdateAnimatedGORotation() {
            if (animatedGO == null) return;

            switch (Settings.RotationMode) {
                case AnimatorRotationMode.Forward:
                    var globalForwardPoint = GetGlobalForwardPoint();

                    RotateObjectWithLookAt(globalForwardPoint);

                    break;

                case AnimatorRotationMode.Custom:
                    // Get rotation point position.
                    var rotationPointPos =
                        PathData.GetRotationValueAtTime(AnimationTimeRatio);

                    // Convert target position to global coordinates.
                    var rotationPointGlobalPos =
                        ThisTransform.TransformPoint(rotationPointPos);

                    // Update animatedGO rotation.
                    RotateObjectWithLookAt(rotationPointGlobalPos);

                    break;

                case AnimatorRotationMode.Target:
                    if (targetGO == null) return;

                    RotateObjectWithLookAt(targetGO.position);
                    break;
            }
        }

        #endregion

        #region HELPER METHODS

        public void UpdateWrapMode() {
            PathData.SetWrapMode(Settings.WrapMode);
        }

        // TODO Remove the globalPosition arg. and create separate method.
        private Vector3 GetForwardPoint() {
            // Timestamp offset of the forward point.
            var forwardPointDelta = Settings.ForwardPointOffset;
            // Forward point timestamp.
            var forwardPointTimestamp = AnimationTimeRatio + forwardPointDelta;
            var localPosition = PathData.GetVectorAtTime(forwardPointTimestamp);

            return localPosition;
        }

        private Vector3 GetGlobalForwardPoint() {
            var localForwardPoint = GetForwardPoint();
            var globalForwardPoint = ThisTransform.TransformPoint(localForwardPoint);

            return globalForwardPoint;
        }

        private Vector3[] GetGlobalRotationPointPositions() {
            // Get rotation point positions.
            var rotPointPositions = PathData.GetRotationPointPositions();

            // Convert positions to global coordinates.
            Utilities.ConvertToGlobalCoordinates(
                ref rotPointPositions,
                ThisTransform);

            return rotPointPositions;
        }

        #endregion METHODS
    }

    // TODO Move to separate file.
    public class NodeReachedEventArgs : EventArgs {

        public NodeReachedEventArgs(int nodeIndex, float timestamp) {
            NodeIndex = nodeIndex;
            Timestamp = timestamp;
        }

        public int NodeIndex { get; set; }
        public float Timestamp { get; set; }

    }

}