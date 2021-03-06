﻿// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com)
//  
// This file is part of the AnimationPath Animator extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnimationPathAnimator.AnimatorComponent {

    /// <summary>
    ///     Animates object along path.
    /// </summary>
    [ExecuteInEditMode]
    public sealed class PathAnimator : MonoBehaviour {
        #region EVENTS

        public delegate void PlayPauseEventHandler(
            object sender,
            float timestamp);

        /// <summary>
        ///     Event fired when animation time reaches 1. Stoping animation
        ///     manually won't fire this event.
        /// </summary>
        public event EventHandler AnimationEnded;

        /// <summary>
        ///     Event fired when animation is paused when playing.
        /// </summary>
        public event EventHandler AnimationPaused;

        /// <summary>
        ///     Event fired on animation resumed from pause state.
        /// </summary>
        public event EventHandler AnimationResumed;

        /// <summary>
        ///     Event fired when animation time is 0 and animation starts playing.
        /// </summary>
        public event EventHandler AnimationStarted;

        /// <summary>
        ///     Event fired after animation was stopped.
        /// </summary>
        public event EventHandler AnimationStopped;

        /// <summary>
        ///     Event called right after animation jump backward to the previous
        ///     node.
        /// </summary>
        public event EventHandler<NodeReachedEventArgs> JumpedToNode;

        /// <summary>
        ///     Event called when animated object passes a node. It'll be called
        ///     when anim. go is positioned before a node in one frame It'll be
        ///     called also when animation starts and timestamp is equal to any
        ///     node. and after in the next one.
        /// </summary>
        public event EventHandler<NodeReachedEventArgs> NodeReached;

        /// <summary>
        ///     Event fired every time path data inspector field is changed or set
        ///     to null.
        /// </summary>
        public event EventHandler PathDataRefChanged;

        /// <summary>
        ///     Event called from Editor after ValidateCommand of type
        ///     UndoRedoPerformed was executed.
        /// </summary>
        public event EventHandler UndoRedoPerformed;

        #endregion EVENTS
        
        #region CONST
        
        public const string VERSION = "v0.2.6";
        
        #endregion

        #region FIELDS

        /// <summary>
        ///     Whether inspector advanced settings foldout should be open or
        ///     folded.
        /// </summary>
        [SerializeField]
        private bool advancedSettingsFoldout;

        [SerializeField]
        private Transform animatedGO;

        [SerializeField]
        private float animationTime;

        [SerializeField]
        private bool drawNodeButtons = true;

        /// <summary>
        ///     Draw animated object path curve on the scene.
        /// </summary>
        [SerializeField]
        private bool drawObjectPath = true;

        [SerializeField]
        private bool drawRotationPathCurve;

        private bool isPlaying;

        [SerializeField]
        private bool moveAllMode;

        [SerializeField]
        private NodeHandle nodeHandle = NodeHandle.Position;

        [SerializeField]
        private PathData pathData;

        [SerializeField]
        private AnimatorSettings settingsAsset;

        [SerializeField]
        private GUISkin skin;

        [SerializeField]
        private Transform targetGO;

        #endregion FIELDS

        #region PROPERTIES

        /// <summary>
        ///     Transform to be animated.
        /// </summary>
        public Transform AnimatedGO {
            get { return animatedGO; }
        }

        /// <summary>
        ///     Animation time ratio. 0.5 means that half of the overall
        ///     animation time is over.
        /// </summary>
        public float AnimationTime {
            get { return animationTime; }
            set {
                // Validate value.
                if (value < 0) {
                    animationTime = 0;
                }
                else if (value > 1) {
                    animationTime = 1;
                }
                else {
                    animationTime = value;
                }

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
        ///     Whether to update animated object positon, rotation and tilting.
        ///     It's executed independent from animation time, so animated object
        ///     can be updated even if animator is stopped or paused.
        /// </summary>
        public bool AnimGOUpdateEnabled { get; set; }

        public bool IsPlaying {
            get { return isPlaying; }
            private set {
                isPlaying = value;

                if (value) {
                    AnimGOUpdateEnabled = true;
                }
            }
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
                if (value != oldValue) OnPathDataRefChanged();
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

        private bool DuringPlayback {
            get { return AnimationTime > 0 && AnimationTime < 1; }
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
        ///     If autoplay is enabled, this is delay before animation starts
        ///     playing.
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
        private Color gizmoCurveColor;

        [SerializeField]
        private NodeTool nodeTool =
            NodeTool.None;

        [SerializeField]
        private float longJumpValue = 0.01f;

        [SerializeField]
        private PositionHandle positionHandle = PositionHandle.Free;

        [SerializeField]
        private float positionLerpSpeed = 1;

        [SerializeField]
        private Color rotationCurveColor;

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

        [SerializeField]
        private bool updateAllMode;

        [SerializeField]
        private AnimatorWrapMode wrapMode = AnimatorWrapMode.Clamp;

        #endregion INSPECTOR SETTINGS

        #region INSPECTOR SETTINGS PROPERTIES

        public bool AutoPlay {
            get { return autoPlay; }
            set { autoPlay = value; }
        }

        /// <summary>
        ///     If autoplay is enabled, this is delay before animation starts
        ///     playing.
        /// </summary>
        public float AutoPlayDelay {
            get { return autoPlayDelay; }
            set { autoPlayDelay = value; }
        }

        public bool DrawNodeButtons {
            get { return drawNodeButtons; }
            set { drawNodeButtons = value; }
        }

        /// <summary>
        ///     Draw animated object path curve on the scene.
        /// </summary>
        public bool DrawObjectPath {
            get { return drawObjectPath; }
            set { drawObjectPath = value; }
        }

        public bool DrawRotationPathCurve {
            get { return drawRotationPathCurve; }
            set { drawRotationPathCurve = value; }
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

        public NodeTool NodeTool {
            get { return nodeTool; }
            set {
                nodeTool = value;

                if (value != NodeTool.None) {
                    PositionHandle = PositionHandle.Free;
                }
            }
        }

        public float LongJumpValue {
            get { return longJumpValue; }
            set { longJumpValue = value; }
        }

        public bool MoveAllMode {
            get { return moveAllMode; }
            set { moveAllMode = value; }
        }

        public NodeHandle NodeHandle {
            get { return nodeHandle; }
            set { nodeHandle = value; }
        }

        public PositionHandle PositionHandle {
            get { return positionHandle; }
            set {
                positionHandle = value;

                // Update scene tools.
                if (value == PositionHandle.Default) {
                    NodeTool = NodeTool.None;
                }
            }
        }

        public float PositionLerpSpeed {
            get { return positionLerpSpeed; }
            set { positionLerpSpeed = value; }
        }

        /// <summary>
        ///     Animation time value from previous frame.
        /// </summary>
        public float PrevAnimationTime { get; set; }

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

        #endregion INSPECTOR SETTINGS PROPERTIES

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
            UnsubscribeFromEvents();
            SubscribeToEvents();
            UnsubscribeFromPathEvents();
            SubscribeToPathEvents();
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
            // Animation does not always starts from time 0.
            PrevAnimationTime = AnimationTime;

            HandleStartAnimationOnEnterPlayMode();
            Invoke("HandleFireNodeReachedEventForStartingNode", AutoPlayDelay);
        }

        private void Update() {
            UpdateAnimationTime();

            HandleClampWrapMode();
            HandleLoopWrapMode();
            HandlePingPongWrapMode();

            HandleAnimateAnimatedGO();
            HandleShortcuts();
        }

        #endregion UNITY MESSAGES

        #region EVENT INVOCATORS

        private void OnAnimationEnded() {
            var handler = AnimationEnded;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnAnimationPaused() {
            var handler = AnimationPaused;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnAnimationResumed() {
            var handler = AnimationResumed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnAnimationStarted() {
            var handler = AnimationStarted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnAnimationStopped() {
            var handler = AnimationStopped;
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

        private void OnPathDataRefChanged() {
            var handler = PathDataRefChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnUndoRedoPerformed() {
            var handler = UndoRedoPerformed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion EVENT INVOCATORS

        #region EVENT HANDLERS

        private void Animator_PathDataRefChanged(object sender, EventArgs e) {
            UnsubscribeFromEvents();
            SubscribeToEvents();
            UnsubscribeFromPathEvents();
            SubscribeToPathEvents();
        }

        private void APAnimator_AnimationEnded(object sender, EventArgs e) {
        }

        private void PathData_NodeAdded(
            object sender,
            NodeAddedRemovedEventArgs e) {
        }

        private void PathData_NodePositionChanged(object sender, EventArgs e) {
            HandleUpdateAnimGOInSceneView();
            AssertNodesInSync();
        }

        private void PathData_NodeRemoved(
            object sender,
            NodeAddedRemovedEventArgs e) {
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

        private void PathData_TiltingCurveUpdated(object sender, EventArgs e) {
            HandleUpdateAnimGOInSceneView();
        }

        #endregion EVENT HANDLERS

        #region GET METHODS

        /// <summary>
        ///     Get positions of all nodes that have ease value assigned.
        /// </summary>
        /// <returns>Node positions.</returns>
        public Vector3[] GetGlobalEasedNodePositions() {
            var globalNodePositions = GetGlobalNodePositions();

            // Filter out unwanted nodes.
            var resultPositions = new List<Vector3>();
            for (var i = 0; i < globalNodePositions.Count; i++) {
                if (PathData.EaseToolState[i]) {
                    resultPositions.Add(globalNodePositions[i]);
                }
            }

            return resultPositions.ToArray();
        }

        /// <summary>
        ///     Returns global node positions.
        /// </summary>
        /// <param name="nodesNo">
        ///     Number of nodes to return, starting from index 0.
        /// </param>
        /// <returns>Global node positions.</returns>
        public List<Vector3> GetGlobalNodePositions(int nodesNo = -1) {
            var nodePositions = PathData.GetNodePositions(nodesNo);

            Utilities.ConvertToGlobalCoordinates(
                ref nodePositions,
                transform);

            return nodePositions;
        }

        /// <summary>
        ///     Returns global rotation path node positions.
        /// </summary>
        /// <returns></returns>
        public List<Vector3> GetGlobalRotationPathPositions() {
            var localRotPointPositions =
                pathData.GetRotationPointPositions();

            var globalRotPointPositions = new List<Vector3>();

            // For each local point..
            for (var i = 0; i < localRotPointPositions.Length; i++) {
                // Transform point to global.
                var globalPoint =
                    transform.TransformPoint(localRotPointPositions[i]);
                // Save point to list.
                globalRotPointPositions.Add(globalPoint);
            }

            return globalRotPointPositions;
        }

        public Vector3[] GetGlobalTiltedNodePositions() {
            var globalNodePositions = GetGlobalNodePositions();

            // Filter out unwanted nodes.
            var resultPositions = new List<Vector3>();
            for (var i = 0; i < globalNodePositions.Count; i++) {
                if (PathData.TiltingToolState[i]) {
                    resultPositions.Add(globalNodePositions[i]);
                }
            }

            return resultPositions.ToArray();
        }

        /// <summary>
        ///     Returns local forward point position for current animation time.
        /// </summary>
        /// <returns>Local forward point position.</returns>
        private Vector3 CalculateLocalForwardPointPosition() {
            // Timestamp offset of the forward point. Forward point timestamp.
            var forwardPointTimestamp = AnimationTime + ForwardPointOffset;
            var localPosition = PathData.GetVectorAtTime(forwardPointTimestamp);

            return localPosition;
        }

        /// <summary>
        ///     Returns global forward point position for current animation time.
        /// </summary>
        /// <returns>Global forward point position.</returns>
        private Vector3 GetGlobalForwardPoint() {
            var localForwardPoint = CalculateLocalForwardPointPosition();
            var globalForwardPoint =
                transform.TransformPoint(localForwardPoint);

            return globalForwardPoint;
        }

        /// <summary>
        ///     Returns timestamp of a node which timestamp is closest to and
        ///     bigger than the current animation time.
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
        ///     Returns timestamp of a node which timestamp is closest to and
        ///     bigger than the current animation time.
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

        #endregion GET METHODS

        #region EDIT METHODS

        /// <summary>
        ///     Pauses animation.
        /// </summary>
        public void Pause() {
            IsPlaying = false;

            OnAnimationPaused();
        }

        /// <summary>
        ///     Starts or continues animation.
        /// </summary>
        public void Play() {
            IsPlaying = true;
            HandleOnAnimationStarted();
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
            PathData.SetPathTangentsToLinear();
        }

        /// <summary>
        ///     Sets tangent mode to Smooth.
        /// </summary>
        public void SetTangentSmooth() {
            TangentMode = TangentMode.Smooth;
            PathData.SmoothAllPathNodeTangents();
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

        public void Stop() {
            IsPlaying = false;
            AnimationTime = 0;

            OnAnimationStopped();
        }

        /// <summary>
        ///     Unpauses animation.
        /// </summary>
        public void Unpause() {
            if (IsPlaying == false && DuringPlayback) {
                IsPlaying = true;
            }

            OnAnimationResumed();
        }

        /// <summary>
        ///     Updates animated game object position.
        /// </summary>
        private bool AnimateAnimatedGOPosition() {
            if (AnimatedGO == null) return false;

            // Local position that the animated object should be at in this
            // frame.
            var localPosAtTime =
                PathData.GetVectorAtTime(AnimationTime);

            // Convert position to global.
            var globalPosAtTime =
                transform.TransformPoint(localPosAtTime);

            // Helper variable.
            var prevGOPosition = animatedGO.position;

            // Update position.
            AnimatedGO.position = Vector3.Lerp(
                AnimatedGO.position,
                globalPosAtTime,
                PositionLerpSpeed);

            // Check if position changed.
            var positionChanged = !Utilities.V3Equal(
                prevGOPosition,
                animatedGO.position);

            // If position changed, return true.
            return positionChanged;
        }

        /// <summary>
        ///     Updates animated game object rotation.
        /// </summary>
        private bool AnimateAnimatedGORotation() {
            if (AnimatedGO == null) return false;

            // Helper variable.
            var prevGORotation = animatedGO.rotation;

            HandleTargetRotationModeInPlayMode();
            HandleCustomRotationModeInPlayMode();
            HandleForwardRotationModeInPlayMode();

            // Check if rotation changed.
            var rotationChanged = !Utilities.QuaternionsEqual(
                prevGORotation,
                animatedGO.rotation);

            return rotationChanged;
        }

        /// <summary>
        ///     Updates animated game object tilting.
        /// </summary>
        private bool AnimateAnimatedGOTilting() {
            if (AnimatedGO == null) return false;

            var prevTilting = AnimatedGO.rotation.eulerAngles.z;

            // Get current animated GO rotation.
            var eulerAngles = AnimatedGO.rotation.eulerAngles;
            // Get tilting value.
            var zRotation = PathData.GetTiltingValueAtTime(AnimationTime);
            // Update value on Z axis.
            eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, zRotation);
            // Update animated GO rotation.
            AnimatedGO.rotation = Quaternion.Euler(eulerAngles);

            // Check if tilting changed.
            var tiltingChanged = !Utilities.FloatsEqual(
                prevTilting,
                AnimatedGO.rotation.eulerAngles.z,
                GlobalConstants.FloatPrecision);

            return tiltingChanged;
        }

        /// <summary>
        ///     Assigns camera tagged "MainCamera" as animated game object.
        /// </summary>
        private void AssignMainCameraAsAnimatedGO() {
            if (AnimatedGO == null && Camera.main != null) {
                animatedGO = Camera.main.transform;
            }
            else {
                Debug.LogWarning("Camera with tag \"MainCamera\" was not found.");
            }
        }

        /// <summary>
        ///     Method responsible for updating animated GO position, rotation and
        ///     tilting in play mode during playback.
        /// </summary>
        private void HandleAnimateAnimatedGO() {
            if (!Application.isPlaying) return;
            if (!AnimGOUpdateEnabled) return;

            // Animate animated GO.
            var positionChanged = AnimateAnimatedGOPosition();
            var rotationChanged = AnimateAnimatedGORotation();
            var tiltingChanged = AnimateAnimatedGOTilting();

            HandleFireNodeReachedEvent();

            // Stop animating anim. GO if none of its properties changes.
            if (!positionChanged && !rotationChanged && !tiltingChanged) {
                // Stop updating animated game object.
                AnimGOUpdateEnabled = false;
            }
        }

        /// <summary>
        ///     Decided what to at the end of animation when Clamp mode is
        ///     selected.
        /// </summary>
        private void HandleClampWrapMode() {
            if (AnimationTime == 1
                && WrapMode == AnimatorWrapMode.Clamp) {

                IsPlaying = false;

                OnAnimationEnded();
            }
        }

        private void HandleCustomRotationModeInEditor() {
            if (RotationMode == RotationMode.Custom) {
                // Get rotation point position.
                var rotationPointPos =
                    PathData.GetRotationAtTime(AnimationTime);

                // Convert target position to global coordinates.
                var rotationPointGlobalPos =
                    transform.TransformPoint(rotationPointPos);

                // Update animated GO rotation.
                RotateObjectWithLookAt(rotationPointGlobalPos);
            }
        }

        private void HandleCustomRotationModeInPlayMode() {
            // Use rotation path.
            if (RotationMode == RotationMode.Custom) {
                RotateObjectWithRotationPath();
            }
        }

        private void HandleForwardRotationModeInEditor() {
            if (RotationMode == RotationMode.Forward) {
                var globalForwardPoint = GetGlobalForwardPoint();

                RotateObjectWithLookAt(globalForwardPoint);
            }
        }

        private void HandleForwardRotationModeInPlayMode() {
            // Look forward.
            if (RotationMode == RotationMode.Forward) {
                var globalForwardPoint = GetGlobalForwardPoint();

                RotateObjectWithSlerp(globalForwardPoint);
            }
        }

        /// <summary>
        ///     Decides what to do on animation end in Loop wrap mode.
        /// </summary>
        private void HandleLoopWrapMode() {
            if (AnimationTime == 1 && WrapMode == AnimatorWrapMode.Loop) {
                AnimationTime = 0;
            }
        }

        /// <summary>
        ///     Decides what to do on animation end in PingPong wrap mode.
        /// </summary>
        private void HandlePingPongWrapMode() {
            if (AnimationTime == 1
                && WrapMode == AnimatorWrapMode.PingPong) {

                Reverse = true;
            }

            if (AnimationTime == 0
                && WrapMode == AnimatorWrapMode.PingPong) {

                Reverse = false;
            }
        }

        /// <summary>
        ///     Action taken when Play/Pause shortcut is pressed. Use in play mode.
        /// </summary>
        private void HandlePlayPause() {
            if (!Application.isPlaying) return;

            if (IsPlaying) {
                Pause();
            }
            else if (!IsPlaying && DuringPlayback) {
                Unpause();
            }
            else if (!IsPlaying && AnimationTime >= 1) {
                AnimationTime = 0;
                Play();
            }
            // Disable play/pause while for animation start being invoked.
            else if (IsInvoking("Play")) {
                // Do nothing.
            }
            else {
                Play();
            }
        }

        /// <summary>
        ///     Decides if to start animation playback on enter play mode.
        /// </summary>
        private void HandleStartAnimationOnEnterPlayMode() {
            if (!PathDataAssetAssigned()) return;
            if (!Application.isPlaying) return;
            if (!AutoPlay) return;

            Invoke("Play", AutoPlayDelay);
        }

        private void HandleTargetRotationModeInEditor() {

            if (RotationMode == RotationMode.Target) {
                if (TargetGO == null) return;

                RotateObjectWithLookAt(TargetGO.position);
            }
        }

        private void HandleTargetRotationModeInPlayMode() {
            // Look at target.
            if (TargetGO != null
                && RotationMode == RotationMode.Target) {

                RotateObjectWithSlerp(TargetGO.position);
            }
        }

        /// <summary>
        ///     Update animated GO position, rotation and tilting accordingly to
        ///     current animation time.
        /// </summary>
        /// <remarks>Used to update animated GO with keys.</remarks>
        private void HandleUpdateAnimGOInSceneView() {
            if (!RequiredAssetsLoaded()) return;
            if (AnimatedGO == null) return;
            if (PathData == null) return;
            if (!enabled) return;

            UpdateAnimatedGOPosition();
            UpdateAnimatedGORotation();
            UpdateAnimatedGOTilting();
        }

        /// <summary>
        ///     Loads asset files from component folder, that are required for the
        ///     component to run.
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
        ///     Reset inspector options.
        /// </summary>
        private void ResetInspectorOptions() {
            TargetGO = null;
            NodeTool = NodeTool.None;
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
            GizmoCurveColor = SettingsAsset.DefaultObjectPathCurveColor;
            RotationCurveColor = SettingsAsset.DefaultRotationPathCurveColor;
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

        /// <summary>
        ///     Rotates animated GO using Slerp function.
        /// </summary>
        /// <param name="targetPosition">Point to look at.</param>
        private void RotateObjectWithSlerp(Vector3 targetPosition) {
            // Return when point to look at is at the same position as the
            // animated object.
            if (targetPosition == AnimatedGO.position) return;

            // Remember tilting.
            var tiltingEulerAngle = AnimatedGO.rotation.eulerAngles.z;
            // Calculate direction to target.
            var targetDirection = targetPosition - AnimatedGO.position;
            // Calculate rotation to target.
            var rotation = Quaternion.LookRotation(targetDirection);
            // Convert rotation to Euler.
            var rotationEuler = rotation.eulerAngles;
            // Restore original tilting.
            rotationEuler.z = tiltingEulerAngle;
            // Convert rotation from Euler to Quaternion.
            rotation = Quaternion.Euler(rotationEuler);
            // Calculate rotation speed.
            var speed = Time.deltaTime * RotationSlerpSpeed;

            // Lerp rotation.
            AnimatedGO.rotation = Quaternion.Slerp(
                AnimatedGO.rotation,
                rotation,
                speed);
        }

        /// <summary>
        ///     Updates animated game object position accordingly to current
        ///     animation time.
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
        ///     Updates animated game object rotation accordingly to current
        ///     animation time.
        /// </summary>
        private void UpdateAnimatedGORotation() {
            if (AnimatedGO == null) return;

            HandleForwardRotationModeInEditor();
            HandleCustomRotationModeInEditor();
            HandleTargetRotationModeInEditor();
        }

        private void UpdateAnimatedGOTilting() {
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

                // Stop animation.
                if (AnimationTime > 1) {
                    AnimationTime = 1;
                    IsPlaying = false;

                    OnAnimationEnded();
                }
            }
        }

        #endregion EDIT METHODS

        #region DO METHODS

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
        ///     Assert that object path and rotation path nodes are in sync.
        /// </summary>
        private void AssertNodesInSync() {
            if (RotationMode != RotationMode.Custom) return;

            Utilities.Assert(
                () =>
                    PathData.NodesNo
                    == PathData.RotationPathNodesNo,
                String.Format(
                    "Number of path nodes ({0}) and number of rotation path nodes ({1}) differ.",
                    PathData.NodesNo,
                    PathData.RotationPathNodesNo));
        }

        private void FireJumpedToNodeEvent() {
            // Create event args.
            var nodeIndex = PathData.GetNodeIndexAtTime(
                animationTime);
            var args = new NodeReachedEventArgs(nodeIndex, animationTime);

            OnJumpedToNode(args);
        }

        private void FireUndoRedoPerformedEvent() {
            OnUndoRedoPerformed();
        }

        /// <summary>
        ///     This method is responsible for firing <c>NodeReached</c> event. Use
        ///     in play mode only.
        /// </summary>
        private void HandleFireNodeReachedEvent() {
            // Get path timestamps.
            var nodeTimestamps = PathData.GetPathTimestamps();

            // For each timestamp..
            for (var i = 0; i < nodeTimestamps.Length; i++) {
                // If animation time "jumped over" a node..
                if (PrevAnimationTime < nodeTimestamps[i]
                    && AnimationTime >= nodeTimestamps[i]) {

                    // Fire event.
                    var args = new NodeReachedEventArgs(i, AnimationTime);
                    OnNodeReached(args);
                }
            }

            // Update helper field.
            PrevAnimationTime = AnimationTime;
        }

        /// <summary>
        ///     Used at animation start to fire <c>NodeReached</c> event for the
        ///     first node.
        /// </summary>
        private void HandleFireNodeReachedEventForStartingNode() {
            if (PathData == null) return;

            var currentNodeIndex = PathData.GetNodeIndexAtTime(AnimationTime);
            if (currentNodeIndex == -1) return;

            var args = new NodeReachedEventArgs(
                currentNodeIndex,
                AnimationTime);

            OnNodeReached(args);
        }

        /// <summary>
        ///     Handle firing <c>AnimationStarted</c> event.
        /// </summary>
        private void HandleOnAnimationStarted() {
            if (AnimationTime == 0) {
                OnAnimationStarted();
            }
        }

        /// <summary>
        ///     Subscribe to events.
        /// </summary>
        private void SubscribeToEvents() {
            AnimationEnded += APAnimator_AnimationEnded;
            PathDataRefChanged += Animator_PathDataRefChanged;
        }

        private void SubscribeToPathEvents() {
            if (pathData == null) return;

            PathData.RotationPointPositionChanged +=
                PathData_RotationPointPositionChanged;
            PathData.NodePositionChanged += PathData_NodePositionChanged;
            PathData.TiltingCurveUpdated += PathData_TiltingCurveUpdated;
            PathData.PathReset += PathData_PathReset;
            PathData.RotationPathReset += PathData_RotationPathReset;
            PathData.NodeAdded += PathData_NodeAdded;
            PathData.NodeRemoved += PathData_NodeRemoved;
        }

        /// <summary>
        ///     Unsubscribe from events.
        /// </summary>
        private void UnsubscribeFromEvents() {
            AnimationEnded -= APAnimator_AnimationEnded;
            PathDataRefChanged -= Animator_PathDataRefChanged;

            UnsubscribeFromPathEvents();
        }

        private void UnsubscribeFromPathEvents() {

            if (PathData == null) return;

            PathData.RotationPointPositionChanged -=
                PathData_RotationPointPositionChanged;
            PathData.NodePositionChanged -= PathData_NodePositionChanged;
            PathData.TiltingCurveUpdated -= PathData_TiltingCurveUpdated;
            PathData.PathReset -= PathData_PathReset;
            PathData.RotationPathReset -= PathData_RotationPathReset;
            PathData.NodeAdded -= PathData_NodeAdded;
            PathData.NodeRemoved -= PathData_NodeRemoved;
        }

        #endregion DO METHODS

        #region SHORTCUTS

        private void HandleJumpToBeginningShortcut() {
            // Jump to beginning.
            if (Input.GetKeyDown(
                SettingsAsset.JumpToPreviousNodeKey)
                && Input.GetKey(SettingsAsset.PlayModeModKey)) {

                AnimationTime = 0;

                FireJumpedToNodeEvent();
            }
        }

        private void HandleJumpToNextNodeShortcut() {
            // Jump to next node.
            if (Input.GetKeyDown(SettingsAsset.JumpToNextNodeKey)) {
                AnimationTime = GetNearestForwardNodeTimestamp();

                FireJumpedToNodeEvent();
            }
        }

        private void HandleJumpToPreviousNodeShortcut() {
            // Jump to previous node.
            if (Input.GetKeyDown(SettingsAsset.JumpToPreviousNodeKey)) {
                AnimationTime = GetNearestBackwardNodeTimestamp();

                FireJumpedToNodeEvent();
            }
        }

        private void HandleLongJumpBackwardShortcut() {
            // Long jump backward.
            if (Input.GetKeyDown(SettingsAsset.LongJumpBackwardKey)) {
                AnimationTime -= LongJumpValue;
            }
        }

        private void HandleLongJumpForwardShortcut() {
            // Long jump forward
            if (Input.GetKeyDown(SettingsAsset.LongJumpForwardKey)) {
                AnimationTime += LongJumpValue;
            }
        }

        private void HandlePlayPauseShortcut() {
            // Play/Pause.
            if (Input.GetKeyDown(SettingsAsset.PlayPauseKey)) {
                HandlePlayPause();
            }
        }

        /// <summary>
        ///     Method responsible for detecting all shortcuts pressed in play
        ///     mode.
        /// </summary>
        private void HandleShortcuts() {
            if (!EnableControlsInPlayMode) return;

            HandlePlayPauseShortcut();
            HandleLongJumpForwardShortcut();
            HandleLongJumpBackwardShortcut();
            HandleJumpToNextNodeShortcut();
            HandleJumpToPreviousNodeShortcut();
            HandleJumpToBeginningShortcut();
        }

        #endregion SHORTCUTS

        #region GIZMOS

        /// <summary>
        ///     Draw animated object path gizmo curve.
        /// </summary>
        private void DrawAnimationCurve() {
            if (pathData == null) return;
            if (!DrawObjectPath) return;

            // Get path points.
            var points = pathData.SampleAnimationPathForPoints(
                SettingsAsset.ObjectCurveSampling);

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
                SettingsAsset.GizmosSubfolder + "/" + SettingsAsset.ForwardPointIcon,
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
                SettingsAsset.GizmosSubfolder + "/" + SettingsAsset.TargetGizmoIcon,
                false);
        }

        /// <summary>
        ///     Handle drawing rotation point gizmo for current animation time.
        ///     It'll be drawn only when animation time is the same as a path node.
        /// </summary>
        private void HandleDrawingCurrentRotationPointGizmo() {
            if (RotationMode != RotationMode.Custom) return;
            if (!DrawRotationPathCurve) return;

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
                + "/" + SettingsAsset.CurrentRotationPointGizmoIcon,
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
            if (RotationMode != RotationMode.Custom) return;
            if (!DrawRotationPathCurve) return;

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
            if (RotationMode != RotationMode.Custom) return;
            if (!DrawRotationPathCurve) return;

            var globalRotPointPositions = GetGlobalRotationPathPositions();

            for (var i = 0; i < globalRotPointPositions.Count; i++) {
                //Draw rotation point gizmo.
                Gizmos.DrawIcon(
                    globalRotPointPositions[i],
                    SettingsAsset.GizmosSubfolder
                    + "/" + SettingsAsset.RotationPointGizmoIcon,
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

        #endregion GIZMOS
    }

}
