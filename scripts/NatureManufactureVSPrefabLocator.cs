#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace VegetationStudioProEditorExtentions
{
    public static class NatureManufactureVSPrefabLocator 
    {
        /// <summary>
        /// Attempts to lookup a VSPrefab that is included in Nature Manufacture Enivornments
        /// Currently only supports Mountain Environment
        /// </summary>
        /// <param name="standardPrefab"></param>
        /// <returns>Either the standard prefab if it fails or the VSPrefab if it succeeds</returns>
        public static GameObject TryToConvertNMPrefabToNMVSPrefab(GameObject standardPrefab)
        {
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(standardPrefab);
            path = path.Remove(path.IndexOf("/Prefabs/"), (path.Length - path.IndexOf("/Prefabs/")));
            string prefabName = standardPrefab.name;
            prefabName = prefabName.Replace("Prefab_Forest", "VS_Prefab");
            path += "/VS Prefabs/" + prefabName + ".prefab";

            if (!System.IO.File.Exists(path))
            {
                Debug.Log("File is invalid");
                return standardPrefab;
            }
            return PrefabUtility.LoadPrefabContents(path);
        }
    }
}
#endif
