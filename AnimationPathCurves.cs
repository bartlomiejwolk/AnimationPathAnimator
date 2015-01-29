using System;
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

        /// <summary>
        /// Animation Curves based on which the Animation Path is constructed.
        /// </summary>
        [SerializeField]
        private AnimationCurve[] _curves = new AnimationCurve[3];

        /// <summary>
        /// Event that is fired every time there's any change to the animation
        /// curves.
        /// </summary>
        public event EventHandler CurvesChanged;

        public int KeysNo {
            get { return _curves[0].length; }
        }

        /// <summary>
        /// Class indexer.
        /// </summary>
        /// <param name="i">Curve index.</param>
        /// <returns>AnimationCurve instance.</returns>
        public AnimationCurve this[int i] {
            get { return _curves[i]; }
            set { _curves[i] = value; }
        }

        // TODO Rename to CreateNewPoint().
        public void AddNewPoint(float timestamp, Vector3 position) {
            _curves[0].AddKey(timestamp, position.x);
            _curves[1].AddKey(timestamp, position.y);
            _curves[2].AddKey(timestamp, position.z);

            // Fire event.
            OnCurvesChanged();
        }

        /// <summary>
        /// Update animation curves' values for a given key with a given
        /// Vector3 value.
        /// </summary>
        /// <param name="curves">Animation curves.</param>
        /// <param name="keyIndex">Index of the key to update.</param>
        /// <param name="position">New key value.</param>
        // TODO Rename to MovePointToPosition().
        // TODO This should accept timestamp instead of index.
        public void MovePoint(
                int keyIndex,
                Vector3 position) {

            // Copy keys.
            Keyframe keyXCopy = _curves[0].keys[keyIndex];
            Keyframe keyYCopy = _curves[1].keys[keyIndex];
            Keyframe keyZCopy = _curves[2].keys[keyIndex];

            // Update keys' values.
            keyXCopy.value = position.x;
            keyYCopy.value = position.y;
            keyZCopy.value = position.z;

            // Move keys.
            _curves[0].MoveKey(keyIndex, keyXCopy);
            _curves[1].MoveKey(keyIndex, keyYCopy);
            _curves[2].MoveKey(keyIndex, keyZCopy);

            // Fire event.
            OnCurvesChanged();
        }

        public void ChangePointTangents(
                int nodeIndex,
                Vector3 tangentDelta) {

            // Copy keys.
            Keyframe keyXCopy = _curves[0].keys[nodeIndex];
            Keyframe keyYCopy = _curves[1].keys[nodeIndex];
            Keyframe keyZCopy = _curves[2].keys[nodeIndex];

            // Update keys' values.
            keyXCopy.inTangent += tangentDelta.x;
            keyYCopy.inTangent += tangentDelta.y;
            keyZCopy.inTangent += tangentDelta.z;

            keyXCopy.outTangent += tangentDelta.x;
            keyYCopy.outTangent += tangentDelta.y;
            keyZCopy.outTangent += tangentDelta.z;

            // Update keys.
            _curves[0].MoveKey(nodeIndex, keyXCopy);
            _curves[1].MoveKey(nodeIndex, keyYCopy);
            _curves[2].MoveKey(nodeIndex, keyZCopy);

            // Fire event.
            OnCurvesChanged();
        }

        public void ChangePointTimestamp(
            int keyIndex,
            float newTimestamp) {

            // For each curve..
            for (int i = 0; i < 3; i++) {
                // Get copy of the key from animation curves.
                Keyframe keyCopy = _curves[i].keys[keyIndex];

                // Change key's time.
                keyCopy.time = newTimestamp;

                // Replace old key with a new one.
                _curves[i].MoveKey(keyIndex, keyCopy);
            }

            // Fire event.
            OnCurvesChanged();
        }

        public float GetTimeAtKey(int keyIndex) {
            return _curves[0].keys[keyIndex].time;
        }

        public Vector3 GetVectorAtKey(int keyIndex) {
            float x = _curves[0].keys[keyIndex].value;
            float y = _curves[1].keys[keyIndex].value;
            float z = _curves[2].keys[keyIndex].value;

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Create 3d vector from animation curves.
        /// </summary>
        /// <param name="curves">
        /// Three animation curves that make 3d path.
        /// </param>
        /// <param name="timestamp">
        /// Point in time for which the 3d vector should be constructed. Time
        /// ranges alwas from 0 to 1.
        /// </param>
        /// <returns>3d point at a given time.</returns>
        public Vector3 GetVectorAtTime(float timestamp) {
            // Get node position.
            var x = _curves[0].Evaluate(timestamp);
            var y = _curves[1].Evaluate(timestamp);
            var z = _curves[2].Evaluate(timestamp);

            // Construct 3d point.
            Vector3 pos = new Vector3(x, y, z);

            return pos;
        }

        public void RemovePoint(int nodeIndex) {
            // For each animation curve..
            for (int i = 0; i < 3; i++) {
                // Remove node keys.
                _curves[i].RemoveKey(nodeIndex);
            }
        }

        /// <summary>
        /// Smooth in/out tangents of a single point.
        /// </summary>
        /// <param name="nodeIndex">Point index.</param>
        /// <param name="tangentWeight">Tangent weight.</param>
        public void SmoothPointTangents(int nodeIndex, float tangentWeight) {
            // For each curve..
            for (int i = 0; i < _curves.Length; i++) {
                // Smooth tangents.
                _curves[i].SmoothTangents(nodeIndex, tangentWeight);
            }
        }

        /// <summary>
        /// Initialize <c>_curves</c> field with empty AnimationCurve objects.
        /// </summary>
        private void InitializeCurves() {
            for (var i = 0; i < _curves.Length; i++) {
                _curves[i] = new AnimationCurve();
            }
        }

        private void OnCurvesChanged() {
            if (CurvesChanged != null) {
                CurvesChanged(this, EventArgs.Empty);
            }
        }

        private void OnEnable() {
            // Initialize _curves field.
            if (_curves[0] == null) {
                InitializeCurves();
            }
        }

        public void SetPointLinear(int nodeIndex) {
             for (int i = 0; i < 3; ++i) {
                float intangent = 0;
                float outtangent = 0;
                bool intangent_set = false;
                bool outtangent_set = false;
                Vector2 point1;
                Vector2 point2;
                Vector2 deltapoint;
                Keyframe key = _curves[i][nodeIndex];

                if (nodeIndex == 0) {
                    intangent = 0; intangent_set = true;
                }

                if (nodeIndex == _curves[i].keys.Length - 1) {
                    outtangent = 0; outtangent_set = true;
                }

                if (!intangent_set) {
                    point1.x = _curves[i].keys[nodeIndex - 1].time;
                    point1.y = _curves[i].keys[nodeIndex - 1].value;
                    point2.x = _curves[i].keys[nodeIndex].time;
                    point2.y = _curves[i].keys[nodeIndex].value;

                    deltapoint = point2 - point1;
                    intangent = deltapoint.y / deltapoint.x;
                }

                if (!outtangent_set) {
                    point1.x = _curves[i].keys[nodeIndex].time;
                    point1.y = _curves[i].keys[nodeIndex].value;
                    point2.x = _curves[i].keys[nodeIndex + 1].time;
                    point2.y = _curves[i].keys[nodeIndex + 1].value;

                    deltapoint = point2 - point1;
                    outtangent = deltapoint.y / deltapoint.x;
                }

                key.inTangent = intangent;
                key.outTangent = outtangent;

                _curves[i].MoveKey(nodeIndex, key);
            }

            // Fire event.
            OnCurvesChanged();
        }

        public void AddNodeAtTime(float timestamp) {
             for (int j = 0; j < 3; j++) {
                float newKeyValue = _curves[j].Evaluate(timestamp);
                _curves[j].AddKey(timestamp, newKeyValue);
            }           
        }
    }
}