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
        public void PasteCurrentCopiedItemToAnotherPackage()
        {
            if (!allowDuplicateItems)
            {
                for (int i = 0; i < VSP.VegetationPackageProList[packageToPasteTo].VegetationInfoList.Count; i++)
                {
                    if (VSP.VegetationPackageProList[packageToPasteTo].VegetationInfoList[i].VegetationPrefab.name ==
                        copiedItem.VegetationPrefab.name)
                    {
                        Debug.Log("Duplicatation of items is not allowed, select allow duplicate items if you want to duplicate this item");
                        return;
                    }
                }
            }
            //Create new item
            string newVegetationItemID = Guid.NewGuid().ToString();
            VSP.VegetationPackageProList[packageToPasteTo].AddVegetationItem(copiedItem.VegetationPrefab, copiedItem.VegetationType, true, newVegetationItemID);

            //Copy settings
            int newItemIndex = VSP.VegetationPackageProList[packageToPasteTo].GetVegetationItemIndexFromID(newVegetationItemID);
            VegetationItemInfoPro pasteToItem = VSP.VegetationPackageProList[packageToPasteTo].VegetationInfoList[newItemIndex];

            if (copyAllSettings) PasteAllItemSettingsToAnotherItem(pasteToItem, copiedItem);
            else if (copiedItem.VegetationPrefab != null) PasteItemSettingsToAnotherItem(pasteToItem, copiedItem);

            //Refresh
            SceneUtility.SetSceneDirtySystemAndVegetaionPackage(VSP, packageToPasteTo);

        }
        public void PasteAllItemsOfTypeToAnotherPackage()
        {
            // Make a list of names to compare against to check for duplication if its not allowed
            List<string> itemsInPasteToPackage = new List<string>();
            foreach (VegetationItemInfoPro itemInfo in VSP.VegetationPackageProList[packageToPasteTo].VegetationInfoList)
                if (!itemsInPasteToPackage.Contains(itemInfo.VegetationPrefab.name))
                    itemsInPasteToPackage.Add(itemInfo.VegetationPrefab.name);
            
            // The type of vegetation to copy from the source package
            VegetationType vegetationTypeToCopy = (VegetationType)VSPVegetationPackageTools.ConvertStringToVegetationType(VegetationTypeNames[packageToPasteAllOfType]);
            for (int i = 0; i < VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList.Count; i++)
            {
                // Item from the original package
                VegetationItemInfoPro itemToCopy = VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[i];

                // Either the vegetation type matches or "All" types has been selected 0 == all
                if (itemToCopy.VegetationType == vegetationTypeToCopy || packageToPasteAllOfType == 0)
                {

                    // Check for duplicate items in the list
                    // or allow them if allowDuplicateItems is set
                    if (allowDuplicateItems || !itemsInPasteToPackage.Contains(itemToCopy.VegetationPrefab.name))
                    {
                        //Create new item id and add it to the vegetion package
                        string newVegetationItemID = Guid.NewGuid().ToString();
                        VSP.VegetationPackageProList[packageToPasteTo].AddVegetationItem(itemToCopy.VegetationPrefab, itemToCopy.VegetationType, true, newVegetationItemID);

                        //Get the new item index and get a reference to the recently added item
                        int newItemIndex = VSP.VegetationPackageProList[packageToPasteTo].GetVegetationItemIndexFromID(newVegetationItemID);
                        VegetationItemInfoPro pasteToItem = VSP.VegetationPackageProList[packageToPasteTo].VegetationInfoList[newItemIndex];
                        
                        //Copy settings either some settings are copied all settings
                        //depending on copyAllSettings being true or not
                        if (copyAllSettings) PasteAllItemSettingsToAnotherItem(pasteToItem, copiedItem);
                        else if (copiedItem.VegetationPrefab != null) PasteItemSettingsToAnotherItem(pasteToItem, copiedItem);
                    }
                }
            }
            //Refresh
            SceneUtility.SetSceneDirtySystemAndVegetaionPackage(VSP, packageToPasteTo);
        }
    }
}
#endif
