#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VegetationStudioProEditorExtentions
{
    public static class LodRemover 
    {
        /// <summary>
        /// Generate a new prefab asset in the same directory with name appended _3LODs
        /// New prefab will have the 4th LOD removed from its hierarchy and the LODs[] of the LODGroup component
        /// If the prefab supplied does not contain 4 LODs or does not contain a LODGroup then it will be returned
        /// </summary>
        /// <param name="prefab">The prefab to copy and remove the 4th LOD from, if it contains a 4th LOD</param>
        /// <returns>Either the original prefab or the new prefab</returns>
        public static GameObject Remove4thLOD(GameObject prefab)
        {
            if (prefab.GetComponent<LODGroup>() != null)
            {
                if (prefab.GetComponent<LODGroup>().lodCount == 4)
                {
                    string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);
                    string newPath = path.Replace(".prefab", "");
                    newPath += "_3LODs.prefab";

                    //Debug.Log(path);
                    //Debug.Log(newPath);

                    if (System.IO.File.Exists(newPath)) // Already created
                    {
                        Debug.Log("Already exists :" + newPath);
                        return AssetDatabase.LoadAssetAtPath<GameObject>(newPath);
                    }

                    if (AssetDatabase.CopyAsset(path, newPath))
                    {
#if UNITY_2020_1_OR_NEWER
                        using (var editScope = new PrefabUtility.EditPrefabContentsScope(newPath))
                        {

                            for (int i = editScope.prefabContentsRoot.transform.childCount - 1; i > 0; i--)
                            {
                                if (editScope.prefabContentsRoot.transform.GetChild(i).name.ToLower().Contains("lod3"))
                                {
                                    //Debug.Log("Removing LOD");
                                    GameObject.DestroyImmediate(editScope.prefabContentsRoot.transform.GetChild(i).gameObject);
                                    List<LOD> newLod = new List<LOD>(editScope.prefabContentsRoot.GetComponent<LODGroup>().GetLODs()); // Get LOD[] to List
                                    newLod.RemoveAt(newLod.Count - 1); // Remove the 4th LOD
                                    editScope.prefabContentsRoot.GetComponent<LODGroup>().SetLODs(newLod.ToArray()); // Replace the LOD[]
                                    return AssetDatabase.LoadAssetAtPath<GameObject>(newPath);
                                }
                            }
                        }
#else
                        using (var editScope = new EditPrefabAssetScope(newPath))
                        {

                            for (int i = editScope.prefabRoot.transform.childCount - 1; i > 0; i++)
                            {
                                if (editScope.prefabRoot.transform.GetChild(i).name.ToLower().Contains("lod3"))
                                {
                                    Debug.Log("Removing LOD");
                                    DestroyImmediate(editScope.prefabRoot.transform.GetChild(i).gameObject);
                                    List<LOD> newLod = new List<LOD>(editScope.prefabRoot.GetComponent<LODGroup>().GetLODs()); // Get LOD[] to List
                                    newLod.RemoveAt(newLod.Count - 1); // Remove the 4th LOD
                                    editScope.prefabRoot.GetComponent<LODGroup>().SetLODs(newLod.ToArray()); // Replace the LOD[]
                                    return AssetDatabase.LoadAssetAtPath<GameObject>(newPath);
                                }
                            }
                        }
#endif
                    }
                }
            }
            return prefab;
        }
    }
#if !UNITY_2020_1_OR_NEWER

    // Credit to Baste on Unity Forum @
    // https://forum.unity.com/threads/how-do-i-edit-prefabs-from-scripts.685711/
    // Unity added this wrapper in 2020.1 

    public class EditPrefabAssetScope : IDisposable
    {

        public readonly string assetPath;
        public readonly GameObject prefabRoot;

        public EditPrefabAssetScope(string assetPath)
        {
            this.assetPath = assetPath;
            prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
        }

        public void Dispose()
        {
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
#endif
}
#endif
