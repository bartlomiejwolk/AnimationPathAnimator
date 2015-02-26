using System;
using UnityEngine;
using System.Collections;

namespace ATP.AnimationPathTools {

	public class PathData : ScriptableObject {

        public event EventHandler RotationPointPositionChanged;

        #region SERIALIZED FIELDS
        [SerializeField]
		private AnimationPath animatedObjectPath;

		[SerializeField]
		private AnimationPath rotationPath;

		[SerializeField]
		private AnimationCurve easeCurve;

		[SerializeField]
		private AnimationCurve tiltingCurve;
        #endregion

        #region PUBLIC PROPERTIES
        public AnimationPath AnimatedObjectPath {
            get { return animatedObjectPath; }
            set { animatedObjectPath = value; }
        }

        public AnimationPath RotationPath {
            get { return rotationPath; }
            set { rotationPath = value; }
        }

        public AnimationCurve EaseCurve {
            get { return easeCurve; }
            set { easeCurve = value; }
        }

        public AnimationCurve TiltingCurve {
            get { return tiltingCurve; }
            set { tiltingCurve = value; }
        }
        #endregion

        #region PRIVATE/PROTECTED PROPERTIES
        protected virtual float DefaultEaseCurveValue {
            get { return 0.05f; }
        }

        protected virtual float FloatPrecision {
            get { return 0.001f; }
        }

        #endregion

        #region UNITY MESSAGES

        private void OnEnable() {
            // Return if fields are initialized.
	        if (animatedObjectPath != null) return;

	        InstantiateReferenceTypes();
	        AssignDefaultValues();
	    }
        #endregion

        #region EVENTINVOCATORS

        protected virtual void OnRotationPointPositionChanged() {
            var handler = RotationPointPositionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region PUBLIC METHODS

        public void Reset() {
            InstantiateReferenceTypes();
            AssignDefaultValues();
        }

        public void ChangeRotationAtTimestamp(
            float timestamp,
            Vector3 newPosition) {
            // Get node timestamps.
            var timestamps = RotationPath.GetTimestamps();
            // If matching timestamp in the path was found.
            var foundMatch = false;
            // For each timestamp..
            for (var i = 0; i < RotationPath.KeysNo; i++) {
                // Check if it is the timestamp to remove..
                if (Math.Abs(timestamps[i] - timestamp) < FloatPrecision) {
                    // Remove node.
                    RotationPath.RemoveNode(i);

                    foundMatch = true;
                }
            }

            // If timestamp was not found..
            if (!foundMatch) {
                Debug.Log("You're trying to change rotation for nonexistent " +
                          "node.");

                return;
            }

            // Create new node.
            RotationPath.CreateNewNode(timestamp, newPosition);
            // Smooth all nodes.
            RotationPath.SmoothAllNodes();

            OnRotationPointPositionChanged();
        }

	    #endregion

        #region PRIVATE METHODS

        private void AssignDefaultValues() {
	        InitializeAnimatedObjectPath();
	        InitializeRotationPath();
	        InitializeEaseCurve();
	        InitializeTiltingCurve();
	    }

	    private void InitializeTiltingCurve() {
	        TiltingCurve.AddKey(0, 0);
	        TiltingCurve.AddKey(1, 0);
	    }

	    private void InitializeEaseCurve() {
	        EaseCurve.AddKey(0, DefaultEaseCurveValue);
	        EaseCurve.AddKey(1, DefaultEaseCurveValue);
	    }


	    private void InitializeRotationPath() {
            var firstNodePos = new Vector3(0, 0, 0);
            RotationPath.CreateNewNode(0, firstNodePos);

            var lastNodePos = new Vector3(1, 0, 1);
            RotationPath.CreateNewNode(1, lastNodePos);
	    }

	    private void InitializeAnimatedObjectPath() {
            var firstNodePos = new Vector3(0, 0, 0);
            AnimatedObjectPath.CreateNewNode(0, firstNodePos);

            var lastNodePos = new Vector3(1, 0, 1);
            AnimatedObjectPath.CreateNewNode(1, lastNodePos);
	    }

	    private void InstantiateAnimationPathCurves(AnimationPath animationPath) {
	        for (var i = 0; i < 3; i++) {
	            animationPath[i] = new AnimationCurve();
	        }
	    }

	    private void InstantiateReferenceTypes() {
	        AnimatedObjectPath = new AnimationPath();
            InstantiateAnimationPathCurves(animatedObjectPath);
	        RotationPath = new AnimationPath();
            InstantiateAnimationPathCurves(rotationPath);
	        EaseCurve = new AnimationCurve();
	        TiltingCurve = new AnimationCurve();
	    }
        #endregion
    }
}
