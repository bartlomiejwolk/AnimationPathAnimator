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
        /// Adds node at timestamp. Node's position will be evaluated using existing path.
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
        public float CalculatePathLength(int samplingFrequency) {
            float pathLength = 0;

            // For each node..
            for (var i = 0; i < KeysNo - 1; i++) {
                // Calculate length of the path between two nodes.
                pathLength += CalculateSectionLength(
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
        public float CalculateSectionLength(
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
        public void ChangeNodeTangents(
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

        /// <summary>
        /// Creates new node at position.
        /// </summary>
        /// <param name="timestamp">Node timestamp.</param>
        /// <param name="position">Node position</param>
        public void CreateNewNode(float timestamp, Vector3 position) {
            curves[0].AddKey(timestamp, position.x);
            curves[1].AddKey(timestamp, position.y);
            curves[2].AddKey(timestamp, position.z);
        }

        /// <summary>
        /// Returns timestamp of a given node.
        /// </summary>
        /// <param name="keyIndex">Node index</param>
        /// <returns></returns>
        public float GetTimeAtKey(int keyIndex) {
            return curves[0].keys[keyIndex].time;
        }

        /// <summary>
        /// Returns timestamps of all nodes.
        /// </summary>
        /// <returns>Array with node timestamps.</returns>
        public float[] GetTimestamps() {
            var timestamps = new float[KeysNo];

            for (var i = 0; i < KeysNo; i++) {
                timestamps[i] = curves[0].keys[i].time;
            }

            return timestamps;
        }

        /// <summary>
        /// Returns 3d position of node at index.
        /// </summary>
        /// <param name="keyIndex">Node index</param>
        /// <returns>Node 3d position.</returns>
        public Vector3 GetVectorAtKey(int keyIndex) {
            var x = curves[0].keys[keyIndex].value;
            var y = curves[1].keys[keyIndex].value;
            var z = curves[2].keys[keyIndex].value;

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Returns node 3d position at timestamp.
        /// </summary>
        /// <param name="timestamp">
        ///     Point in time for which the 3d vector should be constructed.
        /// </param>
        /// <returns>Node 3d position.</returns>
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
        ///     Initializes <c>curves</c> field with empty animation curves.
        /// </summary>
        // TODO This should be a constructor.
        public void InstantiateAnimationPathCurves() {
            for (var i = 0; i < 3; i++) {
                curves[i] = new AnimationCurve();
            }
        }

        /// <summary>
        /// Changes node's position at index.
        /// </summary>
        /// <param name="keyIndex">Index of the key to update.</param>
        /// <param name="position">New node position.</param>
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

        /// <summary>
        /// Removes all nodes.
        /// </summary>
        // TODO Rename to RemoveAllNodes().
        public void RemoveAllKeys() {
            var keysToRemoveNo = KeysNo;
            for (var i = 0; i < keysToRemoveNo; i++) {
                RemoveNode(0);
            }
        }

        /// <summary>
        /// Removes node at index.
        /// </summary>
        /// <param name="nodeIndex">Node index.</param>
        public void RemoveNode(int nodeIndex) {
            // For each animation curve..
            for (var i = 0; i < 3; i++) {
                // Remove node keys.
                curves[i].RemoveKey(nodeIndex);
            }
        }

        /// <summary>
        ///     Extracts 3d points from path.
        /// </summary>
        /// <param name="samplingFrequency">Amount of points to be extracted from one meter of path.</param>
        /// <returns>Extracted points.</returns>
        public List<Vector3> SamplePathForPoints(int samplingFrequency) {
            var points = new List<Vector3>();

            // Call reference overload.
            SamplePathForPoints(samplingFrequency, ref points);

            return points;
        }

        /// <summary>
        ///     Extracts 3d points from path. Reference overload.
        /// </summary>
        /// <param name="samplingFrequency">Amount of points to be extracted from one meter of path.</param>
        /// <param name="points">Reference to a list used to store the extracted points.</param>
        public void SamplePathForPoints(
            int samplingFrequency,
            ref List<Vector3> points) {

            // Calculate linear path length
            var linearPathLength = CalculatePathLinearLength();
            // Calculate amount of points to extract.
            var samplingRate = (int) (linearPathLength * samplingFrequency);

            // NOTE Cannot do any sampling if sampling rate is less than 1.
            if (samplingRate < 1) return;

            // Timestep between each point.
            var timestep = 1f / samplingRate;

            // Clear points list.
            points.Clear();

            // Helper variable.
            // Used to read values from animation curves.
            float time = 0;

            // For each point to extract..
            for (var i = 0; i < samplingRate; i++) {
                // Calculate point position.
                var point = GetVectorAtTime(time);
                // Add to result list.
                points.Add(point);
                // Update time.
                time += timestep;
            }
        }

        /// <summary>
        /// Extracts 3d points from a path section between two given nodes.
        /// </summary>
        /// <param name="firstNodeIndex">First node index.</param>
        /// <param name="lastNodeIndex">Last node index.</param>
        /// <param name="samplingFrequency">Amount of points to be extracted for one meter of the path.</param>
        /// <param name="points">Reference to a list that'll be updated with extracted points.</param>
        // TODO Rename this and overload to ExtractPointsFromSection().
        public void SampleSectionForPoints(
            int firstNodeIndex,
            int lastNodeIndex,
            float samplingFrequency,
            ref List<Vector3> points) {

            // Calculate section linear length.
            var sectionLinearLength = CalculateSectionLinearLength(
                firstNodeIndex,
                lastNodeIndex);
            // Calculate amount of points to extract.
            var samplingRate = (int) (sectionLinearLength * samplingFrequency);
            // Get first node timestamp.
            var firstNodeTime = GetTimeAtKey(firstNodeIndex);
            // Get last node timestamp.
            var lastNodeTime = GetTimeAtKey(lastNodeIndex);
            // Calculate time interval between section extreme nodes.
            var timeInterval = lastNodeTime - firstNodeTime;
            // Calculate timestep between each point.
            var timestep = timeInterval / samplingRate;

            // Clear points list.
            points.Clear();

            // Helper variable.
            // Used to read values from animation curves.
            var time = firstNodeTime;

            // For each point to be extracted..
            for (var i = 0; i < samplingRate; i++) {
                // Get point position..
                var point = GetVectorAtTime(time);
                // Add point to result array.
                points.Add(point);
                // Update time.
                time += timestep;
            }
        }

        /// <summary>
        /// Extracts 3d points from a path section between two given nodes.
        /// </summary>
        /// <param name="firstNodeIndex">First node index.</param>
        /// <param name="lastNodeIndex">Last node index.</param>
        /// <param name="samplingFrequency">Amount of points to be extracted for one meter of the path.</param>
        /// <returns>Extracted points.</returns>
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

        /// <summary>
        /// Smooth in/out tangents of all nodes.
        /// </summary>
        public void SmoothAllNodes() {
            for (var i = 0; i < KeysNo; i++) {
                SmoothPointTangents(i);
            }
        }

        /// <summary>
        ///     Smooth in/out tangents of a single node.
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