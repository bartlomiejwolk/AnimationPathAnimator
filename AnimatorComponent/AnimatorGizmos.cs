using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathAnimator.APAnimatorComponent {

    public sealed class AnimatorGizmos {

        #region FIELDS

        [SerializeField]
        private PathAnimatorSettings settings;
        public PathAnimatorSettings Settings {
            get { return settings; }
            set { settings = value; }
        }
        #endregion

        #region METHODS

        public AnimatorGizmos(PathAnimatorSettings settings) {
            this.settings = settings;
        }

        public void DrawAnimationCurve(PathData pathData,
                    Transform transform) {

            // Return if path asset is not assigned.
            if (pathData == null) return;

            // Get path points.
            var points = pathData.SampleAnimationPathForPoints(
                Settings.GizmoCurveSamplingFrequency);

            // Convert points to global coordinates.
            var globalPoints = new Vector3[points.Count];
            for (var i = 0; i < points.Count; i++) {
                globalPoints[i] = transform.TransformPoint(points[i]);
            }

            // There must be at least 3 points to draw a line.
            if (points.Count < 3) return;

            Gizmos.color = Settings.GizmoCurveColor;

            // Draw curve.
            for (var i = 0; i < points.Count - 1; i++) {
                Gizmos.DrawLine(globalPoints[i], globalPoints[i + 1]);
            }
        }


        // TODO Add "Icon" to method name.
        public void DrawCurrentRotationPointGizmo(PathData pathData,
            Transform transform, float animationTimeRatio) {

            // Node path node timestamps.
            var nodeTimestamps = pathData.GetPathTimestamps();

            // Return if current animation time is the same as any node time.
            if (nodeTimestamps.Any(
                nodeTimestamp =>
                    Math.Abs(nodeTimestamp - animationTimeRatio)
                    < Settings.FloatPrecision)) {
                return;
            }

            // Get rotation point position.
            var localRotationPointPosition =
                pathData.GetRotationAtTime(animationTimeRatio);
            var globalRotationPointPosition =
                transform.TransformPoint(localRotationPointPosition);

            //Draw rotation point gizmo.
            Gizmos.DrawIcon(globalRotationPointPosition,
                Settings.GizmosSubfolder + Settings.CurrentRotationPointGizmoIcon,
                false);
        }

        public void DrawForwardPointIcon(Vector3 forwardPointPosition) {
            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                forwardPointPosition,
                Settings.GizmosSubfolder + Settings.ForwardPointIcon,
                false);
        }

        public void DrawRotationPathCurve(PathData pathData,
            Transform transform) {

            var localPointPositions = pathData.SampleRotationPathForPoints(
                Settings.RotationCurveSampling);

            var globalPointPositions =
                new Vector3[localPointPositions.Count];

            for (var i = 0; i < localPointPositions.Count; i++) {
                globalPointPositions[i] =
                    transform.TransformPoint(localPointPositions[i]);
            }
            if (globalPointPositions.Length < 2) return;

            Gizmos.color = Settings.RotationCurveColor;

            // Draw curve.
            for (var i = 0; i < globalPointPositions.Length - 1; i++) {
                Gizmos.DrawLine(
                    globalPointPositions[i], globalPointPositions[i + 1]);
            }
        }

        // TODO Add "Icons" to method name end.
        public void DrawRotationPointGizmos(PathData pathData,
            Transform transform, float animationTimeRatio) {

            var localRotPointPositions =
                pathData.GetRotationPointPositions();

            var globalRotPointPositions =
                new Vector3[localRotPointPositions.Length];

            for (int i = 0; i < localRotPointPositions.Length; i++) {
                globalRotPointPositions[i] =
                    transform.TransformPoint(localRotPointPositions[i]);
            }

            // Path node timestamps.
            var nodeTimestamps = pathData.GetPathTimestamps();

            for (var i = 0; i < globalRotPointPositions.Length; i++) {
                // Return if current animation time is the same as any node
                // time.
                if (Math.Abs(nodeTimestamps[i] - animationTimeRatio) <
                    Settings.FloatPrecision) {
                    continue;
                }

                //Draw rotation point gizmo.
                Gizmos.DrawIcon(
                globalRotPointPositions[i],
                Settings.GizmosSubfolder + Settings.RotationPointGizmoIcon,
                false);
            }
        }

        public void DrawTargetIcon(Vector3 targetPosition) {

            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                targetPosition,
                Settings.GizmosSubfolder + Settings.TargetGizmoIcon,
                false);
        }

        //public void Init(PathAnimatorSettings settings) {
        //    Settings = settings;
        //}

        #endregion
    }

}