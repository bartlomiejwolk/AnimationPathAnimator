using UnityEngine;
using System.Collections;

namespace ATP.SimplePathAnimator.PathAnimatorComponent {

    public sealed class PathAnimatorSettings : ScriptableObject {

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

        /// <summary>
        ///     Value of the jump when modifier key is pressed.
        /// </summary>
        [SerializeField]
        private float shortJumpValue = 0.002f;
        [SerializeField]
        private KeyCode tiltingModeKey = KeyCode.O;

        [SerializeField]
        private KeyCode updateAllKey = KeyCode.G;
#endregion
        #region SHORTCUT PROPERTIES

        // TODO Add setters everywhere.
        public KeyCode EaseModeKey {
            get { return easeModeKey; }
            set { easeModeKey = value; }
        }

        public KeyCode JumpToEndKey {
            get { return jumpToEndKey; }
            set { jumpToEndKey = value; }
        }

        public KeyCode JumpToNextNodeKey {
            get { return jumpToNextNodeKey; }
            set { jumpToNextNodeKey = value; }
        }

        public KeyCode JumpToPreviousNodeKey {
            get { return jumpToPreviousNodeKey; }
            set { jumpToPreviousNodeKey = value; }
        }

        public KeyCode JumpToStartKey {
            get { return jumpToStartKey; }
            set { jumpToStartKey = value; }
        }

        public KeyCode LongJumpBackwardKey {
            get { return longJumpBackwardKey; }
            set { longJumpBackwardKey = value; }
        }

        public KeyCode LongJumpForwardKey {
            get { return longJumpForwardKey; }
            set { longJumpForwardKey = value; }
        }

        public float LongJumpValue {
            get { return longJumpValue; }
            set { longJumpValue = value; }
        }

        public KeyCode ModKey {
            get { return modKey; }
            set { modKey = value; }
        }

        public KeyCode MoveAllModeKey {
            get { return moveAllModeKey; }
        }

        public KeyCode NoneModeKey {
            get { return noneModeKey; }
            set { noneModeKey = value; }
        }

        public KeyCode PlayPauseKey {
            get { return playPauseKey; }
            set { playPauseKey = value; }
        }

        public KeyCode RotationModeKey {
            get { return rotationModeKey; }
            set { rotationModeKey = value; }
        }

        public KeyCode ShortJumpBackwardKey {
            get { return shortJumpBackwardKey; }
            set { shortJumpBackwardKey = value; }
        }

        public KeyCode ShortJumpForwardKey {
            get { return shortJumpForwardKey; }
            set { shortJumpForwardKey = value; }
        }

        /// <summary>
        ///     Value of the jump when modifier key is pressed.
        /// </summary>
        public float ShortJumpValue {
            get { return shortJumpValue; }
            set { shortJumpValue = value; }
        }
        public KeyCode TiltingModeKey {
            get { return tiltingModeKey; }
            set { tiltingModeKey = value; }
        }
        public KeyCode UpdateAllKey {
            get { return updateAllKey; }
            set { updateAllKey = value; }
        }
        #endregion
        #region GIZMO FIELDS

        /// <summary>
        ///     Color of the gizmo curve.
        /// </summary>
        [SerializeField]
        private Color gizmoCurveColor = Color.yellow;

        [SerializeField]
        private Color rotationCurveColor = Color.gray;

        [SerializeField]
        private string currentRotationPointGizmoIcon = "rec_16x16-yellow";

        [SerializeField]
        private float floatPrecision = 0.001f;

        [SerializeField]
        private string forwardPointIcon = "target_22x22-pink";

        [SerializeField]
        private int gizmoCurveSamplingFrequency = 40;

        [SerializeField]
        private int rotationCurveSampling = 40;

        [SerializeField]
        private string rotationPointGizmoIcon = "rec_16x16";

        [SerializeField]
        private string targetGizmoIcon = "target_22x22-blue";
        #endregion
        #region GIZMO PROPERTIES
        public string CurrentRotationPointGizmoIcon {
            get { return currentRotationPointGizmoIcon; }
        }

        public float FloatPrecision {
            get { return floatPrecision; }
        }
        public string ForwardPointIcon {
            get { return forwardPointIcon; }
        }

        /// <summary>
        ///     Color of the gizmo curve.
        /// </summary>
        public Color GizmoCurveColor {
            get { return gizmoCurveColor; }
            set { gizmoCurveColor = value; }
        }

        public int GizmoCurveSamplingFrequency {
            get { return gizmoCurveSamplingFrequency; }
        }

        public Color RotationCurveColor {
            get { return rotationCurveColor; }
            set { rotationCurveColor = value; }
        }

        public int RotationCurveSampling {
            get { return rotationCurveSampling; }
        }

        public string RotationPointGizmoIcon {
            get { return rotationPointGizmoIcon; }
        }
        public string TargetGizmoIcon {
            get { return targetGizmoIcon; }
        }
        #endregion
        #region ANIMATOR FIELDS
        [SerializeField]
        private string[] iconsSourceDirs = {
            "/AirTimeProductions/animationpathtools/PathAnimatorComponent/Resources/Icons/",
            "/simplepathanimator/PathAnimatorComponent/Resources/Icons/"
        };

        [SerializeField]
        private bool autoPlay = true;

        [SerializeField]
        private bool EnableControlsInPlayMode = true;

