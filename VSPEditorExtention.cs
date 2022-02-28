#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.Vegetation.PersistentStorage;

namespace VegetationStudioProEditorExtentions
{
    public partial class VSPEditorExtention : EditorWindow
    {
        //public bool CreateQuadsForTextureGrass = true;
        private static readonly string[] VegetationTypeNames = { "All", "Trees", "Large Objects", "Objects", "Plants", "Grass" };
        public VegetationSystemPro VSP;
        public VegetationPackagePro Package
        {
            get
            {
                return VSP.VegetationPackageProList[VSP.VegetationPackageIndex];
            }
        } // Current package
        public PersistentVegetationStorage Storage
        {
            get { return VSP.PersistentVegetationStorage; }
        } // The storage component, not the storage package
        public string[] PackageNames
        {
            get
            {
                string[] names = new string[VSP.VegetationPackageProList.Count];
                for (int i = 0; i < VSP.VegetationPackageProList.Count; i++)
                    names[i] = "Name : " + VSP.VegetationPackageProList[i].PackageName;
                return names;
            }
        }

        public bool showPositionRules = false;
        public bool copySamplePosition = false;
        public bool copyDensity = false;
        public bool copyRotationSettings = false;
        public bool copyHeightSettings = false;
        public bool copySteepnessSettings = false;
        public bool copyPositionOffsetSettings = false;
        public bool copyScaleSettings = false;
        public bool copyHeightOffset = false;

        public bool showMiscSettings = false;
        public bool copybillboardSettings = false;
        public bool copyColliderType = false;
        public bool copyLodSettings = false;
        public bool copyDistanceFalloff = false;
        public bool copyFoliageSettings = false;

        public bool showRuleListToCopy = false;
        public bool copyNoiseCutoffRules = false;
        public bool copyNoiseDensityRules = false;
        public bool copyNoiseScaleRules = false;
        public bool copyBiomeEdgeIncludeRules = false;
        public bool copyBiomeEdgeScaleRules = false;
        public bool copyConcaveLocationRules = false;
        public bool copyTerrainTextureIncludeRules = false;
        public bool copyTerrainTextureExcludeRules = false;
        public bool copyTextureMaskIncludeRules = false;
        public bool copyTextureMaskExcludeRules = false;
        public bool copyVegetationMaskRule = false;

        public bool HierarchyAdded = false;

        public bool CreateNewPrefabWith3LODs = false;
        public bool TryToConvertToNMVSPrefabs = false;
        public bool ConvertTerrainTrees = false;
        public bool ConvertTerrainDetails = false;
        public int MinimumNumberOfInstancesRequiredToAdd = 10;
        public int currentTab = 0;
        public int currentBulkEditingTab = 0;
        public int currentEditActionTab = 0;
        public int currentSelectedVegetationType = 0;
        public int currentSelectedGridIndex = 0;
        public int currentSelectedVegetationIndex = 0;

        public int currentSelectedPasteToVegetationType = 0;
        public int currentSelectedPasteToGridIndex = 0;
        public int currentSelectedPasteToVegetationIndex = 0;

        public VegetationItemInfoPro copiedItem;
        public bool showImageList = true;

        public List<bool> vegetationItemFieldsToCopy = new List<bool>();


        public int bulkCopyToSettingsType = 0;
        public int bulkItemsToDelete = 0;
        public int bulkEnableRuntimeSpawn = 0;

        public int packageToPasteTo = 0;
        public int packageToPasteAllOfType = 0;

        public bool copyAllSettings = false;
        public bool allowDuplicateItems = false;
        public bool showPrefabSettings = false;
        public bool alwaysEnableRuntimeSpawnOnPastedItems = true;
        public bool createPackageAndAdd = false;
        public BiomeType createPackageBiomeType = BiomeType.Default;
        public string[] vegetationTypes;

        public string vegetationPackageFileName = "";
        public string storageFileName = "";

        public Dictionary<GameObject, bool> hasPrefabsAttached = new Dictionary<GameObject, bool>();
        public Dictionary<GameObject, VegetationType> prefabType = new Dictionary<GameObject, VegetationType>();
        public Dictionary<GameObject, int> prefabCount = new Dictionary<GameObject, int>();
        
        public List<HierarchyInformation> hierarchyInformation = new  List<HierarchyInformation>();

