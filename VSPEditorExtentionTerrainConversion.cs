#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.Vegetation.PersistentStorage;
namespace VegetationStudioProEditorExtentions
{
    public partial class VSPEditorExtention : EditorWindow
    {
        /// <summary>
        /// Converts Unity terrain TreeInstances into VSP trees, note that not all Unity trees may actually be a tree.
        /// </summary>
        /// <param></param>
        public void ConvertUnityTreeInstanceToVSP()
        {
            if (Terrain.activeTerrain == null || VSP.VegetationPackageProList[VSP.VegetationPackageIndex] == null)
            {
                Debug.LogError("Terrain or VegetationPackage is null");
                return;
            }
            // Debug.Log(VSP.VegetationPackageProList[VSP.VegetationPackageIndex]);
            Terrain terrain = Terrain.activeTerrain;
            TerrainData tdata = terrain.terrainData;
            Dictionary<GameObject, int> prefabIndex = GetPrefabDictionary(VSP.VegetationPackageProList[VSP.VegetationPackageIndex]);
            //foreach (KeyValuePair<GameObject, int> kp in prefabIndex) Debug.Log(kp.Key);
            //Debug.Log("Vegetation items in package " + prefabIndex.Count);

            // Get some information about the terrain
            float width = tdata.size.x;
            float height = tdata.size.z;
            float terrainY = tdata.size.y;

            for (int i = 0; i < tdata.treeInstanceCount; i++)
            {
                // Current treeInstance in treeInstance array (can be plants too, but its a gameObject)
                TreeInstance ti = tdata.treeInstances[i];
                int prefabItemIndex = 0;
                bool vsPrefab = false;

                // Try to convert to VS prefab supplied with nature manufacture environment
                if (TryToConvertToNMVSPrefabs)
                {
                    GameObject prefab = NatureManufactureVSPrefabLocator.TryToConvertNMPrefabToNMVSPrefab(tdata.treePrototypes[ti.prototypeIndex].prefab);
                    // See if this is a valid prefab in our item list
                    if (prefab != tdata.treePrototypes[ti.prototypeIndex].prefab)
                    {
                        vsPrefab = true;
                        if (!prefabIndex.TryGetValue(prefab, out prefabItemIndex))
                            vsPrefab = false;
                        //     Debug.Log("VSPrefab is " + vsPrefab + ". Prefab name " + prefab.name);
                    }
                }

                bool addPrefab = vsPrefab;

                // Try to add the prefab that was on the terrain
                if (!vsPrefab)
                {
                    if (CreateNewPrefabWith3LODs) // Will still try the original prefab if this fails
                    {
                        // Request the modified prefab to see if this is a valid prefab in out item list
                        if (prefabIndex.TryGetValue(GetEquivalentConvertedTo3LOD(tdata.treePrototypes[ti.prototypeIndex].prefab), out prefabItemIndex))
                            addPrefab = true;
                    }
                    if (addPrefab == false) // See if this is a valid prefab in out item list
                    {
                        if (prefabIndex.TryGetValue(tdata.treePrototypes[ti.prototypeIndex].prefab, out prefabItemIndex))
                            addPrefab = true;
                    }
                }
                // Debug.Log("Ok to add prefab " + addPrefab + " Is a VSPrefab? " + vsPrefab);

                if (addPrefab)
                {

                    // Debug.Log("Adding prefab named " + VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[prefabItemIndex].VegetationPrefab.name);
                    //Debug.Log(prefabItemIndex);

                    // The assetGUID for the vegetationInfo (only unique for the INFO not the INSTANCE)
                    string vegetationGUID = VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[prefabItemIndex].VegetationGuid;
                    // Ask VSP to give us a unique GUID for this particular storage instance
                    string vegetationID = VegetationStudioManager.GetVegetationItemID(vegetationGUID);
                    //Debug.Log(vegetationGUID);

                    // Convert the position, rotation and scale to world position, standard rotation and scale
                    // TreeInstance[] stores its data differently than VSP
                    Vector3 position = new Vector3((ti.position.x * width) + terrain.GetPosition().x, ti.position.y * terrainY, (ti.position.z * height) + terrain.GetPosition().z);
                    Vector3 scale = new Vector3(ti.widthScale, ti.heightScale, ti.widthScale);
                    Quaternion rotation = Quaternion.Euler(0f, Mathf.Rad2Deg * ti.rotation, 0f);

                    // What is distance falloff?
                    // Add the current treeInstance that was converted into the persistent vegetation storage. 
                    // CellCache is false because we are bulk adding and will refresh VSP after the whole terrain is converted.

                    Storage.AddVegetationItemInstance(vegetationID, position, scale, rotation, false, 2, 1, false);
                    //ShaderControlOverride(VSP.VegetationPackageProList[VSP.VegetationPackageIndex].GetVegetationInfo(vegetationID));
                }
            }
        }

