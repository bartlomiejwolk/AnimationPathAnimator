using System;
using System.CodeDom;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ATP.SimplePathAnimator.Animator {

	public static class ScriptableObjectUtility
	{
		/// <summary>
		//	This makes it easy to create, name and place unique new ScriptableObject asset files.
		/// </summary>
		public static T CreateAsset<T> (string path) where T : ScriptableObject
		{
            // Path cannot be empty.
		    if (path == "") return null;

			T asset = ScriptableObject.CreateInstance<T> ();
			
			AssetDatabase.CreateAsset (asset, path);
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh();
            //EditorUtility.FocusProjectWindow ();
			Selection.activeObject = asset;

		    return asset;
		}
	}
}