using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ATP.AnimationPathAnimator.APAnimatorComponent {

    /// <summary>
    ///     Component that allows animating transforms position along predefined
    ///     Animation Paths and also animate their rotation on x and y axis in
    ///     time.
    /// </summary>
    [ExecuteInEditMode]
    public sealed class APAnimator : MonoBehaviour {
        #region EVENTS

        public event EventHandler<NodeReachedEventArgs> NodeReached;

        #endregion

        #region FIELDS

        [SerializeField]
        private bool advancedSettingsFoldout;

        /// <summary>
        ///     Transform to be animated.
        /// </summary>
        [SerializeField]
        private Transform animatedGO;

        [SerializeField]
        private float animationTime;

       

        [SerializeField]
        private PathData pathData;

        [SerializeField]
        private APAnimatorSettings settingsAsset;

        private bool pause;

        private bool isPlaying;

        [SerializeField]
        private GUISkin skin;

        [SerializeField]
        private bool subscribedToEvents;

        private bool animGOUpdateEnabled;

        /// <summary>
        ///     Transform that the <c>animatedGO</c> will be looking at.
        /// </summary>
        [SerializeField]
        private Transform targetGO;

        [SerializeField]
        private Transform thisTransform;
        private bool countdownCoroutineIsRunning;

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

        /// <summary>
        ///     If animation is currently enabled.
        /// </summary>
        /// <remarks>When it's true, it means that the HandleEaseTime coroutine is running.</remarks>
        public bool IsPlaying {
            get { return isPlaying; }
            private set {
                isPlaying = value;
                Debug.Log("IsPlaying:" + isPlaying);
            }
        }

        public PathData PathData {
            get { return pathData; }
            set { pathData = value; }
        }

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

        public APAnimatorSettings SettingsAsset {
            get { return settingsAsset; }
        }

        private bool Reverse { get; set; }

        public GUISkin Skin {
            get { return skin; }
        }

        /// <summary>
        /// Helper field.
        /// </summary>
        public Transform ThisTransform {
            get { return thisTransform; }
        }

        /// <summary>
        ///     Transform that the <c>animatedGO</c> will be looking at.
        /// </summary>
        public Transform TargetGO {
            get { return targetGO; }
            set { targetGO = value; }
        }

        /// <summary>
        ///     Transform to be animated.
        /// </summary>
        public Transform AnimatedGO {
            get { return animatedGO; }
        }
        /// <summary>
        ///     If animator is currently subscribed to path events.
        /// </summary>
        /// <remarks>It's only public because of the Editor class.</remarks>
        private bool SubscribedToEvents {
            set { subscribedToEvents = value; }
        }

        public bool AnimGOUpdateEnabled {
            get { return animGOUpdateEnabled; }
            set { animGOUpdateEnabled = value; }
        }

        public bool CountdownCoroutineIsRunning {
            get { return countdownCoroutineIsRunning; }
            set {
                countdownCoroutineIsRunning = value;
            }
        }

        #endregion PROPERTIES

        #region UNITY MESSAGES

        private void OnEnable() {
            thisTransform = GetComponent<Transform>();

            LoadRequiredResources();
            AssignMainCameraAsAnimatedGO();
            SubscribeToEvents();
        }
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

        private void Start() {
            if (Application.isPlaying && SettingsAsset.AutoPlay) {
                StartAnimation();
                IsPlaying = true;
            }
        }

        private void Reset() {
            thisTransform = GetComponent<Transform>();

            LoadRequiredResources();
            AssignMainCameraAsAnimatedGO();
            ResetInspectorOptions();
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

        private void OnValidate() {
            UpdateAnimation();
        }

        private void Update() {
            HandleUpdatingAnimGOInPlayMode();
        }
        #endregion UNITY MESSAGES

        #region EVENT HANDLERS

        private void PathData_RotationPathReset(object sender, EventArgs e) {
            SettingsAsset.RotationMode = RotationMode.Custom;
        }

        private void APAnimator_NodeReached(NodeReachedEventArgs eventArgs) {
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
        private IEnumerator CountdownToStopAnimGOUpdate() {
            var frame = 0;
            var prevGOPosition = animatedGO.position;
            var prevGORotation = animatedGO.rotation;
            CountdownCoroutineIsRunning = true;

            Debug.Log("Start CounddownToStopAnimGOUpdate coroutine.");

            while (true) {
                frame++;

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
                AnimGOUpdateEnabled = false;
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


        private void Animate() {
            AnimateAnimatedGOPosition();
            AnimateAnimatedGORotation();
            AnimateAnimatedGOTilting();
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

        public void StartAnimation() {
            // Check for path data asset.
            if (PathData == null) {
                Debug.LogWarning("Assign Path Asset in the inspector.");
                return;
            }

            // Fire NodeReached event for first node.
            if (AnimationTime == 0) {
                var args = new NodeReachedEventArgs(0, 0);
                APAnimator_NodeReached(args);
            }

            StartCoroutine("HandleEaseTime");
        }

        public void StopAnimation() {
            StopCoroutine("HandleEaseTime");

            IsPlaying = false;
            Pause = false;
            AnimationTime = 0;
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
                SettingsAsset.PositionLerpSpeed);

        }

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
            AnimGOUpdateEnabled = true;

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
                && SettingsAsset.WrapMode == AnimatorWrapMode.Clamp) {

                AnimationTime = 1;
                IsPlaying = false;
            }
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
                ThisTransform.TransformPoint(positionAtTimestamp);

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
        private void UnsubscribeFromEvents() {
            if (PathData == null) return;

            PathData.NodePositionChanged -= PathData_NodePositionChanged;
            PathData.NodeTiltChanged -= PathData_NodeTiltChanged;
            PathData.PathReset -= PathData_PathReset;
            PathData.RotationPathReset -= PathData_RotationPathReset;

            SubscribedToEvents = false;
        }

        private void AssignMainCameraAsAnimatedGO() {
            if (AnimatedGO == null && Camera.main != null) {
                animatedGO = Camera.main.transform;
            }
        }

        private void SubscribeToEvents() {
            if (pathData == null) return;

            PathData.RotationPointPositionChanged +=
                PathData_RotationPointPositionChanged;
            PathData.NodePositionChanged += PathData_NodePositionChanged;
            PathData.NodeTiltChanged += PathData_NodeTiltChanged;
            PathData.PathReset += PathData_PathReset;
            PathData.RotationPathReset += PathData_RotationPathReset;

            SubscribedToEvents = true;
        }

        private void LoadRequiredResources() {
            settingsAsset = Resources.Load("DefaultAnimatorSettings")
                as APAnimatorSettings;
            skin = Resources.Load("DefaultAnimatorSkin") as GUISkin;
        }


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
            APAnimator_NodeReached(args);
        }

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
                ThisTransform.TransformPoint(localForwardPoint);

            return globalForwardPoint;
        }

        private bool RequiredAssetsLoaded() {
            if (SettingsAsset != null
                && Skin != null) {
                return true;
            }
            return false;
        }

        public Vector3[] GetGlobalNodePositions(int nodesNo = -1) {
            var nodePositions = PathData.GetNodePositions(nodesNo);
            Utilities.ConvertToGlobalCoordinates(ref nodePositions, ThisTransform);

            return nodePositions;
        }


        #endregion METHODS
        #region GIZMOS
        private void HandleDrawingForwardPointIcon() {
            if (SettingsAsset.RotationMode == RotationMode.Forward) {
                var globalForwardPointPosition = GetGlobalForwardPoint();

                DrawForwardPointIcon(
                    globalForwardPointPosition);
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


#if UNITY_EDITOR
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
                    globalPointPositions[i], globalPointPositions[i + 1]);
            }
        }

        private void HandleDrawingRotationPointGizmos() {
            if (SettingsAsset.HandleMode != HandleMode.Rotation) return;

            var localRotPointPositions =
                pathData.GetRotationPointPositions();

            var globalRotPointPositions =
                new Vector3[localRotPointPositions.Length];

            for (int i = 0; i < localRotPointPositions.Length; i++) {
                globalRotPointPositions[i] =
                    transform.TransformPoint(localRotPointPositions[i]);
            }

            // Path node timestamps.
            var nodeTimestamps = pathData.GetPathTimestamps();

            for (var i = 0; i < globalRotPointPositions.Length; i++) {
                // Return if current animation time is the same as any node
                // time.
                if (Math.Abs(nodeTimestamps[i] - AnimationTime) <
                    GlobalConstants.FloatPrecision) {
                    continue;
                }

                //Draw rotation point gizmo.
                Gizmos.DrawIcon(
                globalRotPointPositions[i],
                SettingsAsset.GizmosSubfolder + SettingsAsset.RotationPointGizmoIcon,
                false);
            }
        }

        private void DrawTargetIcon(Vector3 targetPosition) {
            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                targetPosition,
                SettingsAsset.GizmosSubfolder + SettingsAsset.TargetGizmoIcon,
                false);
        }

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

        private void HandleDrawingCurrentRotationPointGizmo() {
            if (SettingsAsset.HandleMode != HandleMode.Rotation) return;

            // Node path node timestamps.
            var nodeTimestamps = pathData.GetPathTimestamps();

            // Return if current animation time is the same as any node time.
            if (nodeTimestamps.Any(nodeTimestamp =>
                    Math.Abs(nodeTimestamp - AnimationTime)
                    < GlobalConstants.FloatPrecision)) {
                return;
            }

            // Get rotation point position.
            var localRotationPointPosition =
                pathData.GetRotationAtTime(AnimationTime);
            var globalRotationPointPosition =
                transform.TransformPoint(localRotationPointPosition);

            //Draw rotation point gizmo.
            Gizmos.DrawIcon(globalRotationPointPosition,
                SettingsAsset.GizmosSubfolder + SettingsAsset.CurrentRotationPointGizmoIcon,
                false);
        }

        private void DrawForwardPointIcon(Vector3 forwardPointPosition) {
            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                forwardPointPosition,
                SettingsAsset.GizmosSubfolder + SettingsAsset.ForwardPointIcon,
                false);
        }
#endif
        #endregion
    }

}