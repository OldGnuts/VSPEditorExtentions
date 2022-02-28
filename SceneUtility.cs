#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using AwesomeTechnologies.VegetationSystem;

namespace VegetationStudioProEditorExtentions
{
    public static class SceneUtility
    {
        public static void SetSceneDirtyVegetationSystemOnly(VegetationSystemPro VSP)
        {
            if (!Application.isPlaying)
            {
                VSP.RefreshVegetationSystem();
                EditorSceneManager.MarkSceneDirty(VSP.gameObject.scene);
                EditorUtility.SetDirty(VSP);
            }
        }
        public static void SetSceneDirtySystemAndVegetaionPackage(VegetationSystemPro VSP)
        {
            if (!Application.isPlaying)
            {
                VSP.RefreshVegetationSystem();
                EditorUtility.SetDirty(VSP.VegetationPackageProList[VSP.VegetationPackageIndex]);
                EditorSceneManager.MarkSceneDirty(VSP.gameObject.scene);
                EditorUtility.SetDirty(VSP);
            }
        }
        public static void SetSceneDirtySystemAndVegetaionPackage(VegetationSystemPro VSP, int packageIndex)
        {
            if (!Application.isPlaying)
            {
                VSP.RefreshVegetationSystem();
                EditorUtility.SetDirty(VSP.VegetationPackageProList[packageIndex]);
                EditorSceneManager.MarkSceneDirty(VSP.gameObject.scene);
                EditorUtility.SetDirty(VSP);
            }
        }
        public static void SetSceneDirtySystemAndStoragenPackage(VegetationSystemPro VSP)
        {
            if (!Application.isPlaying)
            {
                VSP.RefreshVegetationSystem();
                EditorUtility.SetDirty(VSP.PersistentVegetationStorage.PersistentVegetationStoragePackage);
                EditorSceneManager.MarkSceneDirty(VSP.gameObject.scene);
                EditorUtility.SetDirty(VSP);
            }
        }
    }
}
#endif