        int[] TreeInstanceCount()
        {
            // Make a quick index of trees
            TreeInstance[] trees = Terrain.activeTerrain.terrainData.treeInstances;
            int[] counts = new int[Terrain.activeTerrain.terrainData.treePrototypes.Length];
            for (int i = 0; i < trees.Length; i++)
            {
                counts[trees[i].prototypeIndex]++;
            }
            return counts;
        }

        int[] DetailInstanceCount()
        {
            Terrain terrain = Terrain.activeTerrain;
            DetailPrototype[] detailPrototypes = terrain.terrainData.detailPrototypes;
            TerrainData tdata = terrain.terrainData;
            int[] instances = new int[detailPrototypes.Length];

            for (int i = 0; i < detailPrototypes.Length; i++)
            {
                int[,] layerInstances = tdata.GetDetailLayer(0, 0, tdata.detailResolution, tdata.detailResolution, i);
                for (int x = 0; x < tdata.detailResolution; x++)
                {
                    for (int y = 0; y < tdata.detailResolution; y++)
                    {
                        instances[i] += layerInstances[x, y];
                    }
                }
            }
            return instances;
        }
        /// <summary>
        /// Adds all of the trees in the treeInstance[] in the terrain as VegetationItemInfo to the current vegetation package
        /// </summary>
        public void AddAllTerrainTreesToVSPItems()
        {
            Dictionary<GameObject, int> prefabIndex = GetPrefabDictionary(VSP.VegetationPackageProList[VSP.VegetationPackageIndex]);
            int[] instances = TreeInstanceCount();
            TreePrototype[] trees = Terrain.activeTerrain.terrainData.treePrototypes;
            for (int i = 0; i < trees.Length; i++)
            {
                // Debug.Log(i + " " + Terrain.activeTerrain.terrainData.treePrototypes.Length + " " + instances.Count);
                // Check to see if there are any instances, and also check the minimum instance count to add
                if (instances[i] == 0)
                    continue;
                else if (instances[i] < MinimumNumberOfInstancesRequiredToAdd)
                    continue;

                bool vsPrefab = false;
                bool addPrefab = false;

                GameObject prefab = trees[i].prefab;
                // Try to convert to VS prefab supplied with nature manufacture environment
                if (TryToConvertToNMVSPrefabs)
                {
                    prefab = NatureManufactureVSPrefabLocator.TryToConvertNMPrefabToNMVSPrefab(trees[i].prefab);
                    // See if this is a valid prefab in our item list
                    if (prefab != trees[i].prefab)
                        vsPrefab = !prefabIndex.ContainsKey(prefab);
                    else prefab = trees[i].prefab;
                    Debug.Log("Tried converting. vsPrefab is " + vsPrefab + ". Prefab name is " + prefab.name);
                }
                else if (CreateNewPrefabWith3LODs)
                {
                    prefab = GetEquivalentConvertedTo3LOD(prefab);
                }
                // Try to add the prefab that was on the terrain
                if (!vsPrefab)
                {
                    // See if this is a valid prefab in out item list
                    addPrefab = !prefabIndex.ContainsKey(prefab);
                }

                // Try to remove storage instances before adding them
                if (VSP.PersistentVegetationStorage.PersistentVegetationStoragePackage != null)
                {
                    if (prefabIndex.TryGetValue(prefab, out int index))
                        RemoveItemFromPersistentStorage(index, 2);
                }

                //Debug.Log("Ok to add prefab " + prefab.name + ", " + addPrefab +  ". vsPrefab ? " + vsPrefab);

                if (addPrefab || vsPrefab)
                {
                    // Debug.Log("Adding terrain tree prefab");
                    string newVegetationItemID = Guid.NewGuid().ToString();
                    // Does not enable runtime spawn for the new item because it will be spawned from the persistent storage
                    float size = 1;
                    if (prefab.GetComponent<MeshFilter>())
                        size = prefab.GetComponent<MeshFilter>().sharedMesh.bounds.size.magnitude;
                    else if (prefab.GetComponent<LODGroup>())
                        if (prefab.transform.GetChild(0).GetComponent<MeshFilter>())
                            size = prefab.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh.bounds.size.magnitude;

                    if (CreateNewPrefabWith3LODs && !vsPrefab) // NM VSPrefabs already only contain 3 LODs
                        prefab = LodRemover.Remove4thLOD(prefab); // Create new prefab with 3 LODs


                    string name = prefab.name.ToLower();

                    // Try to sort out hierarchy with differing item types by size or name
                    // TODO make more robust
                    if ((size < 2f || name.Contains("grass")) && !name.Contains("tree"))
                        VSP.VegetationPackageProList[VSP.VegetationPackageIndex].AddVegetationItem(prefab, VegetationType.Grass, false, newVegetationItemID);

                    else if ((size < 3.6f || name.Contains("bush") || name.Contains("plant") || name.Contains("fern")) && !name.Contains("tree"))
                        VSP.VegetationPackageProList[VSP.VegetationPackageIndex].AddVegetationItem(prefab, VegetationType.Plant, false, newVegetationItemID);

                    else VSP.VegetationPackageProList[VSP.VegetationPackageIndex].AddVegetationItem(prefab, VegetationType.Tree, false, newVegetationItemID);
                }
            }
        }


