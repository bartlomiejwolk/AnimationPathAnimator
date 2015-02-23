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
	        animatedObjectPath =
                ScriptableObject.CreateInstance<AnimationPath>();
	        rotationPath =
	            ScriptableObject.CreateInstance<AnimationPath>();
            easeCurve = new AnimationCurve();
            tiltingCurve = new AnimationCurve();
	    }
	}
}
