using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ATP.AnimationPathAnimator.APAnimatorComponent {

    /// <summary>
    /// Animates object along path.
    /// </summary>
    [ExecuteInEditMode]
    public sealed class APAnimator : MonoBehaviour {
        #region EVENTS

        /// <summary>
        ///     Event fired when animation time is 1 and animated object stops moving.
        /// </summary>
        public event EventHandler AnimationEnded;

        /// <summary>
        ///     Event fired when animation time is 0 and animation starts playing.
        /// </summary>
        public event EventHandler AnimationStarted;

        /// <summary>
        /// Event called when animated object goes through any node.
        /// </summary>
        public event EventHandler<NodeReachedEventArgs> NodeReached;

        #endregion

        #region FIELDS

        /// <summary>
        /// Whether inspector advanced settings foldout should be open or folded.
        /// </summary>
        [SerializeField]
        private bool advancedSettingsFoldout;

        [SerializeField]
        private Transform animatedGO;

        [SerializeField]
        private float animationTime;

        private bool animGOUpdateEnabled;

        private bool countdownCoroutineIsRunning;

        private bool isPlaying;

        [SerializeField]
        private PathData pathData;

        private bool pause;

        /// <summary>
        ///     Animation time value from previous frame.
        /// </summary>
        private float prevAnimationTime;

        [SerializeField]
        private APAnimatorSettings settingsAsset;

        [SerializeField]
        private GUISkin skin;

        [SerializeField]
        private bool subscribedToEvents;

        [SerializeField]
        private Transform targetGO;

        #endregion OPTIONS

        #region PROPERTIES

        /// <summary>
        ///     Transform to be animated.
        /// </summary>
        public Transform AnimatedGO {
            get { return animatedGO; }
        }

        /// <summary>
        /// Percentage of the overall animation progress. 0.5 means position in the middle of the path.
        /// </summary>
        public float AnimationTime {
            get { return animationTime; }
            set {
                animationTime = value;

                // In play mode, while animation is enabled and not paused..
                if (Application.isPlaying && IsPlaying && !Pause) {
                    // Do nothing.
                }
                else {
                    // Update animated GO.
                    UpdateAnimation();
                }
            }
        }

        /// <summary>
        /// Whether to update animated object positon, rotation and tilting. It's executed independent from animation time,
        /// so animated object can be updated even if animator is stopped or paused.
        /// </summary>
        public bool AnimGOUpdateEnabled {
            get { return animGOUpdateEnabled; }
            set { animGOUpdateEnabled = value; }
        }

        /// <summary>
        /// It's true when <c>CountdownToStopAnimGOUpdate</c> coroutine is running.
        /// </summary>
        public bool CountdownCoroutineIsRunning {
            get { return countdownCoroutineIsRunning; }
            set { countdownCoroutineIsRunning = value; }
        }

        /// <summary>
        /// It's set to true when <c>EaseTime</c> coroutine is running.
        /// </summary>
        // TODO Rename to IsRunning.
        public bool IsPlaying {
            get { return isPlaying; }
            private set {
                isPlaying = value;

                Debug.Log("Animator is " + (value ? "enabled" : "disabled"));
            }
        }

        /// <summary>
        /// Reference to asset file holding path data.
        /// </summary>
        public PathData PathData {
            get { return pathData; }
            set { pathData = value; }
        }

        /// <summary>
        /// Whether or not animation is paused. Animation can be paused only when animator is running.
        /// </summary>
        public bool Pause {
            get { return pause; }
            set {
                pause = value;

                Debug.Log("Pause: " + pause);

                // On unpause..
                if (!value) {
                    // Enable animating animated GO.
                    AnimGOUpdateEnabled = true;
                }
            }
        }

        /// <summary>
        /// Reference to asset file holding animator settings.
        /// </summary>
        public APAnimatorSettings SettingsAsset {
            get { return settingsAsset; }
        }

        /// <summary>
        /// Reference to GUISkin asset.
        /// </summary>
        public GUISkin Skin {
            get { return skin; }
        }

        /// <summary>
        ///     Transform that the <c>animatedGO</c> will be looking at.
        /// </summary>
        public Transform TargetGO {
            get { return targetGO; }
            set { targetGO = value; }
        }

        /// <summary>
        /// If animation should be played backward.
        /// </summary>
        private bool Reverse { get; set; }

        /// <summary>
        ///     If animator is currently subscribed to path events.
        /// </summary>
        private bool SubscribedToEvents {
            set { subscribedToEvents = value; }
        }

        #endregion PROPERTIES

        #region UNITY MESSAGES
        private void OnDisable() {
            UnsubscribeFromEvents();
        }

        private void OnDrawGizmosSelected() {
            if (!RequiredAssetsLoaded()) return;
            if (PathData == null) return;

            DrawAnimationCurve();
            HandleDrawingTargetIcon();
            HandleDrawingForwardPointIcon();
            HandleDrawingRotationPathCurve();
            HandleDrawingCurrentRotationPointGizmo();
            HandleDrawingRotationPointGizmos();
        }

        private void OnEnable() {
            LoadRequiredResources();
            SubscribeToEvents();
        }

        private void OnValidate() {
            UpdateAnimation();
            UpdateSubscribedToEventsFlag();
        }

        private void Reset() {
            LoadRequiredResources();
            AssignMainCameraAsAnimatedGO();
            ResetInspectorOptions();
        }

        private void Start() {
            HandleStartAnimation();
        }

        private void Update() {
            HandleUpdatingAnimGOInPlayMode();
            HandleShortcuts();
        }
        #endregion UNITY MESSAGES

        #region EVENT INVOCATORS

        private void OnAnimationEnded() {
            var handler = AnimationEnded;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnAnimationStarted() {
            var handler = AnimationStarted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnNodeReached(NodeReachedEventArgs eventArgs) {
            var handler = NodeReached;
            if (handler != null) handler(this, eventArgs);
        }

        #endregion

        #region EVENT HANDLERS

        private void APAnimator_AnimationEnded(object sender, EventArgs e) {
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

        private void PathData_RotationPathReset(object sender, EventArgs e) {
            SettingsAsset.RotationMode = RotationMode.Custom;
        }

        private void PathData_RotationPointPositionChanged(
            object sender,
            EventArgs e) {
            UpdateAnimation();
        }

        #endregion

        #region ANIMATION

        /// <summary>
        /// Pause animation.
        /// </summary>
        public void PauseAnimation() {
            Pause = true;
        }

        /// <summary>
        /// Toggle play/pause animation.
        /// </summary>
        public void PlayPauseAnimation() {
            Pause = !Pause;
        }

        /// <summary>
        /// Set rotation mode to Custom.
        /// </summary>
        public void SetRotationCustom() {
            SettingsAsset.RotationMode = RotationMode.Custom;
        }

        /// <summary>
        /// Set rotation mode to Forward.
        /// </summary>
        public void SetRotationForward() {
            SettingsAsset.RotationMode = RotationMode.Forward;
        }

        /// <summary>
        /// Set rotation mode to Target.
        /// </summary>
        public void SetRotationTarget() {
            SettingsAsset.RotationMode = RotationMode.Target;
        }

        /// <summary>
        /// Set tangent mode to Linear.
        /// </summary>
        public void SetTangentLinear() {
            SettingsAsset.TangentMode = TangentMode.Linear;
            PathData.SetLinearAnimObjPathTangents();
        }

        /// <summary>
        /// Set tangent mode to Smooth.
        /// </summary>
        public void SetTangentSmooth() {
            SettingsAsset.TangentMode = TangentMode.Smooth;
            PathData.SmoothAnimObjPathTangents();
        }

        /// <summary>
        /// Set wrap mode to Clamp.
        /// </summary>
        public void SetWrapClamp() {
            SettingsAsset.WrapMode = AnimatorWrapMode.Clamp;
        }

        /// <summary>
        /// Set wrap mode to Loop.
        /// </summary>
        public void SetWrapLoop() {
            SettingsAsset.WrapMode = AnimatorWrapMode.Loop;
        }

        /// <summary>
        /// Set wrap mode to PingPong.
        /// </summary>
        public void SetWrapPingPong() {
            SettingsAsset.WrapMode = AnimatorWrapMode.PingPong;
        }

        /// <summary>
        /// Start animation.
        /// </summary>
        public void StartAnimation() {
            if (!PathDataAssetAssigned()) return;

            HandleFireNodeReachedEventForFirstNode();
            StartCoroutine("HandleEaseTime");

            Debug.Log("Animation started");
        }

        /// <summary>
        /// Stop animation.
        /// </summary>
        public void StopAnimation() {
            StopCoroutine("HandleEaseTime");

            Debug.Log("Animation stopped");

            IsPlaying = false;
            Pause = false;
            AnimationTime = 0;
        }

        /// <summary>
        /// Unpause animation.
        /// </summary>
        public void UnpauseAnimation() {
            Pause = false;
        }

        /// <summary>
        /// Update animated game object position, rotation and tilting.
        /// </summary>
        // TODO Rename to UpdateGO().
        private void Animate() {
            AnimateAnimatedGOPosition();
            AnimateAnimatedGORotation();
            AnimateAnimatedGOTilting();
        }

        /// <summary>
        /// Update animated game object position.
        /// </summary>
        // TODO Rename to UpdateAnimatedGOPosition().
        private void AnimateAnimatedGOPosition() {
            if (AnimatedGO == null) return;

            var localPosAtTime =
                PathData.GetVectorAtTime(AnimationTime);

            // Global position that the animated object should be at in this frame.
            var globalPosAtTime =
                transform.TransformPoint(localPosAtTime);

            // Update position.
            AnimatedGO.position = Vector3.Lerp(
                AnimatedGO.position,
                globalPosAtTime,
                SettingsAsset.PositionLerpSpeed);
        }

        /// <summary>
        /// Update animated game object rotation.
        /// </summary>
        // TODO Rename to UpdateAnimatedGORotation().
        private void AnimateAnimatedGORotation() {
            if (AnimatedGO == null) return;

            // Look at target.
            if (TargetGO != null
                && SettingsAsset.RotationMode == RotationMode.Target) {

                RotateObjectWithSlerp(TargetGO.position);
            }
            // Use rotation path.
            if (SettingsAsset.RotationMode == RotationMode.Custom) {
                RotateObjectWithAnimationCurves();
            }
            // Look forward.
            else if (SettingsAsset.RotationMode == RotationMode.Forward) {
                var globalForwardPoint = GetGlobalForwardPoint();

                RotateObjectWithSlerp(globalForwardPoint);
            }
        }

        /// <summary>
        /// Update animated game object tilting.
        /// </summary>
        // todo Rename to UpdateAnimatedGOTilting().
        private void AnimateAnimatedGOTilting() {
            if (AnimatedGO == null) return;

            // Get current animated GO rotation.
            var eulerAngles = AnimatedGO.rotation.eulerAngles;
            // Get tilting value.
            var zRotation = PathData.GetTiltingValueAtTime(AnimationTime);
            // Update value on Z axis.
            eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, zRotation);
            // Update animated GO rotation.
            AnimatedGO.rotation = Quaternion.Euler(eulerAngles);
        }

        /// <summary>
        /// Coroutine that will remember animated game object's current position, rotation and tilting
        /// and after a given number of frames checks if any of those values changed. If none changed, animated GO
        /// stops being updated and AnimatonEnded event is called.
        /// </summary>
        // TODO Should check also for tilting.
        private IEnumerator CountdownToStopAnimGOUpdate() {
            // Helper variable.
            var frame = 0;
            var prevGOPosition = animatedGO.position;
            var prevGORotation = animatedGO.rotation;
            CountdownCoroutineIsRunning = true;

            while (true) {
                frame++;

                // Break after given number of frames.
                if (frame > SettingsAsset.CountdownToStopFramesNo) break;

                yield return null;
            }

            CountdownCoroutineIsRunning = false;

            var positionChanged = !Utilities.V3Equal(
                prevGOPosition,
                animatedGO.position);

            var rotationChanged = !Utilities.QuaternionsEqual(
                prevGORotation,
                animatedGO.rotation);

            if (!positionChanged && !rotationChanged) {
                // Stop updating animated game object.
                AnimGOUpdateEnabled = false;
                // Fire event.
                OnAnimationEnded();
            }
        }

        private void RotateObjectWithAnimationCurves() {
            var lookAtTarget =
                PathData.GetRotationAtTime(AnimationTime);
            // Convert target position to global coordinates.
            var lookAtTargetGlobal = transform.TransformPoint(lookAtTarget);

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
            var speed = Time.deltaTime * SettingsAsset.RotationSlerpSpeed;

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
                transform.TransformPoint(positionAtTimestamp);

            // Update animatedGO position.
            AnimatedGO.position = globalPositionAtTimestamp;
        }

        private void UpdateAnimatedGORotation() {
            if (AnimatedGO == null) return;

            switch (SettingsAsset.RotationMode) {
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
                        transform.TransformPoint(rotationPointPos);

                    // Update animatedGO rotation.
                    RotateObjectWithLookAt(rotationPointGlobalPos);

                    break;

                case RotationMode.Target:
                    if (TargetGO == null) return;

                    RotateObjectWithLookAt(TargetGO.position);
                    break;
            }
        }

        /// <summary>
        ///     Update animatedGO position, rotation and tilting based on current
        ///     AnimationTime.
        /// </summary>
        /// <remarks>
        ///     Used to update animatedGO with keys, in play mode.
        /// </remarks>
        private void UpdateAnimation() {
            if (!RequiredAssetsLoaded()) return;
            if (AnimatedGO == null) return;
            if (PathData == null) return;
            if (!enabled) return;

            UpdateAnimatedGOPosition();
            UpdateAnimatedGORotation();
            AnimateAnimatedGOTilting();
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

        #endregion

        #region ANIMATION HANDLERS
        /// <summary>
        /// Decides whether to start animation playback on enter play mode.
        /// </summary>
        private void HandleStartAnimation() {
            if (Application.isPlaying && SettingsAsset.AutoPlay) {
                StartAnimation();
                IsPlaying = true;
            }
        }


        private void HandleClampWrapMode() {
            // Break from animation in Clamp wrap mode.
            if (AnimationTime > 1
                && SettingsAsset.WrapMode == AnimatorWrapMode.Clamp) {
                AnimationTime = 1;
                IsPlaying = false;
            }
        }

        // TODO Rename to EaseTime().
        private IEnumerator HandleEaseTime() {
            IsPlaying = true;
            Pause = false;
            AnimGOUpdateEnabled = true;

            while (true) {
                // If animation is not paused..
                if (!Pause) {
                    HandleFireOnAnimationStartedEvent();

                    UpdateAnimationTime();

                    HandleClampWrapMode();
                    HandleLoopWrapMode();
                    HandlePingPongWrapMode();
                }

                if (!IsPlaying) break;

                yield return null;
            }
        }

        private void HandleFireNodeReachedEvent() {
            // Get path timestamps.
            var nodeTimestamps = PathData.GetPathTimestamps();

            for (var i = 0; i < nodeTimestamps.Length; i++) {
                if (prevAnimationTime < nodeTimestamps[i]
                    && AnimationTime >= nodeTimestamps[i]) {

                    // Create event args.
                    var args = new NodeReachedEventArgs(i, AnimationTime);
                    // Fire event.
                    OnNodeReached(args);
                    Debug.Log("node reached");
                }
            }

            prevAnimationTime = AnimationTime;
        }

        private void HandleFireOnAnimationStartedEvent() {
            if (AnimationTime == 0) OnAnimationStarted();
        }

        private void HandleLoopWrapMode() {
            if (AnimationTime > 1
                && SettingsAsset.WrapMode == AnimatorWrapMode.Loop) {
                AnimationTime = 0;
            }
        }

        private void HandlePingPongWrapMode() {
            if (AnimationTime > 1
                && SettingsAsset.WrapMode == AnimatorWrapMode.PingPong) {
                Reverse = true;
            }

            if (AnimationTime < 0
                && SettingsAsset.WrapMode == AnimatorWrapMode.PingPong) {
                Reverse = false;
            }
        }

        private void HandlePlayPause() {
            if (!Application.isPlaying) return;

            if (IsPlaying && !Pause) {
                // Pause animation.
                Pause = true;
            }
            else if (IsPlaying && Pause) {
                // Unpause animation.
                Pause = false;
            }
            // Animation ended.
            else if (!IsPlaying && AnimationTime >= 1) {
                AnimationTime = 0;
                StartAnimation();
            }
            else {
                // Start animation.
                StartAnimation();
            }
        }

        private void HandleUpdatingAnimGOInPlayMode() {
            // Return if not in play mode.
            if (!Application.isPlaying) return;
            // Return if anim. GO update is disabled.
            if (!AnimGOUpdateEnabled) return;

            // Remember anim. GO position.
            var prevAnimGOPosition = animatedGO.position;
            // Remember anim. GO rotation.
            var prevAnimGORotation = animatedGO.rotation;

            Animate();
            HandleFireNodeReachedEvent();

            var movementDetected = !Utilities.V3Equal(
                prevAnimGOPosition,
                animatedGO.position);

            var rotationDetected = !Utilities.QuaternionsEqual(
                prevAnimGORotation,
                animatedGO.rotation);

            if (!movementDetected && !rotationDetected) {
                if (!CountdownCoroutineIsRunning) {
                    StartCoroutine(CountdownToStopAnimGOUpdate());
                }
            }
        }
        #endregion

        #region OTHER HANDLERS
        /// <summary>
        /// Used at animation start to fire NodeReached event for the first node.
        /// </summary>
        private void HandleFireNodeReachedEventForFirstNode() {
            if (AnimationTime == 0) {
                // Fire event.
                var args = new NodeReachedEventArgs(0, 0);
                OnNodeReached(args);
            }
        }
        #endregion

        #region HELPER METHODS
        /// <summary>
        /// Use it to guard agains null path data asset.
        /// </summary>
        /// <returns>True if pata data asset is not null.</returns>
        private bool PathDataAssetAssigned() {
            if (PathData == null) {
                Debug.LogWarning("Assign Path Asset in the inspector.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Method responsible for detecting all shortcuts pressed in play mode.
        /// </summary>
        private void HandleShortcuts() {
            if (!SettingsAsset.EnableControlsInPlayMode) return;

            // Play/Pause.
            if (Input.GetKeyDown(SettingsAsset.PlayPauseKey)) {
                HandlePlayPause();
            }

            // Long jump forward
            if (Input.GetKeyDown(SettingsAsset.LongJumpForwardKey)) {
                animationTime += SettingsAsset.LongJumpValue;
            }

            // Long jump backward. 
            if (Input.GetKeyDown(SettingsAsset.LongJumpBackwardKey)) {
                animationTime -= SettingsAsset.LongJumpValue;
            }

            // Jump to next node.
            if (Input.GetKeyDown(SettingsAsset.JumpToNextNodeKey)) {
                animationTime = GetNearestForwardNodeTimestamp();
            }

            // Jump to previous node.
            if (Input.GetKeyDown(SettingsAsset.JumpToPreviousNodeKey)) {
                animationTime = GetNearestBackwardNodeTimestamp();
            }

            // Jump to beginning.
            if (Input.GetKeyDown(
                SettingsAsset.JumpToPreviousNodeKey)
                && Input.GetKey(SettingsAsset.PlayModeModKey)) {

                AnimationTime = 0;
            }
        }

        private void UpdateSubscribedToEventsFlag() {
            // Update flag when path data asset is removed.
            if (PathData == null && subscribedToEvents) {
                subscribedToEvents = false;
            }
        }

        /// <summary>
        /// Gets timestamp of a node which timestamp is closest to and bigger than the current animation time. 
        /// </summary>
        /// <returns>Node timestamp.</returns>
        private float GetNearestForwardNodeTimestamp() {
            var pathTimestamps = PathData.GetPathTimestamps();

            foreach (var timestamp in pathTimestamps
                .Where(timestamp => timestamp > AnimationTime)) {
                return timestamp;
            }

            // Return timestamp of the last node.
            return 1.0f;
        }

        /// <summary>
        /// Gets timestamp of a node which timestamp is closest to and bigger than the current animation time.
        /// </summary>
        /// <returns>Node timestamp.</returns>
        private float GetNearestBackwardNodeTimestamp() {
            var pathTimestamps = PathData.GetPathTimestamps();

            for (var i = pathTimestamps.Length - 1; i >= 0; i--) {
                if (pathTimestamps[i] < AnimationTime) {
                    return pathTimestamps[i];
                }
            }

            // Return timestamp of the last node.
            return 0;
        }


        public Vector3[] GetGlobalNodePositions(int nodesNo = -1) {
            var nodePositions = PathData.GetNodePositions(nodesNo);
            Utilities.ConvertToGlobalCoordinates(
                ref nodePositions,
                transform);

            return nodePositions;
        }

        private void AssignMainCameraAsAnimatedGO() {
            if (AnimatedGO == null && Camera.main != null) {
                animatedGO = Camera.main.transform;
            }
            else {
                Debug.LogWarning("Camera with tag \"MainCamera\" not found.");
            }
        }

        // TODO Rename to CalculateForwardPointPosition().
        private Vector3 GetForwardPoint() {
            // Timestamp offset of the forward point.
            var forwardPointDelta = SettingsAsset.ForwardPointOffset;
            // Forward point timestamp.
            var forwardPointTimestamp = AnimationTime + forwardPointDelta;
            var localPosition = PathData.GetVectorAtTime(forwardPointTimestamp);

            return localPosition;
        }

        private Vector3 GetGlobalForwardPoint() {
            var localForwardPoint = GetForwardPoint();
            var globalForwardPoint =
                transform.TransformPoint(localForwardPoint);

            return globalForwardPoint;
        }

        private void LoadRequiredResources() {
            settingsAsset = Resources.Load("DefaultAnimatorSettings")
                as APAnimatorSettings;
            skin = Resources.Load("DefaultAnimatorSkin") as GUISkin;
        }

        private bool RequiredAssetsLoaded() {
            if (SettingsAsset != null
                && Skin != null) {
                return true;
            }
            return false;
        }

        private void ResetInspectorOptions() {
            TargetGO = null;
            SettingsAsset.HandleMode = HandleMode.None;
            SettingsAsset.TangentMode = TangentMode.Smooth;
            SettingsAsset.UpdateAllMode = false;
            AnimationTime = 0;
            SettingsAsset.AutoPlay = true;
            SettingsAsset.EnableControlsInPlayMode = true;
            SettingsAsset.RotationMode = RotationMode.Forward;
            SettingsAsset.WrapMode = AnimatorWrapMode.Clamp;
            SettingsAsset.ForwardPointOffset = 0.001f;
            SettingsAsset.PositionLerpSpeed = 1;
            SettingsAsset.RotationSlerpSpeed = 999;
            SettingsAsset.ExportSamplingFrequency = 5;
        }

        private void SubscribeToEvents() {
            if (pathData == null) return;

            PathData.RotationPointPositionChanged +=
                PathData_RotationPointPositionChanged;
            PathData.NodePositionChanged += PathData_NodePositionChanged;
            PathData.NodeTiltChanged += PathData_NodeTiltChanged;
            PathData.PathReset += PathData_PathReset;
            PathData.RotationPathReset += PathData_RotationPathReset;
            AnimationEnded += APAnimator_AnimationEnded;

            SubscribedToEvents = true;
        }

        private void UnsubscribeFromEvents() {
            if (PathData == null) return;

            PathData.NodePositionChanged -= PathData_NodePositionChanged;
            PathData.NodeTiltChanged -= PathData_NodeTiltChanged;
            PathData.PathReset -= PathData_PathReset;
            PathData.RotationPathReset -= PathData_RotationPathReset;

            SubscribedToEvents = false;
        }

        #endregion METHODS

        #region GIZMOS

        private void DrawAnimationCurve() {
            // Return if path asset is not assigned.
            if (pathData == null) return;

            // Get path points.
            var points = pathData.SampleAnimationPathForPoints(
                SettingsAsset.GizmoCurveSamplingFrequency);

            // Convert points to global coordinates.
            var globalPoints = new Vector3[points.Count];
            for (var i = 0; i < points.Count; i++) {
                globalPoints[i] = transform.TransformPoint(points[i]);
            }

            // There must be at least 3 points to draw a line.
            if (points.Count < 3) return;

            Gizmos.color = SettingsAsset.GizmoCurveColor;

            // Draw curve.
            for (var i = 0; i < points.Count - 1; i++) {
                Gizmos.DrawLine(globalPoints[i], globalPoints[i + 1]);
            }
        }

        private void DrawForwardPointIcon(Vector3 forwardPointPosition) {
            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                forwardPointPosition,
                SettingsAsset.GizmosSubfolder + SettingsAsset.ForwardPointIcon,
                false);
        }

        private void DrawTargetIcon(Vector3 targetPosition) {
            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                targetPosition,
                SettingsAsset.GizmosSubfolder + SettingsAsset.TargetGizmoIcon,
                false);
        }

        private Vector3[] GetGlobalRotationPointPositions() {
            var localRotPointPositions =
                pathData.GetRotationPointPositions();

            var globalRotPointPositions =
                new Vector3[localRotPointPositions.Length];

            for (var i = 0; i < localRotPointPositions.Length; i++) {
                globalRotPointPositions[i] =
                    transform.TransformPoint(localRotPointPositions[i]);
            }

            return globalRotPointPositions;
        }

        private void HandleDrawingCurrentRotationPointGizmo() {
            if (SettingsAsset.HandleMode != HandleMode.Rotation) return;

            // Node path node timestamps.
            var nodeTimestamps = pathData.GetPathTimestamps();

            // Return if current animation time is the same as any node time.
            if (nodeTimestamps.Any(
                nodeTimestamp => Utilities.FloatsEqual(
                    nodeTimestamp,
                    AnimationTime,
                    GlobalConstants.FloatPrecision))) {

                return;
            }

            // Get rotation point position.
            var localRotationPointPosition =
                pathData.GetRotationAtTime(AnimationTime);
            var globalRotationPointPosition =
                transform.TransformPoint(localRotationPointPosition);

            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                globalRotationPointPosition,
                SettingsAsset.GizmosSubfolder
                + SettingsAsset.CurrentRotationPointGizmoIcon,
                false);
        }

        private void HandleDrawingForwardPointIcon() {
            if (SettingsAsset.RotationMode == RotationMode.Forward) {
                var globalForwardPointPosition = GetGlobalForwardPoint();

                DrawForwardPointIcon(
                    globalForwardPointPosition);
            }
        }

        private void HandleDrawingRotationPathCurve() {
            if (SettingsAsset.HandleMode != HandleMode.Rotation) return;

            var localPointPositions = pathData.SampleRotationPathForPoints(
                SettingsAsset.RotationCurveSampling);

            var globalPointPositions =
                new Vector3[localPointPositions.Count];

            for (var i = 0; i < localPointPositions.Count; i++) {
                globalPointPositions[i] =
                    transform.TransformPoint(localPointPositions[i]);
            }
            if (globalPointPositions.Length < 2) return;

            Gizmos.color = SettingsAsset.RotationCurveColor;

            // Draw curve.
            for (var i = 0; i < globalPointPositions.Length - 1; i++) {
                Gizmos.DrawLine(
                    globalPointPositions[i],
                    globalPointPositions[i + 1]);
            }
        }

        private void HandleDrawingRotationPointGizmos() {
            if (SettingsAsset.HandleMode != HandleMode.Rotation) return;

            var globalRotPointPositions = GetGlobalRotationPointPositions();

            // Path node timestamps.
            var nodeTimestamps = pathData.GetPathTimestamps();

            for (var i = 0; i < globalRotPointPositions.Length; i++) {
                // Return if current animation time is the same as any node
                // time.
                if (Utilities.FloatsEqual(
                    nodeTimestamps[i],
                    AnimationTime,
                    GlobalConstants.FloatPrecision)) {

                    continue;
                }

                //Draw rotation point gizmo.
                Gizmos.DrawIcon(
                    globalRotPointPositions[i],
                    SettingsAsset.GizmosSubfolder
                    + SettingsAsset.RotationPointGizmoIcon,
                    false);
            }
        }

        private void HandleDrawingTargetIcon() {
            // If rotation mode set to target..
            if (SettingsAsset.RotationMode == RotationMode.Target
                // and target obj. is assigned..
                && TargetGO != null) {
                DrawTargetIcon(TargetGO.position);
            }
        }

        #endregion
    }

}