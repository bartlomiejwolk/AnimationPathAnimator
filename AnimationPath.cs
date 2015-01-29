using ATP.ReorderableList;
using System;
using System.Collections.Generic;
using UnityEngine;

/// Classes that allow for creation and usage of \link AnimationPath Animation
/// Paths\endlink.
/// 
/// Collaboration diagram for _AnimationPath_ component \image html
/// AnimationPathTools-collaboration.png
namespace ATP.AnimationPathTools {

    /// <summary>
    /// Allows creating and drawing 3d paths using Unity's animation curves.
    /// </summary>
    /// <remarks>
    /// - It uses array of three AnimationCurve objects to construct the path.
    /// - Class fields are updated in <c>AnimationPath_PathChanged</c> event
    /// handler. <c>CurvesChanged</c> event is called after animation curves
    /// inside <c>_animationCurves</c> are changed.
    /// </remarks>
    [ExecuteInEditMode]
    public class AnimationPath : GameComponent {

        #region Constants

        /// <summary>
        /// Key shortcut to enable handles mode.
        /// </summary>
        /// <remarks>
        /// Handles mode will change only while key is pressed.
        /// </remarks>
        public const KeyCode HandlesModeKey = KeyCode.J;

        /// <summary>
        /// Key shortcut to toggle movement mode.
        /// </summary>
        /// <remarks>
        /// Movement mode will change only while key is pressed.
        /// </remarks>
        public const KeyCode MoveAllKey = KeyCode.H;

        /// <summary>
        /// Animation path sampling rate used to calculate speed between two
        /// nodes.
        /// </summary>
        private const int SpeedSampling = 10;
        #endregion Constants

        #region Fields

        /// <summary>
        /// Animation Curves based on which the Animation Path is constructed.
        /// </summary>
        [SerializeField]
        private AnimationCurve[] _curves = new AnimationCurve[3];

        public int NodesNo {
            get { return _curves[0].length; }
        }

        #endregion Fields

        #region Editor

        /// <summary>
        /// If true, advenced setting in the inspector will be folded out.
        /// </summary>
        [SerializeField]
        private bool advancedSettingsFoldout = false;

        /// <summary>
        /// How many transforms should be created for 1 m of gizmo curve when
        /// exporting nodes to transforms.
        /// </summary>
        /// <remarks>Exporting is implemented in <c>Editor</c> class.</remarks>
        [SerializeField]
        private int exportSamplingFrequency = 2;

        /// <summary>
        /// Color of the gizmo curve.
        /// </summary>
        [SerializeField]
        private Color gizmoCurveColor = Color.yellow;

        /// <summary>
        /// How many points should be drawn for one meter of a gizmo curve.
        /// </summary>
        // TODO Rename to curveSamplingFrequency;
        [SerializeField]
        private int gizmoCurveSamplingFrequency = 20;

        /// <summary>
        /// If "Move All" mode is enabled.
        /// </summary>
        [SerializeField]
        private bool moveAllMode = false;

        [SerializeField]
        private bool sceneControls = true;

        /// <summary>
        /// Styles for multiple GUI elements.
        /// </summary>
        [SerializeField]
        private GUISkin skin;

#pragma warning disable 0414

        /// <summary>
        /// If enabled, on-scene handles will be use to change node's in/out
        /// tangents.
        /// </summary>
        [SerializeField]
        private bool tangentMode = false;

        /// <summary>
        /// Tangent weight used by <c>AnimationCurve.SmoothNodesTangents()</c>.
        /// </summary>
        [SerializeField]
        private float tangentWeight;
#pragma warning restore 0414

        #endregion Editor

        #region PUBLIC PROPERTIES

        public bool MoveAllMode {
            get { return moveAllMode; }
            set { moveAllMode = value; }
        }

        public bool SceneControls {
            get { return sceneControls; }
            set { sceneControls = value; }
        }

        public GUISkin Skin {
            get { return skin; }
        }
        public bool TangentMode {
            get { return tangentMode; }
            set { tangentMode = value; }
        }
        #endregion PUBLIC PROPERTIES

        #region Unity Messages

        private void Awake() {
            // Load default skin.
            skin = Resources.Load("GUISkin/default") as GUISkin;
        }

