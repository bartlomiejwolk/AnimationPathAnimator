using UnityEngine;
using UnityEditor;
using System.Collections;
using Assets.Extensions.animationpathtools.Include.Editor;

namespace ATP.AnimationPathTools {

	public static class PathDataMenu  {

		[MenuItem("Assets/Create/PathData")]
		public static void CreatePathDataAsset() {
			ScriptableObjectUtility.CreateAsset<PathData>();
		}
	}
}