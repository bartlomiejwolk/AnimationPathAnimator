using System;
using System.Linq;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace ATP.AnimationPathTools {

    public class PathData : ScriptableObject {

        public event EventHandler NodeTiltChanged;

        public event EventHandler RotationPointPositionChanged;
        #region SERIALIZED FIELDS

        [SerializeField]
        private AnimationPath animatedObjectPath;

        [SerializeField]
        private AnimationCurve easeCurve;

        [SerializeField]
        private AnimationPath rotationPath;
        [SerializeField]
        private AnimationCurve tiltingCurve;

        #endregion SERIALIZED FIELDS

        #region PUBLIC PROPERTIES

        public AnimationPath AnimatedObjectPath {
            get { return animatedObjectPath; }
            set { animatedObjectPath = value; }
        }

        public AnimationCurve EaseCurve {
            get { return easeCurve; }
            set { easeCurve = value; }
        }

        public int NodesNo {
            get { return animatedObjectPath[0].length; }
        }
        public AnimationPath RotationPath {
            get { return rotationPath; }
            set { rotationPath = value; }
        }
        public AnimationCurve TiltingCurve {
            get { return tiltingCurve; }
            set { tiltingCurve = value; }
        }

        #endregion PUBLIC PROPERTIES

        #region PRIVATE/PROTECTED PROPERTIES

        protected virtual float DefaultEaseCurveValue {
            get { return 0.05f; }
        }

        protected virtual float FloatPrecision {
            get { return 0.001f; }
        }

        #endregion PRIVATE/PROTECTED PROPERTIES

        #region UNITY MESSAGES

        // ReSharper disable once UnusedMember.Local
        private void OnEnable() {
            // Return if fields are initialized.
            if (animatedObjectPath != null) return;

            InstantiateReferenceTypes();
            AssignDefaultValues();
        }

        #endregion UNITY MESSAGES

        #region EVENTINVOCATORS

        protected virtual void OnNodeTiltChanged() {
            var handler = NodeTiltChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnRotationPointPositionChanged() {
            var handler = RotationPointPositionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion EVENTINVOCATORS

        #region PUBLIC METHODS

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
                Debug.Log("You're trying to change rotation for nonexistent " +
                          "node.");

                return;
            }

            // Create new node.
            RotationPath.CreateNewNode(timestamp, newPosition);
            // Smooth all nodes.
            RotationPath.SmoothAllNodes();

            OnRotationPointPositionChanged();
        }

        public float GetNodeEaseValue(int i) {
            return EaseCurve.keys[i].value;
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

        public void Reset() {
            InstantiateReferenceTypes();
            AssignDefaultValues();
        }

        public void SmoothCurve(AnimationCurve curve) {
            for (var i = 0; i < curve.length; i++) {
                curve.SmoothTangents(i, 0);
            }
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
        /// Update AnimationCurve with keys added to the path.
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
                var valueExists = curveTimestamps.Any(t =>
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
                var keyExists = nodeTimestamps.Any(t =>
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
                    // Update rotation point timestamp.
                    RotationPath.ChangeNodeTimestamp(
                        i,
                        nodeTimestamps[i]);
                }
            }
        }

        public void UpdateRotationCurvesWithAddedKeys() {
            // AnimationPathBuilder node timestamps.
            var animationCurvesTimestamps = GetPathTimestamps();
            // Get values from rotationPath.
            var rotationCurvesTimestamps = RotationPath.GetTimestamps();
            var rotationCurvesKeysNo = rotationCurvesTimestamps.Length;

            // For each timestamp in rotationPath..
            for (var i = 0; i < animationCurvesTimestamps.Length; i++) {
                var keyExists = false;
                for (var j = 0; j < rotationCurvesKeysNo; j++) {
                    if (Math.Abs(rotationCurvesTimestamps[j]
                                 - animationCurvesTimestamps[i])
                        < FloatPrecision) {
                        keyExists = true;
                        break;
                    }
                }

                if (!keyExists) {
                    var addedKeyTimestamp = GetNodeTimestamp(i);
                    var defaultRotation =
                        RotationPath.GetVectorAtTime(addedKeyTimestamp);

                    RotationPath.CreateNewNode(
                        animationCurvesTimestamps[i],
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
                var keyExists = pathTimestamps.Any(nodeTimestamp =>
                    Math.Abs(rotationCurvesTimestamps[i] - nodeTimestamp)
                    < FloatPrecision);

                // If key exists check next timestamp.
                if (keyExists) continue;

                // Remove node from rotationPath.
                RotationPath.RemoveNode(i);

                break;
            }
        }
        #endregion PUBLIC METHODS

        #region PRIVATE METHODS

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

        #endregion PRIVATE METHODS
    }
}