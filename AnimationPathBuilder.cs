using System;
using System.Diagnostics.CodeAnalysis;
using ATP.ReorderableList;
using UnityEngine;

namespace ATP.AnimationPathTools {
    /// <summary>
    ///     Allows creating and drawing 3d paths using Unity's animation curves.
    /// </summary>
    [ExecuteInEditMode]
    public class AnimationPathBuilder : GameComponent {
        #region CONSTANTS

        /// <summary>
        ///     How many points should be drawn for one meter of a gizmo curve.
        /// </summary>
        public const int GizmoCurveSamplingFrequency = 20;

        #endregion CONSTANTS
        #region FIELDS

        //public event EventHandler NodeAdded;




        public event EventHandler PathReset;

        #endregion FIELDS

        #region EDITOR

        /// <summary>
        ///     If true, advenced setting in the inspector will be folded out.
        /// </summary>
        [SerializeField]
#pragma warning disable 414
            private bool advancedSettingsFoldout;

#pragma warning restore 414

        /// <summary>
        ///     How many transforms should be created for 1 m of gizmo curve when
        ///     exporting nodes to transforms.
        /// </summary>
        /// <remarks>Exporting is implemented in <c>Editor</c> class.</remarks>
        [SerializeField]
#pragma warning disable 414
            private int exportSamplingFrequency = 5;

#pragma warning restore 414

        /// <summary>
        ///     Color of the gizmo curve.
        /// </summary>
        [SerializeField] private Color gizmoCurveColor = Color.yellow;

        [SerializeField] private AnimationPathBuilderHandleMode handleMode =
            AnimationPathBuilderHandleMode.MoveSingle;

        [SerializeField] private PathData pathData;

        /// <summary>
        ///     Styles for multiple GUI elements.
        /// </summary>
        [SerializeField] private GUISkin skin;

#pragma warning disable 0414
        [SerializeField] private AnimationPathBuilderTangentMode tangentMode =
            AnimationPathBuilderTangentMode.Smooth;
#pragma warning restore 0414

        #endregion EDITOR

        #region PROPERTIES

        /// <summary>
        ///     Color of the gizmo curve.
        /// </summary>
        public Color GizmoCurveColor {
            get { return gizmoCurveColor; }
            set { gizmoCurveColor = value; }
        }

        public AnimationPathBuilderHandleMode HandleMode {
            get { return handleMode; }
            set { handleMode = value; }
        }

        /// <summary>
        ///     Number of keys in an animation curve.
        /// </summary>
        public int NodesNo {
            get { return pathData.AnimationPathKeysNo; }
        }

        public PathData PathData {
            get { return pathData; }
            set { pathData = value; }
        }

        public GUISkin Skin {
            get { return skin; }
        }

        public AnimationPathBuilderTangentMode TangentMode {
            get { return tangentMode; }
            set { tangentMode = value; }
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
            PathReset += this_PathReset;
        }

        private void this_PathReset(object sender, EventArgs eventArgs) {
            // Change handle mode to MoveAll.
            handleMode = AnimationPathBuilderHandleMode.MoveAll;
        }

        #endregion UNITY MESSAGES

        #region EVENT INVOCATORS

        public virtual void this_PathReset() {
            var handler = PathReset;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        //protected virtual void OnNodeAdded() {
        //    var handler = NodeAdded;
        //    if (handler != null) handler(this, EventArgs.Empty);
        //}

        #endregion EVENT INVOCATORS

        #region METHODS

        //public void ChangeNodeTimestamp(
        //    int keyIndex,
        //    float newTimestamp) {
        //    pathData.AnimatedObjectPath.ChangeNodeTimestamp(keyIndex, newTimestamp);
        //    OnNodeTimeChanged();
        //}

        //public void CreateNewNode(float timestamp, Vector3 position) {
        //    pathData.AnimatedObjectPath.CreateNewNode(timestamp, position);
        //    OnNodeAdded();
        //}

        //public void CreateNodeAtTime(float timestamp) {
        //    pathData.AnimatedObjectPath.AddNodeAtTime(timestamp);
        //    OnNodeAdded();
        //}

        public Vector3[] GetNodeGlobalPositions() {
            var nodePositions = PathData.GetNodePositions();

            for (var i = 0; i < nodePositions.Length; i++) {
                // Convert each position to global coordinate.
                nodePositions[i] = transform.TransformPoint(nodePositions[i]);
            }

            return nodePositions;
        }


        private void DrawGizmoCurve() {
            // Return if path asset is not assigned.
            if (pathData == null) return;

            // Get transform component.
            var transform = GetComponent<Transform>();

            // Get path points.
            var points = pathData.SampleAnimationPathForPoints(
                GizmoCurveSamplingFrequency);

            // Convert points to global coordinates.
            var globalPoints = new Vector3[points.Count];
            for (var i = 0; i < points.Count; i++) {
                globalPoints[i] = transform.TransformPoint(points[i]);
            }

            // There must be at least 3 points to draw a line.
            if (points.Count < 3) return;

            Gizmos.color = gizmoCurveColor;

            // Draw curve.
            for (var i = 0; i < points.Count - 1; i++) {
                Gizmos.DrawLine(globalPoints[i], globalPoints[i + 1]);
            }
        }

        #endregion PRIVATE METHODS
    }
}