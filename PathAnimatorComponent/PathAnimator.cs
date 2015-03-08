using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace ATP.SimplePathAnimator.PathAnimatorComponent {

    /// <summary>
    ///     Component that allows animating transforms position along predefined
    ///     Animation Paths and also animate their rotation on x and y axis in
    ///     time.
    /// </summary>
    [ExecuteInEditMode]
    public sealed class PathAnimator : GameComponent {
        #region EVENTS

        public event EventHandler<NodeReachedEventArgs> NodeReached;

        #endregion
        #region FIELDS
        //[SerializeField]
        //private Transform thisTransform;


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

        //[SerializeField]
        //private AnimatorGizmos animatorGizmos;

        [SerializeField]
        private PathData pathData;

        [SerializeField]
        private PathAnimatorSettings settings;

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

                if (Application.isPlaying && IsPlaying && !Pause) {
                }
                else {
                    UpdateAnimation();
                }
            }
        }

#if UNITY_EDITOR
        AnimatorGizmos AnimatorGizmos { get; set; }
#endif

        private bool isPlaying;

        /// <summary>
        ///     If animation is currently enabled.
        /// </summary>
        /// <remarks>When it's true, it means that the EaseTime coroutine is running.</remarks>
        public bool IsPlaying {
            get { return isPlaying; }
            set {
                isPlaying = value;
                Debug.Log("IsPlaying:" + isPlaying);
            }
        }

        public PathData PathData {
            get { return pathData; }
            set { pathData = value; }
        }

        private bool pause;

        public bool Pause {
            get { return pause; }
            set {
                pause = value;
                Debug.Log("Pause: " + pause);
            }
        }

        public PathAnimatorSettings Settings {
            get { return settings; }
        }

        public GUISkin Skin {
            get { return skin; }
        }

        public Transform ThisTransform { get; set; }

        /// <summary>
        ///     Transform that the <c>animatedGO</c> will be looking at.
        /// </summary>
        public Transform TargetGO {
            get { return targetGO; }
        }

        /// <summary>
        ///     Transform to be animated.
        /// </summary>
        public Transform AnimatedGO {
            get { return animatedGO; }
        }
        #endregion PROPERTIES

        #region UNITY MESSAGES

        private void OnEnable() {
            ThisTransform = GetComponent<Transform>();
            settings = Resources.Load("DefaultPathAnimatorSettings")
                as PathAnimatorSettings;
            skin = Resources.Load("DefaultPathAnimatorSkin") as GUISkin;

            // Initialize animatedGO field.
            if (AnimatedGO == null && Camera.main != null) {
                animatedGO = Camera.main.transform;
            }

            if (pathData != null) {
                PathData.RotationPointPositionChanged +=
                    PathData_RotationPointPositionChanged;

                PathData.NodePositionChanged += PathData_NodePositionChanged;
                PathData.NodeTiltChanged += PathData_NodeTiltChanged;
                PathData.PathReset += PathData_PathReset;
                PathData.RotationPathReset += PathData_RotationPathReset;
            }

#if UNITY_EDITOR
            if (AnimatorGizmos == null) {
                AnimatorGizmos =
                    AnimatorGizmos = new AnimatorGizmos(Settings);
            }
#endif
        }

        private void OnDisable() {
            if (PathData != null) {
                PathData.NodePositionChanged -= PathData_NodePositionChanged;
                PathData.NodeTiltChanged -= PathData_NodeTiltChanged;
                PathData.PathReset -= PathData_PathReset;
                PathData.RotationPathReset -= PathData_RotationPathReset;
            }
        }
        private void Awake() {

        }

        // TODO Refactor.
