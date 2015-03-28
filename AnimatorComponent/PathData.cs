using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ATP.AnimationPathTools.AnimatorComponent {

    public sealed class PathData : ScriptableObject,
        ISerializationCallbackReceiver {
        #region EVENTS

        public event EventHandler<NodeAddedRemovedEventArgs> NodeAdded;

        public event EventHandler NodePositionChanged;

        public event EventHandler<NodeAddedRemovedEventArgs> NodeRemoved;

        public event EventHandler NodeTiltChanged;

        public event EventHandler NodeTimeChanged;

        public event EventHandler PathReset;

        public event EventHandler RotationPathReset;

        public event EventHandler RotationPointPositionChanged;

        #endregion EVENTS

        #region CONST

        private const float DefaultEaseCurveValue = 0.05f;
        private const float DefaultTiltingCurveValue = 0.001f;
        private const float DefaultSmoothWeight = 0;

        #endregion

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

        public int EaseCurveKeysNo {
            get { return EaseCurve.length; }
        }

        public int NodesNo {
            get { return animatedObjectPath[0].length; }
        }

        public int RotationPathNodesNo {
            get { return RotationPath.KeysNo; }
        }

        public int TiltingCurveKeysNo {
            get { return TiltingCurve.length; }
        }

        private AnimationPath AnimatedObjectPath {
            get { return animatedObjectPath; }
        }

        private AnimationCurve EaseCurve {
            get { return easeCurve; }
        }

        private AnimationPath RotationPath {
            get { return rotationPath; }
        }

        private AnimationCurve TiltingCurve {
            get { return tiltingCurve; }
        }

        /// <summary>
        /// List with with indexes of nodes that have ease value assigned.
        /// </summary>
        public List<bool> NodeEaseEnabled { get; set; }

        /// <summary>
        /// List with with indexes of nodes that have tilting value assigned.
        /// </summary>
        public List<bool> NodeTiltingEnabled { get; set; }

        #endregion PROPERTIES

        #region UNITY MESSAGES

        public void OnAfterDeserialize() {
            SubscribeToEvents();
        }

        public void OnBeforeSerialize() {
        }

        private void OnEnable() {
            HandleInstantiateReferenceTypes();

            SubscribeToEvents();
        }

        #endregion UNITY MESSAGES

        #region EVENT INVOCATORS

        private void OnNodeAdded(int nodeIndex) {
            var args = new NodeAddedRemovedEventArgs(nodeIndex);

            var handler = NodeAdded;
            if (handler != null) handler(this, args);
        }

        private void OnNodePositionChanged() {
            var handler = NodePositionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnNodeRemoved(int nodeIndex) {
            var args = new NodeAddedRemovedEventArgs(nodeIndex);

            var handler = NodeRemoved;
            if (handler != null) handler(this, args);
        }

        private void OnNodeTiltChanged() {
            var handler = NodeTiltChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnNodeTimeChanged() {
            var handler = NodeTimeChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnPathReset() {
            var handler = PathReset;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnRotationPathReset() {
            var handler = RotationPathReset;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnRotationPointPositionChanged() {
            var handler = RotationPointPositionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion EVENT INVOCATORS

        #region EVENT HANDLERS

        private void PathData_NodeAdded(object sender, NodeAddedRemovedEventArgs e) {
            //UpdateCurveWithAddedKeys(EaseCurve);
            //UpdateCurveWithAddedKeys(TiltingCurve);
            //AddEntryToNodeEaseEnabledList(e.NodeIndex);
            NodeEaseEnabled.Insert(e.NodeIndex, false);
            
            UpdateRotationPathWithAddedKeys();
        }

        /// <summary>
        /// Add new entry <c>NodeTiltingEnabled</c> list.
        /// </summary>
        //private void AddEntryToNodeEaseEnabledList(int nodeIndex) {

        //}

        private void PathData_NodePositionChanged(object sender, EventArgs e) {
        }

        private void PathData_NodeRemoved(object sender, NodeAddedRemovedEventArgs e) {
            //UpdateCurveWithRemovedKeys(EaseCurve);
            //UpdateCurveWithRemovedKeys(TiltingCurve);
            NodeEaseEnabled.RemoveAt(e.NodeIndex);
            UpdateRotationPathWithRemovedKeys();
        }

        private void PathData_NodeTiltChanged(object sender, EventArgs e) {
        }

        private void PathData_NodeTimeChanged(object sender, EventArgs e) {
            //UpdateCurveTimestamps(EaseCurve);
            UpdateCurveEnabledTimestamps(EaseCurve, GetEasedNodeTimestamps);
            UpdateCurveTimestamps(TiltingCurve);
            UpdateRotationPathTimestamps();
        }

        #endregion EVENT HANDLERS

        #region INIT METHODS

        private void AssignDefaultValues() {
            InitializeAnimatedObjectPath();
            InitializeRotationPath();
            InitializeEaseCurve();
            InitializeTiltingCurve();
        }

        private void HandleInstantiateReferenceTypes() {
            if (animatedObjectPath == null) {
                animatedObjectPath = new AnimationPath();
                InitializeAnimatedObjectPath();
            }
            if (rotationPath == null) {
                rotationPath = new AnimationPath();
                InitializeRotationPath();
            }
            if (easeCurve == null) {
                easeCurve = new AnimationCurve();
                InitializeEaseCurve();
            }
            if (tiltingCurve == null) {
                tiltingCurve = new AnimationCurve();
                InitializeTiltingCurve();
            }
            if (NodeEaseEnabled == null) {
                NodeEaseEnabled = new List<bool>() {true, true};
            }
            if (NodeTiltingEnabled== null) {
                NodeTiltingEnabled = new List<bool>() {true, true};
            }
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

        private void SubscribeToEvents() {
            NodeAdded += PathData_NodeAdded;
            NodeRemoved += PathData_NodeRemoved;
            NodeTiltChanged += PathData_NodeTiltChanged;
            NodeTimeChanged += PathData_NodeTimeChanged;
            NodePositionChanged += PathData_NodePositionChanged;
        }

        #endregion METHODS

        #region EDIT METHODS

        /// <summary>
        /// Add a new key to ease curve. Value will be read from existing curve.
        /// </summary>
        /// <param name="time"></param>
        public void AddKeyToEaseCurve(float time) {
            var valueAtTime = EaseCurve.Evaluate(time);
            EaseCurve.AddKey(time, valueAtTime);
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
                if (Utilities.FloatsEqual(
                    timestamps[i],
                    timestamp,
                    GlobalConstants.FloatPrecision)) {

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
            RotationPath.SmoothAllNodes(DefaultSmoothWeight);

            OnRotationPointPositionChanged();
        }

        public void CreateNewNode(float timestamp, Vector3 position) {
            AnimatedObjectPath.CreateNewNode(timestamp, position);

            var nodeIndex = AnimatedObjectPath.GetNodeIndexAtTime(timestamp);
            OnNodeAdded(nodeIndex);
        }

        public void CreateNodeAtTime(float timestamp) {
            AnimatedObjectPath.AddNodeAtTime(timestamp);

            var nodeIndex = AnimatedObjectPath.GetNodeIndexAtTime(timestamp);
            OnNodeAdded(nodeIndex);
        }

        public void DistributeTimestamps() {
            // Calculate path curved length.
            var pathLength = AnimatedObjectPath.CalculatePathLinearLength();

            // Calculate time for one meter of curve length.
            var timeForMeter = 1 / pathLength;

            // Helper variable.
            float prevTimestamp = 0;

            // For each node calculate and apply new timestamp.
            for (var i = 1; i < NodesNo - 1; i++) {
                // Calculate section curved length.
                var sectionLength = AnimatedObjectPath
                    .CalculateSectionLinearLength(i - 1, i);

                // Calculate time interval for the section.
                var sectionTimeInterval = sectionLength * timeForMeter;

                // Calculate new timestamp.
                var newTimestamp = prevTimestamp + sectionTimeInterval;

                // Update previous timestamp.
                prevTimestamp = newTimestamp;

                // NOTE When nodes on the scene overlap, it's possible that new
                // timestamp is > 0, which is invalid.
                if (newTimestamp > 1) break;

                // Update node timestamp.
                AnimatedObjectPath.ChangeNodeTimestamp(i, newTimestamp);
            }

            OnNodeTimeChanged();
        }

        public void MoveNodeToPosition(
            int nodeIndex,
            Vector3 position) {

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

        public void OffsetRotationPathPosition(Vector3 moveDelta) {
            // For each node..
            for (var i = 0; i < NodesNo; i++) {
                // Old node position.
                var oldPosition = GetRotationPointPosition(i);
                // New node position.
                var newPosition = oldPosition + moveDelta;
                // Update node positions.
                RotationPath.MovePointToPosition(i, newPosition);
            }
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

            OnNodeRemoved(nodeIndex);
        }

        public void ResetEaseCurve() {
            easeCurve = new AnimationCurve();
            UpdateCurveWithAddedKeys(EaseCurve);
            // Set default value for each key.
            UpdateEaseCurveValues(DefaultEaseCurveValue);
        }

        public void ResetPath() {
            ForceInstantiatePathsAndCurves();
            AssignDefaultValues();

            OnPathReset();
        }

        public void ResetRotationPath() {
            rotationPath = new AnimationPath();

            UpdateRotationPathWithAddedKeys();
            ResetRotationPathValues();

            OnRotationPathReset();
        }

        public void ResetTiltingCurve() {
            tiltingCurve = new AnimationCurve();
            UpdateCurveWithAddedKeys(TiltingCurve);
            // Set default value for each key.
            UpdateTiltingCurveValues(DefaultTiltingCurveValue);
        }

        public void SetLinearAnimObjPathTangents() {
            for (var i = 0; i < 3; i++) {
                Utilities.SetCurveLinear(AnimatedObjectPath[i]);
            }
        }

        public void SetNodeTangents(int index, float inTangent, float outTangent) {
            AnimatedObjectPath.ChangeNodeTangents(index, inTangent, outTangent);
        }

        /// <summary>
        ///     Smooth tangents in all nodes in all animation curves.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="weight">Weight to be applied to the tangents.</param>
        public void SmoothAllNodeTangents(
            AnimationPath path,
            float weight = 0) {

            // For each key..
            for (var j = 0; j < NodesNo; j++) {
                // Smooth in and out tangents.
                path.SmoothNodeInOutTangents(j, DefaultSmoothWeight);
            }
        }

        public void SmoothAnimObjPathTangents() {
            SmoothAllNodeTangents(AnimatedObjectPath);
        }

        public void SmoothRotationPathTangents() {
            SmoothAllNodeTangents(RotationPath);
        }

        public void UpdateEaseCurveValues(float delta) {
            UpdateCurveValues(easeCurve, delta);
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

        public void UpdateTiltingCurveValues(float delta) {
            UpdateCurveValues(TiltingCurve, delta);

            OnNodeTiltChanged();
        }

        public void UpdateTiltingValue(int keyIndex, float newValue) {
            // Copy keyframe.
            var keyframeCopy = TiltingCurve.keys[keyIndex];
            // Update keyframe value.
            keyframeCopy.value = newValue;

            // Remove old key.
            TiltingCurve.RemoveKey(keyIndex);
            // Add updated key.
            TiltingCurve.AddKey(keyframeCopy);

            SmoothCurve(TiltingCurve);
            EaseCurveExtremeNodes(TiltingCurve);

            OnNodeTiltChanged();
        }

        private void AddKeyToCurve(
            AnimationCurve curve,
            float timestamp) {

            var value = curve.Evaluate(timestamp);
            curve.AddKey(timestamp, value);
        }

        /// <summary>
        ///     Create rotation point for given path node.
        /// </summary>
        /// <param name="nodeTimestamp"></param>
        private void CreateRotationPoint(float nodeTimestamp) {
            // Calculate value for new rotation point.
            var rotationValue =
                RotationPath.GetVectorAtTime(nodeTimestamp);

            // Create new rotation point.
            RotationPath.CreateNewNode(
                nodeTimestamp,
                rotationValue);
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

        private void ForceInstantiatePathsAndCurves() {
            animatedObjectPath = new AnimationPath();
            rotationPath = new AnimationPath();
            easeCurve = new AnimationCurve();
            tiltingCurve = new AnimationCurve();
        }

        /// <summary>
        ///     Set RotationPath node positions to the same as in AnimatedObjectPath.
        /// </summary>
        private void ResetRotationPathValues() {
            var animPathNodePositions = GetNodePositions();

            for (var i = 0; i < animPathNodePositions.Length; i++) {
                RotationPath.MovePointToPosition(i, animPathNodePositions[i]);
            }
        }

        private void SmoothCurve(AnimationCurve curve) {
            for (var i = 0; i < curve.length; i++) {
                curve.SmoothTangents(i, 0);
            }
        }

        private void SmoothSingleNodeTangents(int nodeIndex) {
            AnimatedObjectPath.SmoothNodeInOutTangents(
                nodeIndex,
                DefaultSmoothWeight);
        }

        private void UpdateCurveTimestamps(AnimationCurve curve) {
            // Get path timestamps.
            var pathNodeTimestamps = GetPathTimestamps();
            // For each key in easeCurve..
            for (var i = 1; i < curve.length - 1; i++) {
                // If resp. node timestamp is different from easeCurve
                // timestamp..
                if (!Utilities.FloatsEqual(
                    pathNodeTimestamps[i],
                    curve.keys[i].value,
                    GlobalConstants.FloatPrecision)) {

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
        /// Updates curve timestamps but only for nodes that have tool enabled.
        /// </summary>
        /// <param name="curve"></param>
        private void UpdateCurveEnabledTimestamps(
            AnimationCurve curve,
            Func<List<float>> nodeTimestampsCallback) {

            // Get path timestamps.
            var pathNodeTimestamps = nodeTimestampsCallback();
            // For each key in easeCurve..
            for (var i = 1; i < curve.length - 1; i++) {
                // If resp. node timestamp is different from easeCurve
                // timestamp..
                if (!Utilities.FloatsEqual(
                    pathNodeTimestamps[i],
                    curve.keys[i].value,
                    GlobalConstants.FloatPrecision)) {

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


        public List<float> GetEasedNodeTimestamps() {
            var pathTimestamps = GetPathTimestamps();
            var resultTimestamps = new List<float>();

            for (int i = 0; i < pathTimestamps.Length; i++) {
                if (NodeEaseEnabled[i]) {
                    resultTimestamps.Add(pathTimestamps[i]);
                }
            }

            return resultTimestamps;
        } 

        private void UpdateCurveValues(AnimationCurve curve, float delta) {
            for (var i = 0; i < curve.length; i++) {
                // Copy key.
                var keyCopy = curve[i];
                // Update key value.
                keyCopy.value += delta;
                // Remove old key.
                curve.RemoveKey(i);
                // Add key.
                curve.AddKey(keyCopy);
                // Smooth all tangents.
                SmoothCurve(curve);
            }
        }

        /// <summary>
        ///     Update AnimationCurve with keys added to the path.
        /// </summary>
        /// <param name="curve"></param>
        private void UpdateCurveWithAddedKeys(AnimationCurve curve) {
            var nodeTimestamps = GetPathTimestamps();
            // Get curve value.
            var curveTimestamps = new float[curve.length];
            for (var i = 0; i < curve.length; i++) {
                curveTimestamps[i] = curve.keys[i].time;
            }

            // For each path timestamp..
            foreach (var nodeTimestamp in nodeTimestamps) {
                var valueExists = curveTimestamps.Any(
                    t => Utilities.FloatsEqual(
                        nodeTimestamp,
                        t,
                        GlobalConstants.FloatPrecision));

                // Add missing key.
                if (valueExists) continue;

                AddKeyToCurve(curve, nodeTimestamp);
                SmoothCurve(curve);
            }
        }

        private void UpdateCurveWithRemovedKeys(AnimationCurve curve) {
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
                    t => Utilities.FloatsEqual(
                        curveTimestamps[i],
                        t,
                        GlobalConstants.FloatPrecision));

                if (keyExists) continue;

                curve.RemoveKey(i);
                SmoothCurve(curve);

                break;
            }
        }

        private void UpdateRotationPathTimestamps() {
            // Get node timestamps.
            var nodeTimestamps = GetPathTimestamps();
            // Get rotation point timestamps.
            var rotationCurvesTimestamps =
                RotationPath.GetTimestamps();

            // For each node in rotationPath..
            for (var i = 1; i < RotationPath.KeysNo - 1; i++) {
                // If resp. path node timestamp is different from rotation
                // point timestamp..
                if (!Utilities.FloatsEqual(
                    nodeTimestamps[i],
                    rotationCurvesTimestamps[i],
                    GlobalConstants.FloatPrecision)) {

                    // Update rotation point timestamp.
                    RotationPath.ChangeNodeTimestamp(
                        i,
                        nodeTimestamps[i]);
                }
            }
        }

        private void UpdateRotationPathWithAddedKeys() {
            // Get path timestamps.
            var pathTimestamps = GetPathTimestamps();
            // Get rotation path timestamps.
            var rotationPathTimestamps = RotationPath.GetTimestamps();

            // For each timestamp in the path..
            foreach (var pathTimestamp in pathTimestamps) {
                // Check if same timestamp exists in rotation path.
                var keyExists = rotationPathTimestamps.Any(
                    t => Utilities.FloatsEqual(
                        t,
                        pathTimestamp,
                        GlobalConstants.FloatPrecision));

                // If not..
                if (!keyExists) {
                    CreateRotationPoint(pathTimestamp);
                }
            }
        }

        private void UpdateRotationPathWithRemovedKeys() {
            // AnimationPathBuilder node timestamps.
            var pathTimestamps = GetPathTimestamps();
            // Get values from rotationPath.
            var rotationCurvesTimestamps = RotationPath.GetTimestamps();

            // For each timestamp in rotationPath..
            for (var i = 0; i < rotationCurvesTimestamps.Length; i++) {
                // Check if same timestamp exist in rotationPath.
                var keyExists = pathTimestamps.Any(
                    nodeTimestamp => Utilities.FloatsEqual(
                        rotationCurvesTimestamps[i],
                        nodeTimestamp,
                        GlobalConstants.FloatPrecision));

                // If key exists check next timestamp.
                if (keyExists) continue;

                // Remove node from rotationPath.
                RotationPath.RemoveNode(i);

                break;
            }
        }

        #endregion

        #region GET METHODS

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

        public Vector3 GetGlobalNodePosition(
            int nodeIndex,
            Transform transform) {

            var localNodePosition = GetNodePosition(nodeIndex);
            var globalNodePosition = transform.TransformPoint(localNodePosition);

            return globalNodePosition;
        }

        public float GetNodeEaseValue(int i) {
            return EaseCurve.keys[i].value;
        }

        public Vector3 GetNodePosition(int nodeIndex) {
            return AnimatedObjectPath.GetVectorAtKey(nodeIndex);
        }

        /// <summary>
        ///     Got positions of all path nodes.
        /// </summary>
        /// <param name="nodesNo">Number of nodes to return. If not specified, all nodes will be returned.</param>
        /// <returns></returns>
        public Vector3[] GetNodePositions(int nodesNo = -1) {
            // Specify number of nodes to return.
            var returnNodesNo = nodesNo > -1 ? nodesNo : NodesNo;
            // Create empty result array.
            var result = new Vector3[returnNodesNo];

            // Fill in array with node positions.
            for (var i = 0; i < returnNodesNo; i++) {
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

        public float GetPathLinearLength() {
            return AnimatedObjectPath.CalculatePathLinearLength();
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

        public float[] GetTiltingCurveValues() {
            var values = new float[TiltingCurveKeysNo];

            for (var i = 0; i < values.Length; i++) {
                values[i] = TiltingCurve.keys[i].value;
            }

            return values;
        }

        public float GetTiltingValueAtIndex(int keyIndex) {
            var timestamp = GetEaseTimestampAtIndex(keyIndex);
            var value = GetTiltingValueAtTime(timestamp);

            return value;
        }

        public float GetTiltingValueAtTime(float timestamp) {
            return TiltingCurve.Evaluate(timestamp);
        }

        public Vector3 GetVectorAtTime(float timestamp) {
            return AnimatedObjectPath.GetVectorAtTime(timestamp);
        }

        public List<Vector3> SampleAnimationPathForPoints(
            int samplingFrequency) {

            return animatedObjectPath.SamplePathForPoints(samplingFrequency);
        }

        public List<Vector3> SampleRotationPathForPoints(
            int samplingFrequency) {

            return RotationPath.SamplePathForPoints(samplingFrequency);
        }

        /// <summary>
        /// Returns animated object node index at the specified timestamp.
        /// </summary>
        /// <param name="timestamp">Timestamp to search for.</param>
        /// <returns>Node index.</returns>
        public int GetAnimObjNodeIndexAtTime(float timestamp) {
            return AnimatedObjectPath.GetNodeIndexAtTime(timestamp);
        }

        #endregion

        #region HELPER METHODS

        #endregion

        /// <summary>
        /// Multiply ease curve values by a given value.
        /// </summary>
        /// <param name="multiplier">Multiplier value.</param>
        public void MultiplyEaseCurveValues(float multiplier) {
            MultiplyCurveValues(EaseCurve, multiplier);
        }

        /// <summary>
        /// Multiply tilting curve values by a given value.
        /// </summary>
        /// <param name="multiplier">Multiplier value.</param>
        public void MultiplyTiltingCurveValues(float multiplier) {
            MultiplyCurveValues(TiltingCurve, multiplier);
        }

        private void MultiplyCurveValues(AnimationCurve curve, float multiplier) {
            // For each curve..
            for (var i = 0; i < curve.length; i++) {
                // Copy key.
                var keyCopy = curve[i];
                // Update key value.
                keyCopy.value *= multiplier;
                // Remove old key.
                curve.RemoveKey(i);
                // Add key.
                curve.AddKey(keyCopy);
                // Smooth all tangents.
                SmoothCurve(curve);
            }
        }

        /// <summary>
        /// Returns indexes of nodes that have a ease value assigned.
        /// </summary>
        /// <returns></returns>
        //public int[] GetNodesWithEaseEnabled() {
        //}

        public void RemoveKeyFromEaseCurve(int index) {
            EaseCurve.RemoveKey(index);
        }

    }

}