        private void OnDrawGizmosSelected() {
            DrawGizmoCurve();
        }

        private void OnEnable() {
            // Initialize _curves field.
            if (_curves[0] == null) {
                InitializeCurves();
            }
        }

        private void OnValidate() {
            // Sampling frequency inspector option cannot be less than 2.
            if (gizmoCurveSamplingFrequency <= 2) {
                gizmoCurveSamplingFrequency = 2;
            }
        }

        #endregion Unity Messages

        #region Public Methods

        // TODO Move to Editor class.
        public void AddNodeAuto(int nodeIndex) {
            // If this node is the last one in the path..
            if (nodeIndex == NodesNo - 1) {
                AddNodeEnd(nodeIndex);
            }
            // Any other node than the last one.
            else {
                AddNodeBetween(nodeIndex);
            }
        }

        public void ChangeNodeTangents(int index, Vector3 inOutTangent) {
            ChangePointTangents(index, inOutTangent);
        }

        public void ChangeNodeTimestamp(
                            int keyIndex,
                            float newTimestamp) {

            ChangePointTimestamp(keyIndex, newTimestamp);
        }

        public void DistributeNodeSpeedValues() {
            float pathLength = CalculatePathCurvedLength(
                gizmoCurveSamplingFrequency);

            // Calculate time for one meter of curve length.
            float timeForMeter = 1 / pathLength;

            // Helper variable.
            float prevTimestamp = 0;

            for (var i = 1; i < NodesNo - 1; i++) {
                // Calculate section curved length.
                float sectionLength = CalculateSectionCurvedLength(
                    i - 1,
                    i,
                    gizmoCurveSamplingFrequency);

                // Calculate time interval.
                float sectionTimeInterval = sectionLength * timeForMeter;

                // Calculate new timestamp.
                float newTimestamp = prevTimestamp + sectionTimeInterval;

                // Update previous timestamp.
                prevTimestamp = newTimestamp;

                // Add timestamp to the list.
                ChangePointTimestamp(i, newTimestamp);
            }
        }

        /// <summary>
        /// Export Animation Path nodes as transforms.
        /// </summary>
        /// <param name="exportSampling">
        /// Amount of result transforms for one meter of Animation Path.
        /// </param>
        // TODO Move to Editor class.
        public void ExportNodes(int exportSampling) {
            // Points to be exported.
            List<Vector3> points;

            // If exportSampling arg. is zero then export one transform for
            // each Animation Path node.
            if (exportSampling == 0) {
                // Initialize points.
                points = new List<Vector3>(NodesNo);

                // For each node in the path..
                for (int i = 0; i < NodesNo; i++) {
                    // Get it 3d position.
                    points[i] = GetNodePosition(i);
                }
            }
            // exportSampling not zero..
            else {
                // Initialize points array with nodes to export.
                points = SamplePathForPoints(exportSampling);
            }

            // Create parent GO.
            GameObject exportedPath = new GameObject("exported_path");

            // Create child GOs.
            for (int i = 0; i < points.Count; i++) {
                // Create child GO.
                GameObject node = new GameObject("Node " + i);

                // Move node under the path GO.
                node.transform.parent = exportedPath.transform;

                // Assign node local position.
                node.transform.localPosition = points[i];
            }
        }

        public Vector3 GetNodePosition(int nodeIndex) {
            return GetVectorAtKey(nodeIndex);
        }

        public Vector3[] GetNodePositions() {
            Vector3[] result = new Vector3[NodesNo];

            for (int i = 0; i < NodesNo; i++) {
                // Get node 3d position.
                result[i] = GetVectorAtKey(i);
            }

            return result;
        }

        public float[] GetNodeTimestamps() {
            // Output array.
            float[] result = new float[NodesNo];

            // For each key..
            for (int i = 0; i < NodesNo; i++) {
                // Get key time.
                result[i] = GetTimeAtKey(i);
            }

            return result;
        }