        /// <summary>
        /// Checks if the necessary VSP components are availalbe for use with editing tools
        /// </summary>
        /// <returns></returns>
        public bool ValidityCheck()
        {
            // Check all references
            string errorString = "";
            bool valid = true;
            if (VSP == null)
            {
                VSP = GameObject.FindObjectOfType<VegetationSystemPro>();
                if (VSP == null)
                {
                    valid = false;
                    errorString += " Vegetation System Pro not found.";
                }
            }
            if (VSP != null)
            {
                // Validate vegetation packages
                if (VSP.VegetationPackageProList == null || VSP.VegetationPackageProList.Count == 0)
                {
                    errorString += "\n No Vegetation Packages found in the Vegetation System Pro component.";
                    valid = false;
                }
                // Validate persistent storage
                if (VSP.PersistentVegetationStorage == null || VSP.PersistentVegetationStorage.PersistentVegetationStoragePackage == null)
                {
                    errorString += "\n No Vegetation storage found.";
                    //valid = false;
                }
            }

            //if (errorString != "") Debug.LogError(errorString);
            return valid;
        }

        /// <summary>
        /// Checks that the necessary VSP components are available to use with import tools
        /// </summary>
        /// <returns></returns>
        public bool ImporterValidityCheck()
        {
            // Check all references
            string errorString = "";
            bool valid = true;
            if (VSP == null)
            {
                VSP = GameObject.FindObjectOfType<VegetationSystemPro>();
                if (VSP == null)
                {
                    valid = false;
                    errorString += " Vegetation System Pro not found.";
                }
            }
            if (Terrain.activeTerrain == null)
            {
                errorString += "\n No terrain found.";
                valid = false;
            }
            //if (errorString != "") Debug.LogError(errorString);
            return valid;
        }
      
        /// <summary>
        /// Creates a dictionary with the current vegetation item prefabs paired to an index of the vegetationItemInfo for that prefab
        /// </summary>
        /// <param name="vegetationPackage"></param>
        /// <returns></returns>
        public Dictionary<GameObject, int> GetPrefabDictionary(VegetationPackagePro vegetationPackage)
        {
            Dictionary<GameObject, int> prefabInstances = new Dictionary<GameObject, int>();
            for (int i = 0; i < vegetationPackage.VegetationInfoList.Count; i++)
            {
                if (vegetationPackage.VegetationInfoList[i].PrefabType == VegetationPrefabType.Mesh)
                {
                    if (!prefabInstances.ContainsKey(vegetationPackage.VegetationInfoList[i].VegetationPrefab))
                        prefabInstances.Add(vegetationPackage.VegetationInfoList[i].VegetationPrefab, i);
                }
            }
            return prefabInstances;
        }

        /// <summary>
        /// Returns a prefab that has been reduced to 3 LODS if it exists
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns>The modified prefab asset if it exists, otherwise returns the input prefab </returns>
        public GameObject GetEquivalentConvertedTo3LOD(GameObject prefab)
        {
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);
            string newPath = path.Replace(".prefab", "");
            newPath += "_3LODs.prefab";

            if (System.IO.File.Exists(newPath)) // Already created
            {
                //Debug.Log("Already exists :" + newPath);
                return AssetDatabase.LoadAssetAtPath<GameObject>(newPath);
            }
            else return prefab;
        }

        /// <summary>
        /// Hack to integrate alternate shader systems 
        /// </summary>
        /// <param name="item"></param>
        public void ShaderControlOverride(VegetationItemInfoPro item)
        {
            //item.ShaderControllerSettings.Description = "Use NM";
            if (item.VegetationType == VegetationType.Grass)
            {
                item.ShaderControllerSettings.Heading = "Grass"; // Hacky way to interface with vsp but it works
            }
            else if (item.VegetationType == VegetationType.Plant)
            {
                item.ShaderControllerSettings.Heading = "Plant"; // Hacky way to interface with vsp but it works
            }
            else if (item.VegetationType != VegetationType.Tree)
            {
                item.ShaderControllerSettings.Heading = "Not Foliage"; // Hacky way to interface with vsp but it works
            }
            //Debug.Log("Overriding Shader");
            //if (Shader.Find("NatureManufacture Shaders/Standard Shaders/Standard Metalic Snow"))
            //Debug.Log("Found Shader");

        }
    }
}
#endif