        [SerializeField]
        private int exportSamplingFrequency = 5;
        /// <summary>
        ///     How much look forward point should be positioned away from the
        ///     animated object.
        /// </summary>
        /// <remarks>Value is a time in range from 0 to 1.</remarks>
        [SerializeField]
        private float forwardPointOffset = 0.05f;

        [SerializeField]
        private HandleMode handleMode =
            HandleMode.None;

        [SerializeField]
        private float MaxAnimationSpeed = 0.3f;
        [SerializeField]
        private MovementMode movementMode =
            MovementMode.MoveSingle;

        private MovementMode movementModeAfterReset =
            MovementMode.MoveSingle;

        [SerializeField]
        private float positionLerpSpeed = 1;

        [SerializeField]
        private RotationMode rotationMode =
            RotationMode.Forward;

        [SerializeField]
        // TODO Rename to rotationLerpSpeed.
        private float rotationSpeed = 1f;

        [SerializeField]
        private TangentMode tangentMode =
            TangentMode.Smooth;

        [SerializeField]
        private bool updateAllMode;

        [SerializeField]
        private WrapMode wrapMode = WrapMode.ClampForever;
        #endregion
        #region ANIMATOR PROPERTIES
        public string[] IconsSourceDirs {
            get { return iconsSourceDirs; }
            set { iconsSourceDirs = value; }
        }

        public bool AutoPlay {
            get { return autoPlay; }
            set { autoPlay = value; }
        }

        public int ExportSamplingFrequency {
            get { return exportSamplingFrequency; }
            set { exportSamplingFrequency = value; }
        }

        /// <summary>
        ///     How much look forward point should be positioned away from the
        ///     animated object.
        /// </summary>
        /// <remarks>Value is a time in range from 0 to 1.</remarks>
        public float ForwardPointOffset {
            get { return forwardPointOffset; }
            set { forwardPointOffset = value; }
        }

        public HandleMode HandleMode {
            get { return handleMode; }
            set { handleMode = value; }
        }

        public MovementMode MovementMode {
            get { return movementMode; }
            set { movementMode = value; }
        }

        public MovementMode MovementModeAfterReset {
            get { return movementModeAfterReset; }
            set { movementModeAfterReset = value; }
        }
        public float PositionLerpSpeed {
            get { return positionLerpSpeed; }
            set { positionLerpSpeed = value; }
        }

        public RotationMode RotationMode {
            get { return rotationMode; }
            set { rotationMode = value; }
        }

        public float RotationSpeed {
            get { return rotationSpeed; }
            set { rotationSpeed = value; }
        }

        public TangentMode TangentMode {
            get { return tangentMode; }
            set { tangentMode = value; }
        }

        public bool UpdateAllMode {
            get { return updateAllMode; }
            set { updateAllMode = value; }
        }

        public WrapMode WrapMode {
            get { return wrapMode; }
            set { wrapMode = value; }
        }
        #endregion
        #region HANDLES FIELDS

        [SerializeField]
        private int addButtonH = 25;
        [SerializeField]
        private int addButtonV = 10;
        [SerializeField]
        private float arcHandleRadius = 0.6f;

        [SerializeField]
        private int defaultLabelHeight = 10;

        [SerializeField]
        private int defaultLabelWidth = 30;

        [SerializeField]
        private float moveAllModeSize = 0.15f;

        [SerializeField]
        private float movementHandleSize = 0.12f;

        [SerializeField]
        private int removeButtonH = 44;
        [SerializeField]
        private int removeButtonV = 10;
        [SerializeField]
        private float scaleHandleSize = 1.5f;
        #endregion
        #region HANDLES PROPERTIES
        public int AddButtonH {
            get { return addButtonH; }
            set { addButtonH = value; }
        }

        public int AddButtonV {
            get { return addButtonV; }
            set { addButtonV = value; }
        }

        public float ArcHandleRadius {
            get { return arcHandleRadius; }
            set { arcHandleRadius = value; }
        }

        public int DefaultLabelHeight {
            get { return defaultLabelHeight; }
            set { defaultLabelHeight = value; }
        }

        public int DefaultLabelWidth {
            get { return defaultLabelWidth; }
            set { defaultLabelWidth = value; }
        }

        public int EaseValueLabelOffsetX {
            get { return -20; }
        }

        public int EaseValueLabelOffsetY {
            get { return -25; }
        }
        public float InitialArcValue {
            get { return 15f; }
        }

        public Color MoveAllModeColor {
            get { return Color.red; }
        }

        public float MovementHandleSize {
            get { return movementHandleSize; }
            set { movementHandleSize = value; }
        }

        public Color PositionHandleColor {
            get { return Color.yellow; }
        }

        public int RemoveButtonH {
            get { return removeButtonH; }
            set { removeButtonH = value; }
        }

        public int RemoveButtonV {
            get { return removeButtonV; }
            set { removeButtonV = value; }
        }

        public float RotationHandleSize {
            get { return 0.26f; }
        }

        public float ScaleHandleSize {
            get { return scaleHandleSize; }
            set { scaleHandleSize = value; }
        }

        public int UpdateAllLabelOffsetX {
            get { return 0; }
        }

        public int UpdateAllLabelOffsetY {
            get { return -25; }
        }
        public string UpdateAllLabelText {
            get { return "A"; }
        }
        #endregion
    }

}
