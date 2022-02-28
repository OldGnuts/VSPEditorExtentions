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
        /// Pastes the selected settings from the copied item to the paste item
        /// </summary>
        /// <param name="pasteToInfo">The item that will be pasted on to </param>
        public void PasteItemSettingsToAnotherItem(VegetationItemInfoPro pasteToInfo, VegetationItemInfoPro pasteFromInfo)
        {

            if (copySamplePosition)
                pasteToInfo.SampleDistance = pasteFromInfo.SampleDistance;
            if (copyDensity)
                pasteToInfo.Density = pasteFromInfo.Density;
            if (copyRotationSettings)
            {
                pasteToInfo.Rotation = pasteFromInfo.Rotation;
                pasteToInfo.RotationOffset = pasteFromInfo.RotationOffset;
            }
            if (copyScaleSettings)
            {
                pasteToInfo.ScaleMultiplier = pasteFromInfo.ScaleMultiplier;
                pasteToInfo.MaxScale = pasteFromInfo.MaxScale;
                pasteToInfo.MinScale = pasteFromInfo.MinScale;
            }
            if (copyHeightSettings)
            {
                pasteToInfo.UseHeightRule = pasteFromInfo.UseHeightRule;
                pasteToInfo.UseAdvancedHeightRule = pasteFromInfo.UseAdvancedHeightRule;
                pasteToInfo.MaxCurveHeight = pasteFromInfo.MaxCurveHeight;
                pasteToInfo.HeightRuleCurve = pasteFromInfo.HeightRuleCurve;
                pasteToInfo.MaxHeight = pasteFromInfo.MaxHeight;
                pasteToInfo.MinHeight = pasteFromInfo.MinHeight;
            }
            if (copySteepnessSettings)
            {
                pasteToInfo.UseSteepnessRule = pasteFromInfo.UseSteepnessRule;
                pasteToInfo.UseAdvancedSteepnessRule = pasteFromInfo.UseAdvancedSteepnessRule;
                pasteToInfo.MaxSteepness = pasteFromInfo.MaxSteepness;
                pasteToInfo.MinSteepness = pasteFromInfo.MinSteepness;
                pasteToInfo.SteepnessRuleCurve = pasteFromInfo.SteepnessRuleCurve;
            }
            if (copyPositionOffsetSettings)
                pasteToInfo.Offset = pasteFromInfo.Offset;
            if (copyHeightOffset)
            {
                pasteToInfo.MaxUpOffset = pasteFromInfo.MaxUpOffset;
                pasteToInfo.MinUpOffset = pasteFromInfo.MinUpOffset;
            }
            if (copyColliderType)
            {
                pasteToInfo.ColliderType = pasteFromInfo.ColliderType;
                pasteToInfo.ColliderTrigger = pasteFromInfo.ColliderTrigger;
                pasteToInfo.ColliderTag = pasteFromInfo.ColliderTag;
                pasteToInfo.ColliderSize = pasteFromInfo.ColliderSize;
                pasteToInfo.ColliderRadius = pasteFromInfo.ColliderRadius;
                pasteToInfo.ColliderOffset = pasteFromInfo.ColliderOffset;
                pasteToInfo.ColliderMesh = pasteFromInfo.ColliderMesh;
                pasteToInfo.ColliderHeight = pasteFromInfo.ColliderHeight;
                pasteToInfo.ColliderDistanceFactor = pasteFromInfo.ColliderDistanceFactor;
                pasteToInfo.ColliderConvex = pasteFromInfo.ColliderConvex;
            }
            if (copybillboardSettings)
            {
                pasteToInfo.UseBillboards = pasteFromInfo.UseBillboards;
                pasteToInfo.UseBillboardFade = pasteFromInfo.UseBillboardFade;
                pasteToInfo.UseBillboardSnow = pasteFromInfo.UseBillboardSnow;
                pasteToInfo.UseBillboardWind = pasteFromInfo.UseBillboardWind;
                pasteToInfo.BillboardColorSpace = pasteFromInfo.BillboardColorSpace;
                pasteToInfo.BillboardBrightness = pasteFromInfo.BillboardBrightness;
                pasteToInfo.BillboardCutoff = pasteFromInfo.BillboardCutoff;
                pasteToInfo.BillboardFadeDistance = pasteFromInfo.BillboardFadeDistance;
                pasteToInfo.BillboardFlipBackNormals = pasteFromInfo.BillboardFlipBackNormals;
                pasteToInfo.BillboardMetallic = pasteFromInfo.BillboardMetallic;
                pasteToInfo.BillboardMipmapBias = pasteFromInfo.BillboardMipmapBias;
                pasteToInfo.BillboardNormalBlendFactor = pasteFromInfo.BillboardNormalBlendFactor;
                pasteToInfo.BillboardNormalStrength = pasteFromInfo.BillboardNormalStrength;
                pasteToInfo.BillboardOcclusion = pasteFromInfo.BillboardOcclusion;
                pasteToInfo.BillboardQuality = pasteFromInfo.BillboardQuality;
                pasteToInfo.BillboardRenderMode = pasteFromInfo.BillboardRenderMode;
                pasteToInfo.BillboardShadowOffset = pasteFromInfo.BillboardShadowOffset;
                pasteToInfo.BillboardSmoothness = pasteFromInfo.BillboardSmoothness;
                pasteToInfo.BillboardSpecular = pasteFromInfo.BillboardSpecular;
                pasteToInfo.BillboardTintColor = pasteFromInfo.BillboardTintColor;
                pasteToInfo.BillboardWindSpeed = pasteFromInfo.BillboardWindSpeed;
            }
            if (copyBiomeEdgeScaleRules)
            {
                pasteToInfo.UseBiomeEdgeScaleRule = pasteFromInfo.UseBiomeEdgeScaleRule;
                pasteToInfo.BiomeEdgeScaleDistance = pasteFromInfo.BiomeEdgeScaleDistance;
                pasteToInfo.BiomeEdgeScaleInverse = pasteFromInfo.BiomeEdgeScaleInverse;
                pasteToInfo.BiomeEdgeScaleMaxScale = pasteFromInfo.BiomeEdgeScaleMaxScale;
                pasteToInfo.BiomeEdgeScaleMinScale = pasteFromInfo.BiomeEdgeScaleMinScale;
            }
            if (copyBiomeEdgeIncludeRules)
            {
                pasteToInfo.UseBiomeEdgeIncludeRule = pasteFromInfo.UseBiomeEdgeIncludeRule;
                pasteToInfo.BiomeEdgeIncludeDistance = pasteFromInfo.BiomeEdgeIncludeDistance;
                pasteToInfo.BiomeEdgeIncludeInverse = pasteFromInfo.BiomeEdgeIncludeInverse;
            }
            if (copyTerrainTextureExcludeRules)
            {
                pasteToInfo.UseTerrainSourceExcludeRule = pasteFromInfo.UseTerrainSourceExcludeRule;
                pasteToInfo.TerrainTextureExcludeRuleList = pasteFromInfo.TerrainTextureExcludeRuleList;
            }
            if (copyTerrainTextureIncludeRules)
            {
                pasteToInfo.UseTerrainTextureIncludeRules = pasteFromInfo.UseTerrainTextureIncludeRules;
                pasteToInfo.TerrainTextureIncludeRuleList = pasteFromInfo.TerrainTextureIncludeRuleList;
            }
            if (copyTextureMaskExcludeRules)
            {
                pasteToInfo.UseTextureMaskExcludeRules = pasteFromInfo.UseTextureMaskExcludeRules;
                pasteToInfo.TextureMaskExcludeRuleList = pasteFromInfo.TextureMaskExcludeRuleList;
            }
            if (copyTerrainTextureIncludeRules)
            {
                pasteToInfo.UseTextureMaskIncludeRules = pasteFromInfo.UseTextureMaskIncludeRules;
                pasteToInfo.TextureMaskIncludeRuleList = pasteFromInfo.TextureMaskIncludeRuleList;
            }
            if (copyVegetationMaskRule)
            {
                pasteToInfo.UseVegetationMask = pasteFromInfo.UseVegetationMask;
                pasteToInfo.VegetationTypeIndex = pasteFromInfo.VegetationTypeIndex;
            }
            if (copyConcaveLocationRules)
            {
                pasteToInfo.UseConcaveLocationRule = pasteFromInfo.UseConcaveLocationRule;
                pasteToInfo.ConcaveLocationInverse = pasteFromInfo.ConcaveLocationInverse;
                pasteToInfo.ConcaveLoactionMinHeightDifference = pasteFromInfo.ConcaveLoactionMinHeightDifference;
                pasteToInfo.ConcaveLoactionDistance = pasteFromInfo.ConcaveLoactionDistance;
                pasteToInfo.ConcaveLoactionAverage = pasteFromInfo.ConcaveLoactionAverage;
            }
            if (copyNoiseCutoffRules)
            {
                pasteToInfo.UseNoiseCutoff = pasteFromInfo.UseNoiseCutoff;
                pasteToInfo.NoiseCutoffScale = pasteFromInfo.NoiseCutoffScale;
                pasteToInfo.NoiseCutoffOffset = pasteFromInfo.NoiseCutoffOffset;
                pasteToInfo.NoiseCutoffInverse = pasteFromInfo.NoiseCutoffInverse;
                pasteToInfo.NoiseCutoffValue = pasteFromInfo.NoiseCutoffValue;
            }
            if (copyNoiseDensityRules)
            {
                pasteToInfo.UseNoiseDensity = pasteFromInfo.UseNoiseDensity;
                pasteToInfo.NoiseDensityInverse = pasteFromInfo.NoiseDensityInverse;
                pasteToInfo.NoiseDensityOffset = pasteFromInfo.NoiseDensityOffset;
                pasteToInfo.NoiseDensityScale = pasteFromInfo.NoiseDensityScale;
            }
            if (copyNoiseScaleRules)
            {
                pasteToInfo.UseNoiseScaleRule = pasteFromInfo.UseNoiseScaleRule;
                pasteToInfo.NoiseScaleInverse = pasteFromInfo.NoiseScaleInverse;
                pasteToInfo.NoiseScaleMaxScale = pasteFromInfo.NoiseScaleMaxScale;
                pasteToInfo.NoiseScaleMinScale = pasteFromInfo.NoiseScaleMinScale;
                pasteToInfo.NoiseScaleOffset = pasteFromInfo.NoiseScaleOffset;
                pasteToInfo.NoiseScaleScale = pasteFromInfo.NoiseScaleScale;
            }
            if (copyDistanceFalloff)
                pasteToInfo.DistanceFalloffStartDistance = pasteFromInfo.DistanceFalloffStartDistance;

            if (copyLodSettings)
            {
                pasteToInfo.LODFactor = pasteFromInfo.LODFactor;
                pasteToInfo.DisableLOD = pasteFromInfo.DisableLOD;
                pasteToInfo.EnableCrossFade = pasteFromInfo.EnableCrossFade;
            }
            if (copyFoliageSettings)
            {
                // Only copy the settings if the items share the same shader
                // VSP uses shader controller selector and is dependent on specific shaders
                if (pasteFromInfo.ShaderName == pasteToInfo.ShaderName)
                    pasteToInfo.ShaderControllerSettings = pasteFromInfo.ShaderControllerSettings;
            }
            if (alwaysEnableRuntimeSpawnOnPastedItems) pasteToInfo.EnableRuntimeSpawn = true;
        }

        public void PasteAllItemSettingsToAnotherItem(VegetationItemInfoPro pasteToInfo, VegetationItemInfoPro pasteFromInfo)
        {
            pasteToInfo.SampleDistance = pasteFromInfo.SampleDistance;
            pasteToInfo.Density = pasteFromInfo.Density;
            pasteToInfo.Rotation = pasteFromInfo.Rotation;
            pasteToInfo.RotationOffset = pasteFromInfo.RotationOffset;
            pasteToInfo.ScaleMultiplier = pasteFromInfo.ScaleMultiplier;
            pasteToInfo.MaxScale = pasteFromInfo.MaxScale;
            pasteToInfo.MinScale = pasteFromInfo.MinScale;
            pasteToInfo.UseHeightRule = pasteFromInfo.UseHeightRule;
            pasteToInfo.UseAdvancedHeightRule = pasteFromInfo.UseAdvancedHeightRule;
            pasteToInfo.MaxCurveHeight = pasteFromInfo.MaxCurveHeight;
            pasteToInfo.HeightRuleCurve = pasteFromInfo.HeightRuleCurve;
            pasteToInfo.MaxHeight = pasteFromInfo.MaxHeight;
            pasteToInfo.MinHeight = pasteFromInfo.MinHeight;
            pasteToInfo.UseSteepnessRule = pasteFromInfo.UseSteepnessRule;
            pasteToInfo.UseAdvancedSteepnessRule = pasteFromInfo.UseAdvancedSteepnessRule;
            pasteToInfo.MaxSteepness = pasteFromInfo.MaxSteepness;
            pasteToInfo.MinSteepness = pasteFromInfo.MinSteepness;
            pasteToInfo.SteepnessRuleCurve = pasteFromInfo.SteepnessRuleCurve;
            pasteToInfo.Offset = pasteFromInfo.Offset;
            pasteToInfo.MaxUpOffset = pasteFromInfo.MaxUpOffset;
            pasteToInfo.MinUpOffset = pasteFromInfo.MinUpOffset;
            pasteToInfo.ColliderType = pasteFromInfo.ColliderType;
            pasteToInfo.ColliderTrigger = pasteFromInfo.ColliderTrigger;
            pasteToInfo.ColliderTag = pasteFromInfo.ColliderTag;
            pasteToInfo.ColliderSize = pasteFromInfo.ColliderSize;
            pasteToInfo.ColliderRadius = pasteFromInfo.ColliderRadius;
            pasteToInfo.ColliderOffset = pasteFromInfo.ColliderOffset;
            pasteToInfo.ColliderMesh = pasteFromInfo.ColliderMesh;
            pasteToInfo.ColliderHeight = pasteFromInfo.ColliderHeight;
            pasteToInfo.ColliderDistanceFactor = pasteFromInfo.ColliderDistanceFactor;
            pasteToInfo.ColliderConvex = pasteFromInfo.ColliderConvex;
            pasteToInfo.UseBillboards = pasteFromInfo.UseBillboards;
            pasteToInfo.UseBillboardFade = pasteFromInfo.UseBillboardFade;
            pasteToInfo.UseBillboardSnow = pasteFromInfo.UseBillboardSnow;
            pasteToInfo.UseBillboardWind = pasteFromInfo.UseBillboardWind;
            pasteToInfo.BillboardColorSpace = pasteFromInfo.BillboardColorSpace;
            pasteToInfo.BillboardBrightness = pasteFromInfo.BillboardBrightness;
            pasteToInfo.BillboardCutoff = pasteFromInfo.BillboardCutoff;
            pasteToInfo.BillboardFadeDistance = pasteFromInfo.BillboardFadeDistance;
            pasteToInfo.BillboardFlipBackNormals = pasteFromInfo.BillboardFlipBackNormals;
            pasteToInfo.BillboardMetallic = pasteFromInfo.BillboardMetallic;
            pasteToInfo.BillboardMipmapBias = pasteFromInfo.BillboardMipmapBias;
            pasteToInfo.BillboardNormalBlendFactor = pasteFromInfo.BillboardNormalBlendFactor;
            pasteToInfo.BillboardNormalStrength = pasteFromInfo.BillboardNormalStrength;
            pasteToInfo.BillboardOcclusion = pasteFromInfo.BillboardOcclusion;
            pasteToInfo.BillboardQuality = pasteFromInfo.BillboardQuality;
            pasteToInfo.BillboardRenderMode = pasteFromInfo.BillboardRenderMode;
            pasteToInfo.BillboardShadowOffset = pasteFromInfo.BillboardShadowOffset;
            pasteToInfo.BillboardSmoothness = pasteFromInfo.BillboardSmoothness;
            pasteToInfo.BillboardSpecular = pasteFromInfo.BillboardSpecular;
            pasteToInfo.BillboardTintColor = pasteFromInfo.BillboardTintColor;
            pasteToInfo.BillboardWindSpeed = pasteFromInfo.BillboardWindSpeed;
            pasteToInfo.UseBiomeEdgeScaleRule = pasteFromInfo.UseBiomeEdgeScaleRule;
            pasteToInfo.BiomeEdgeScaleDistance = pasteFromInfo.BiomeEdgeScaleDistance;
            pasteToInfo.BiomeEdgeScaleInverse = pasteFromInfo.BiomeEdgeScaleInverse;
            pasteToInfo.BiomeEdgeScaleMaxScale = pasteFromInfo.BiomeEdgeScaleMaxScale;
            pasteToInfo.BiomeEdgeScaleMinScale = pasteFromInfo.BiomeEdgeScaleMinScale;
            pasteToInfo.UseBiomeEdgeIncludeRule = pasteFromInfo.UseBiomeEdgeIncludeRule;
            pasteToInfo.BiomeEdgeIncludeDistance = pasteFromInfo.BiomeEdgeIncludeDistance;
            pasteToInfo.BiomeEdgeIncludeInverse = pasteFromInfo.BiomeEdgeIncludeInverse;
            pasteToInfo.UseTerrainSourceExcludeRule = pasteFromInfo.UseTerrainSourceExcludeRule;
            pasteToInfo.TerrainTextureExcludeRuleList = pasteFromInfo.TerrainTextureExcludeRuleList;
            pasteToInfo.UseTerrainTextureIncludeRules = pasteFromInfo.UseTerrainTextureIncludeRules;
            pasteToInfo.TerrainTextureIncludeRuleList = pasteFromInfo.TerrainTextureIncludeRuleList;
            pasteToInfo.UseTextureMaskExcludeRules = pasteFromInfo.UseTextureMaskExcludeRules;
            pasteToInfo.TextureMaskExcludeRuleList = pasteFromInfo.TextureMaskExcludeRuleList;
            pasteToInfo.UseTextureMaskIncludeRules = pasteFromInfo.UseTextureMaskIncludeRules;
            pasteToInfo.TextureMaskIncludeRuleList = pasteFromInfo.TextureMaskIncludeRuleList;
            pasteToInfo.UseVegetationMask = pasteFromInfo.UseVegetationMask;
            pasteToInfo.VegetationTypeIndex = pasteFromInfo.VegetationTypeIndex;
            pasteToInfo.UseConcaveLocationRule = pasteFromInfo.UseConcaveLocationRule;
            pasteToInfo.ConcaveLocationInverse = pasteFromInfo.ConcaveLocationInverse;
            pasteToInfo.ConcaveLoactionMinHeightDifference = pasteFromInfo.ConcaveLoactionMinHeightDifference;
            pasteToInfo.ConcaveLoactionDistance = pasteFromInfo.ConcaveLoactionDistance;
            pasteToInfo.ConcaveLoactionAverage = pasteFromInfo.ConcaveLoactionAverage;
            pasteToInfo.UseNoiseCutoff = pasteFromInfo.UseNoiseCutoff;
            pasteToInfo.NoiseCutoffScale = pasteFromInfo.NoiseCutoffScale;
            pasteToInfo.NoiseCutoffOffset = pasteFromInfo.NoiseCutoffOffset;
            pasteToInfo.NoiseCutoffInverse = pasteFromInfo.NoiseCutoffInverse;
            pasteToInfo.NoiseCutoffValue = pasteFromInfo.NoiseCutoffValue;
            pasteToInfo.UseNoiseDensity = pasteFromInfo.UseNoiseDensity;
            pasteToInfo.NoiseDensityInverse = pasteFromInfo.NoiseDensityInverse;
            pasteToInfo.NoiseDensityOffset = pasteFromInfo.NoiseDensityOffset;
            pasteToInfo.NoiseDensityScale = pasteFromInfo.NoiseDensityScale;
            pasteToInfo.UseNoiseScaleRule = pasteFromInfo.UseNoiseScaleRule;
            pasteToInfo.NoiseScaleInverse = pasteFromInfo.NoiseScaleInverse;
            pasteToInfo.NoiseScaleMaxScale = pasteFromInfo.NoiseScaleMaxScale;
            pasteToInfo.NoiseScaleMinScale = pasteFromInfo.NoiseScaleMinScale;
            pasteToInfo.NoiseScaleOffset = pasteFromInfo.NoiseScaleOffset;
            pasteToInfo.NoiseScaleScale = pasteFromInfo.NoiseScaleScale;
            pasteToInfo.DistanceFalloffStartDistance = pasteFromInfo.DistanceFalloffStartDistance;
            pasteToInfo.LODFactor = pasteFromInfo.LODFactor;
            pasteToInfo.DisableLOD = pasteFromInfo.DisableLOD;
            pasteToInfo.EnableCrossFade = pasteFromInfo.EnableCrossFade;
            pasteToInfo.ShaderControllerSettings = pasteFromInfo.ShaderControllerSettings;

            if (alwaysEnableRuntimeSpawnOnPastedItems) pasteToInfo.EnableRuntimeSpawn = true;
        }
    }
}
#endif