#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            // Return if path asset file is not assigned.
            if (PathData == null) return;

            if (Settings.RotationMode == RotationMode.Target
                && TargetGO != null) {

                AnimatorGizmos.DrawTargetIcon(TargetGO.position);
            }

            if (Settings.RotationMode == RotationMode.Forward) {
                var globalForwardPointPosition = GetGlobalForwardPoint();
                AnimatorGizmos.DrawForwardPointIcon(globalForwardPointPosition);
            }

            if (Settings.HandleMode == HandleMode.Rotation) {
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
#endif
     
        private void Start() {
            if (Application.isPlaying && Settings.AutoPlay) {
                StartEaseTimeCoroutine();
                IsPlaying = true;
            }
        }


        // TODO Check for null first.
        private void Reset() {
            // Create separate method InitializeFields().
            ThisTransform = GetComponent<Transform>();
            settings = Resources.Load("DefaultPathAnimatorSettings")
                as PathAnimatorSettings;
            skin = Resources.Load("DefaultPathAnimatorSkin") as GUISkin;

#if UNITY_EDITOR
            if (AnimatorGizmos == null) {
                AnimatorGizmos = new AnimatorGizmos(Settings);
            }
#endif
        }

        private void OnValidate() {
            ThisTransform = GetComponent<Transform>();

            UpdateAnimation();
        }

        private void Update() {
            // Animate while animation is running and not paused.
            if (Application.isPlaying && !Pause) {
                Animate();
                HandleFireNodeReachedEvent();
            }
        }

        #endregion UNITY MESSAGES

        #region EVENT HANDLERS
        private void PathData_RotationPathReset(object sender, EventArgs e) {
            Settings.RotationMode = RotationMode.Custom;
        }

        private void OnNodeReached(NodeReachedEventArgs eventArgs) {
            var handler = NodeReached;
            if (handler != null) handler(this, eventArgs);
        }


        private void PathData_NodePositionChanged(object sender, EventArgs e) {
            UpdateAnimation();
        }

        private void PathData_NodeTiltChanged(object sender, EventArgs e) {
            UpdateAnimation();
        }

        private void PathData_PathReset(object sender, EventArgs e) {
            AnimationTimeRatio = 0;
            UpdateAnimation();
        }

        private void PathData_RotationPointPositionChanged(
            object sender,
            EventArgs e) {

            UpdateAnimation();
        }

        #endregion

        #region ANIMATION

        private void Animate() {
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
                animationTimeRatio = 0;
                // Start animation.
                StartEaseTimeCoroutine();
            }
        }

        public void StartEaseTimeCoroutine() {
            if (PathData == null) {
                Debug.LogWarning("Assign Path Asset in the inspector.");
                return;
            }
            
            Debug.Log("StartCoroutine");

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

            Debug.Log("StopCoroutine");

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
            if (AnimatedGO == null) return;
            if (PathData == null) return;

            UpdateAnimatedGOPosition();
            UpdateAnimatedGORotation();
            AnimateAnimatedGOTilting();
        }

        private void AnimateAnimatedGOPosition() {
            if (AnimatedGO == null) return;

            var localPosAtTime =
                PathData.GetVectorAtTime(AnimationTimeRatio);

            var globalPosAtTime =
                ThisTransform.TransformPoint(localPosAtTime);

            // Update position.
            AnimatedGO.position = Vector3.Lerp(
                AnimatedGO.position,
                globalPosAtTime,
                Settings.PositionLerpSpeed);

        }

        private void AnimateAnimatedGORotation() {
            if (AnimatedGO == null) return;

            // Look at target.
            if (TargetGO != null
                && Settings.RotationMode == RotationMode.Target) {

                RotateObjectWithSlerp(TargetGO.position);
            }
            // Use rotation path.
            if (Settings.RotationMode == RotationMode.Custom) {
                RotateObjectWithAnimationCurves();
            }
            // Look forward.
            else if (Settings.RotationMode == RotationMode.Forward) {
                var globalForwardPoint = GetGlobalForwardPoint();

                RotateObjectWithSlerp(globalForwardPoint);
            }
        }

        private void AnimateAnimatedGOTilting() {
            if (AnimatedGO == null) return;

            // Get current animatedGO rotation.
            var eulerAngles = AnimatedGO.rotation.eulerAngles;
            // Get rotation from tiltingCurve.
            var zRotation = PathData.GetTiltingValueAtTime(AnimationTimeRatio);
            // Update value on Z axis.
            eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, zRotation);
            // Update animatedGO rotation.
            AnimatedGO.rotation = Quaternion.Euler(eulerAngles);
        }

        private IEnumerator EaseTime() {
            while (true) {
                // If animation is enabled and not paused..
                if (!Pause) {
                    // Get ease value.
                    var timeStep =
                        PathData.GetEaseValueAtTime(AnimationTimeRatio);

                    // Increase AnimationTimeRatio.
                    AnimationTimeRatio += timeStep * Time.deltaTime;
                }

                // Break from animation in Clamp wrap mode.
                if (Settings.WrapMode == AnimatorWrapMode.Clamp
                    && AnimationTimeRatio > 1) {

                    AnimationTimeRatio = 1;
                    IsPlaying = false;
                    // TODO
                    Debug.Log("Break from coroutine.");
                    break;
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
            AnimatedGO.LookAt(targetPos);
        }

        private void RotateObjectWithSlerp(Vector3 targetPosition) {
            // Return when point to look at is at the same position as the
            // animated object.
            if (targetPosition == AnimatedGO.position) return;

            // Calculate direction to target.
            var targetDirection = targetPosition - AnimatedGO.position;
            // Calculate rotation to target.
            var rotation = Quaternion.LookRotation(targetDirection);
            // Calculate rotation speed.
            var speed = Time.deltaTime * Settings.RotationSlerpSpeed;

            // Lerp rotation.
            AnimatedGO.rotation = Quaternion.Slerp(
                AnimatedGO.rotation,
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
            AnimatedGO.position = globalPositionAtTimestamp;
        }

        private void UpdateAnimatedGORotation() {
            if (AnimatedGO == null) return;

            switch (Settings.RotationMode) {
                case RotationMode.Forward:
                    var globalForwardPoint = GetGlobalForwardPoint();

                    RotateObjectWithLookAt(globalForwardPoint);

                    break;

                case RotationMode.Custom:
                    // Get rotation point position.
                    var rotationPointPos =
                        PathData.GetRotationValueAtTime(AnimationTimeRatio);

                    // Convert target position to global coordinates.
                    var rotationPointGlobalPos =
                        ThisTransform.TransformPoint(rotationPointPos);

                    // Update animatedGO rotation.
                    RotateObjectWithLookAt(rotationPointGlobalPos);

                    break;

                case RotationMode.Target:
                    if (TargetGO == null) return;

                    RotateObjectWithLookAt(TargetGO.position);
                    break;
            }
        }

        #endregion

        #region HELPER METHODS
   
        // TODO Rename to HandleFiringNodeReachedEvent.
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


        public void UpdateWrapMode() {
            PathData.SetPathWrapMode(Settings.WrapMode);
            PathData.SetEaseWrapMode(Settings.WrapMode);
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

}