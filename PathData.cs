using UnityEngine;
using System.Collections;

namespace ATP.AnimationPathTools {

	public class PathData : ScriptableObject {

		[SerializeField]
		private AnimationPath animatedObjectPath;

		[SerializeField]
		private AnimationPath rotationPath;

		[SerializeField]
		private AnimationCurve easeCurve;

		[SerializeField]
		private AnimationCurve tiltingCurve;

	    private void OnEnable() {
            // Return if fields are initialized.
	        if (animatedObjectPath != null) return;

	        InstantiateReferenceTypes();
	        AssignDefaultValues();
	    }

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

	    protected virtual float DefaultEaseCurveValue {
            get { return 0.05f; }
	    }

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

	    private void InstantiateReferenceTypes() {
	        AnimatedObjectPath =
	            ScriptableObject.CreateInstance<AnimationPath>();
	        RotationPath =
	            ScriptableObject.CreateInstance<AnimationPath>();
	        EaseCurve = new AnimationCurve();
	        TiltingCurve = new AnimationCurve();
	    }
	}
}
