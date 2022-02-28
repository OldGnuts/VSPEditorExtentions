#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;

namespace VegetationStudioProEditorExtentions
{
    public partial class VSPEditorExtention : EditorWindow
    {
        /// <summary>
        /// Checks if a prefab is in the count dictionary and adds it if not
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="vegetationType"></param>
        public void CountPrefabInstance(GameObject prefab, VegetationType vegetationType)
        {
            if (prefabCount.ContainsKey(prefab))
                prefabCount[prefab] = prefabCount[prefab] + 1;
            else
            {
                prefabCount.Add(prefab, 1);
                prefabType.Add(prefab, vegetationType);
            }
        }
        /// <summary>
        /// Records original prefab and instance count and individual instance information
        /// </summary>
        /// <param name="t"></param>
        /// <param name="vegetationType"></param>
        void RecordPrefabTransformsAndInstanceCount(Transform t, VegetationType vegetationType)
        {
            // Keep a record of the number of instances that use this prefab
            CountPrefabInstance(PrefabUtility.GetCorrespondingObjectFromOriginalSource(t.gameObject), vegetationType); // Count it
            // Keep record of this transform for instantiation
            prefabTransforms.Add(t);
        }
        /// <summary>
        /// Performs a single layer search in a prefab instance transform for children that are original prefabs
        /// This only performs a single layer search on the nested prefab. 
        /// TODO: Search for nested prefabs in the nested prefab parent
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="vegetationType"></param>
        void ScanPrefabForNestedPrefabs(Transform transform, VegetationType vegetationType)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform t = PrefabUtility.GetCorrespondingObjectFromOriginalSource(transform.GetChild(i));
                // Validate that this has rendering components
                if (t.GetComponent<LODGroup>() != null || t.GetComponent<MeshRenderer>() != null)
                {
                    RecordPrefabTransformsAndInstanceCount(t, vegetationType);
                }
            }
        }

        List<Transform> prefabTransforms = new List<Transform>();
        /// <summary>
        /// Scans hierarchy objects for prefabs and passes them on to the prefab counter
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="vegetationType"></param>
        void ScanParentForPrefabs(Transform transform, VegetationType vegetationType)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(transform.gameObject))
            {
                //Debug.Log(transform.gameObject.name +" is part of instance");

                // Identify meshrenderer and LODGroup which indicate a non prefab instance
                if (transform.gameObject.GetComponent<MeshRenderer>() || transform.gameObject.GetComponent<LODGroup>())
                {
                    RecordPrefabTransformsAndInstanceCount(transform, vegetationType);
                }
                // transform is part of prefab but does not contain rendering components
                // if this is a nested prefab, we can try to cut into individual prefabs
                // for some puroses this will help speed up rendering of premade scene layouts
                // IE: Many trees placed in a configuration to make placement more consistent etc.
                else if (transform.childCount > 0)
                {
                    ScanPrefabForNestedPrefabs(transform, vegetationType);
                }
            }
            else if (transform.childCount > 0)
            {
                // Debug.Log("Scanning children");
                // This may be a directory of prefab instances so lets scan it recursively
                for (int i = 0; i < transform.childCount; i++)
                {
                    // Debug.Log("Scanning child " + i);
                    ScanParentForPrefabs(transform.GetChild(i), vegetationType);
                }
            }
        }
        /// <summary>
        /// Scan every folder that the user has placed into the hierachy list for examination
        /// </summary>
        public void ScanAllHierarchyItems()
        {
            // Scan each parent that the user has dragged into the list
            for (int i = 0; i < hierarchyInformation.Count; i++)
            {
                for (int j = 0; j < hierarchyInformation[i].hierarchyFolder.transform.childCount; j++)
                {
                    // Scan the first children of each parent via recursive method
                    ScanParentForPrefabs(hierarchyInformation[i].hierarchyFolder.transform.GetChild(j), hierarchyInformation[i].vegetationType);
                }
            }
        }
        /// <summary>
        /// Scans all of the hierarchy folders that the user supplied and identifies prefab instances that can be converted to VSP
        /// Checks for duplicates and evaluates if the minimum number of instances is larger than what the user specified.
        /// Attemps to remove the 4th LOD if the user specifies, and will create a new prefab asset. (Useful to remove "CROSS" models which do not react to wind very well)
        /// </summary>
        /// <param name="vegetationPackage"> The vegetation package to add the items into</param>
        public void AddAllHierarchyPrefabsToVSPItems(VegetationPackagePro vegetationPackage)
        {
            prefabTransforms.Clear();
            prefabType.Clear();
            prefabCount.Clear();

            // Build the records of instances from the hierarchy provided to search
            ScanAllHierarchyItems();
            if (prefabCount.Count < 1) return; // Nothing was found 


            GameObject prefab;

            // Add the prefabs if they do not exist already in the vegetation package
            foreach (KeyValuePair<GameObject, int> kvp in prefabCount)
            {
                bool addPrefabToVegetationPackage = true;
                if (kvp.Value >= MinimumNumberOfInstancesRequiredToAdd)
                {
                    prefab = kvp.Key; // Default assignment

                    // Process removal of 4th LOD if applicable
                    if (CreateNewPrefabWith3LODs) prefab = LodRemover.Remove4thLOD(kvp.Key);

                    // Check for duplication in existing package and add only new items
                    foreach (VegetationItemInfoPro item in vegetationPackage.VegetationInfoList)
                    {
                        // Texture type prefabs wont be applicable in hierarchy search so we can skip them
                        if (item.PrefabType != VegetationPrefabType.Texture)
                        {
                            if (item.VegetationPrefab.name == prefab.name)
                            {
                                // Remove the instances from storage and then re-add them
                                if (VSP.PersistentVegetationStorage.PersistentVegetationStoragePackage != null)
                                    RemoveItemFromPersistentStorage(item.VegetationGuid, 3);

                                addPrefabToVegetationPackage = false;
                                break;
                            }
                        }
                    }
                    if (addPrefabToVegetationPackage)
                    {
                        string newVegetationItemID = Guid.NewGuid().ToString(); // Unique id for VSP lookup
                        VegetationType vegetationType = prefabType[kvp.Key];

                        // Add the vegetation item, enable runtime spawn is false because it will be stored
                        vegetationPackage.AddVegetationItem(prefab, vegetationType, false, newVegetationItemID);
                        /*
                        // For sorting viewing distance based on size
                        float size = 1;
                        if (prefab.GetComponent<MeshFilter>())
                            size = prefab.GetComponent<MeshFilter>().sharedMesh.bounds.size.magnitude;
                        else if (prefab.GetComponent<LODGroup>())
                            if (prefab.transform.GetChild(0).GetComponent<MeshFilter>())
                                size = prefab.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh.bounds.size.magnitude;
                        ShaderControlOverride(vegetationPackage.GetVegetationInfo(newVegetationItemID));
                        // TODO make pulbic array that the user can specify the values
                        if (vegetationType != VegetationType.Tree)
                        {
                            if (size < .5f)
                                vegetationPackage.GetVegetationInfo(newVegetationItemID).RenderDistanceFactor = .2f;
                            else if (size < .75f)
                                vegetationPackage.GetVegetationInfo(newVegetationItemID).RenderDistanceFactor = .3f;
                            else if (size < 2)
                                vegetationPackage.GetVegetationInfo(newVegetationItemID).RenderDistanceFactor = .4f;
                            else if (size < 4)
                                vegetationPackage.GetVegetationInfo(newVegetationItemID).RenderDistanceFactor = .5f;
                            else if (size < 6)
                                vegetationPackage.GetVegetationInfo(newVegetationItemID).RenderDistanceFactor = .6f;
                            else if (size < 8)
                                vegetationPackage.GetVegetationInfo(newVegetationItemID).RenderDistanceFactor = .7f;
                            else if (size > 8)
                                vegetationPackage.GetVegetationInfo(newVegetationItemID).RenderDistanceFactor = 3f;
                        }
                        */
                    }
                }
            }
        }
        /// <summary>
        /// Cycle through a hierarch for children that can be instanced
        /// does not instance objects with negative scaling 
        /// disables objects that are instanced and added to storage
        /// </summary>
        /// <param name="originOfThePrefab"></param>
        /// <param name="hierarchyIndex"></param>
        /// 
        public void ConvertHierarchyToVSPStorage()
        {
            if (VSP.VegetationPackageProList[VSP.VegetationPackageIndex] == null || VSP == null)
            {
                Debug.LogError("VSP or VegetationPackage is null");
                return;
            }

            Dictionary<GameObject, int> prefabIndex = GetPrefabDictionary(VSP.VegetationPackageProList[VSP.VegetationPackageIndex]);

            Bounds terrainBounds = Terrain.activeTerrain.terrainData.bounds;
            Bounds worldBounds = new Bounds(Terrain.activeTerrain.terrainData.bounds.center + Terrain.activeTerrain.transform.position, Terrain.activeTerrain.terrainData.bounds.size);
            foreach (Transform t in prefabTransforms)
            {
                t.gameObject.SetActive(true);
                GameObject go = t.gameObject;
                GameObject prefab;

                if (CreateNewPrefabWith3LODs)
                {
                    GameObject test = GetEquivalentConvertedTo3LOD(go);
                    prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(test);
                }
                else prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);

                if (prefab == null)
                {
                    Debug.Log("Heirachy item" + go.name + " is not an instance of a prefab");
                    continue;
                }

                if (!prefabIndex.ContainsKey(prefab))
                {
                    Debug.Log("Game object " + prefab.name + " does not exist as a vegetation item and will remain in the hierarchy");
                    continue;
                }

                // Negative scaling causes inside out meshes from being rendered
                bool scaleError = (go.transform.localScale.x < 0 || go.transform.localScale.y < 0 || go.transform.localScale.z < 0);
                if (scaleError)
                {
                    Debug.Log("Object (" + go.name + ") contains negative scaling and can not be added and will remain enabled in hierarchy");
                    continue;
                }

                bool outOfBounds = false;
                if (go.GetComponentInChildren<MeshRenderer>())
                {
                    Bounds modifiedObjectBounds = go.GetComponentInChildren<MeshRenderer>().bounds;
                    if (modifiedObjectBounds.center.x > worldBounds.center.x + worldBounds.extents.x || modifiedObjectBounds.center.x < worldBounds.center.x - worldBounds.extents.x)
                        outOfBounds = true;
                    else if (modifiedObjectBounds.center.z > worldBounds.center.z + worldBounds.extents.z || modifiedObjectBounds.center.z < worldBounds.center.z - worldBounds.extents.z)
                        outOfBounds = true;
                    else if (modifiedObjectBounds.center.y > worldBounds.center.y + worldBounds.extents.y || modifiedObjectBounds.center.y < worldBounds.center.y - worldBounds.extents.y)
                        outOfBounds = true;
                    //Debug.Log(worldBounds + " " + modifiedObjectBounds + " " + go.transform.position);
                }

                // Objects out of a terrain cannot be added because they will render incorrectly 
                if (outOfBounds)
                {
                    Debug.Log("Object (" + go.name + ") is not inside of a terrain and can not be added and will remain enabled in hierarchy");
                    continue;
                }

                int prefabItemIndex = -1; // Get the index for the prefab relative to the stored vegetation info
                bool vsPrefab = false;

                // Try to convert to VS prefab supplied with nature manufacture environment
                if (TryToConvertToNMVSPrefabs)
                {
                    GameObject ConvertedPrefab = NatureManufactureVSPrefabLocator.TryToConvertNMPrefabToNMVSPrefab(prefab);
                    // See if this is a valid prefab in our item list
                    if (prefab != ConvertedPrefab)
                    {
                        if (prefabIndex.TryGetValue(prefab, out prefabItemIndex))
                        {
                            vsPrefab = true;
                            prefab = ConvertedPrefab;
                        }
                        else vsPrefab = false;
                    }
                }

                bool addPrefab = vsPrefab;

                // Try to add the prefab that was on the terrain
                if (!vsPrefab)
                {
                    if (CreateNewPrefabWith3LODs) // Will still try the original prefab if this fails
                    {
                        // Request the modified prefab to see if this is a valid prefab in out item list
                        if (prefabIndex.TryGetValue(GetEquivalentConvertedTo3LOD(prefab), out prefabItemIndex))
                            addPrefab = true;
                    }
                    if (addPrefab == false)
                    {
                        // See if this is a valid prefab in out item list
                        if (prefabIndex.TryGetValue(prefab, out prefabItemIndex))
                            addPrefab = true;
                    }
                }

                if (prefabItemIndex == -1) continue; // Dictionary did not contain this prefab

                if (addPrefab)
                {
                    string vegetationGUID = VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[prefabItemIndex].VegetationGuid;
                    string vegetationID = VegetationStudioManager.GetVegetationItemID(vegetationGUID);

                    Vector3 position = go.transform.position;
                    Vector3 scale = go.transform.localScale;
                    Quaternion rotation = go.transform.rotation;

                    //Debug.Log(position + " " + scale + " " + rotation);
                    Storage.AddVegetationItemInstance(vegetationID, position, scale, rotation, false, 3, 1, false);
                    ShaderControlOverride(VSP.VegetationPackageProList[VSP.VegetationPackageIndex].GetVegetationInfo(vegetationID));
                    t.gameObject.SetActive(false);
                }
            }
        }
    }
}
#endif