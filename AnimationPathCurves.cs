using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace ATP.AnimationPathTools {

    /// <summary>
    /// </summary>
    /// <remarks>
    /// - All three curves are always synchronized, ie. keys number and
    /// respective keys' timestamps are the same.
    /// - Three keys with the same timestamp make a point.
    /// </remarks>
    public class AnimationPathCurves : ScriptableObject {

        #region FIELDS

        /// <summary>
        /// Animation Curves based on which the Animation Path is constructed.
        /// </summary>
        [SerializeField]
        private AnimationCurve[] curves = new AnimationCurve[3];

        public int KeysNo {
            get { return curves[0].length; }
        }

        /// <summary>
        /// Class indexer.
        /// </summary>
        /// <param name="i">Curve index.</param>
        /// <returns>AnimationCurve instance.</returns>
        public AnimationCurve this[int i] {
            get { return curves[i]; }
            set { curves[i] = value; }
        }

        #endregion FIELDS

        #region UNITY MESSAGES
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnEnable() {
            // Initialize curves field.
            if (curves[0] == null) {
                InitializeCurves();
            }
        }
        #endregion

        #region PUBLIC METHODS

        public void AddNodeAtTime(float timestamp) {
            for (var j = 0; j < 3; j++) {
                var newKeyValue = curves[j].Evaluate(timestamp);
                curves[j].AddKey(timestamp, newKeyValue);
            }
        }

        public void ChangePointTangents(
                int nodeIndex,
                Vector3 tangentDelta) {

            // Copy keys.
            var keyXCopy = curves[0].keys[nodeIndex];
            var keyYCopy = curves[1].keys[nodeIndex];
            var keyZCopy = curves[2].keys[nodeIndex];

            // Update keys' values.
            keyXCopy.inTangent += tangentDelta.x;
            keyYCopy.inTangent += tangentDelta.y;
            keyZCopy.inTangent += tangentDelta.z;

            keyXCopy.outTangent += tangentDelta.x;
            keyYCopy.outTangent += tangentDelta.y;
            keyZCopy.outTangent += tangentDelta.z;

            // Update keys.
            curves[0].MoveKey(nodeIndex, keyXCopy);
            curves[1].MoveKey(nodeIndex, keyYCopy);
            curves[2].MoveKey(nodeIndex, keyZCopy);
        }

        // TODO Rename Point to Node.
        public void ChangePointTimestamp(
            int keyIndex,
            float newTimestamp) {

            // For each curve..
            for (var i = 0; i < 3; i++) {
                // Get copy of the key from animation curves.
                var keyCopy = curves[i].keys[keyIndex];

                // Change key's time.
                keyCopy.time = newTimestamp;

                // Replace old key with a new one.
                curves[i].MoveKey(keyIndex, keyCopy);
            }
        }

        public void CreateNewPoint(float timestamp, Vector3 position) {
            curves[0].AddKey(timestamp, position.x);
            curves[1].AddKey(timestamp, position.y);
            curves[2].AddKey(timestamp, position.z);
        }

        public float GetTimeAtKey(int keyIndex) {
            return curves[0].keys[keyIndex].time;
        }

        public Vector3 GetVectorAtKey(int keyIndex) {
            var x = curves[0].keys[keyIndex].value;
            var y = curves[1].keys[keyIndex].value;
            var z = curves[2].keys[keyIndex].value;

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Create 3d vector from animation curves.
        /// </summary>
        /// <param name="timestamp">
        /// Point in time for which the 3d vector should be constructed. Time
        /// ranges alwas from 0 to 1.
        /// </param>
        /// <returns>3d point at a given time.</returns>
        public Vector3 GetVectorAtTime(float timestamp) {
            // Get node position.
            var x = curves[0].Evaluate(timestamp);
            var y = curves[1].Evaluate(timestamp);
            var z = curves[2].Evaluate(timestamp);

            // Construct 3d point.
            var pos = new Vector3(x, y, z);

            return pos;
        }

        /// <summary>
        /// Update animation curves' values for a given key with a given
        /// Vector3 value.
        /// </summary>
        /// <param name="keyIndex">Index of the key to update.</param>
        /// <param name="position">New key value.</param>
        public void MovePointToPosition(
                int keyIndex,
                Vector3 position) {

            // Copy keys.
            var keyXCopy = curves[0].keys[keyIndex];
            var keyYCopy = curves[1].keys[keyIndex];
            var keyZCopy = curves[2].keys[keyIndex];

            // Update keys' values.
            keyXCopy.value = position.x;
            keyYCopy.value = position.y;
            keyZCopy.value = position.z;

            // Move keys.
            curves[0].MoveKey(keyIndex, keyXCopy);
            curves[1].MoveKey(keyIndex, keyYCopy);
            curves[2].MoveKey(keyIndex, keyZCopy);
        }
        public void RemovePoint(int nodeIndex) {
            // For each animation curve..
            for (var i = 0; i < 3; i++) {
                // Remove node keys.
                curves[i].RemoveKey(nodeIndex);
            }
        }

        /// <summary>
        /// Smooth in/out tangents of a single point.
        /// </summary>
        /// <param name="nodeIndex">Point index.</param>
        public void SmoothPointTangents(int nodeIndex) {
            // For each curve..
            for (var i = 0; i < curves.Length; i++) {
                // Smooth tangents.
                curves[i].SmoothTangents(nodeIndex, 0);
            }
        }

        #endregion PUBLIC METHODS
        #region PRIVATE METHODS

        /// <summary>
        /// Initialize <c>curves</c> field with empty AnimationCurve objects.
        /// </summary>
        private void InitializeCurves() {
            for (var i = 0; i < curves.Length; i++) {
                curves[i] = new AnimationCurve();
            }
        }
        #endregion PRIVATE METHODS

        public float[] GetTimestamps() {
            var timestamps = new float[KeysNo];

            for (var i = 0; i < KeysNo; i++) {
                timestamps[i] = curves[0].keys[i].time;
            }

            return timestamps;
        }

        public void SmoothAllNodes() {
            for (var i = 0; i < KeysNo; i++) {
                SmoothPointTangents(i);
            }
        }
    }
}