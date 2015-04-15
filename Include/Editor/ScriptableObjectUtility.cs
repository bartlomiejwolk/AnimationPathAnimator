using System.IO;
using UnityEditor;
using UnityEngine;

namespace AnimationPathAnimator {

    public static class ScriptableObjectUtility {

        public static T CreateAsset<T>(
            string name = "",
            string fullPath = "") where T : ScriptableObject {

            // Create instance of asset.
            var asset = ScriptableObject.CreateInstance<T>();

   
            // Result file path.
            string assetPathAndName;

            // Set file path to path specified through argument.
            if (fullPath != "") {
                // Use it.
                assetPathAndName = fullPath;
            }
            // No path specified.
            else {
                // Generate file name.
                if (name == "") {
                    name = typeof(T).ToString();
                }

                // Get file path from selection.
                var selectionFilePath = AssetDatabase.GetAssetPath(Selection.activeObject);

                // If selection returned empty string..
                if (selectionFilePath == "") {
                    selectionFilePath = "Assets";
                }
                // Selection not empty..
                else if (Path.GetExtension(selectionFilePath) != "") {
                    selectionFilePath = selectionFilePath.Replace(Path.GetFileName(
                        AssetDatabase.GetAssetPath(Selection.activeObject)),
                        "");
                }

                // Generate result file path.
                assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(
                    selectionFilePath + "/" + name + ".asset");
            }

            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            //EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            return asset;
        }
    }

}