        /// <summary>
        /// Update speed value for each node.
        /// </summary>
        /// <param name="curves">
        /// Animation curves based on which the speed values will be
        /// calculated.
        /// </param>
        public float[] GetSpeedValues() {
            // Result array.
            float[] result = new float[NodesNo];

            // For each node (except the first one)..
            for (int i = 1; i < NodesNo; i++) {
                // Calculate speed for section between two nodes.
                //result[i] = nodes[i].Speed;
                result[i] = CalculateNodeSpeed(
                    SpeedSampling,
                    i - 1,
                    i);
            }

            return result;
        }

        public void MoveAllNodes(Vector3 moveDelta) {
            // For each node..
            for (int i = 0; i < NodesNo; i++) {
                // Old node position.
                Vector3 oldPosition = GetNodePosition(i);
                // New node position.
                Vector3 newPosition = oldPosition + moveDelta;
                // Update node positions.
                MovePoint(i, newPosition);
            }
        }

        public void MoveNodeToPosition(int nodeIndex, Vector3 position) {
            MovePoint(nodeIndex, position);
        }
        public void RemoveNode(int nodeIndex) {
            RemovePoint(nodeIndex);
        }

        /// <summary>
        /// Remove all keys in animation curves and create new, default ones.
        /// </summary>
        // TODO Move to Editor class and remove param.
        public void ResetPath(Vector3 worldPoint) {
            // Number of nodes to remove.
            int noOfNodesToRemove = NodesNo;

            // Remove all nodes.
            for (var i = 0; i < noOfNodesToRemove; i++) {
                // NOTE After each removal, next node gets index 0.
                RemovePoint(0);
            }

            // Calculate end point.
            Vector3 endPoint = worldPoint + new Vector3(1, 1, 1);

            // Add beginning and end points.
            AddNewPoint(0, worldPoint);
            AddNewPoint(1, endPoint);
        }

        /// <summary>
        /// Set tangent mode for a single node to linear.
        /// </summary>
        /// <param name="keyIndex">Node index.</param>
        public void SetNodeLinear(int keyIndex) {
            SetPointLinear(keyIndex);
        }

        /// <summary>
        /// Set all nodes' tangent mode to linear.
        /// </summary>
        public void SetNodesLinear() {
            for (int i = 0; i < 3; i++) {
                SetCurveLinear(_curves[i]);
            }
        }

        /// <summary>
        /// Smooth all tangents in the Animation Curves.
        /// </summary>
        /// <param name="weight">Weight to be applied to the tangents.</param>
        public void SmoothNodesTangents(float weight = 0) {
            // For each key..
            for (int j = 0; j < NodesNo; j++) {
                // Smooth in and out tangents.
                SmoothPointTangents(j, weight);
            }
        }

        public void SmoothNodeTangents(int nodeIndex, float tangentWeigth) {
            SmoothPointTangents(nodeIndex, tangentWeigth);
        }

        #endregion Public Methods

        #region Private Methods

