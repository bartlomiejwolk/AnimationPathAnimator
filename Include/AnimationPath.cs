using System;
using System.Collections.Generic;
using UnityEngine;

namespace ATP.AnimationPathAnimator.APAnimatorComponent {

    /// <summary>
    /// Represents 3d points with three animation curves.
    /// </summary>
    /// <remarks>
    ///     - All three curves are always synchronized, ie. keys number and
    ///     respective keys' timestamps are the same.
    ///     - Three keys with the same timestamp make a node.
    /// </remarks>
    [Serializable]
    public class AnimationPath {
        #region FIELDS

        /// <summary>
        ///     Animation Curves based on which the Animation Path is constructed.
        /// </summary>
        [SerializeField]
        private AnimationCurve[] curves;

        public AnimationPath() {
            curves = new AnimationCurve[3];
        }

        #endregion FIELDS

        #region PROPERTIES

        public int KeysNo {
            get { return curves[0].length; }
        }

        /// <summary>
        ///     Class indexer.
        /// </summary>
        /// <param name="i">Curve index.</param>
        /// <returns>AnimationCurve instance.</returns>
        public AnimationCurve this[int i] {
            get { return curves[i]; }
            set { curves[i] = value; }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Adds node at timestamp. Node's value will be evaluated using existing path.
        /// </summary>
        /// <param name="timestamp">Timestamp of for the node.</param>
        public void AddNodeAtTime(float timestamp) {
            // For each curve..
            for (var j = 0; j < 3; j++) {
                // Get key value.
                var newKeyValue = curves[j].Evaluate(timestamp);
                // Add new key to path.
                curves[j].AddKey(timestamp, newKeyValue);
            }
        }

        /// <summary>
        /// Calculates path length.
        /// </summary>
        /// <param name="samplingFrequency">Amount of sample points for one meter of path.</param>
        /// <returns>Path length in meters.</returns>
        // TODO Rename to CalculatePathLength().
        public float CalculatePathCurvedLength(int samplingFrequency) {
            float pathLength = 0;

            // For each node..
            for (var i = 0; i < KeysNo - 1; i++) {
                // Calculate length of the path between two nodes.
                pathLength += CalculateSectionCurvedLength(
                    i,
                    i + 1,
                    samplingFrequency);
            }

            return pathLength;
        }

        /// <summary>
        /// Calculates shortest path between path nodes.
        /// </summary>
        /// <returns>Path length in meters.</returns>
        public float CalculatePathLinearLength() {
            // Result distance.
            float dist = 0;

            // For each node (exclude the first one)..
            for (var i = 0; i < KeysNo - 1; i++) {
                // Calculate distance between two nodes.
                dist += CalculateSectionLinearLength(i, i + 1);
            }

            return dist;
        }

        /// <summary>
        /// Calculates path length between two nodes.
        /// </summary>
        /// <param name="firstNodeIndex">Index of the first node.</param>
        /// <param name="secondNodeIndex">Index of the second node.</param>
        /// <param name="samplingFrequency">Amount of sample points for one meter of path.</param>
        /// <returns>Section length in meters.</returns>
        // TODO Rename to CalculateSectionLength().
        public float CalculateSectionCurvedLength(
            int firstNodeIndex,
            int secondNodeIndex,
            int samplingFrequency) {

            // Result path length.
            float pathLength = 0;

            var points = SampleSectionForPoints(
                firstNodeIndex,
                secondNodeIndex,
                samplingFrequency);

            // For each point..
            for (var i = 1; i < points.Count; i++) {
                // Calculate distance this and previous point.
                pathLength += Vector3.Distance(points[i - 1], points[i]);
            }

            return pathLength;
        }

        /// <summary>
        /// Calculates shortest distance between two nodes.
        /// </summary>
        /// <param name="firstNodeIndex">Index of the first node.</param>
        /// <param name="secondNodeIndex">Index of the second node.</param>
        /// <returns>Length in meters.</returns>
        public float CalculateSectionLinearLength(
            int firstNodeIndex,
            int secondNodeIndex) {

            var firstNodePosition = GetVectorAtKey(firstNodeIndex);
            var secondNodePosition = GetVectorAtKey(secondNodeIndex);

            var sectionLength =
                Vector3.Distance(firstNodePosition, secondNodePosition);

            return sectionLength;
        }

        /// <summary>
        /// Changes node timestamp.
        /// </summary>
        /// <param name="keyIndex">Node index.</param>
        /// <param name="newTimestamp">New timestamp.</param>
        public void ChangeNodeTimestamp(
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

        /// <summary>
        /// Changes in/out tangents of a single node.
        /// </summary>
        /// <param name="nodeIndex">Node index.</param>
        /// <param name="inTangent">New in tangent.</param>
        /// <param name="outTangent">New out tangent</param>
        // TODO Rename to ChangeNodeTangent().
        public void ChangePointTangents(
            int nodeIndex,
            float inTangent,
            float outTangent) {

            // Copy keys.
            var keyXCopy = curves[0].keys[nodeIndex];
            var keyYCopy = curves[1].keys[nodeIndex];
            var keyZCopy = curves[2].keys[nodeIndex];

            // Update keys' values.
            keyXCopy.inTangent = inTangent;
            keyYCopy.inTangent = inTangent;
            keyZCopy.inTangent = inTangent;

            keyXCopy.outTangent = outTangent;
            keyYCopy.outTangent = outTangent;
            keyZCopy.outTangent = outTangent;

            // Update keys.
            curves[0].MoveKey(nodeIndex, keyXCopy);
            curves[1].MoveKey(nodeIndex, keyYCopy);
            curves[2].MoveKey(nodeIndex, keyZCopy);
        }

        public void CreateNewNode(float timestamp, Vector3 position) {
            curves[0].AddKey(timestamp, position.x);
            curves[1].AddKey(timestamp, position.y);
            curves[2].AddKey(timestamp, position.z);
        }

        public float GetTimeAtKey(int keyIndex) {
            return curves[0].keys[keyIndex].time;
        }

        public float[] GetTimestamps() {
            var timestamps = new float[KeysNo];

            for (var i = 0; i < KeysNo; i++) {
                timestamps[i] = curves[0].keys[i].time;
            }

            return timestamps;
        }

        public Vector3 GetVectorAtKey(int keyIndex) {
            var x = curves[0].keys[keyIndex].value;
            var y = curves[1].keys[keyIndex].value;
            var z = curves[2].keys[keyIndex].value;

            return new Vector3(x, y, z);
        }

        /// <summary>
        ///     Create 3d vector from animation curves.
        /// </summary>
        /// <param name="timestamp">
        ///     Point in time for which the 3d vector should be constructed. Time
        ///     ranges alwas from 0 to 1.
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
        ///     Initialize <c>curves</c> field with empty AnimationCurve objects.
        /// </summary>
        public void InstantiateAnimationPathCurves() {
            for (var i = 0; i < 3; i++) {
                curves[i] = new AnimationCurve();
            }
        }

        /// <summary>
        ///     Update animation curves' values for a given key with a given
        ///     Vector3 value.
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

        public void RemoveAllKeys() {
            var keysToRemoveNo = KeysNo;
            for (var i = 0; i < keysToRemoveNo; i++) {
                RemoveNode(0);
            }
        }

        public void RemoveNode(int nodeIndex) {
            // For each animation curve..
            for (var i = 0; i < 3; i++) {
                // Remove node keys.
                curves[i].RemoveKey(nodeIndex);
            }
        }

        /// <summary>
        ///     Extract 3d points from path.
        /// </summary>
        /// <param name="samplingFrequency"></param>
        /// <returns></returns>
        public List<Vector3> SamplePathForPoints(int samplingFrequency) {
            var points = new List<Vector3>();

            // Call reference overload.
            SamplePathForPoints(samplingFrequency, ref points);

            return points;
        }

        public void SamplePathForPoints(
            int samplingFrequency,
            ref List<Vector3> points) {

            var linearPathLength = CalculatePathLinearLength();

            // Calculate amount of points to extract.
            var samplingRate = (int) (linearPathLength * samplingFrequency);

            // NOTE Cannot do any sampling if sampling rate is less than 1.
            if (samplingRate < 1) return;

            // Used to read values from animation curves.
            float time = 0;

            // Time step between each point.
            var timestep = 1f / samplingRate;

            // Clear points list.
            points.Clear();

            // Fill points array with 3d points.
            for (var i = 0; i < samplingRate; i++) {
                // Calculate single point.
                var point = GetVectorAtTime(time);

                // Construct 3d point from animation curves at a given time.
                points.Add(point);

                // Time goes towards 1.
                time += timestep;
            }
        }

        public void SampleSectionForPoints(
            int firstNodeIndex,
            int lastNodeIndex,
            float samplingFrequency,
            ref List<Vector3> points) {

            var sectionLinearLength = CalculateSectionLinearLength(
                firstNodeIndex,
                lastNodeIndex);

            // Calculate amount of points to extract.
            var samplingRate = (int) (sectionLinearLength * samplingFrequency);

            var firstNodeTime = GetTimeAtKey(firstNodeIndex);
            var secondNodeTime = GetTimeAtKey(lastNodeIndex);

            var timeInterval = secondNodeTime - firstNodeTime;

            // Used to read values from animation curves.
            var time = firstNodeTime;

            // Time step between each point.
            var timestep = timeInterval / samplingRate;

            // Clear points list.
            points.Clear();

            // Fill points array with 3d points.
            for (var i = 0; i < samplingRate; i++) {
                // Calculate single point.
                var point = GetVectorAtTime(time);

                // Construct 3d point from animation curves at a given time.
                points.Add(point);

                // Time goes towards 1.
                time += timestep;
            }
        }

        public List<Vector3> SampleSectionForPoints(
            int firstNodeIndex,
            int lastNodeIndex,
            float samplingFrequency) {

            var points = new List<Vector3>();

            SampleSectionForPoints(
                firstNodeIndex,
                lastNodeIndex,
                samplingFrequency,
                ref points);

            return points;
        }

        public void SmoothAllNodes() {
            for (var i = 0; i < KeysNo; i++) {
                SmoothPointTangents(i);
            }
        }

        /// <summary>
        ///     Smooth in/out tangents of a single point.
        /// </summary>
        /// <param name="nodeIndex">Point index.</param>
        public void SmoothPointTangents(int nodeIndex) {
            // For each curve..
            foreach (var curve in curves) {
                // Smooth tangents.
                curve.SmoothTangents(nodeIndex, 0);
            }
        }

        #endregion PRIVATE METHODS
    }

}