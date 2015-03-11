using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace ATP.AnimationPathAnimator.APAnimatorComponent {

    /// <summary>
    ///     Component that allows animating transforms position along predefined
    ///     Animation Paths and also animate their rotation on x and y axis in
    ///     time.
    /// </summary>
    [ExecuteInEditMode]
    public sealed class APAnimator : GameComponent {
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
        private float animationTime;

        //[SerializeField]
        //private APAnimatorGizmos animatorGizmos;

        [SerializeField]
        private PathData pathData;

        [SerializeField]
        private APAnimatorSettings settings;

        [SerializeField]
        private GUISkin skin;

        /// <summary>
        ///     Transform that the <c>animatedGO</c> will be looking at.
        /// </summary>
        [SerializeField]
        private Transform targetGO;

        private bool Reverse { get; set; }

        #endregion OPTIONS

        #region PROPERTIES

        public float AnimationTime {
            get { return animationTime; }
            set {
                animationTime = value;

                if (Application.isPlaying && IsPlaying && !Pause) {
                }
                // Update animated GO in editor mode.
                else {
                    UpdateAnimation();
                }
            }
        }

#if UNITY_EDITOR
        // TODO Rename to AnimatorGizmos.
        APAnimatorGizmos ApAnimatorGizmos { get; set; }
#endif

        private bool isPlaying;

        /// <summary>
        ///     If animation is currently enabled.
        /// </summary>
        /// <remarks>When it's true, it means that the HandleEaseTime coroutine is running.</remarks>
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

        private bool animatedObjectUpdateEnabled;

        public bool Pause {
            get { return pause; }
            set {
                pause = value;
                Debug.Log("Pause: " + pause);

                // On unpause..
                if (!value) {
                    // Update animation time.
                    UpdateAnimationTime();
                    // Enable animating animated GO.
                    AnimatedObjectUpdateEnabled = true;
                }
            }
        }

        // TODO Rename to SettingsAsset.
        public APAnimatorSettings Settings {
            get { return settings; }
        }

        public GUISkin Skin {
            get { return skin; }
        }

        private Transform thisTransform;

        public Transform ThisTransform {
            get { return thisTransform; }
        }

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

        public bool AnimatedObjectUpdateEnabled {
            get { return animatedObjectUpdateEnabled; }
            set {
                animatedObjectUpdateEnabled = value;
                //Debug.Log("AnimatedObjectUpdateEnabled: " + animatedObjectUpdateEnabled);
            }
        }

        #endregion PROPERTIES

        #region UNITY MESSAGES

        private void OnEnable() {
            Debug.Log("OnEnable");
            thisTransform = GetComponent<Transform>();

            LoadRequiredResources();
            AssignMainCameraAsAnimatedGO();
            SubscribeToEvents();

#if UNITY_EDITOR
            HandleInstantiateAnimatorGizmos();
#endif
        }

        private void HandleInstantiateAnimatorGizmos() {

            if (ApAnimatorGizmos == null) {
                ApAnimatorGizmos =
                    ApAnimatorGizmos = new APAnimatorGizmos(Settings);
            }
        }

        private void LoadRequiredResources() {
            settings = Resources.Load("DefaultAnimatorSettings")
                as APAnimatorSettings;
            skin = Resources.Load("DefaultAnimatorSkin") as GUISkin;
        }

        public void SubscribeToEvents() {
            if (pathData != null) {
                Debug.Log("subscribe");
                PathData.RotationPointPositionChanged +=
                    PathData_RotationPointPositionChanged;
                PathData.NodePositionChanged += PathData_NodePositionChanged;
                PathData.NodeTiltChanged += PathData_NodeTiltChanged;
                PathData.PathReset += PathData_PathReset;
                PathData.RotationPathReset += PathData_RotationPathReset;

                SubscribedToEvents = true;
            }
        }

        private void AssignMainCameraAsAnimatedGO() {
            if (AnimatedGO == null && Camera.main != null) {
                animatedGO = Camera.main.transform;
            }
        }

        private void OnDisable() {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents() {
            if (PathData != null) {
                PathData.NodePositionChanged -= PathData_NodePositionChanged;
                PathData.NodeTiltChanged -= PathData_NodeTiltChanged;
                PathData.PathReset -= PathData_PathReset;
                PathData.RotationPathReset -= PathData_RotationPathReset;

                SubscribedToEvents = false;
            }
        }

        private void Awake() {

        }

        // TODO Refactor.
#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            if (!AssetsLoaded()) return;

            // Return if path asset file is not assigned.
            if (PathData == null) return;

            if (Settings.RotationMode == RotationMode.Target
                && TargetGO != null) {

                ApAnimatorGizmos.DrawTargetIcon(TargetGO.position);
            }

            if (Settings.RotationMode == RotationMode.Forward) {
                var globalForwardPointPosition = GetGlobalForwardPoint();
                ApAnimatorGizmos.DrawForwardPointIcon(globalForwardPointPosition);
            }

            if (Settings.HandleMode == HandleMode.Rotation) {
                ApAnimatorGizmos.DrawRotationPathCurve(PathData, transform);

                ApAnimatorGizmos.DrawCurrentRotationPointGizmo(
                    PathData,
                    transform,
                    AnimationTime);

                ApAnimatorGizmos.DrawRotationPointGizmos(
                    PathData,
                    transform,
                    AnimationTime);
            }

            ApAnimatorGizmos.DrawAnimationCurve(PathData, transform);
        }
#endif
     
        private void Start() {
            if (Application.isPlaying && Settings.AutoPlay) {
                StartEaseTimeCoroutine();
                IsPlaying = true;
            }
        }


        private void Reset() {
            // Create separate method InitializeFields().
            thisTransform = GetComponent<Transform>();

            LoadRequiredResources();
            UnsubscribeFromEvents();
            AssignMainCameraAsAnimatedGO();
#if UNITY_EDITOR
            HandleInstantiateAnimatorGizmos();
#endif
        }

        private void OnValidate() {
            thisTransform = GetComponent<Transform>();
            UpdateAnimation();
            //if (!SubscribedToEvents) SubscribeToEvents();
        }

        /// <summary>
        /// If animator is currently subscribed to path events.
        /// </summary>
        /// <remarks>It's only public because of the Editor class.</remarks>
        public bool SubscribedToEvents { get; set; }

        //private int frame;
        private void Update() {
            //var frame = Time.frameCount;
            //frame++;
            HandleUpdatingAnimGOInPlayMode();

            //if (frame == 2) StartCoroutine("HandleEaseTime");
        }

        private void HandleUpdatingAnimGOInPlayMode() {
            // Update animated GO in play mode.
            if (Application.isPlaying && AnimatedObjectUpdateEnabled) {
                var prevAnimGOPosition = animatedGO.position;

                Animate();
                HandleFireNodeReachedEvent();

                var destPointReached = Utilities.V3Equal(
                    prevAnimGOPosition,
                    animatedGO.position,
                    // TODO Add to global constants.
                    //0.00000001f)) {
                    0.000000000001f);

                // Stop updating animated GO after reaching destination point.
                // It cannot be done in first two frames because no movement
                // would be detected.
                if (destPointReached && Time.frameCount > 2) {
                    AnimatedObjectUpdateEnabled = false;
                }
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
            AnimationTime = 0;
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
                AnimationTime = 0;
                // Start animation.
                StartEaseTimeCoroutine();
            }
        }

        // TODO Rename to StartAnimation().
        public void StartEaseTimeCoroutine() {
            if (PathData == null) {
                Debug.LogWarning("Assign Path Asset in the inspector.");
                return;
            }

            // Starting node was reached.
            if (AnimationTime == 0) {
                var args = new NodeReachedEventArgs(0, 0);
                OnNodeReached(args);
            }

            StartCoroutine("HandleEaseTime");
        }

        // TODO Rename to StopAnimation().
        public void StopEaseTimeCoroutine() {
            StopCoroutine("HandleEaseTime");

            IsPlaying = false;
            Pause = false;
            // Reset animation.
            //AnimationTime = 0;
        }

        /// <summary>
        ///     Update animatedGO position, rotation and tilting based on current
        ///     AnimationTime.
        /// </summary>
        /// <remarks>
        ///     Used to update animatedGO with keys, in play mode.
        /// </remarks>
        public void UpdateAnimation() {
            if (!AssetsLoaded()) return;
            if (AnimatedGO == null) return;
            if (PathData == null) return;

            UpdateAnimatedGOPosition();
            UpdateAnimatedGORotation();
            AnimateAnimatedGOTilting();
        }

        private void AnimateAnimatedGOPosition() {
            if (AnimatedGO == null) return;

            var localPosAtTime =
                PathData.GetVectorAtTime(AnimationTime);

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
            var zRotation = PathData.GetTiltingValueAtTime(AnimationTime);
            // Update value on Z axis.
            eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, zRotation);
            // Update animatedGO rotation.
            AnimatedGO.rotation = Quaternion.Euler(eulerAngles);
        }

        private IEnumerator HandleEaseTime() {
            IsPlaying = true;
            Pause = false;
            AnimatedObjectUpdateEnabled = true;

            while (true) {
                // If animation is not paused..
                if (!Pause) {
                    UpdateAnimationTime();

                    HandleClampWrapMode();
                    HandleLoopWrapMode();
                    HandlePingPongWrapMode();
                }

                if (!IsPlaying) break;

                yield return null;
            }
        }

        private void UpdateAnimationTime() {
            // Get ease value.
            var timeStep =
                PathData.GetEaseValueAtTime(AnimationTime);

            if (Reverse) {
                // Increase animation time.
                AnimationTime -= timeStep * Time.deltaTime;
            }
            else {
                // Decrease animation time.
                AnimationTime += timeStep * Time.deltaTime;
            }
        }

        private void HandleClampWrapMode() {
            // Break from animation in Clamp wrap mode.
            if (AnimationTime > 1
                && Settings.WrapMode == WrapMode.Clamp) {

                AnimationTime = 1;
                IsPlaying = false;
            }
        }

        private void HandleLoopWrapMode() {

            if (AnimationTime > 1
                && Settings.WrapMode == WrapMode.Loop) {

                AnimationTime = 0;
            }
        }

        private void HandlePingPongWrapMode() {
            if (AnimationTime > 1
                && Settings.WrapMode == WrapMode.PingPong) {

                Reverse = true;
            }

            if (AnimationTime < 0
                && Settings.WrapMode == WrapMode.PingPong) {

                Reverse = false;
            }
        }

        private void RotateObjectWithAnimationCurves() {
            var lookAtTarget =
                PathData.GetRotationAtTime(AnimationTime);
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
                PathData.GetVectorAtTime(AnimationTime);

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
                        PathData.GetRotationValueAtTime(AnimationTime);

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

            // Compare current AnimationTime to node timestamps.
            var index = Array.FindIndex(
                nodeTimestamps,
                x => Math.Abs(x - AnimationTime)
                     < GlobalConstants.FloatPrecision);

            // Return if current AnimationTime is not equal to any node
            // timestamp.
            if (index < 0) return;

            // Create event args.
            var args = new NodeReachedEventArgs(index, AnimationTime);

            // Fire event.
            OnNodeReached(args);
        }


        // TODO Remove.
        public void UpdateWrapMode() {
            PathData.SetPathWrapMode(Settings.WrapMode);
            PathData.SetEaseWrapMode(Settings.WrapMode);
        }

        // TODO Remove the globalPosition arg. and create separate method.
        private Vector3 GetForwardPoint() {
            // Timestamp offset of the forward point.
            var forwardPointDelta = Settings.ForwardPointOffset;
            // Forward point timestamp.
            var forwardPointTimestamp = AnimationTime + forwardPointDelta;
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

        public bool AssetsLoaded() {
            if (Settings != null
                && Skin != null) {

                return true;
            }

            return false;
        }

        #endregion METHODS
    }

}