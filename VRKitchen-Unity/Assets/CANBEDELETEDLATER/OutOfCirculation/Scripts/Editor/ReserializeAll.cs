using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ReserializeAll : MonoBehaviour
{
    [MenuItem("Tools/Dirty All")]
    static void DoReserialize()
    {
        var paths = AssetDatabase.GetAllAssetPaths();

        List<string> pathToReserialize = new List<string>();

        for (int i = 0; i < paths.Length; ++i)
        {
            EditorUtility.DisplayProgressBar("Reserialization in progress", $"Reserializing all assets {i}/{paths.Length}", i / (float)paths.Length);

            var path = paths[i];

            if (!path.StartsWith("Assets"))
            {
                continue;
            }

            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);

            if (asset is GameObject)
            {
                if (!PrefabUtility.IsPartOfImmutablePrefab(asset))
                {
                    pathToReserialize.Add(path);
                }
            }
            else if (asset is ScriptableObject)
            {
                pathToReserialize.Add(path);
            }
            else if (asset is SceneAsset)
            {
                pathToReserialize.Add(path);
            }
        }
        
        EditorUtility.ClearProgressBar();
        
        AssetDatabase.ForceReserializeAssets(pathToReserialize, ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
    }
}
