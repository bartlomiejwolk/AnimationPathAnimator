using System.IO;
using UnityEditor;
using UnityEngine;

public static class ScriptableObjectUtility {

    public static void CreateAsset<T>() where T : ScriptableObject {
        var asset = ScriptableObject.CreateInstance<T>();

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "") {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "") {
            path =
                path.Replace(
                    Path.GetFileName(
                        AssetDatabase.GetAssetPath(Selection.activeObject)),
                    "");
        }

        var assetPathAndName =
            AssetDatabase.GenerateUniqueAssetPath(
                path + "/New " + typeof (T) + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }

    //	This makes it easy to create, name and place unique new ScriptableObject asset files.
    /// <summary>
    /// </summary>
    public static T CreateAsset<T>(string path) where T : ScriptableObject {
        // Path cannot be empty.
        if (path == "") {
            Debug.LogWarning("Path to create new asset cannot be empty.");

            return null;
        }

        var asset = ScriptableObject.CreateInstance<T>();

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = asset;

        return asset;
    }

}