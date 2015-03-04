using UnityEngine;
using System.Collections;

namespace ATP.SimplePathAnimator.Animator {

    public class AnimatorSettings : ScriptableObject {

        #region SHORTCUT FIELDS
        [SerializeField]
        private KeyCode easeModeKey = KeyCode.U;

        /// <summary>
        ///     Key shortcut to jump to the end of the animation.
        /// </summary>
        [SerializeField]
        private KeyCode jumpToEndKey = KeyCode.L;

        [SerializeField]
        private KeyCode jumpToNextNodeKey = KeyCode.L;

        [SerializeField]
        private KeyCode jumpToPreviousNodeKey = KeyCode.H;

        [SerializeField]
        private KeyCode jumpToStartKey = KeyCode.H;

        [SerializeField]
        private KeyCode longJumpBackwardKey = KeyCode.J;

        [SerializeField]
        private KeyCode longJumpForwardKey = KeyCode.K;

        [SerializeField]
        private float longJumpValue = 0.01f;

        [SerializeField]
        private KeyCode modKey = KeyCode.RightAlt;

        [SerializeField]
        private Color moveAllModeColor = Color.red;

        [SerializeField]
        private KeyCode moveAllModeKey = KeyCode.P;

        [SerializeField]
        private KeyCode noneModeKey = KeyCode.Y;

        [SerializeField]
        private KeyCode playPauseKey = KeyCode.Space;

        [SerializeField]
        private KeyCode rotationModeKey = KeyCode.I;

        [SerializeField]
        private KeyCode shortJumpBackwardKey = KeyCode.J;

        [SerializeField]
        private KeyCode shortJumpForwardKey = KeyCode.K;

        [SerializeField]
        private KeyCode tiltingModeKey = KeyCode.O;

        [SerializeField]
        private KeyCode updateAllKey = KeyCode.G;
#endregion
        #region SHORTCUT PROPERTIES

        // TODO Add setters everywhere.
        public KeyCode EaseModeKey {
            get { return easeModeKey; }
        }

        public KeyCode JumpToEndKey {
            get { return jumpToEndKey; }
        }

        public KeyCode JumpToNextNodeKey {
            get { return jumpToNextNodeKey; }
        }

        public KeyCode JumpToPreviousNodeKey {
            get { return jumpToPreviousNodeKey; }
        }

        public KeyCode JumpToStartKey {
            get { return jumpToStartKey; }
        }

        public KeyCode LongJumpBackwardKey {
            get { return longJumpBackwardKey; }
        }

        public KeyCode LongJumpForwardKey {
            get { return longJumpForwardKey; }
        }

        public float LongJumpValue {
            get { return longJumpValue; }
        }

        public virtual KeyCode ModKey {
            get { return modKey; }
        }

        public virtual Color MoveAllModeColor {
            get { return moveAllModeColor; }
        }

        public virtual KeyCode MoveAllModeKey {
            get { return moveAllModeKey; }
        }

        //public virtual KeyCode MoveSingleModeKey {
        //    get { return KeyCode.Y; }
        //}
        public virtual KeyCode NoneModeKey {
            get { return noneModeKey; }
        }

        public KeyCode PlayPauseKey {
            get { return playPauseKey; }
            set { playPauseKey = value; }
        }
        //public virtual KeyCode PlayPauseKey {
        //    get { return KeyCode.Space; }
        //}
        public virtual KeyCode RotationModeKey {
            get { return rotationModeKey; }
        }
        public KeyCode ShortJumpBackwardKey {
            get { return shortJumpBackwardKey; }
        }
        public KeyCode ShortJumpForwardKey {
            get { return shortJumpForwardKey; }
        }
        public virtual KeyCode TiltingModeKey {
            get { return tiltingModeKey; }
        }
        public virtual KeyCode UpdateAllKey {
            get { return updateAllKey; }
        }
        #endregion
        #region GIZMO FIELDS

        [SerializeField]
        private Color rotationCurveColor = Color.gray;

        /// <summary>
        ///     Color of the gizmo curve.
        /// </summary>
        [SerializeField]
        private Color gizmoCurveColor = Color.yellow;

        #endregion
        #region GIZMO PROPERTIES

        [SerializeField]
        private float floatPrecision = 0.001f;

        public float FloatPrecision {
            get { return floatPrecision; }
        }

        [SerializeField]
        private string rotationPointGizmoIcon = "rec_16x16";
        public string RotationPointGizmoIcon {
            get { return rotationPointGizmoIcon; }
        }

        [SerializeField]
        private string targetGizmoIcon = "target_22x22-blue";
        public string TargetGizmoIcon {
            get { return targetGizmoIcon; }
        }

        [SerializeField]
        private string currentRotationPointGizmoIcon = "rec_16x16-yellow";
        public string CurrentRotationPointGizmoIcon {
            get { return currentRotationPointGizmoIcon; }
        }

        [SerializeField]
        private string forwardPointIcon = "target_22x22-pink";
        public string ForwardPointIcon {
            get { return forwardPointIcon; }
        }

        public Color RotationCurveColor {
            get { return rotationCurveColor; }
            set { rotationCurveColor = value; }
        }

        [SerializeField]
        private int rotationCurveSampling = 40;
        public int RotationCurveSampling {
            get { return rotationCurveSampling; }
        }

        [SerializeField]
        private int gizmoCurveSamplingFrequency = 40;
        public int GizmoCurveSamplingFrequency {
            get { return gizmoCurveSamplingFrequency; }
        }

        /// <summary>
        ///     Color of the gizmo curve.
        /// </summary>
        public Color GizmoCurveColor {
            get { return gizmoCurveColor; }
            set { gizmoCurveColor = value; }
        }

        #endregion
    }

}
