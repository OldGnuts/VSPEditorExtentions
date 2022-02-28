#if UNITY_EDITOR

using System;
using UnityEngine;
using AwesomeTechnologies.VegetationSystem;

namespace VegetationStudioProEditorExtentions
{
    public struct HierarchyProperties
    {
        public int meshRenderers;
        public int lodGroups;
        public int total;
    }
    [Serializable]
    public struct HierarchyInformation
    {
        public VegetationType vegetationType;
        public GameObject hierarchyFolder;
    }
    public static class VSPHierarchyTools
    {
        public static HierarchyProperties ScanHierarchy(int index, HierarchyInformation[] hierarchyInformation)
        {
            if (index == -1) return new HierarchyProperties();

            HierarchyProperties properties = new HierarchyProperties();

            if (hierarchyInformation[index].hierarchyFolder != null)
            {
                int childrencount = hierarchyInformation[index].hierarchyFolder.transform.childCount;
                for (int i = 0; i < childrencount; i++)
                {
                    if (hierarchyInformation[index].hierarchyFolder.transform.GetChild(i).GetComponent<MeshRenderer>()) properties.meshRenderers++;
                    if (hierarchyInformation[index].hierarchyFolder.transform.GetChild(i).GetComponent<LODGroup>()) properties.lodGroups++;
                }
                properties.total = properties.meshRenderers + properties.lodGroups;
            }
            return properties;
        }
    }
}
#endif
