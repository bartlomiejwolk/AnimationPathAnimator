using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ATP.AnimationPathTools.AnimatorComponent {

    /// <summary>
    ///     Animates object along path.
    /// </summary>
    [ExecuteInEditMode]
    public sealed class Animator : MonoBehaviour {
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
        ///     Event called right after animation jump backward to the previous node.
        /// </summary>
        public event EventHandler<NodeReachedEventArgs> JumpedToNode;

        /// <summary>
        ///     Event called when animated object passes a node.
        ///     It'll be called only when anim. go is before a node in one frame
        ///     and after in the next one.
        /// </summary>
        public event EventHandler<NodeReachedEventArgs> NodeReached;

        /// <summary>
        ///     Event fired every time path data inspector field is changed or set to null.
        /// </summary>
        public event EventHandler PathDataRefChanged;

        /// <summary>
        ///     Event called after a new <c>PathData</c> asset is successfully created.
        /// </summary>
        /// <summary>
        ///     Event called from Editor after ValidateCommand of type UndoRedoPerformed was executed.
        /// </summary>
        public event EventHandler UndoRedoPerformed;

        #endregion

        #region FIELDS

        /// <summary>
        ///     Whether inspector advanced settings foldout should be open or folded.
        /// </summary>
        [SerializeField]
        private bool advancedSettingsFoldout;

        [SerializeField]
        private Transform animatedGO;

        [SerializeField]
        private float animationTime;

        private bool animGOUpdateEnabled;

        //private bool countdownCoroutineIsRunning;

        //private bool easeCoroutineRunning;

        private bool isPlaying;

        [SerializeField]
        private PathData pathData;

        private bool pause;

        /// <summary>
        ///     Animation time value from previous frame.
        /// </summary>
        private float prevAnimationTime;

        [SerializeField]
        private AnimatorSettings settingsAsset;

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
        ///     Percentage of the overall animation progress. 0.5 means position in the middle of the path.
        /// </summary>
        public float AnimationTime {
            get { return animationTime; }
            set {
                animationTime = value;

                // In play mode, when animation is playing..
                if (Application.isPlaying && IsPlaying) {
                    // Do nothing.
                }
                else {
                    // Update animated GO.
                    HandleUpdateAnimGOInSceneView();
                }
            }
        }

        /// <summary>
        ///     Whether to update animated object positon, rotation and tilting. It's executed independent from animation time,
        ///     so animated object can be updated even if animator is stopped or paused.
        /// </summary>
        public bool AnimGOUpdateEnabled {
            get { return animGOUpdateEnabled; }
            set { animGOUpdateEnabled = value; }
        }

        /// <summary>
        ///     It's true when <c>CountdownToStopAnimGOUpdate</c> coroutine is running.
        /// </summary>
        /// <summary>
        ///     It's set to true when <c>EaseTime</c> coroutine is running.
        /// </summary>
        /// <summary>
        ///     If animation is playing. It's true only when <c>EaseCoroutineRunning</c> is true and <c>Pause</c> is false.
        /// </summary>
        public bool IsPlaying {
            get { return isPlaying; }
            set { isPlaying = value; }
        }

        /// <summary>
        ///     Reference to asset file holding path data.
        /// </summary>
        public PathData PathData {
            get { return pathData; }
            set {
                // Remember current value.
                var oldValue = pathData;

                pathData = value;

                // Call event.
                if (pathData != oldValue) OnPathDataRefChanged();
            }
        }

        /// <summary>
        ///     Whether or not animation is paused. Animation can be paused only when animator is running.
        /// </summary>
        public bool Pause {
            get { return pause; }
            set {
                pause = value;

                // On unpause..
                if (!value) {
                    // Enable animating animated GO.
                    AnimGOUpdateEnabled = true;
                }
            }
        }

        /// <summary>
        ///     Reference to asset file holding animator settings.
        /// </summary>
        public AnimatorSettings SettingsAsset {
            get { return settingsAsset; }
        }

        /// <summary>
        ///     Reference to GUISkin asset.
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
        ///     If animation should be played backward.
        /// </summary>
        private bool Reverse { get; set; }

        #endregion PROPERTIES

        #region INSPECTOR SETTINGS

        [SerializeField]
        private bool autoPlay;

        /// <summary>
        ///     If autoplay is enabled, this is delay before animation starts playing.
        /// </summary>
        [SerializeField]
        private float autoPlayDelay;

        [SerializeField]
        private bool enableControlsInPlayMode = true;

        [SerializeField]
        private int exportSamplingFrequency = 5;

        [SerializeField]
        private float forwardPointOffset = 0.05f;

        [SerializeField]
        private Color gizmoCurveColor = Color.yellow;

        [SerializeField]
        private HandleMode handleMode =
            HandleMode.None;

        [SerializeField]
        private float longJumpValue = 0.01f;

        [SerializeField]
        private PositionHandle positionHandle = PositionHandle.Free;

        [SerializeField]
        private float positionLerpSpeed = 1;

        [SerializeField]
        private Color rotationCurveColor = Color.gray;

        [SerializeField]
        private RotationMode rotationMode =
            RotationMode.Forward;

        [SerializeField]
        private float rotationSlerpSpeed = 999.0f;

        [SerializeField]
        private float shortJumpValue = 0.002f;

        [SerializeField]
        private TangentMode tangentMode =
            TangentMode.Smooth;

        // todo rename to updateAllValues.
        [SerializeField]
        private bool updateAllMode;

        [SerializeField]
        private AnimatorWrapMode wrapMode = AnimatorWrapMode.Clamp;
#endregion
        #region INSPECTOR SETTINGS PROPERTIES

        public bool AutoPlay {
            get { return autoPlay; }
            set { autoPlay = value; }
        }

        /// <summary>
        ///     If autoplay is enabled, this is delay before animation starts playing.
        /// </summary>
        public float AutoPlayDelay {
            get { return autoPlayDelay; }
            set { autoPlayDelay = value; }
        }

        public bool EnableControlsInPlayMode {
            get { return enableControlsInPlayMode; }
            set { enableControlsInPlayMode = value; }
        }

        public int ExportSamplingFrequency {
            get { return exportSamplingFrequency; }
            set { exportSamplingFrequency = value; }
        }

        /// <summary>
        ///     How much look forward point should be positioned away from the
        ///     animated object.
        /// </summary>
        /// <remarks>Value is a time in range from 0 to 1.</remarks>
        public float ForwardPointOffset {
            get { return forwardPointOffset; }
            set { forwardPointOffset = value; }
        }

        /// <summary>
        ///     Color of the gizmo curve.
        /// </summary>
        public Color GizmoCurveColor {
            get { return gizmoCurveColor; }
            set { gizmoCurveColor = value; }
        }

        public HandleMode HandleMode {
            get { return handleMode; }
            set { handleMode = value; }
        }

        public float LongJumpValue {
            get { return longJumpValue; }
            set { longJumpValue = value; }
        }

        public PositionHandle PositionHandle {
            get { return positionHandle; }
            set { positionHandle = value; }
        }

        public float PositionLerpSpeed {
            get { return positionLerpSpeed; }
            set { positionLerpSpeed = value; }
        }

        public Color RotationCurveColor {
            get { return rotationCurveColor; }
            set { rotationCurveColor = value; }
        }

        public RotationMode RotationMode {
            get { return rotationMode; }
            set { rotationMode = value; }
        }

        public float RotationSlerpSpeed {
            get { return rotationSlerpSpeed; }
            set { rotationSlerpSpeed = value; }
        }

        /// <summary>
        ///     Value of the jump when modifier key is pressed.
        /// </summary>
        public float ShortJumpValue {
            get { return shortJumpValue; }
            set { shortJumpValue = value; }
        }

        public TangentMode TangentMode {
            get { return tangentMode; }
            set { tangentMode = value; }
        }

        public bool UpdateAllMode {
            get { return updateAllMode; }
            set { updateAllMode = value; }
        }

        public AnimatorWrapMode WrapMode {
            get { return wrapMode; }
            set { wrapMode = value; }
        }

        #endregion
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
            LoadRequiredAssets();
            SubscribeToEvents();
        }

        private void OnValidate() {
            HandleUpdateAnimGOInSceneView();
        }

        private void Reset() {
            LoadRequiredAssets();
            AssignMainCameraAsAnimatedGO();
            ResetInspectorOptions();
        }

        private void Start() {
            HandleStartAnimation();
        }

        private void Update() {
            UpdateAnimationTime();

            HandleClampWrapMode();
            HandleLoopWrapMode();
            HandlePingPongWrapMode();

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

        private void OnJumpedToNode(NodeReachedEventArgs e) {
            var handler = JumpedToNode;
            if (handler != null) handler(this, e);
        }

        private void OnNodeReached(NodeReachedEventArgs eventArgs) {
            var handler = NodeReached;
            if (handler != null) handler(this, eventArgs);
        }

        // todo move to region
        private void OnPathDataRefChanged() {
            Debug.Log("PathDataRefChanged");
            var handler = PathDataRefChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        // todo move to region
        private void OnUndoRedoPerformed() {
            var handler = UndoRedoPerformed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region EVENT HANDLERS

        private void APAnimator_AnimationEnded(object sender, EventArgs e) {
        }

        private void PathData_NodePositionChanged(object sender, EventArgs e) {
            HandleUpdateAnimGOInSceneView();
        }

        private void PathData_NodeTiltChanged(object sender, EventArgs e) {
            HandleUpdateAnimGOInSceneView();
        }

        private void PathData_PathReset(object sender, EventArgs e) {
            AnimationTime = 0;
            HandleUpdateAnimGOInSceneView();
        }

        private void PathData_RotationPathReset(object sender, EventArgs e) {
        }

        private void PathData_RotationPointPositionChanged(
            object sender,
            EventArgs e) {

            HandleUpdateAnimGOInSceneView();
        }

        #endregion

        #region ANIMATION
        private void UpdateAnimationTime() {
            if (!Application.isPlaying) return;
            // Return if not playing.
            if (!IsPlaying) return;

            // Get ease value.
            var timeStep =
                PathData.GetEaseValueAtTime(AnimationTime);

            // If animation is set to play backward..
            if (Reverse) {
                // Decrease animation time.
                AnimationTime -= timeStep * Time.deltaTime;
            }
            else {
                // Increase animation time.
                AnimationTime += timeStep * Time.deltaTime;
            }
        }


        /// <summary>
        ///     Pauses animation.
        /// </summary>
        public void PauseAnimation() {
            Pause = true;
        }

        /// <summary>
        ///     Toggles play/pause animation.
        /// </summary>
        public void PlayPauseAnimation() {
            Pause = !Pause;
        }

        /// <summary>
        ///     Sets rotation mode to Custom.
        /// </summary>
        public void SetRotationCustom() {
            RotationMode = RotationMode.Custom;
        }

        /// <summary>
        ///     Sets rotation mode to Forward.
        /// </summary>
        public void SetRotationForward() {
            RotationMode = RotationMode.Forward;
        }

        /// <summary>
        ///     Sets rotation mode to Target.
        /// </summary>
        public void SetRotationTarget() {
            RotationMode = RotationMode.Target;
        }

        /// <summary>
        ///     Sets tangent mode to Linear.
        /// </summary>
        public void SetTangentLinear() {
            TangentMode = TangentMode.Linear;
            PathData.SetLinearAnimObjPathTangents();
        }

        /// <summary>
        ///     Sets tangent mode to Smooth.
        /// </summary>
        public void SetTangentSmooth() {
            TangentMode = TangentMode.Smooth;
            PathData.SmoothAnimObjPathTangents();
        }

        /// <summary>
        ///     Sets wrap mode to Clamp.
        /// </summary>
        public void SetWrapClamp() {
            WrapMode = AnimatorWrapMode.Clamp;
        }

        /// <summary>
        ///     Sets wrap mode to Loop.
        /// </summary>
        public void SetWrapLoop() {
            WrapMode = AnimatorWrapMode.Loop;
        }

        /// <summary>
        ///     Set wrap mode to PingPong.
        /// </summary>
        public void SetWrapPingPong() {
            WrapMode = AnimatorWrapMode.PingPong;
        }

        /// <summary>
        ///     Starts animation.
        /// </summary>
        public void StartAnimation() {
            if (!PathDataAssetAssigned()) return;

            IsPlaying = true;
            AnimGOUpdateEnabled = true;

            HandleFireNodeReachedEventForFirstNode();
        }

        /// <summary>
        ///     Stops animation.
        /// </summary>
        /// <summary>
        ///     Unpauses animation.
        /// </summary>
        public void UnpauseAnimation() {
            Pause = false;
        }

        /// <summary>
        ///     Updates animated game object position.
        /// </summary>
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
                PositionLerpSpeed);
        }

        /// <summary>
        ///     Updates animated game object rotation.
        /// </summary>
        private void AnimateAnimatedGORotation() {
            if (AnimatedGO == null) return;

            // Look at target.
            if (TargetGO != null
                && RotationMode == RotationMode.Target) {

                RotateObjectWithSlerp(TargetGO.position);
            }
            // Use rotation path.
            if (RotationMode == RotationMode.Custom) {
                RotateObjectWithRotationPath();
            }
            // Look forward.
            else if (RotationMode == RotationMode.Forward) {
                var globalForwardPoint = GetGlobalForwardPoint();

                RotateObjectWithSlerp(globalForwardPoint);
            }
        }

        /// <summary>
        ///     Updates animated game object tilting.
        /// </summary>
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
        ///     Coroutine responsible for updating animation time during playback in play mode.
        /// </summary>
        /// <returns></returns>
        private IEnumerator EaseTime() {
            //EaseCoroutineRunning = true;
            //Pause = false;
            //IsPlaying = true;
            //AnimGOUpdateEnabled = true;

            while (true) {
                // If animation is not paused..
                if (!Pause) {
                    //HandleFireOnAnimationStartedEvent();

                    //UpdateAnimationTime();

                    //HandleClampWrapMode();
                    //HandleLoopWrapMode();
                    //HandlePingPongWrapMode();
                }

                //if (!EaseCoroutineRunning) break;

                yield return null;
            }
        }

        /// <summary>
        ///     Update animated GO position, rotation and tilting accordingly to current
        ///     animation time.
        /// </summary>
        /// <remarks>
        ///     Used to update animated GO with keys.
        /// </remarks>
        private void HandleUpdateAnimGOInSceneView() {
            if (!RequiredAssetsLoaded()) return;
            if (AnimatedGO == null) return;
            if (PathData == null) return;
            if (!enabled) return;

            UpdateAnimatedGOPosition();
            UpdateAnimatedGORotation();
            AnimateAnimatedGOTilting();
        }

        /// <summary>
        ///     Rotates animated GO using LookAt().
        /// </summary>
        /// <param name="targetPos">Point to look at.</param>
        private void RotateObjectWithLookAt(Vector3 targetPos) {
            AnimatedGO.LookAt(targetPos);
        }

        /// <summary>
        ///     Updates animated GO rotation using data from rotation path.
        /// </summary>
        private void RotateObjectWithRotationPath() {
            // Get 3d point to look at.
            var lookAtTarget =
                PathData.GetRotationAtTime(AnimationTime);
            // Convert target position to global coordinates.
            var lookAtTargetGlobal = transform.TransformPoint(lookAtTarget);

            // In play mode..
            if (Application.isPlaying) {
                RotateObjectWithSlerp(lookAtTargetGlobal);
            }
            // In editor mode..
            else {
                RotateObjectWithLookAt(lookAtTargetGlobal);
            }
        }

        //    if (!positionChanged && !rotationChanged) {
        //        // Stop updating animated game object.
        //        AnimGOUpdateEnabled = false;
        //        // Fire event.
        //        OnAnimationEnded();
        //    }
        //}
        /// <summary>
        ///     Rotates animated GO using Slerp function.
        /// </summary>
        /// <param name="targetPosition">Point to look at.</param>
        private void RotateObjectWithSlerp(Vector3 targetPosition) {
            // Return when point to look at is at the same position as the
            // animated object.
            if (targetPosition == AnimatedGO.position) return;

            // Calculate direction to target.
            var targetDirection = targetPosition - AnimatedGO.position;
            // Calculate rotation to target.
            var rotation = Quaternion.LookRotation(targetDirection);
            // Calculate rotation speed.
            var speed = Time.deltaTime * RotationSlerpSpeed;

            // Lerp rotation.
            AnimatedGO.rotation = Quaternion.Slerp(
                AnimatedGO.rotation,
                rotation,
                speed);
        }

        /// <summary>
        ///     Updates animated game object position, rotation and tilting.
        /// </summary>
        private void AnimateAnimatedGO() {
            AnimateAnimatedGOPosition();
            AnimateAnimatedGORotation();
            AnimateAnimatedGOTilting();
        }

        /// <summary>
        ///     Coroutine that will remember animated game object's current position, rotation and tilting
        ///     and after a given number of frames will check if any of those values changed. If none was changed, animated GO
        ///     stops being updated and <c>AnimatonEnded</c> event is called.
        /// </summary>
        /// <summary>
        ///     Updates animated game object position accordingly to current animation time.
        /// </summary>
        private void UpdateAnimatedGOPosition() {
            // Get animated GO position at current animation time.
            var positionAtTimestamp =
                PathData.GetVectorAtTime(AnimationTime);

            var globalPositionAtTimestamp =
                transform.TransformPoint(positionAtTimestamp);

            // Update animatedGO position.
            AnimatedGO.position = globalPositionAtTimestamp;
        }

        /// <summary>
        ///     Updates animated game object rotation accordingly to current animation time.
        /// </summary>
        private void UpdateAnimatedGORotation() {
            if (AnimatedGO == null) return;

            // If rotation mode is set to..
            switch (RotationMode) {
                case RotationMode.Forward:
                    var globalForwardPoint = GetGlobalForwardPoint();

                    RotateObjectWithLookAt(globalForwardPoint);

                    break;
                case RotationMode.Custom:
                    // Get rotation point position.
                    var rotationPointPos =
                        PathData.GetRotationAtTime(AnimationTime);

                    // Convert target position to global coordinates.
                    var rotationPointGlobalPos =
                        transform.TransformPoint(rotationPointPos);

                    // Update animated GO rotation.
                    RotateObjectWithLookAt(rotationPointGlobalPos);

                    break;
                case RotationMode.Target:
                    if (TargetGO == null) return;

                    RotateObjectWithLookAt(TargetGO.position);
                    break;
            }
        }
        #endregion

        #region ANIMATION HANDLERS
        /// <summary>
        ///     Update animation time with values taken from ease curve.
        /// </summary>
        /// <remarks>This is used to update animation time when animator is running.</remarks>
        /// <summary>
        ///     Decided what to at the end of animation when Clamp mode is selected.
        /// </summary>
        private void HandleClampWrapMode() {
            if (AnimationTime > 1
                && WrapMode == AnimatorWrapMode.Clamp) {

                AnimationTime = 1;
                IsPlaying = false;

                // Fire event.
                OnAnimationEnded();
            }
        }

        /// <summary>
        ///     This method is responsible for firing <c>NodeReached</c> event.
        ///     Use in play mode only.
        /// </summary>
        private void HandleFireNodeReachedEvent() {
            // Get path timestamps.
            var nodeTimestamps = PathData.GetPathTimestamps();

            // For each timestamp..
            for (var i = 0; i < nodeTimestamps.Length; i++) {
                // If animation time "jumped over" a node..
                if (prevAnimationTime < nodeTimestamps[i]
                    && AnimationTime >= nodeTimestamps[i]) {

                    // Create event args.
                    var args = new NodeReachedEventArgs(i, AnimationTime);
                    // Fire event.
                    OnNodeReached(args);
                }
            }

            // Update helper field.
            prevAnimationTime = AnimationTime;
        }

        /// <summary>
        ///     Method responsible for firing <c>AnimationStarted</c> event.
        ///     Use in play mode only.
        /// </summary>
        private void HandleFireOnAnimationStartedEvent() {
            if (AnimationTime == 0) OnAnimationStarted();
        }

        /// <summary>
        ///     Decides what to do on animation end in Loop wrap mode.
        /// </summary>
        private void HandleLoopWrapMode() {
            if (AnimationTime > 1 && WrapMode == AnimatorWrapMode.Loop) {
                AnimationTime = 0;
            }
        }

        /// <summary>
        ///     Decides what to do on animation end in PingPong wrap mode.
        /// </summary>
        private void HandlePingPongWrapMode() {
            if (AnimationTime > 1
                && WrapMode == AnimatorWrapMode.PingPong) {

                Reverse = true;
            }

            if (AnimationTime < 0
                && WrapMode == AnimatorWrapMode.PingPong) {

                Reverse = false;
            }
        }

        /// <summary>
        ///     Allows toggle pause. Use only in play mode.
        /// </summary>
        private void HandlePlayPause() {
            if (!Application.isPlaying) return;

            // Animation is playing and unpaused.
            if (!Pause) {
                // Pause animation.
                Pause = true;
                IsPlaying = false;
            }
            // Animation is playing but paused.
            else if (Pause) {
                // Unpause animation.
                Pause = false;
                IsPlaying = true;
            }
            // Animation ended.
            else if (AnimationTime >= 1) {
                AnimationTime = 0;
                StartAnimation();
            }
            // Disable play/pause while for animation start being invoked.
            else if (IsInvoking("StartAnimation")) {
                // Do nothing.
            }
            else {
                // Start animation.
                StartAnimation();
            }
        }

        /// <summary>
        ///     Decides if to start animation playback on enter play mode.
        /// </summary>
        private void HandleStartAnimation() {
            if (Application.isPlaying && AutoPlay) {
                Invoke("StartAnimation", AutoPlayDelay);
            }
        }

        /// <summary>
        ///     Method responsible for updating animated GO position, rotation and tilting in play mode during playback.
        /// </summary>
        private void HandleUpdatingAnimGOInPlayMode() {
            // Return if not in play mode.
            if (!Application.isPlaying) return;
            // Return if anim. GO update is disabled.
            if (!AnimGOUpdateEnabled) return;

            var prevGOPosition = animatedGO.position;
            var prevGORotation = animatedGO.rotation;

            AnimateAnimatedGO();
            HandleFireNodeReachedEvent();
            //HandleStartCountdownCoroutine();

            var positionChanged = !Utilities.V3Equal(
                prevGOPosition,
                animatedGO.position);

            var rotationChanged = !Utilities.QuaternionsEqual(
                prevGORotation,
                animatedGO.rotation);

            if (!positionChanged && !rotationChanged) {
                // Stop updating animated game object.
                AnimGOUpdateEnabled = false;
            }
        }

        /// <summary>
        ///     Handle starting <c>CountdownToStopAnimGOUpdate</c> coroutine.
        /// </summary>
        /// <summary>
        ///     Used at animation start to fire <c>NodeReached </c> event for the first node.
        /// </summary>
        private void HandleFireNodeReachedEventForFirstNode() {
            if (AnimationTime == 0) {
                var args = new NodeReachedEventArgs(0, 0);
                // Fire event.
                OnNodeReached(args);
            }
        }
        #endregion
        #region OTHER HANDLERS

        /// <summary>
        ///     Method responsible for detecting all shortcuts pressed in play mode.
        /// </summary>
        private void HandleShortcuts() {
            if (!EnableControlsInPlayMode) return;

            // Play/Pause.
            if (Input.GetKeyDown(SettingsAsset.PlayPauseKey)) {
                HandlePlayPause();
            }

            // Long jump forward
            if (Input.GetKeyDown(SettingsAsset.LongJumpForwardKey)) {
                AnimationTime += LongJumpValue;
            }

            // Long jump backward. 
            if (Input.GetKeyDown(SettingsAsset.LongJumpBackwardKey)) {
                AnimationTime -= LongJumpValue;
            }

            // Jump to next node.
            if (Input.GetKeyDown(SettingsAsset.JumpToNextNodeKey)) {
                AnimationTime = GetNearestForwardNodeTimestamp();

                FireJumpedToNodeEvent();
            }

            // Jump to previous node.
            if (Input.GetKeyDown(SettingsAsset.JumpToPreviousNodeKey)) {
                AnimationTime = GetNearestBackwardNodeTimestamp();

                FireJumpedToNodeEvent();
            }

            // Jump to beginning.
            if (Input.GetKeyDown(
                SettingsAsset.JumpToPreviousNodeKey)
                && Input.GetKey(SettingsAsset.PlayModeModKey)) {

                AnimationTime = 0;

                FireJumpedToNodeEvent();
            }
        }

        #endregion

        #region METHODS

        /// <summary>
        ///     Export path nodes as transforms.
        /// </summary>
        /// <param name="exportSampling">
        ///     Number of transforms to be extracted from one meter of the path.
        /// </param>
        public void ExportNodes(
            int exportSampling) {

            // exportSampling cannot be less than 0.
            if (exportSampling < 0) return;

            // Points to export.
            var points = pathData.SampleAnimationPathForPoints(
                exportSampling);

            // Convert points to global coordinates.
            Utilities.ConvertToGlobalCoordinates(ref points, transform);

            // Create parent GO.
            var exportedPath = new GameObject("exported_path");

            // Create child GOs.
            for (var i = 0; i < points.Count; i++) {
                // Create child GO.
                var nodeGo = new GameObject("Node " + i);

                // Move node under the path GO.
                nodeGo.transform.parent = exportedPath.transform;

                // Assign node local position.
                nodeGo.transform.localPosition = points[i];
            }
        }

        /// <summary>
        ///     Returns global node positions.
        /// </summary>
        /// <param name="nodesNo">Number of nodes to return, starting from index 0.</param>
        /// <returns>Global node positions.</returns>
        public Vector3[] GetGlobalNodePositions(int nodesNo = -1) {
            var nodePositions = PathData.GetNodePositions(nodesNo);

            Utilities.ConvertToGlobalCoordinates(
                ref nodePositions,
                transform);

            return nodePositions;
        }

        /// <summary>
        ///     Assigns camera tagged "MainCamera" as animated game object.
        /// </summary>
        private void AssignMainCameraAsAnimatedGO() {
            if (AnimatedGO == null && Camera.main != null) {
                animatedGO = Camera.main.transform;
            }
            else {
                Debug.LogWarning("Camera with tag \"MainCamera\" not found.");
            }
        }

        /// <summary>
        ///     Returns local forward point position for current animation time.
        /// </summary>
        /// <returns>Local forward point position.</returns>
        // todo rename to CalculateLocalForwardPointPosition().
        private Vector3 CalculateForwardPointPosition() {
            // Timestamp offset of the forward point.
            var forwardPointDelta = ForwardPointOffset;
            // Forward point timestamp.
            var forwardPointTimestamp = AnimationTime + forwardPointDelta;
            var localPosition = PathData.GetVectorAtTime(forwardPointTimestamp);

            return localPosition;
        }

        private void FireJumpedToNodeEvent() {
            // Create event args.
            var nodeIndex = PathData.GetAnimObjNodeIndexAtTime(
                animationTime);
            var args = new NodeReachedEventArgs(nodeIndex, animationTime);

            // Fire event.
            OnJumpedToNode(args);
        }

        private void FireUndoRedoPerformedEvent() {
            OnUndoRedoPerformed();
        }

        /// <summary>
        ///     Returns global forward point position for current animation time.
        /// </summary>
        /// <returns>Global forward point position.</returns>
        private Vector3 GetGlobalForwardPoint() {
            var localForwardPoint = CalculateForwardPointPosition();
            var globalForwardPoint =
                transform.TransformPoint(localForwardPoint);

            return globalForwardPoint;
        }

        /// <summary>
        ///     Returns global rotation path node positions.
        /// </summary>
        /// <returns></returns>
        private Vector3[] GetGlobalRotationPointPositions() {
            var localRotPointPositions =
                pathData.GetRotationPointPositions();

            var globalRotPointPositions =
                new Vector3[localRotPointPositions.Length];

            // For each local point..
            for (var i = 0; i < localRotPointPositions.Length; i++) {
                // Convert to global.
                globalRotPointPositions[i] =
                    transform.TransformPoint(localRotPointPositions[i]);
            }

            return globalRotPointPositions;
        }

        /// <summary>
        ///     Returns timestamp of a node which timestamp is closest to and bigger than the current animation time.
        /// </summary>
        /// <returns>Node timestamp.</returns>
        private float GetNearestBackwardNodeTimestamp() {
            var pathTimestamps = PathData.GetPathTimestamps();

            // For node timestamp that is smaller than animation time..
            for (var i = pathTimestamps.Length - 1; i >= 0; i--) {
                if (pathTimestamps[i] < AnimationTime) {
                    return pathTimestamps[i];
                }
            }

            // Return timestamp of the last node.
            return 0;
        }

        /// <summary>
        ///     Returns timestamp of a node which timestamp is closest to and bigger than the current animation time.
        /// </summary>
        /// <returns>Node timestamp.</returns>
        private float GetNearestForwardNodeTimestamp() {
            var pathTimestamps = PathData.GetPathTimestamps();

            // For node timestamp that is bigger than animation time..
            foreach (var timestamp in pathTimestamps
                .Where(timestamp => timestamp > AnimationTime)) {

                return timestamp;
            }

            // Return timestamp of the last node.
            return 1.0f;
        }

        /// <summary>
        ///     Loads asset files from component folder, that are required for the component to run.
        /// </summary>
        private void LoadRequiredAssets() {
            if (settingsAsset == null) {
                settingsAsset = Resources.Load("DefaultAnimatorSettings")
                    as AnimatorSettings;
            }

            if (skin == null) {
                skin = Resources.Load("DefaultAnimatorSkin") as GUISkin;
            }
        }

        /// <summary>
        ///     Use it to guard agains null path data asset.
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
        ///     Returns true if assets required by the component are referenced.
        /// </summary>
        /// <returns></returns>
        private bool RequiredAssetsLoaded() {
            if (SettingsAsset != null
                && Skin != null) {
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Reset inspector options.
        /// </summary>
        private void ResetInspectorOptions() {
            TargetGO = null;
            HandleMode = HandleMode.None;
            TangentMode = TangentMode.Smooth;
            UpdateAllMode = false;
            AnimationTime = 0;
            AutoPlay = true;
            EnableControlsInPlayMode = true;
            RotationMode = RotationMode.Forward;
            WrapMode = AnimatorWrapMode.Clamp;
            ForwardPointOffset = 0.001f;
            PositionLerpSpeed = 1;
            RotationSlerpSpeed = 999;
            ExportSamplingFrequency = 5;
        }

        /// <summary>
        ///     Subscribe to events.
        /// </summary>
        private void SubscribeToEvents() {
            if (pathData == null) return;

            PathData.RotationPointPositionChanged +=
                PathData_RotationPointPositionChanged;
            PathData.NodePositionChanged += PathData_NodePositionChanged;
            PathData.NodeTiltChanged += PathData_NodeTiltChanged;
            PathData.PathReset += PathData_PathReset;
            PathData.RotationPathReset += PathData_RotationPathReset;
            AnimationEnded += APAnimator_AnimationEnded;

        }

        /// <summary>
        ///     Unsubscribe from events.
        /// </summary>
        private void UnsubscribeFromEvents() {
            if (PathData == null) return;

            PathData.RotationPointPositionChanged -=
                PathData_RotationPointPositionChanged;
            PathData.NodePositionChanged -= PathData_NodePositionChanged;
            PathData.NodeTiltChanged -= PathData_NodeTiltChanged;
            PathData.PathReset -= PathData_PathReset;
            PathData.RotationPathReset -= PathData_RotationPathReset;
            AnimationEnded -= APAnimator_AnimationEnded;
        }

        #endregion

        #region GIZMOS

        /// <summary>
        ///     Draw animated object path gizmo curve.
        /// </summary>
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

            Gizmos.color = GizmoCurveColor;

            // Draw curve.
            for (var i = 0; i < points.Count - 1; i++) {
                Gizmos.DrawLine(globalPoints[i], globalPoints[i + 1]);
            }
        }

        /// <summary>
        ///     Draw forward poing gizmo icon.
        /// </summary>
        /// <param name="forwardPointPosition"></param>
        private void DrawForwardPointIcon(Vector3 forwardPointPosition) {
            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                forwardPointPosition,
                SettingsAsset.GizmosSubfolder + SettingsAsset.ForwardPointIcon,
                false);
        }

        /// <summary>
        ///     Draw gizmo icon for target transform.
        /// </summary>
        /// <param name="targetPosition"></param>
        private void DrawTargetIcon(Vector3 targetPosition) {
            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                targetPosition,
                SettingsAsset.GizmosSubfolder + SettingsAsset.TargetGizmoIcon,
                false);
        }

        /// <summary>
        ///     Handle drawing rotation point gizmo for current animation time.
        ///     It'll be drawn only when animation time is the same as a path node.
        /// </summary>
        private void HandleDrawingCurrentRotationPointGizmo() {
            if (HandleMode != HandleMode.Rotation) return;

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

        /// <summary>
        ///     Handle drawing forward point icon.
        /// </summary>
        private void HandleDrawingForwardPointIcon() {
            if (RotationMode == RotationMode.Forward) {
                var globalForwardPointPosition = GetGlobalForwardPoint();

                DrawForwardPointIcon(
                    globalForwardPointPosition);
            }
        }

        /// <summary>
        ///     Handle drawing rotation path curve.
        /// </summary>
        private void HandleDrawingRotationPathCurve() {
            if (HandleMode != HandleMode.Rotation) return;

            var localPointPositions = pathData.SampleRotationPathForPoints(
                SettingsAsset.RotationCurveSampling);

            var globalPointPositions =
                new Vector3[localPointPositions.Count];

            for (var i = 0; i < localPointPositions.Count; i++) {
                globalPointPositions[i] =
                    transform.TransformPoint(localPointPositions[i]);
            }
            if (globalPointPositions.Length < 2) return;

            Gizmos.color = RotationCurveColor;

            // Draw curve.
            for (var i = 0; i < globalPointPositions.Length - 1; i++) {
                Gizmos.DrawLine(
                    globalPointPositions[i],
                    globalPointPositions[i + 1]);
            }
        }

        /// <summary>
        ///     Handle drawing rotation point gizmos.
        /// </summary>
        private void HandleDrawingRotationPointGizmos() {
            if (HandleMode != HandleMode.Rotation) return;

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

        /// <summary>
        ///     Handle drawing target game object icon.
        /// </summary>
        private void HandleDrawingTargetIcon() {
            // If rotation mode set to target..
            if (RotationMode == RotationMode.Target
                // and target obj. is assigned..
                && TargetGO != null) {
                DrawTargetIcon(TargetGO.position);
            }
        }

        #endregion
    }

}