        public void DrawGizmoCurve() {
            List<Vector3> points = SamplePathForPoints(gizmoCurveSamplingFrequency);

            if (points.Count < 3) return;

            // Draw curve.
            for (int i = 0; i < points.Count - 1; i++) {
                Gizmos.color = gizmoCurveColor;
                Gizmos.DrawLine(points[i], points[i + 1]);
            }
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

        private void AddNewPoint(float timestamp, Vector3 position) {
            _curves[0].AddKey(timestamp, position.x);
            _curves[1].AddKey(timestamp, position.y);
            _curves[2].AddKey(timestamp, position.z);
        }

        // TODO Should receive two node indexes.
        private void AddNodeBetween(int nodeIndex) {
            // Timestamp of node on which was taken action.
            float currentKeyTime = GetTimeAtKey(nodeIndex);
            // Get timestamp of the next node.
            float nextKeyTime = GetTimeAtKey(nodeIndex + 1);

            // Calculate timestamps for new key. It'll be placed exactly
            // between the two nodes.
            float newKeyTime =
                currentKeyTime +
                ((nextKeyTime - currentKeyTime) / 2);

            // Add node to the animation curves.
            for (int j = 0; j < 3; j++) {
                float newKeyValue = _curves[j].Evaluate(newKeyTime);
                _curves[j].AddKey(newKeyTime, newKeyValue);
            }
        }

        // TODO Move to Editor class.
        private void AddNodeEnd(int nodeIndex) {
            // Calculate position for the new node.
            var newNodePosition = CalculateNewEndNodePosition(nodeIndex);

            // Decrease current last node timestamp to make place for the new
            // node.
            DecreaseNodeTimestampByHalfInterval(nodeIndex);

            // Add new node to animation curves.
            AddNewPoint(1, newNodePosition);
        }

        // TODO Move to Editor class.
        private Vector3 CalculateNewEndNodePosition(int nodeIndex) {
            // Get positions of all nodes.
            Vector3[] nodePositions = GetNodePositions();

            // Timestamp of node on which was taken action.
            float currentKeyTime = GetTimeAtKey(nodeIndex);

            // Timestamp of some point behind and close to the node.
            float refTime = currentKeyTime - 0.001f;

            // Create Vector3 for the reference point.
            Vector3 refPoint = GetVectorAtTime(refTime);

            // Calculate direction from ref. point to current node.
            Vector3 dir = nodePositions[nodeIndex] - refPoint;

            // Normalize direction.
            dir.Normalize();

            // Create vector with new node's position.
            Vector3 newNodePosition = nodePositions[nodeIndex] + dir * 0.5f;

            return newNodePosition;
        }

        /// <summary>
        /// Calculate speed between two nodes.
        /// </summary>
        /// <param name="samplingRate">
        /// Amount of samples taken between two nodes to determine the line's
        /// shape.
        /// </param>
        /// <param name="leftNodeIdx">
        /// Index of a node with smaller timestamp.
        /// </param>
        /// <param name="rightNodeIdx">
        /// Index of a node with bigger timestamp.
        /// </param>
        /// <returns>
        /// Speed of an object that would travel between given two nodes in
        /// time that is determined by their timestamps.
        /// </returns>
        private float CalculateNodeSpeed(
            int samplingRate,
            int leftNodeIdx,
            int rightNodeIdx) {

            // Time between two nodes.
            float sectionTime = GetTimeAtKey(rightNodeIdx)
                - GetTimeAtKey(leftNodeIdx);

            // Overall distance for node pair.
            float sectionDistance = 0;

            // A 3d point on the path.
            Vector3 point = new Vector3();
            Vector3 prevPoint = new Vector3();

            // For each point between two nodes..
            // + 1 because it must include also the last point.
            for (int i = 0; i < samplingRate + 1; i++) {
                float sampleTime = sectionTime / samplingRate;

                // Calculate time for point.
                float pointTimestamp = GetTimeAtKey(leftNodeIdx)
                    + i * sampleTime;

                point = GetVectorAtTime(pointTimestamp);

                // Needs at least two points to calculate distance.
                if (i != 0) {
                    // Calculate distance between two nodes and increase total.
                    sectionDistance +=
                        Vector3.Distance(prevPoint, point);
                }

                // Update previous point value.
                prevPoint = point;
            }

            // Calculate section speed.
            float sectionSpeed = sectionDistance / sectionTime;

            return sectionSpeed;
        }

        private float CalculatePathCurvedLength(int samplingFrequency) {
            float pathLength = 0;

            for (var i = 0; i < NodesNo - 1; i++) {
                pathLength += CalculateSectionCurvedLength(
                    i,
                    i + 1,
                    gizmoCurveSamplingFrequency);
            }

            return pathLength;
        }

        /// <summary>
        /// Calculate path length using shortest path between each node.
        /// </summary>
        /// <param name="curves">
        /// Array of three animation curves, each of them represents one of
        /// three 3d axis. \return Length of the curve in meters.
        /// </param>
        /// <returns>Path length in meters.</returns>
        private float CalculatePathLinearLength() {
            // Result distance.
            float dist = 0;

            // For each node (exclude the first one)..
            for (int i = 0; i < NodesNo - 1; i++) {
                dist += CalculateSectionLinearLength(i, i + 1);
            }

            return dist;
        }

        private float CalculateSectionCurvedLength(
                            int firstNodeIndex,
                            int secondNodeIndex,
                            int samplingFrequency) {

            // Sampled points.
            List<Vector3> points;

            // Result path length.
            float pathLength = 0;

            points = SampleSectionForPoints(
                firstNodeIndex,
                secondNodeIndex,
                samplingFrequency);

            for (var i = 1; i < points.Count; i++) {
                pathLength += Vector3.Distance(points[i - 1], points[i]);
            }

            return pathLength;
        }

        private float CalculateSectionLinearLength(
                    int firstNodeIndex,
                    int secondNodeIndex) {

            Vector3 firstNodePosition = GetVectorAtKey(firstNodeIndex);
            Vector3 secondNodePosition = GetVectorAtKey(secondNodeIndex);

            float sectionLength =
                Vector3.Distance(firstNodePosition, secondNodePosition);

            return sectionLength;
        }

        private void ChangePointTangents(
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
        }

        private void ChangePointTimestamp(
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
        }

        /// <summary>
        /// Decrease node timestamp by half its time interval.
        /// </summary>
        /// <remarks>
        /// Node interval is a time betwenn this and previous node.
        /// </remarks>
        /// <param name="nodeIndex"></param>
        // TODO Move to Editor class.
        private void DecreaseNodeTimestampByHalfInterval(int nodeIndex) {
            // Get timestamp of the penultimate node.
            float penultimateNodeTimestamp =
                GetTimeAtKey((nodeIndex - 1));

            // Calculate time between last and penultimate nodes.
            float deltaTime = 1 - penultimateNodeTimestamp;

            // Calculate new, smaller time for the last node.
            float newLastNodeTimestamp =
                penultimateNodeTimestamp + (deltaTime * 0.5f);

            // Update point timestamp in animation curves.
            ChangePointTimestamp(
                nodeIndex,
                newLastNodeTimestamp);
        }
        private float GetTimeAtKey(int keyIndex) {
            return _curves[0].keys[keyIndex].time;
        }

        private Vector3 GetVectorAtKey(int keyIndex) {
            float x = _curves[0].keys[keyIndex].value;
            float y = _curves[1].keys[keyIndex].value;
            float z = _curves[2].keys[keyIndex].value;

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Initialize <c>_curves</c> field with empty AnimationCurve objects.
        /// </summary>
        private void InitializeCurves() {
            for (var i = 0; i < _curves.Length; i++) {
                _curves[i] = new AnimationCurve();
            }
        }

        /// <summary>
        /// Update animation curves' values for a given key with a given
        /// Vector3 value.
        /// </summary>
        /// <param name="curves">Animation curves.</param>
        /// <param name="keyIndex">Index of the key to update.</param>
        /// <param name="position">New key value.</param>
        // TODO This should accept timestamp instead of index.
        private void MovePoint(
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
        }

        private void RemovePoint(int nodeIndex) {
            // For each animation curve..
            for (int i = 0; i < 3; i++) {
                // Remove node keys.
                _curves[i].RemoveKey(nodeIndex);
            }
        }

        /// <summary>
        /// Sample Animation Path for 3d points.
        /// </summary>
        /// <param name="samplingFrequency">
        /// How many 3d points should be extracted from the path for 1 m of its
        /// length.
        /// </param>
        /// <param name="pathLength">
        /// Length of the Animation Path in meters.
        /// </param>
        /// <returns>Array of 3d points.</returns>
        private List<Vector3> SamplePathForPoints(int samplingFrequency) {
            // TODO Why initialise with nodes number?!
            List<Vector3> points = new List<Vector3>(NodesNo);

            // Call reference overload.
            SamplePathForPoints(samplingFrequency, points);

            return points;
        }

        private void SamplePathForPoints(
                    int samplingFrequency,
                    List<Vector3> points) {

            float linearPathLength = CalculatePathLinearLength();

            // Calculate amount of points to extract.
            int samplingRate = (int)(linearPathLength * samplingFrequency);

            // NOTE Cannot do any sampling if sampling rate is less than 1.
            if (samplingRate < 1) return;

            // Used to read values from animation curves.
            float time = 0;

            // Time step between each point.
            float timestep = 1f / samplingRate;

            // Clear points list.
            points.Clear();

            // Fill points array with 3d points.
            for (int i = 0; i < samplingRate + 1; i++) {
                // Calculate single point.
                Vector3 point = GetVectorAtTime(time);

                // Construct 3d point from animation curves at a given time.
                points.Add(point);

                // Time goes towards 1.
                time += timestep;
            }
        }

        private List<Vector3> SampleSectionForPoints(
            int firstNodeIndex,
            int secondNodeIndex,
            float samplingFrequency) {

            List<Vector3> points = new List<Vector3>();

            SampleSectionForPoints(
                firstNodeIndex,
                secondNodeIndex,
                samplingFrequency,
                points);

            return points;
        }

        private void SampleSectionForPoints(
            int firstNodeIndex,
            int secondNodeIndex,
            float samplingFrequency,
            List<Vector3> points) {

            float linearPathLength = CalculatePathLinearLength();

            // Throw exception when there's nothing to draw.
            if (linearPathLength == 0) {
                throw new Exception("Animation path length is 0. At least " +
                        "two keys in a curve must differ in value.");
            }

            float sectionLinearLength = CalculateSectionLinearLength(
                firstNodeIndex,
                secondNodeIndex);

            // Calculate amount of points to extract.
            int samplingRate = (int)(sectionLinearLength * samplingFrequency);

            float firstNodeTime =
                GetTimeAtKey(firstNodeIndex);
            float secondNodeTime =
                GetTimeAtKey(secondNodeIndex);

            float timeInterval = secondNodeTime - firstNodeTime;

            // Used to read values from animation curves.
            float time = firstNodeTime;

            // Time step between each point.
            float timestep = timeInterval / samplingRate;

            // Clear points list.
            points.Clear();

            // Fill points array with 3d points.
            for (int i = 0; i < samplingRate + 1; i++) {
                // Calculate single point.
                Vector3 point = GetVectorAtTime(time);

                // Construct 3d point from animation curves at a given time.
                points.Add(point);

                // Time goes towards 1.
                time += timestep;
            }
        }

        // // Find curves to update. List<AnimationCurve> curvesToUpdate = new
        // List<AnimationCurve>();
        /// <summary>
        /// Set tangent mode for all keys in one AnimationCurve to linear.
        /// </summary>
        /// <remarks>Posted by eagle555 at Unity forum.</remarks>
        /// <param name="curve">Animation curve</param>
        private void SetCurveLinear(AnimationCurve curve) {
            for (int i = 0; i < curve.keys.Length; ++i) {
                float intangent = 0;
                float outtangent = 0;
                bool intangent_set = false;
                bool outtangent_set = false;
                Vector2 point1;
                Vector2 point2;
                Vector2 deltapoint;
                Keyframe key = curve[i];

                if (i == 0) {
                    intangent = 0; intangent_set = true;
                }

                if (i == curve.keys.Length - 1) {
                    outtangent = 0; outtangent_set = true;
                }

                if (!intangent_set) {
                    point1.x = curve.keys[i - 1].time;
                    point1.y = curve.keys[i - 1].value;
                    point2.x = curve.keys[i].time;
                    point2.y = curve.keys[i].value;

                    deltapoint = point2 - point1;
                    intangent = deltapoint.y / deltapoint.x;
                }

                if (!outtangent_set) {
                    point1.x = curve.keys[i].time;
                    point1.y = curve.keys[i].value;
                    point2.x = curve.keys[i + 1].time;
                    point2.y = curve.keys[i + 1].value;

                    deltapoint = point2 - point1;
                    outtangent = deltapoint.y / deltapoint.x;
                }

                key.inTangent = intangent;
                key.outTangent = outtangent;

                curve.MoveKey(i, key);
            }
        }
        private void SetPointLinear(int nodeIndex) {
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
        }

        /// <summary>
        /// Smooth in/out tangents of a single point.
        /// </summary>
        /// <param name="nodeIndex">Point index.</param>
        /// <param name="tangentWeight">Tangent weight.</param>
        private void SmoothPointTangents(int nodeIndex, float tangentWeight) {
            // For each curve..
            for (int i = 0; i < _curves.Length; i++) {
                // Smooth tangents.
                _curves[i].SmoothTangents(nodeIndex, tangentWeight);
            }
        }
        #endregion Private Methods
    }
}