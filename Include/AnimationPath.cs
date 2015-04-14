/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com).
 * Please direct any bugs/comments/suggestions to bartlomiejwolk@gmail.com
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnimationPathTools.AnimatorComponent {

    /// <summary>
    ///     Represents 3d points with three animation curves.
    /// </summary>
    /// <remarks>
    ///     - All three curves are always synchronized, ie. keys number and
    ///     respective keys' timestamps are the same.
    ///     - Three keys with the same timestamp make a node.
    /// </remarks>
    [Serializable]
    public class AnimationPath {
        #region DO METHODS

        /// <summary>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        /// <remarks>http://stackoverflow.com/questions/3874627/floating-point-comparison-functions-for-c-sharp</remarks>
        private bool FloatsEqual(float a, float b, float epsilon) {
            var absA = Math.Abs(a);
            var absB = Math.Abs(b);
            var diff = Math.Abs(a - b);

            if (a == b) {
                // shortcut, handles infinities
                return true;
            }
            if (a == 0 || b == 0) {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * Single.MinValue);
            }
            // use relative error
            return diff / (absA + absB) < epsilon;
        }

        #endregion

        #region FIELDS

        private const float FloatPrecision = 0.0000001f;

        private const int PathLengthSampling = 10;

        /// <summary>
        ///     Animation curves based on which the animation path is constructed.
        /// </summary>
        [SerializeField]
        private AnimationCurve[] curves;

        #endregion FIELDS

        #region PROPERTIES

        /// <summary>
        ///     Number of nodes in the path.
        /// </summary>
        public int KeysNo {
            get { return Curves[0].length; }
        }

        public float[] Timestamps {
            get { return GetTimestamps(); }
        }

        /// <summary>
        ///     Animation curves based on which the animation path is constructed.
        /// </summary>
        private AnimationCurve[] Curves {
            get { return curves; }
        }

        /// <summary>
        ///     Class indexer.
        /// </summary>
        /// <param name="i">Curve index.</param>
        /// <returns>AnimationCurve instance.</returns>
        public AnimationCurve this[int i] {
            get { return Curves[i]; }
            set { Curves[i] = value; }
        }

        #endregion

        #region GET METHODS

        /// <summary>
        ///     Calculates path length.
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
        ///     Calculates shortest path between path nodes.
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
        ///     Calculates path length between two nodes.
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

            var points = ExtractPointsFromSection(
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
        ///     Calculates shortest distance between two nodes.
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
        ///     Extracts 3d points from a path section between two given nodes.
        /// </summary>
        /// <param name="firstNodeIndex">First node index.</param>
        /// <param name="lastNodeIndex">Last node index.</param>
        /// <param name="samplingFrequency">Amount of points to be extracted for one meter of the path.</param>
        /// <param name="points">Reference to a list that'll be updated with extracted points.</param>
        public void ExtractPointsFromSection(
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
        ///     Extracts 3d points from a path section between two given nodes.
        /// </summary>
        /// <param name="firstNodeIndex">First node index.</param>
        /// <param name="lastNodeIndex">Last node index.</param>
        /// <param name="samplingFrequency">Amount of points to be extracted for one meter of the path.</param>
        /// <returns>Extracted points.</returns>
        public List<Vector3> ExtractPointsFromSection(
            int firstNodeIndex,
            int lastNodeIndex,
            float samplingFrequency) {

            var points = new List<Vector3>();

            ExtractPointsFromSection(
                firstNodeIndex,
                lastNodeIndex,
                samplingFrequency,
                ref points);

            return points;
        }

        /// <summary>
        ///     Returns node index at the specified timestamp.
        /// </summary>
        /// <param name="searchedTimestamps">Timestamp to search for.</param>
        /// <returns>Node index. -1 if no node at time was found.</returns>
        public int GetNodeIndexAtTime(float searchedTimestamps) {
            var timestamps = Curves[0].keys.Select(key => key.time).ToList();

            // For each timestamp in the path..
            for (var i = 0; i < timestamps.Count; i++) {
                // If is equal with arg.
                if (Utilities.FloatsEqual(
                    timestamps[i],
                    searchedTimestamps,
                    FloatPrecision)) {

                    // Return node index.
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        ///     Returns timestamp of a given node.
        /// </summary>
        /// <param name="keyIndex">Node index</param>
        /// <returns></returns>
        public float GetTimeAtKey(int keyIndex) {
            return Curves[0].keys[keyIndex].time;
        }

        /// <summary>
        ///     Returns 3d position of node at index.
        /// </summary>
        /// <param name="keyIndex">Node index</param>
        /// <returns>Node 3d position.</returns>
        public Vector3 GetVectorAtKey(int keyIndex) {
            var x = Curves[0].keys[keyIndex].value;
            var y = Curves[1].keys[keyIndex].value;
            var z = Curves[2].keys[keyIndex].value;

            return new Vector3(x, y, z);
        }

        /// <summary>
        ///     Returns node 3d position at timestamp.
        /// </summary>
        /// <param name="timestamp">
        ///     Point in time for which the 3d vector should be constructed.
        /// </param>
        /// <returns>Node 3d position.</returns>
        public Vector3 GetVectorAtTime(float timestamp) {
            // Get node position.
            var x = Curves[0].Evaluate(timestamp);
            var y = Curves[1].Evaluate(timestamp);
            var z = Curves[2].Evaluate(timestamp);

            // Construct 3d point.
            var pos = new Vector3(x, y, z);

            return pos;
        }

        public bool NodeAtTimeExists(float timestamp) {
            var foundMatch = false;
            for (var i = 0; i < KeysNo; i++) {
                // If it is the node to change..
                if (FloatsEqual(Timestamps[i], timestamp, FloatPrecision)) {
                    foundMatch = true;
                }
            }

            return foundMatch;
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
            var linearPathLength = CalculatePathLength(PathLengthSampling);
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

        public List<float> SamplePathForTimestamps(int samplingFrequency) {
            // Result list.
            var resultTimestamps = new List<float>();

            // Calculate linear path length
            var linearPathLength = CalculatePathLength(PathLengthSampling);
            // Calculate amount of points to extract.
            var samplingRate = (int) (linearPathLength * samplingFrequency);

            // NOTE Cannot do any sampling if sampling rate is less than 1.
            if (samplingRate < 1) return new List<float>();

            // Timestep between each point.
            var timestep = 1f / samplingRate;

            // Helper variable.
            // Used to read values from animation curves.
            float time = 0;

            // For each point to extract..
            for (var i = 0; i < samplingRate; i++) {
                // Add to result list.
                resultTimestamps.Add(time);
                // Update time.
                time += timestep;
            }

            return resultTimestamps;
        }

        /// <summary>
        ///     Returns timestamps of all nodes.
        /// </summary>
        /// <returns>Array with node timestamps.</returns>
        private float[] GetTimestamps() {
            var timestamps = new float[KeysNo];

            for (var i = 0; i < KeysNo; i++) {
                timestamps[i] = Curves[0].keys[i].time;
            }

            return timestamps;
        }

        #endregion PRIVATE METHODS

        #region EDIT METHODS

        /// <summary>
        ///     Constructor.
        /// </summary>
        public AnimationPath() {
            InitializeAnimationPathCurves();
        }

        /// <summary>
        ///     Adds node at timestamp. Node's position will be evaluated using existing path.
        /// </summary>
        /// <param name="timestamp">Timestamp of for the node.</param>
        public void AddNodeAtTime(float timestamp) {
            // For each curve..
            for (var j = 0; j < 3; j++) {
                // Get key value.
                var newKeyValue = Curves[j].Evaluate(timestamp);
                // Add new key to path.
                Curves[j].AddKey(timestamp, newKeyValue);
            }
        }

        /// <summary>
        ///     Changes in/out tangents of a single node.
        /// </summary>
        /// <param name="nodeIndex">Node index.</param>
        /// <param name="inTangent">New in tangent.</param>
        /// <param name="outTangent">New out tangent</param>
        public void ChangeNodeTangents(
            int nodeIndex,
            float inTangent,
            float outTangent) {

            // Copy keys.
            var keyXCopy = Curves[0].keys[nodeIndex];
            var keyYCopy = Curves[1].keys[nodeIndex];
            var keyZCopy = Curves[2].keys[nodeIndex];

            // Update keys' values.
            keyXCopy.inTangent = inTangent;
            keyYCopy.inTangent = inTangent;
            keyZCopy.inTangent = inTangent;

            keyXCopy.outTangent = outTangent;
            keyYCopy.outTangent = outTangent;
            keyZCopy.outTangent = outTangent;

            // Update keys.
            Curves[0].MoveKey(nodeIndex, keyXCopy);
            Curves[1].MoveKey(nodeIndex, keyYCopy);
            Curves[2].MoveKey(nodeIndex, keyZCopy);
        }

        /// <summary>
        ///     Changes node timestamp.
        /// </summary>
        /// <param name="keyIndex">Node index.</param>
        /// <param name="newTimestamp">New timestamp.</param>
        public void ChangeNodeTimestamp(
            int keyIndex,
            float newTimestamp) {

            // For each curve..
            for (var i = 0; i < 3; i++) {
                // Get copy of the key from animation curves.
                var keyCopy = Curves[i].keys[keyIndex];

                // Change key's time.
                keyCopy.time = newTimestamp;

                // Replace old key with a new one.
                Curves[i].MoveKey(keyIndex, keyCopy);
            }
        }

        /// <summary>
        ///     Creates new node at position.
        /// </summary>
        /// <param name="timestamp">Node timestamp.</param>
        /// <param name="position">Node position</param>
        public void CreateNewNode(float timestamp, Vector3 position) {
            Curves[0].AddKey(timestamp, position.x);
            Curves[1].AddKey(timestamp, position.y);
            Curves[2].AddKey(timestamp, position.z);
        }

        /// <summary>
        ///     Changes node's position at index.
        /// </summary>
        /// <param name="keyIndex">Index of the key to update.</param>
        /// <param name="position">New node position.</param>
        public void MovePointToPosition(
            int keyIndex,
            Vector3 position) {

            // Copy keys.
            var keyXCopy = Curves[0].keys[keyIndex];
            var keyYCopy = Curves[1].keys[keyIndex];
            var keyZCopy = Curves[2].keys[keyIndex];

            // Update keys' values.
            keyXCopy.value = position.x;
            keyYCopy.value = position.y;
            keyZCopy.value = position.z;

            // Move keys.
            Curves[0].MoveKey(keyIndex, keyXCopy);
            Curves[1].MoveKey(keyIndex, keyYCopy);
            Curves[2].MoveKey(keyIndex, keyZCopy);
        }

        public void MovePointToPosition(
            float timestamp,
            Vector3 position) {

            var nodeIndex = GetNodeIndexAtTime(timestamp);
            MovePointToPosition(nodeIndex, position);
        }

        public void OffsetNodeTangents(
            int nodeIndex,
            Vector3 tangentDelta) {

            // Copy keys.
            var keyXCopy = Curves[0].keys[nodeIndex];
            var keyYCopy = Curves[1].keys[nodeIndex];
            var keyZCopy = Curves[2].keys[nodeIndex];

            // Update keys' values.
            keyXCopy.inTangent += tangentDelta.x;
            keyYCopy.inTangent += tangentDelta.y;
            keyZCopy.inTangent += tangentDelta.z;

            keyXCopy.outTangent += tangentDelta.x;
            keyYCopy.outTangent += tangentDelta.y;
            keyZCopy.outTangent += tangentDelta.z;

            // Update keys.
            Curves[0].MoveKey(nodeIndex, keyXCopy);
            Curves[1].MoveKey(nodeIndex, keyYCopy);
            Curves[2].MoveKey(nodeIndex, keyZCopy);
        }

        /// <summary>
        ///     Removes all nodes.
        /// </summary>
        public void RemoveAllNodes() {
            var keysToRemoveNo = KeysNo;
            for (var i = 0; i < keysToRemoveNo; i++) {
                RemoveNode(0);
            }
        }

        /// <summary>
        ///     Removes node at index.
        /// </summary>
        /// <param name="nodeIndex">Node index.</param>
        public void RemoveNode(int nodeIndex) {
            // For each animation curve..
            for (var i = 0; i < 3; i++) {
                // Remove node keys.
                Curves[i].RemoveKey(nodeIndex);
            }
        }

        /// <summary>
        ///     Replaces path timestamps with those passed in the argument.
        /// </summary>
        /// <param name="newTimestamps"></param>
        public void ReplaceTimestamps(List<float> newTimestamps) {
            // Number of new timestamps must match path nodes number.
            if (newTimestamps.Count != KeysNo) return;

            var xNodesCopy = new List<Keyframe>(KeysNo);
            var yNodesCopy = new List<Keyframe>(KeysNo);
            var zNodesCopy = new List<Keyframe>(KeysNo);

            // Copy path nodes.
            for (var i = 0; i < KeysNo; i++) {
                xNodesCopy.Add(Curves[0].keys[i]);
                yNodesCopy.Add(Curves[1].keys[i]);
                zNodesCopy.Add(Curves[2].keys[i]);
            }

            // Update node timestamps (only non-extreme nodes).
            for (var i = 1; i < KeysNo - 1; i++) {
                var modifiedKeyframe = new Keyframe(
                    newTimestamps[i],
                    xNodesCopy[i].value,
                    xNodesCopy[i].inTangent,
                    xNodesCopy[i].outTangent);

                xNodesCopy[i] = modifiedKeyframe;

                modifiedKeyframe = new Keyframe(
                    newTimestamps[i],
                    yNodesCopy[i].value,
                    yNodesCopy[i].inTangent,
                    yNodesCopy[i].outTangent);

                yNodesCopy[i] = modifiedKeyframe;

                modifiedKeyframe = new Keyframe(
                    newTimestamps[i],
                    zNodesCopy[i].value,
                    zNodesCopy[i].inTangent,
                    zNodesCopy[i].outTangent);

                zNodesCopy[i] = modifiedKeyframe;
            }

            var keysNo = KeysNo;

            RemoveAllNodes();

            // Add updated nodes to animation path.
            for (var i = 0; i < keysNo; i++) {
                Curves[0].AddKey(xNodesCopy[i]);
                Curves[1].AddKey(yNodesCopy[i]);
                Curves[2].AddKey(zNodesCopy[i]);
            }
        }

        /// <summary>
        ///     Smooth in/out tangents of all nodes.
        /// </summary>
        public void SmoothAllNodes(float weight = 0) {
            for (var i = 0; i < KeysNo; i++) {
                SmoothNodeInOutTangents(i, weight);
            }
        }

        /// <summary>
        ///     Smooth in/out tangents of a single node.
        /// </summary>
        /// <param name="nodeIndex">Point index.</param>
        /// <param name="weight">A weight of 0 evens out tangents.</param>
        public void SmoothNodeInOutTangents(int nodeIndex, float weight) {
            // For each curve..
            foreach (var curve in Curves) {
                // Smooth tangents.
                curve.SmoothTangents(nodeIndex, weight);
            }
        }

        /// <summary>
        ///     Initializes <c>curves</c> field with empty animation curves.
        /// </summary>
        private void InitializeAnimationPathCurves() {
            curves = new AnimationCurve[3];

            for (var i = 0; i < 3; i++) {
                Curves[i] = new AnimationCurve();
            }
        }

        #endregion
    }

}