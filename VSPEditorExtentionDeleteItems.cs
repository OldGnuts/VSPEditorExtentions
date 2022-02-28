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
        public void RemoveItemFromPersistentStorage(int itemIndex, byte source)
        {
            string vegetationGUID = VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[itemIndex].VegetationGuid;
            string vegetationID = VegetationStudioManager.GetVegetationItemID(vegetationGUID);

            // Remove from all standard sources (Baked, painted, imported etc)
            VSP.PersistentVegetationStorage.RemoveVegetationItemInstances(vegetationID, source);
            // Refresh
            VSP.ClearCache(vegetationID);
            EditorUtility.SetDirty(VSP.PersistentVegetationStorage.PersistentVegetationStoragePackage);
        }
        /// <summary>
        /// Removes an all instances of an item type form persistent storage from a specific source
        /// </summary>
        /// <param></param>
        public void RemoveItemFromPersistentStorage(string vegetationGUID, byte source)
        {
              string vegetationID = VegetationStudioManager.GetVegetationItemID(vegetationGUID);

            // Remove from all standard sources (Baked, painted, imported etc)
            VSP.PersistentVegetationStorage.RemoveVegetationItemInstances(vegetationID, source);
            // Refresh
            VSP.ClearCache(vegetationID);
            EditorUtility.SetDirty(VSP.PersistentVegetationStorage.PersistentVegetationStoragePackage);
        }
        /// <summary>
        /// Removes an all instances of an item type form persistent storage
        /// </summary>
        /// <param name="itemIndex"></param>
        public void RemoveItemFromPersistentStorage(int itemIndex)
        {
            string vegetationGUID = VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[itemIndex].VegetationGuid;
            string vegetationID = VegetationStudioManager.GetVegetationItemID(vegetationGUID);

            // Remove from all standard sources (Baked, painted, imported etc)
            VSP.PersistentVegetationStorage.RemoveVegetationItemInstances(vegetationID, (byte)0);
            VSP.PersistentVegetationStorage.RemoveVegetationItemInstances(vegetationID, (byte)1);
            VSP.PersistentVegetationStorage.RemoveVegetationItemInstances(vegetationID, (byte)2);
            VSP.PersistentVegetationStorage.RemoveVegetationItemInstances(vegetationID, (byte)3);
            VSP.PersistentVegetationStorage.RemoveVegetationItemInstances(vegetationID, (byte)4);
            VSP.PersistentVegetationStorage.RemoveVegetationItemInstances(vegetationID, (byte)5);
            // Refresh
            VSP.ClearCache(vegetationID);
            EditorUtility.SetDirty(VSP.PersistentVegetationStorage.PersistentVegetationStoragePackage);
        }
        /// <summary>
        /// Removes all items imported from terrain or hierarchy
        /// </summary>
        public void ClearStoredVegetationFromImportSource(byte source)
        {
            for (int i = 0; i < Storage.PersistentVegetationStoragePackage.PersistentVegetationInstanceInfoList.Count; i++)
            {
                for (int j = 0; j < Storage.PersistentVegetationStoragePackage.PersistentVegetationInstanceInfoList[i].SourceCountList.Count; j++)
                {
                    if (Storage.PersistentVegetationStoragePackage.PersistentVegetationInstanceInfoList[i].SourceCountList[j].VegetationSourceID == source)
                    {
                        Storage.RemoveVegetationItemInstances(Storage.PersistentVegetationStoragePackage.PersistentVegetationInstanceInfoList[i].VegetationItemID, source);
                        VSP.ClearCache(Storage.PersistentVegetationStoragePackage.PersistentVegetationInstanceInfoList[i].VegetationItemID);
                    }
                }
            }
        }
        /// <summary>
        /// Removes the selected vegetation item from the selected vegetation package
        /// </summary>
        public void DeleteVegetationItem()
        {
            if (Storage.PersistentVegetationStoragePackage != null) RemoveItemFromPersistentStorage(currentSelectedVegetationIndex);
            VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList.RemoveAt(currentSelectedVegetationIndex);
            SceneUtility.SetSceneDirtySystemAndVegetaionPackage(VSP);
        }
        /// <summary>
        /// Deletes all of a specified vegetation type. The type can be all types as well.
        /// </summary>
        public void DeleteVegetationItems()
        {
            if (VSP.VegetationPackageProList[VSP.VegetationPackageIndex] != null)
            {
                Debug.Log(bulkItemsToDelete);
                if (bulkItemsToDelete == 0) // All
                {
                    for (int i = VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList.Count - 1; i > -1; i--)
                        VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList.RemoveAt(i);
                    SceneUtility.SetSceneDirtySystemAndVegetaionPackage(VSP);
                }
                else DeleteVegetationItemsOfType(); // By type
            }

        }
        /// <summary>
        /// Deleting logic
        /// </summary>
        void DeleteVegetationItemsOfType()
        {
            VegetationType type = (VegetationType)VSPVegetationPackageTools.ConvertStringToVegetationType(VegetationTypeNames[bulkItemsToDelete]);
            for (int i = VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList.Count - 1; i > -1; i--)
            {
                if (VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[i].VegetationType == type)
                {
                    if (Storage.PersistentVegetationStoragePackage != null) RemoveItemFromPersistentStorage(i);
                    VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList.RemoveAt(i);
                    SceneUtility.SetSceneDirtySystemAndVegetaionPackage(VSP);
                }
            }
        }
    }
}
#endif