        public void ConvertTerrainDetailsToVSP(Terrain terrain)
        {
            PersistentVegetationStorage storage = VSP.PersistentVegetationStorage;
            storage.SelectedVegetationPackageIndex = VSP.VegetationPackageIndex;

            AddTerrainDetailPrototypesToVSPItemInfo(VSP, terrain); // Adds detail prototypes as the VSP Equivalent
            SceneUtility.SetSceneDirtyVegetationSystemOnly(VSP);
            CreateItemInstancesAndStore(VSP, terrain);
            SceneUtility.SetSceneDirtySystemAndVegetaionPackage(VSP);
        }
        /// <summary>
        /// Adds detail prototypes as the VSP Equivalent VegetationItemInfoPro
        /// </summary>
        /// <param name="VSP"></param>
        /// <param name="terrain"></param>
        void AddTerrainDetailPrototypesToVSPItemInfo(VegetationSystemPro VSP, Terrain terrain)
        {
            TerrainData tdata = terrain.terrainData;
            int[] instances = DetailInstanceCount();
            for (int i = 0; i < tdata.detailPrototypes.Length; i++)
            {
                // Check minimum requirements
                if (instances[i] == 0 || instances[i] < MinimumNumberOfInstancesRequiredToAdd)
                    continue;

                bool duplicate = false;
                foreach (VegetationItemInfoPro item in VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList)
                {
                    if (item.VegetationTexture != null)
                    {
                        if (item.VegetationTexture == tdata.detailPrototypes[i].prototypeTexture)
                        {
                            duplicate = true;
                            break;
                        }
                    }
                    else if (item.VegetationPrefab == tdata.detailPrototypes[i].prototype)
                    {
                        duplicate = true;
                        break;
                    }
                }
                if (!duplicate)
                {
                    string newVegetationItemID = Guid.NewGuid().ToString();
                    if (tdata.detailPrototypes[i].renderMode == DetailRenderMode.Grass)
                    {
                        VSP.VegetationPackageProList[VSP.VegetationPackageIndex].AddVegetationItem(tdata.detailPrototypes[i].prototypeTexture, VegetationType.Grass, false, newVegetationItemID);
                    }
                    else if (tdata.detailPrototypes[i].renderMode == DetailRenderMode.VertexLit)
                    {
                        VSP.VegetationPackageProList[VSP.VegetationPackageIndex].AddVegetationItem(tdata.detailPrototypes[i].prototype, VegetationType.Grass, false, newVegetationItemID);
                    }
                    for (int j = 0; j < VSP.VegetationPackageProList[VSP.VegetationPackageIndex].GetVegetationInfo(newVegetationItemID).ShaderControllerSettings.ControlerPropertyList.Count; j++)
                    {
                        if (VSP.VegetationPackageProList[VSP.VegetationPackageIndex].GetVegetationInfo(newVegetationItemID).ShaderControllerSettings.ControlerPropertyList[j].PropertyName == "TintColor1")
                        {
                            VSP.VegetationPackageProList[VSP.VegetationPackageIndex].GetVegetationInfo(newVegetationItemID).ShaderControllerSettings.ControlerPropertyList[j].ColorValue =
                                tdata.detailPrototypes[i].dryColor;
                        }
                        if (VSP.VegetationPackageProList[VSP.VegetationPackageIndex].GetVegetationInfo(newVegetationItemID).ShaderControllerSettings.ControlerPropertyList[j].PropertyName == "TintColor2")
                        {
                            VSP.VegetationPackageProList[VSP.VegetationPackageIndex].GetVegetationInfo(newVegetationItemID).ShaderControllerSettings.ControlerPropertyList[j].ColorValue =
                                tdata.detailPrototypes[i].healthyColor;
                        }
                    }
                }
            }
        }
        void CreateItemInstancesAndStore(VegetationSystemPro VSP, Terrain terrain)
        {
            //PersistentVegetationStorage storage = VSP.PersistentVegetationStorage;
            RaycastHit hit;
            Ray ray;
            TerrainData tdata = terrain.terrainData;

            int patchSize = (int)(tdata.size.x / tdata.detailPatchCount);

            int detailWidth = tdata.detailWidth;
            int detailHeight = tdata.detailHeight;

            float delatilWToTerrainW = tdata.size.x / detailWidth;
            float delatilHToTerrainW = tdata.size.z / detailHeight;

            Vector3 mapPosition = terrain.transform.position;
            DetailPrototype[] details = tdata.detailPrototypes;

            for (int i = 0; i < details.Length; i++)
            {
                GameObject Prefab = details[i].prototype;
                Texture2D texture = details[i].prototypeTexture;

                float minWidth = details[i].minWidth;
                float maxWidth = details[i].maxWidth;

                float minHeight = details[i].minHeight;
                float maxHeight = details[i].maxHeight;
                int itemIndex = -1;
                bool prefabTexture = false;
                for (int index = 0; index < VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList.Count; index++)
                {
                    if (VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[index].PrefabType == VegetationPrefabType.Texture && tdata.detailPrototypes[i].prototypeTexture != null)
                    {
                        if (VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[index].VegetationTexture == tdata.detailPrototypes[i].prototypeTexture)
                        {
                            prefabTexture = true;
                            itemIndex = index;
                            // Debug.Log("TexturePrefab " + itemIndex);
                            break;
                        }
                    }
                    else if (VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[index].VegetationPrefab == tdata.detailPrototypes[i].prototype)
                    {
                        prefabTexture = false;
                        itemIndex = index;
                        // Debug.Log("Prefab");
                        break;
                    }
                }
                // Item is not part of the vegetation package
                if (itemIndex == -1)
                {
                    Debug.Log("Prefab is not in VSP package");
                    continue;
                }
                // Remove the stored instances that would be previously added
                if (VSP.PersistentVegetationStorage.PersistentVegetationStoragePackage != null)
                {
                    RemoveItemFromPersistentStorage(itemIndex, 4);
                }
                //Debug.Log(i);
                int[,] map = tdata.GetDetailLayer(0, 0, tdata.detailWidth, tdata.detailHeight, i);

                for (var y = 0; y < tdata.detailHeight; y++)
                {
                    for (var x = 0; x < tdata.detailWidth; x++)
                    {
                        if (map[x, y] > 0)
                        {
                            for (int j = 0; j < map[x, y]; j++)
                            {
                                float rndZ = (float)delatilWToTerrainW * UnityEngine.Random.Range(0f, 1f);
                                float rndx = (float)delatilHToTerrainW * UnityEngine.Random.Range(0f, 1f);
                                float _z = (x * delatilWToTerrainW) + mapPosition.z + rndZ;
                                float _x = (y * delatilHToTerrainW) + mapPosition.x + rndx;
                                float _y = terrain.SampleHeight(new Vector3(_x, 0, _z));
                                Vector3 position = new Vector3(_x, _y, _z);
                                Vector3 scale = new Vector3(UnityEngine.Random.Range(minWidth, maxWidth), UnityEngine.Random.Range(minHeight, maxHeight), UnityEngine.Random.Range(minWidth, maxWidth));
                                Quaternion rotation = Quaternion.identity;
                                if (prefabTexture)
                                {
                                    ray = new Ray(position + Vector3.up * 2, Vector3.down);
                                    if (Physics.Raycast(ray, out hit))
                                    {
                                        Vector3 normal = hit.normal.normalized;
                                        Vector3 lookAt = Vector3.Cross(-normal, new Vector3(1, 0, 0));
                                        if (lookAt.y < 0) lookAt = -lookAt;
                                        rotation = UnityEngine.Quaternion.LookRotation(lookAt, normal);
                                        rotation *= Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), new Vector3(0, 1, 0));
                                    }
                                    string vegetationGUID = VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[itemIndex].VegetationGuid;
                                    string vegetationID = VegetationStudioManager.GetVegetationItemID(vegetationGUID);
                                    //Debug.Log("Adding " + VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[itemIndex].VegetationTexture.name);
                                    VSP.PersistentVegetationStorage.AddVegetationItemInstance(vegetationID, position, scale, rotation, false, 4, 2, false);
                                }
                                else
                                {
                                    ray = new Ray(position + Vector3.up * 2, Vector3.down);
                                    if (Physics.Raycast(ray, out hit))
                                    {
                                        Vector3 normal = hit.normal.normalized;
                                        Vector3 lookAt = Vector3.Cross(-normal, new Vector3(1, 0, 0));
                                        if (lookAt.y < 0) lookAt = -lookAt;
                                        rotation = UnityEngine.Quaternion.LookRotation(lookAt, normal);
                                        rotation *= Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), new Vector3(0, 1, 0));
                                    }
                                    string vegetationGUID = VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[itemIndex].VegetationGuid;
                                    string vegetationID = VegetationStudioManager.GetVegetationItemID(vegetationGUID);
                                    VSP.PersistentVegetationStorage.AddVegetationItemInstance(vegetationID, position, scale, rotation, false, 4, 100, false);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
#endif