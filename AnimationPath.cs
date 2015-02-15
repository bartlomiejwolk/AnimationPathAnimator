using System;
using ATP.ReorderableList;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

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

        #region CONSTANTS

        /// <summary>
        /// How many points should be drawn for one meter of a gizmo curve.
        /// </summary>
        public const int GizmoCurveSamplingFrequency = 20;

        /// <summary>
        /// Key shortcut to enable handles mode.
        /// </summary>
        /// <remarks>
        /// Handles mode will change only while key is pressed.
        /// </remarks>
        // TODO Move to Editor class.
        public const KeyCode TangentModeKey = KeyCode.J;

        /// <summary>
        /// Key shortcut to toggle movement mode.
        /// </summary>
        /// <remarks>
        /// Movement mode will change only while key is pressed.
        /// </remarks>
        // TODO Move to Editor class.
        public const KeyCode MoveAllKey = KeyCode.H;
        #endregion Constants

        #region FIELDS

        public event EventHandler PathChanged;
        public event EventHandler PathReset;
        public event EventHandler NodeTimeChanged;
        public event EventHandler NodeAdded;
        public event EventHandler NodeRemoved;

        /// <summary>
        /// Animation curves that make the animation path.
        /// </summary>
        [SerializeField]
        private AnimationPathCurves animationCurves;

        #endregion Fields

        #region EDITOR

        /// <summary>
        /// If true, advenced setting in the inspector will be folded out.
        /// </summary>
        [SerializeField]
#pragma warning disable 414
        private bool advancedSettingsFoldout;

#pragma warning restore 414

        /// <summary>
        /// How many transforms should be created for 1 m of gizmo curve when
        /// exporting nodes to transforms.
        /// </summary>
        /// <remarks>Exporting is implemented in <c>Editor</c> class.</remarks>
        [SerializeField]
#pragma warning disable 414
        private int exportSamplingFrequency = 5;

#pragma warning restore 414

        /// <summary>
        /// Color of the gizmo curve.
        /// </summary>
        [SerializeField]
        private Color gizmoCurveColor = Color.yellow;

        /// <summary>
        /// If "Move All" mode is enabled.
        /// </summary>
        //[SerializeField]
        //private bool moveAllMode;

        //[SerializeField]
        //private bool sceneControls = true;

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
        //[SerializeField]
        //private bool tangentMode;

        [SerializeField] private AnimationPathHandlesMode handlesMode =
            AnimationPathHandlesMode.MoveSingle;

        [SerializeField] private AnimationPathTangentMode tangentMode =
            AnimationPathTangentMode.Smooth;

        public const KeyCode MoveSingleModeKey = KeyCode.G;

