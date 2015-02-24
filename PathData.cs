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
	        tiltingCurve.AddKey(0, 0);
	        tiltingCurve.AddKey(1, 0);
	    }

	    private void InitializeEaseCurve() {
	        easeCurve.AddKey(0, DefaultEaseCurveValue);
	        easeCurve.AddKey(1, DefaultEaseCurveValue);
	    }

	    protected virtual float DefaultEaseCurveValue {
            get { return 0.05f; }
	    }

	    private void InitializeRotationPath() {
            var firstNodePos = new Vector3(0, 0, 0);
            rotationPath.CreateNewNode(0, firstNodePos);

            var lastNodePos = new Vector3(1, 0, 1);
            rotationPath.CreateNewNode(1, lastNodePos);
	    }

	    private void InitializeAnimatedObjectPath() {
            var firstNodePos = new Vector3(0, 0, 0);
            animatedObjectPath.CreateNewNode(0, firstNodePos);

            var lastNodePos = new Vector3(1, 0, 1);
            animatedObjectPath.CreateNewNode(1, lastNodePos);
	    }

	    private void InstantiateReferenceTypes() {
	        animatedObjectPath =
	            ScriptableObject.CreateInstance<AnimationPath>();
	        rotationPath =
	            ScriptableObject.CreateInstance<AnimationPath>();
	        easeCurve = new AnimationCurve();
	        tiltingCurve = new AnimationCurve();
	    }
	}
}
