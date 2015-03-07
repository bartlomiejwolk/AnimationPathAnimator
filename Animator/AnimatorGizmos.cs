using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ATP.SimplePathAnimator.Animator {

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

            // TODO Use directive to disable this code in standalone.
            CopyIconsToGizmosFolder();
        }

        private void CopyIconsToGizmosFolder() {

            // Create Asset/Gizmos folder if not exists.
            if (!Directory.Exists(Application.dataPath + "/Gizmos")) {
                Directory.CreateDirectory(Application.dataPath + "/Gizmos");
            }

            // Check if settings asset has any paths to be searched for icons.
            if (Settings.IconsSourceDirs == null) return;

            // For each path check if specified folder exists..
            foreach (var iconDir in Settings.IconsSourceDirs) {
                if (Directory.Exists(Application.dataPath + iconDir)) {
                    // Copy icon to Asset/Gizmos folder.
                    CopyIcon(iconDir, Settings.CurrentRotationPointGizmoIcon);
                    CopyIcon(iconDir, Settings.ForwardPointIcon);
                    CopyIcon(iconDir, Settings.RotationPointGizmoIcon);
                    CopyIcon(iconDir, Settings.TargetGizmoIcon);

                    break;
                }
            }
        }

        private void CopyIcon(string sourceDir, string iconName) {
            // Check if icon file exists in Assets/Gizmos folder.
            if (!File.Exists(
                Application.dataPath + "/Gizmos/"
                + iconName + ".png")) {

                // TODO If source directory doesn't exist, show info about how
                // to copy icons to the Gizmos folder.

                // Copy icon.
                FileUtil.CopyFileOrDirectory(
                    Application.dataPath
                        + sourceDir
                        + iconName + ".png",
                    Application.dataPath + "/Gizmos/"
                        + iconName + ".png");
            }
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
                Settings.CurrentRotationPointGizmoIcon,
                false);
        }

        public void DrawForwardPointIcon(Vector3 forwardPointPosition) {
            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                forwardPointPosition,
                Settings.ForwardPointIcon,
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
                Settings.RotationPointGizmoIcon,
                false);
            }
        }

        public void DrawTargetIcon(Vector3 targetPosition) {

            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                targetPosition,
                Settings.TargetGizmoIcon,
                false);
        }

        //public void Init(PathAnimatorSettings settings) {
        //    Settings = settings;
        //}

        #endregion
    }

}