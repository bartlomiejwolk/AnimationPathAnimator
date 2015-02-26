using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ATP.AnimationPathTools {
    [System.Serializable]
    // TODO Make it inherit from ScriptableObject and then create
    // SerializedObject in the Editor class to display curve color field.
    public class GizmoDrawer {

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
        #endregion

        #region METHODS

        public void DrawCurrentRotationPointGizmo(Vector3 rotationPointPosition) {
            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                rotationPointPosition,
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

        public void DrawRotationGizmoCurve(Vector3[] globalPointPositions) {
            if (globalPointPositions.Length < 2) return;

            Gizmos.color = RotationCurveColor;

            // Draw curve.
            for (var i = 0; i < globalPointPositions.Length - 1; i++) {
                Gizmos.DrawLine(globalPointPositions[i], globalPointPositions[i + 1]);
            }
        }

        public void DrawRotationPointGizmo(Vector3 rotationPointPosition) {
            //Draw rotation point gizmo.
            Gizmos.DrawIcon(
                rotationPointPosition,
                RotationPointGizmoIcon,
                false);
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