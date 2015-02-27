#region

using System;
using System.Collections.Generic;
using System.Linq;
using ATP.LoggingTools;
using UnityEngine;

#endregion

// ReSharper disable once CheckNamespace

namespace ATP.AnimationPathTools {

    // TODO Use public API instead of using path and curve fields directly.
    public class PathData : ScriptableObject {

        public event EventHandler RotationPointPositionChanged;

        #region EVENTS

        public event EventHandler NodeAdded;

        public event EventHandler NodePositionChanged;

        public event EventHandler NodeRemoved;

        public event EventHandler NodeTiltChanged;

        public event EventHandler NodeTimeChanged;

        #endregion EVENTS

        #region FIELDS

        [SerializeField]
        private AnimationPath animatedObjectPath;

        [SerializeField]
        private AnimationCurve easeCurve;

        [SerializeField]
        private AnimationPath rotationPath;

        [SerializeField]
        private AnimationCurve tiltingCurve;

        #endregion FIELDS

        #region PROPERTIES

        public int AnimationPathKeysNo {
            get { return animatedObjectPath.KeysNo; }
        }

        public int EaseCurveKeysNo {
            get { return EaseCurve.length; }
        }

        public int NodesNo {
            get { return animatedObjectPath[0].length; }
        }

        public int TiltingCurveKeysNo {
            get { return TiltingCurve.length; }
        }

        protected virtual float DefaultEaseCurveValue {
            get { return 0.05f; }
        }

        protected virtual float FloatPrecision {
            get { return 0.001f; }
        }

        protected virtual int PathLengthSamplingFrequency {
            get { return 20; }
        }

        private AnimationPath AnimatedObjectPath {
            get { return animatedObjectPath; }
            set { animatedObjectPath = value; }
        }

        private AnimationCurve EaseCurve {
            get { return easeCurve; }
            set { easeCurve = value; }
        }

        private AnimationPath RotationPath {
            get { return rotationPath; }
            set { rotationPath = value; }
        }

        private AnimationCurve TiltingCurve {
            get { return tiltingCurve; }
            set { tiltingCurve = value; }
        }

        #endregion PROPERTIES

        #region UNITY MESSAGES

        // ReSharper disable once UnusedMember.Local
        private void OnEnable() {
            // Return if fields are initialized.
            if (animatedObjectPath != null) return;

            InstantiateReferenceTypes();
            AssignDefaultValues();

            NodeAdded += PathData_NodeAdded;
            NodeRemoved += PathData_NodeRemoved;
            NodeTiltChanged += PathData_NodeTiltChanged;
            NodeTimeChanged += PathData_NodeTimeChanged;
            NodePositionChanged += PathData_NodePositionChanged;
        }

        #endregion UNITY MESSAGES

        #region EVENT INVOCATORS