#pragma warning restore 0414

        #endregion Editor

        #region PUBLIC PROPERTIES

        public AnimationPathCurves AnimationCurves {
            get { return animationCurves; }
        }

        /// <summary>
        /// Color of the gizmo curve.
        /// </summary>
        public Color GizmoCurveColor {
            get { return gizmoCurveColor; }
            set { gizmoCurveColor = value; }
        }

        //public bool MoveAllMode {
        //    get { return moveAllMode; }
        //    set { moveAllMode = value; }
        //}

        /// <summary>
        /// Number of keys in an animation curve.
        /// </summary>
        public int NodesNo {
            get { return animationCurves.KeysNo; }
        }

        //public bool SceneControls {
        //    get { return sceneControls; }
        //    set { sceneControls = value; }
        //}

        public GUISkin Skin {
            get { return skin; }
        }

        //public bool TangentMode {
        //    get { return tangentMode; }
        //    set { tangentMode = value; }
        //}

        public bool IsInitialized {
            get { return (animationCurves.KeysNo >= 2); }
        }

        public AnimationPathTangentMode TangentMode {
            get { return tangentMode; }
        }

        #endregion PUBLIC PROPERTIES

        #region UNITY MESSAGES

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Awake() {
            // Load default skin.
            skin = Resources.Load("GUISkin/default") as GUISkin;
        }

        private void OnDestroy() {
        }
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnDrawGizmosSelected() {
            DrawGizmoCurve();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnEnable() {
            // Instantiate animationCurves.
            if (animationCurves == null) {
                animationCurves =
                    ScriptableObject.CreateInstance<AnimationPathCurves>();
            }

        }

        #endregion Unity Messages
        #region EVENT INVOCATORS
        protected virtual void OnNodeRemoved() {
            var handler = NodeRemoved;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnNodeAdded() {
            var handler = NodeAdded;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnNodeTimeChanged() {
            var handler = NodeTimeChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public virtual void OnPathReset() {
            // Change handle mode to MoveAll.
            handlesMode = AnimationPathHandlesMode.MoveAll;

            // Call handler methods.
            var handler = PathReset;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public virtual void OnPathChanged() {
            var handler = PathChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region PUBLIC METHODS
        public float CalculatePathCurvedLength(int samplingFrequency) {
            float pathLength = 0;

            for (var i = 0; i < NodesNo - 1; i++) {
                pathLength += CalculateSectionCurvedLength(
                    i,
                    i + 1,
                    GizmoCurveSamplingFrequency);
            }

            return pathLength;
        }

        /// <summary>
        /// Calculate path length as if all nodes were in linear mode.
        /// </summary>
        /// <returns>Path length.</returns>
        public float CalculatePathLinearLength() {
            // Result distance.
            float dist = 0;

            // For each node (exclude the first one)..
            for (var i = 0; i < animationCurves.KeysNo - 1; i++) {
                dist += CalculateSectionLinearLength(i, i + 1);
            }

            return dist;
        }

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

            for (var i = 1; i < points.Count; i++) {
                pathLength += Vector3.Distance(points[i - 1], points[i]);
            }

            return pathLength;
        }

        public float CalculateSectionLinearLength(
            int firstNodeIndex,
            int secondNodeIndex) {

            var firstNodePosition =
                animationCurves.GetVectorAtKey(firstNodeIndex);
            var secondNodePosition =
                animationCurves.GetVectorAtKey(secondNodeIndex);

            var sectionLength =
                Vector3.Distance(firstNodePosition, secondNodePosition);

            return sectionLength;
        }

        public void SetNodeTangents(int index, Vector3 inOutTangent) {
            animationCurves.ChangePointTangents(index, inOutTangent);
        }

        public void ChangeNodeTimestamp(
                            int keyIndex,
                            float newTimestamp) {

            animationCurves.ChangePointTimestamp(keyIndex, newTimestamp);
            OnNodeTimeChanged();
        }

        public void CreateNode(float timestamp, Vector3 position) {
            animationCurves.CreateNewPoint(timestamp, position);
            OnNodeAdded();
        }

        public void CreateNodeAtTime(float timestamp) {
            animationCurves.AddNodeAtTime(timestamp);
            OnNodeAdded();
        }
        public void DistributeTimestamps() {
            // Calculate path curved length.
            var pathLength = CalculatePathCurvedLength(
                GizmoCurveSamplingFrequency);
            // Calculate time for one meter of curve length.
            var timeForMeter = 1 / pathLength;
            // Helper variable.
            float prevTimestamp = 0;

            // For each node calculate and apply new timestamp.
            for (var i = 1; i < NodesNo - 1; i++) {
                // Calculate section curved length.
                var sectionLength = CalculateSectionCurvedLength(
                    i - 1,
                    i,
                    GizmoCurveSamplingFrequency);
                // Calculate time interval.
                var sectionTimeInterval = sectionLength * timeForMeter;
                // Calculate new timestamp.
                var newTimestamp = prevTimestamp + sectionTimeInterval;
                // Update previous timestamp.
                prevTimestamp = newTimestamp;

                // Update node timestamp.
                ChangeNodeTimestamp(i, newTimestamp);
            }
        }

        public Vector3 GetNodePosition(int nodeIndex) {
            return animationCurves.GetVectorAtKey(nodeIndex);
        }

        public Vector3[] GetNodePositions() {
            var result = new Vector3[NodesNo];

            for (var i = 0; i < NodesNo; i++) {
                // Get node 3d position.
                result[i] = animationCurves.GetVectorAtKey(i);
            }

            return result;
        }
        public float GetNodeTimestamp(int nodeIndex) {
            return animationCurves.GetTimeAtKey(nodeIndex);
        }

        public float[] GetNodeTimestamps() {
            // Output array.
            var result = new float[NodesNo];

            // For each key..
            for (var i = 0; i < NodesNo; i++) {
                // Get key time.
                result[i] = animationCurves.GetTimeAtKey(i);
            }

            return result;
        }

        public Vector3 GetVectorAtTime(float timestamp) {
            return animationCurves.GetVectorAtTime(timestamp);
        }

        // TODO Rename to MoveNodesByDelta().
        public void MoveAllNodes(Vector3 moveDelta) {
            // For each node..
            for (var i = 0; i < NodesNo; i++) {
                // Old node position.
                var oldPosition = GetNodePosition(i);
                // New node position.
                var newPosition = oldPosition + moveDelta;
                // Update node positions.
                animationCurves.MovePointToPosition(i, newPosition);
            }
        }

        public void MoveNodeToPosition(int nodeIndex, Vector3 position) {
            animationCurves.MovePointToPosition(nodeIndex, position);
        }

        public void RemoveNode(int nodeIndex) {
            animationCurves.RemovePoint(nodeIndex);
            OnNodeRemoved();
        }

        /// <summary>
        /// Extract 3d points from path.
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

            // TODO Use curved path length.
            var linearPathLength = CalculatePathLinearLength();

            // Calculate amount of points to extract.
            var samplingRate = (int)(linearPathLength * samplingFrequency);

            // NOTE Cannot do any sampling if sampling rate is less than 1.
            if (samplingRate < 1) return;

            // Used to read values from animation curves.
            float time = 0;

            // Time step between each point.
            var timestep = 1f / samplingRate;

            // Clear points list.
            points.Clear();

            // Fill points array with 3d points.
            for (var i = 0; i < samplingRate + 1; i++) {
                // Calculate single point.
                var point = animationCurves.GetVectorAtTime(time);

                // Construct 3d point from animation curves at a given time.
                points.Add(point);

                // Time goes towards 1.
                time += timestep;
            }
        }

        public List<Vector3> SampleSectionForPoints(
                    int firstNodeIndex,
                    int secondNodeIndex,
                    float samplingFrequency) {

            var points = new List<Vector3>();

            SampleSectionForPoints(
                firstNodeIndex,
                secondNodeIndex,
                samplingFrequency,
                ref points);

            return points;
        }

        public void SampleSectionForPoints(
                    int firstNodeIndex,
                    int secondNodeIndex,
                    float samplingFrequency,
                    ref List<Vector3> points) {

            // Throw exception when there's nothing to draw.
            // TODO Use curved length.
            //if (Math.Abs(linearPathLength) < 0.01f) {
            //    throw new Exception("Path is too short.");
            //}

            var sectionLinearLength = CalculateSectionLinearLength(
                firstNodeIndex,
                secondNodeIndex);

            // Calculate amount of points to extract.
            var samplingRate = (int)(sectionLinearLength * samplingFrequency);

            var firstNodeTime =
                animationCurves.GetTimeAtKey(firstNodeIndex);
            var secondNodeTime =
                animationCurves.GetTimeAtKey(secondNodeIndex);

            var timeInterval = secondNodeTime - firstNodeTime;

            // Used to read values from animation curves.
            var time = firstNodeTime;

            // Time step between each point.
            var timestep = timeInterval / samplingRate;

            // Clear points list.
            points.Clear();

            // Fill points array with 3d points.
            for (var i = 0; i < samplingRate + 1; i++) {
                // Calculate single point.
                var point = animationCurves.GetVectorAtTime(time);

                // Construct 3d point from animation curves at a given time.
                points.Add(point);

                // Time goes towards 1.
                time += timestep;
            }
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// http:
        ///       //forum.unity3d.com/threads/how-to-set-an-animation-curve-to-linear-through-scripting.151683/#post-1121021
        /// </remarks>
        /// <param name="curve"></param>
        public void SetCurveLinear(AnimationCurve curve) {
            for (var i = 0; i < curve.keys.Length; ++i) {
                float intangent = 0;
                float outtangent = 0;
                var inTangentSet = false;
                var outTangentSet = false;
                Vector2 point1;
                Vector2 point2;
                Vector2 deltapoint;
                var key = curve[i];

                if (i == 0) {
                    intangent = 0; inTangentSet = true;
                }

                if (i == curve.keys.Length - 1) {
                    outtangent = 0; outTangentSet = true;
                }

                if (!inTangentSet) {
                    point1.x = curve.keys[i - 1].time;
                    point1.y = curve.keys[i - 1].value;
                    point2.x = curve.keys[i].time;
                    point2.y = curve.keys[i].value;

                    deltapoint = point2 - point1;
                    intangent = deltapoint.y / deltapoint.x;
                }

                if (!outTangentSet) {
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

        public void SetNodesLinear() {
            for (var i = 0; i < 3; i++) {
                SetCurveLinear(animationCurves[i]);
            }
        }

        /// <summary>
        /// Smooth all tangents in the Animation Curves.
        /// </summary>
        /// <param name="weight">Weight to be applied to the tangents.</param>
        // TODO Rename to SmoothAllNodeTangents().
        public void SmoothAllNodeTangents(float weight = 0) {
            // For each key..
            for (var j = 0; j < NodesNo; j++) {
                // Smooth in and out tangents.
                animationCurves.SmoothPointTangents(j);
            }
        }

        public void SmoothSingleNodeTangents(int nodeIndex) {
            animationCurves.SmoothPointTangents(nodeIndex);
        }

        #endregion Public Methods

        #region PRIVATE METHODS



        private void DrawGizmoCurve() {
            var points = SamplePathForPoints(GizmoCurveSamplingFrequency);

            if (points.Count < 3) return;

            // Draw curve.
            for (var i = 0; i < points.Count - 1; i++) {
                Gizmos.color = gizmoCurveColor;
                Gizmos.DrawLine(points[i], points[i + 1]);
            }
        }

        #endregion PRIVATE METHODS
    }
}