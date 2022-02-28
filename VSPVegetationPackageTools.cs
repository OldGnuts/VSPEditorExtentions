#if UNITY_EDITOR

using System;
using UnityEditor;
using AwesomeTechnologies.VegetationSystem;

namespace VegetationStudioProEditorExtentions
{
    public static class VSPVegetationPackageTools
    {
        /// <summary>
        /// Creates a new vegetation package in the vegetation package folder.
        /// If the vegetation package folder does not exist, this attempt to create one.
        /// TODO: Allow user to specify the folder
        /// </summary>
        /// <returns VegetationPackagePro></returns>
        public static VegetationPackagePro CreateNewVegetationPackage(VegetationSystemPro VSP, string vegetationPackageFileName, BiomeType biomeType)
        {
            VegetationPackagePro vegetationPackage;
            vegetationPackage = VegetationPackagePro.CreateInstance<VegetationPackagePro>();
            if (!AssetDatabase.IsValidFolder("Assets/VegetationPackages"))
                AssetDatabase.CreateFolder("Assets", "VegetationPackages");

            if (vegetationPackageFileName == "")
                vegetationPackageFileName = "VegetationPackage" + Guid.NewGuid() + "Generated" + ".asset";

            if (!vegetationPackageFileName.Contains(".asset")) // Add the file extention if necessary
                vegetationPackageFileName += ".asset";

            vegetationPackage.TerrainTextureCount = 0;
            vegetationPackage.GenerateBiomeSplamap = false;
            vegetationPackage.BiomeType = biomeType;
            vegetationPackage.PackageName = vegetationPackageFileName; // Use the same name for now until something new comes to mind

            // Create the vegetation package
            AssetDatabase.CreateAsset(vegetationPackage, "Assets/VegetationPackages/" + vegetationPackageFileName);
            VSP.AddVegetationPackage(vegetationPackage);
            SceneUtility.SetSceneDirtySystemAndVegetaionPackage(VSP);

            return vegetationPackage;
        }
        /// <summary>
        /// Converts a string name into an int that can be used with the VegetationType enum
        /// Not the best way to do this.
        /// </summary>
        /// <param name="s"></param>
        /// <returns>int that is relative to the VegetationType </returns>
        public static int ConvertStringToVegetationType(string s)
        {
            if (s.ToLower().Contains("tree")) return 2;
            if (s.ToLower().Contains("plant")) return 1;
            if (s.ToLower().Contains("grass")) return 0;
            if (s.ToLower().Contains("large objects")) return 4;
            if (s.ToLower().Contains("all")) return 5;
            return 3; // objects
        }
    }
}
#endif