        protected virtual void OnNodeAdded() {
            var handler = NodeAdded;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnNodeTiltChanged() {
            var handler = NodeTiltChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnNodeTimeChanged() {
            var handler = NodeTimeChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnRotationPointPositionChanged() {
            var handler = RotationPointPositionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion EVENT INVOCATORS

        #region EVENT HANDLERS

        private void PathData_NodeAdded(object sender, EventArgs e) {
            UpdateCurveWithAddedKeys(EaseCurve);
            UpdateCurveWithAddedKeys(TiltingCurve);
            UpdateRotationCurvesWithAddedKeys();
        }

        private void PathData_NodePositionChanged(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void PathData_NodeRemoved(object sender, EventArgs e) {
            UpdateCurveWithRemovedKeys(EaseCurve);
            UpdateCurveWithRemovedKeys(TiltingCurve);
            UpdateRotationPathWithRemovedKeys();
        }

        private void PathData_NodeTiltChanged(object sender, EventArgs e) {
        }

        private void PathData_NodeTimeChanged(object sender, EventArgs e) {
            UpdateCurveTimestamps(EaseCurve);
            UpdateCurveTimestamps(TiltingCurve);
            // TODO Rename to UpdateRotationPathTimestamps().
            UpdateRotationCurvesTimestamps();
        }

        #endregion EVENT HANDLERS

        #region METHODS

        //public void CreateAnimationPathNode(int index, Vector3 value) {
        //    animatedObjectPath.CreateNewNode(index, value);
        //}

        public void AddKeyToCurve(
            AnimationCurve curve,
            float timestamp) {
            var value = curve.Evaluate(timestamp);

            curve.AddKey(timestamp, value);
        }

        public void ChangeRotationAtTimestamp(
            float timestamp,
            Vector3 newPosition) {
            // Get node timestamps.
            var timestamps = RotationPath.GetTimestamps();
            // If matching timestamp in the path was found.
            var foundMatch = false;
            // For each timestamp..
            for (var i = 0; i < RotationPath.KeysNo; i++) {
                // Check if it is the timestamp to remove..
                if (Math.Abs(timestamps[i] - timestamp) < FloatPrecision) {
                    // Remove node.
                    RotationPath.RemoveNode(i);

                    foundMatch = true;
                }
            }

            // If timestamp was not found..
            if (!foundMatch) {
                Debug.Log(
                    "You're trying to change rotation for nonexistent " +
                    "node.");

                return;
            }

            // Create new node.
            RotationPath.CreateNewNode(timestamp, newPosition);
            // Smooth all nodes.
            RotationPath.SmoothAllNodes();

            OnRotationPointPositionChanged();
        }

        public void CreateAnimationPathNode(float timestamp, Vector3 position) {
            AnimatedObjectPath.CreateNewNode(timestamp, position);

            OnNodeAdded();
        }

        public void CreateNodeAtTime(float timestamp) {
            AnimatedObjectPath.AddNodeAtTime(timestamp);
            OnNodeAdded();
        }

        public void DistributeTimestamps() {
            // Calculate path curved length.
            var pathLength = AnimatedObjectPath.CalculatePathCurvedLength(
                PathLengthSamplingFrequency);

            // Calculate time for one meter of curve length.
            var timeForMeter = 1 / pathLength;

            // Helper variable.
            float prevTimestamp = 0;

            // For each node calculate and apply new timestamp.
            for (var i = 1; i < NodesNo - 1; i++) {
                // Calculate section curved length.
                var sectionLength = AnimatedObjectPath
                    .CalculateSectionCurvedLength(
                        i - 1,
                        i,
                        PathLengthSamplingFrequency);

                // Calculate time interval for the section.
                var sectionTimeInterval = sectionLength * timeForMeter;

                // Calculate new timestamp.
                var newTimestamp = prevTimestamp + sectionTimeInterval;

                // Update previous timestamp.
                prevTimestamp = newTimestamp;

                // NOTE When nodes on the scene overlap, it's possible that new
                // timestamp is > 0, which is invalid.
                if (newTimestamp > 1) break;

                if (i == 1)
                    Logger.LogString("OBJECT NEW TIMESTAMP: {0}", newTimestamp);

                // Update node timestamp.
                AnimatedObjectPath.ChangeNodeTimestamp(i, newTimestamp);
            }
        }

        public float[] GetEaseCurveValues() {
            var values = new float[EaseCurveKeysNo];

            for (var i = 0; i < values.Length; i++) {
                values[i] = EaseCurve.keys[i].value;
            }

            return values;
        }

        public float GetEaseTimestampAtIndex(int keyIndex) {
            return EaseCurve.keys[keyIndex].time;
        }

        public float GetEaseValueAtIndex(int index) {
            var timestamp = GetEaseTimestampAtIndex(index);
            var value = GetEaseValueAtTime(timestamp);

            return value;
        }

        public float GetEaseValueAtTime(float timestamp) {
            return EaseCurve.Evaluate(timestamp);
        }

        public float GetNodeEaseValue(int i) {
            return EaseCurve.keys[i].value;
        }

        public Vector3 GetNodePosition(int nodeIndex) {
            return AnimatedObjectPath.GetVectorAtKey(nodeIndex);
        }

        public Vector3[] GetNodePositions() {
            var result = new Vector3[NodesNo];

            for (var i = 0; i < NodesNo; i++) {
                // Get node 3d position.
                result[i] = AnimatedObjectPath.GetVectorAtKey(i);
            }

            return result;
        }

        public float GetNodeTiltValue(int nodeIndex) {
            return TiltingCurve.keys[nodeIndex].value;
        }

        public float GetNodeTimestamp(int nodeIndex) {
            return AnimatedObjectPath.GetTimeAtKey(nodeIndex);
        }

        public float[] GetPathTimestamps() {
            // Output array.
            var result = new float[NodesNo];

            // For each key..
            for (var i = 0; i < NodesNo; i++) {
                // Get key time.
                result[i] = AnimatedObjectPath.GetTimeAtKey(i);
            }

            return result;
        }

        public Vector3 GetRotationAtTime(float timestamp) {
            return RotationPath.GetVectorAtTime(timestamp);
        }

        public Vector3 GetRotationPointPosition(int nodeIndex) {
            return RotationPath.GetVectorAtKey(nodeIndex);
        }

        public Vector3[] GetRotationPointPositions() {
            // Get number of existing rotation points.
            var rotationPointsNo = RotationPath.KeysNo;
            // Result array.
            var rotationPointPositions = new Vector3[rotationPointsNo];

            // For each rotation point..
            for (var i = 0; i < rotationPointsNo; i++) {
                // Get rotation point local position.
                rotationPointPositions[i] = GetRotationPointPosition(i);
            }

            return rotationPointPositions;
        }

        public Vector3 GetRotationValueAtTime(float timestamp) {
            return RotationPath.GetVectorAtTime(timestamp);
        }

        public float[] GetTiltingCurveValues() {
            var values = new float[TiltingCurveKeysNo];

            for (var i = 0; i < values.Length; i++) {
                values[i] = TiltingCurve.keys[i].value;
            }

            return values;
        }

        public float GetTiltingValueAtTime(float timestamp) {
            return TiltingCurve.Evaluate(timestamp);
        }

        public Vector3 GetVectorAtTime(float timestamp) {
            return AnimatedObjectPath.GetVectorAtTime(timestamp);
        }

        public void MoveNodeToPosition(
            int nodeIndex,
            Vector3 position,
            AnimationPathBuilder animationPathBuilder) {
            AnimatedObjectPath.MovePointToPosition(nodeIndex, position);
            OnNodePositionChanged();
        }

        public void OffsetNodePositions(Vector3 moveDelta) {
            // For each node..
            for (var i = 0; i < NodesNo; i++) {
                // Old node position.
                var oldPosition = GetNodePosition(i);
                // New node position.
                var newPosition = oldPosition + moveDelta;
                // Update node positions.
                AnimatedObjectPath.MovePointToPosition(i, newPosition);

                OnNodePositionChanged();
            }
        }

        public virtual void OnNodePositionChanged() {
            var handler = NodePositionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public virtual void OnNodeRemoved() {
            var handler = NodeRemoved;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void RemoveAllNodes() {
            var nodesNo = NodesNo;
            for (var i = 0; i < nodesNo; i++) {
                // NOTE After each removal, next node gets index 0.
                RemoveNode(0);
            }
        }

        public void RemoveNode(int nodeIndex) {
            AnimatedObjectPath.RemoveNode(nodeIndex);
            OnNodeRemoved();
        }

        public void Reset() {
            InstantiateReferenceTypes();
            AssignDefaultValues();
        }

        public List<Vector3> SampleAnimationPathForPoints(
            int samplingFrequency) {

            return animatedObjectPath.SamplePathForPoints(samplingFrequency);
        }

        public List<Vector3> SampleRotationPathForPoints(
            int samplingFrequency) {

            return RotationPath.SamplePathForPoints(samplingFrequency);
        }

        public void SetNodesLinear() {
            for (var i = 0; i < 3; i++) {
                Utilities.SetCurveLinear(AnimatedObjectPath[i]);
            }
        }

        public void SetNodeTangents(int index, Vector3 inOutTangent) {
            AnimatedObjectPath.ChangePointTangents(index, inOutTangent);
        }

        public void SetWrapMode(WrapMode wrapMode) {
            AnimatedObjectPath.SetWrapMode(wrapMode);
        }

        /// <summary>
        ///     Smooth tangents in all nodes in all animation curves.
        /// </summary>
        /// <param name="weight">Weight to be applied to the tangents.</param>
        public void SmoothAllNodeTangents(float weight = 0) {
            // For each key..
            for (var j = 0; j < NodesNo; j++) {
                // Smooth in and out tangents.
                AnimatedObjectPath.SmoothPointTangents(j);
            }
        }

        public void SmoothCurve(AnimationCurve curve) {
            for (var i = 0; i < curve.length; i++) {
                curve.SmoothTangents(i, 0);
            }
        }

        public void SmoothSingleNodeTangents(int nodeIndex) {
            AnimatedObjectPath.SmoothPointTangents(nodeIndex);
        }

        public void UpdateCurveTimestamps(AnimationCurve curve) {
            // Get node timestamps.
            var pathNodeTimestamps = GetPathTimestamps();
            // For each key in easeCurve..
            for (var i = 1; i < curve.length - 1; i++) {
                // If resp. node timestamp is different from easeCurve
                // timestamp..
                if (Math.Abs(pathNodeTimestamps[i] - curve.keys[i].value)
                    > FloatPrecision) {
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
        public void UpdateCurveWithAddedKeys(AnimationCurve curve) {
            var nodeTimestamps = GetPathTimestamps();
            // Get curve value.
            var curveTimestamps = new float[curve.length];
            for (var i = 0; i < curve.length; i++) {
                curveTimestamps[i] = curve.keys[i].time;
            }

            // For each path timestamp..
            foreach (var nodeTimestamp in nodeTimestamps) {
                var valueExists = curveTimestamps.Any(
                    t =>
                        Math.Abs(nodeTimestamp - t) < FloatPrecision);

                // Add missing key.
                if (valueExists) continue;

                AddKeyToCurve(curve, nodeTimestamp);
                SmoothCurve(curve);

                break;
            }
        }

        public void UpdateCurveWithRemovedKeys(AnimationCurve curve) {
            // AnimationPathBuilder node timestamps.
            var nodeTimestamps = GetPathTimestamps();
            // Get values from curve.
            var curveTimestamps = new float[curve.length];
            for (var i = 0; i < curveTimestamps.Length; i++) {
                curveTimestamps[i] = curve.keys[i].time;
            }

            // For each curve timestamp..
            for (var i = 0; i < curveTimestamps.Length; i++) {
                // Check if key at this timestamp exists..
                var keyExists = nodeTimestamps.Any(
                    t =>
                        Math.Abs(curveTimestamps[i] - t) < FloatPrecision);

                if (keyExists) continue;

                curve.RemoveKey(i);
                SmoothCurve(curve);

                break;
            }
        }

        public void UpdateEaseValue(int keyIndex, float newValue) {
            // Copy keyframe.
            var keyframeCopy = EaseCurve.keys[keyIndex];
            // Update keyframe value.
            keyframeCopy.value = newValue;

            // Replace old key with updated one.
            EaseCurve.RemoveKey(keyIndex);
            EaseCurve.AddKey(keyframeCopy);

            SmoothCurve(EaseCurve);
        }

        public void UpdateEaseValues(float delta) {
            for (var i = 0; i < EaseCurve.length; i++) {
                // Copy key.
                var keyCopy = EaseCurve[i];
                // Update key value.
                keyCopy.value += delta;

                // Remove old key.
                EaseCurve.RemoveKey(i);

                // Add key.
                EaseCurve.AddKey(keyCopy);

                // Smooth all tangents.
                SmoothCurve(EaseCurve);
            }
        }

        public void UpdateNodeTilting(int keyIndex, float newValue) {
            // Copy keyframe.
            var keyframeCopy = TiltingCurve.keys[keyIndex];
            // Update keyframe value.
            keyframeCopy.value = newValue;

            // Replace old key with updated one.
            TiltingCurve.RemoveKey(keyIndex);
            TiltingCurve.AddKey(keyframeCopy);
            SmoothCurve(TiltingCurve);
            EaseCurveExtremeNodes(TiltingCurve);

            OnNodeTiltChanged();
        }

        public void UpdateRotationCurvesTimestamps() {
            // Get node timestamps.
            var nodeTimestamps = GetPathTimestamps();
            // Get rotation point timestamps.
            var rotationCurvesTimestamps =
                RotationPath.GetTimestamps();

            // For each node in rotationPath..
            for (var i = 1; i < RotationPath.KeysNo - 1; i++) {
                // If resp. path node timestamp is different from rotation
                // point timestamp..
                if (Math.Abs(nodeTimestamps[i] - rotationCurvesTimestamps[i])
                    > FloatPrecision) {

                    if (i == 1)
                        Logger.LogString(
                            "Rotation NEW TIMESTAMP: {0}",
                            nodeTimestamps[i]);

                    // Update rotation point timestamp.
                    RotationPath.ChangeNodeTimestamp(
                        i,
                        nodeTimestamps[i]);
                }
            }
        }

        public void UpdateRotationCurvesWithAddedKeys() {
            // Get animatedObjectPath timestamps.
            var pathTimestamps = GetPathTimestamps();

            // Get rotationPath values.
            var rotationPathTimestamps = RotationPath.GetTimestamps();

            // Get number of nodes in the rotation path.
            var rotationPathNodeNo = rotationPathTimestamps.Length;

            // For each timestamp in animatedObjectPath ..
            for (var i = 0; i < pathTimestamps.Length; i++) {
                var keyExists = false;
                // For each node in rotationPath..
                for (var j = 0; j < rotationPathNodeNo; j++) {
                    // If both timestamps are the same..
                    if (Math.Abs(rotationPathTimestamps[j] - pathTimestamps[i])
                        < FloatPrecision) {

                        keyExists = true;

                        break;
                    }
                }

                // If both timestamps are different..
                if (!keyExists) {
                    // Get timestamp of the added key.
                    var addedKeyTimestamp = GetNodeTimestamp(i);
                    // Calculate value for new rotation point.
                    var defaultRotation =
                        RotationPath.GetVectorAtTime(addedKeyTimestamp);

                    // Create new rotation point.
                    RotationPath.CreateNewNode(
                        pathTimestamps[i],
                        defaultRotation);
                }
            }
        }

        public void UpdateRotationPathWithRemovedKeys() {
            // AnimationPathBuilder node timestamps.
            var pathTimestamps = GetPathTimestamps();
            // Get values from rotationPath.
            var rotationCurvesTimestamps = RotationPath.GetTimestamps();

            // For each timestamp in rotationPath..
            for (var i = 0; i < rotationCurvesTimestamps.Length; i++) {
                // Check if same timestamp exist in rotationPath.
                var keyExists = pathTimestamps.Any(
                    nodeTimestamp =>
                        Math.Abs(rotationCurvesTimestamps[i] - nodeTimestamp)
                        < FloatPrecision);

                // If key exists check next timestamp.
                if (keyExists) continue;

                // Remove node from rotationPath.
                RotationPath.RemoveNode(i);

                break;
            }
        }

        private void AssignDefaultValues() {
            InitializeAnimatedObjectPath();
            InitializeRotationPath();
            InitializeEaseCurve();
            InitializeTiltingCurve();
        }

        private void EaseCurveExtremeNodes(AnimationCurve curve) {
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

        private void InitializeAnimatedObjectPath() {
            var firstNodePos = new Vector3(0, 0, 0);
            AnimatedObjectPath.CreateNewNode(0, firstNodePos);

            var lastNodePos = new Vector3(1, 0, 1);
            AnimatedObjectPath.CreateNewNode(1, lastNodePos);
        }

        private void InitializeEaseCurve() {
            EaseCurve.AddKey(0, DefaultEaseCurveValue);
            EaseCurve.AddKey(1, DefaultEaseCurveValue);
        }

        private void InitializeRotationPath() {
            var firstNodePos = new Vector3(0, 0, 0);
            RotationPath.CreateNewNode(0, firstNodePos);

            var lastNodePos = new Vector3(1, 0, 1);
            RotationPath.CreateNewNode(1, lastNodePos);
        }

        private void InitializeTiltingCurve() {
            TiltingCurve.AddKey(0, 0);
            TiltingCurve.AddKey(1, 0);
        }

        private void InstantiateAnimationPathCurves(AnimationPath animationPath) {
            for (var i = 0; i < 3; i++) {
                animationPath[i] = new AnimationCurve();
            }
        }

        private void InstantiateReferenceTypes() {
            AnimatedObjectPath = new AnimationPath();
            InstantiateAnimationPathCurves(animatedObjectPath);
            RotationPath = new AnimationPath();
            InstantiateAnimationPathCurves(rotationPath);
            EaseCurve = new AnimationCurve();
            TiltingCurve = new AnimationCurve();
        }

        #endregion METHODS
    }

}