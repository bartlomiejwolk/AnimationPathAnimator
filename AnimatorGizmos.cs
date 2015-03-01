using System;
using System.Linq;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace ATP.AnimationPathTools {

    public class AnimatorGizmos : ScriptableObject {
        #region FIELDS

        [SerializeField]
        private Color rotationCurveColor = Color.gray;

        #endregion

        #region PROPERIES

        public virtual float FloatPrecision {
            get { return 0.001f; }
        }

        protected virtual string RotationPointGizmoIcon {
            get { return "rec_16x16"; }
        }

        protected virtual string TargetGizmoIcon {
            get { return "target_22x22-blue"; }
        }

        protected virtual string CurrentRotationPointGizmoIcon {
            get { return "rec_16x16-yellow"; }
        }

        protected virtual string ForwardPointIcon {
            get { return "target_22x22-pink"; }
        }

        public Color RotationCurveColor {
            get { return rotationCurveColor; }
            set { rotationCurveColor = value; }
        }

        protected virtual int RotationCurveSampling {
            get { return 20; }
        }

        #endregion

        #region METHODS

        public void DrawCurrentRotationPointGizmo(PathData pathData,
            Transform transform, float animationTimeRatio) {

            // Node path node timestamps.
            var nodeTimestamps = pathData.GetPathTimestamps();

            // Return if current animation time is the same as any node time.
            if (nodeTimestamps.Any(
                nodeTimestamp =>
                    Math.Abs(nodeTimestamp - animationTimeRatio)
                    < FloatPrecision)) {
                return;
            }

            // Get rotation point position.
            var localRotationPointPosition =
                pathData.GetRotationAtTime(animationTimeRatio);
            var globalRotationPointPosition =
                transform.TransformPoint(localRotationPointPosition);

            //Draw rotation point gizmo.
            Gizmos.DrawIcon(globalRotationPointPosition,
                CurrentRotationPointGizmoIcon,
                false);
        }

        public void DrawForwardPointIcon(Vector3 forwardPointPosition) {
            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                forwardPointPosition,
                ForwardPointIcon,
                false);
        }

        public void DrawRotationPathCurve(PathData pathData,
            Transform transform) {

            var localPointPositions = pathData.SampleRotationPathForPoints(
                RotationCurveSampling);

            var globalPointPositions =
                new Vector3[localPointPositions.Count];

            for (var i = 0; i < localPointPositions.Count; i++) {
                globalPointPositions[i] =
                    transform.TransformPoint(localPointPositions[i]);
            }
            if (globalPointPositions.Length < 2) return;

            Gizmos.color = RotationCurveColor;

            // Draw curve.
            for (var i = 0; i < globalPointPositions.Length - 1; i++) {
                Gizmos.DrawLine(
                    globalPointPositions[i], globalPointPositions[i + 1]);
            }
        }

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
                    FloatPrecision) {
                    continue;
                }

                //Draw rotation point gizmo.
                Gizmos.DrawIcon(
                globalRotPointPositions[i],
                RotationPointGizmoIcon,
                false);
            }
        }

        public void DrawTargetIcon(Vector3 targetPosition) {

            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                targetPosition,
                TargetGizmoIcon,
                false);
        }

        #endregion
    }

}