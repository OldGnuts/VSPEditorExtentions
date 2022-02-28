#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.Vegetation.PersistentStorage;

namespace VegetationStudioProEditorExtentions
{
    public static class VSPPersistentStorageTools
    {
        /// <summary>
        /// Creates or loads a persistent storage package. If the Persistent storage folder does not exist, this will attempt to create one.
        /// TODO: Allow user to specify the folder 
        /// </summary>
        public static void CreateOrLoadPersistentVegetationStorage(VegetationSystemPro VSP, string storageFileName)
        {
            if (VSP.PersistentVegetationStorage.PersistentVegetationStoragePackage == null)
            {
                Debug.Log("Creating new storage");
                if (storageFileName == "")
                    storageFileName = "PersistentVegetationStorage_" + Guid.NewGuid() + "Generated" + ".asset";

                PersistentVegetationStoragePackage storagePackage =
                    PersistentVegetationStoragePackage.CreateInstance<PersistentVegetationStoragePackage>();

                if (!AssetDatabase.IsValidFolder("Assets/PersistentVegetationStorageData"))
                    AssetDatabase.CreateFolder("Assets", "PersistentVegetationStorageData");

                AssetDatabase.CreateAsset(storagePackage, "Assets/PersistentVegetationStorageData/" + storageFileName);
                PersistentVegetationStoragePackage loadedPackage = AssetDatabase.LoadAssetAtPath<PersistentVegetationStoragePackage>("Assets/PersistentVegetationStorageData/" + storageFileName);

                VSP.PersistentVegetationStorage.PersistentVegetationStoragePackage = loadedPackage;
                VSP.PersistentVegetationStorage.InitializePersistentStorage();
            }
            else
            {
                if (!IsPersistentStorageInitializedAndValid(VSP))
                {
                    Debug.Log("InitializingStorage because InstaceInfoList is < 1");
                    VSP.PersistentVegetationStorage.InitializePersistentStorage();
                }
            }
        }
        /// <summary>
        /// Checks if the cell count on the vegetation system and the storage package are equal
        /// </summary>
        /// <param name="VSP">VegetationSystemPro component</param>
        /// <returns></returns>
        public static bool IsPersistentStorageInitializedAndValid(VegetationSystemPro VSP)
        {
            if (VSP.PersistentVegetationStorage.PersistentVegetationStoragePackage.PersistentVegetationCellList.Count !=
                VSP.VegetationCellList.Count) return false;
            return true; ;
        }

        /// <summary>
        /// Looks through the persistent storage and look for a matching prefab name
        /// if the prefab model name exists in this package and in the storage then return the instance count
        /// </summary>
        /// <param name="prefabName"></param>
        /// <returns></returns>
        public static int GetStoredInstancesCount(string prefabName, VegetationSystemPro VSP)
        {
            List<PersistentVegetationInstanceInfo> instanceList = VSP.PersistentVegetationStorage.PersistentVegetationStoragePackage.GetPersistentVegetationInstanceInfoList();
            string id = "";
            for (int i = 0; i < VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList.Count; i++)
                if (VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[i].VegetationPrefab.name == prefabName)
                    id = VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[i].VegetationItemID;

            if (id == "") return -1;

            int count = 0;
            for (int i = 0; i <= instanceList.Count - 1; i++)
                if (instanceList[i].VegetationItemID == id)
                    count = instanceList[i].Count;
            return count;
        }
    }
}
#endif
