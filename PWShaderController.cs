/*
#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class PWShaderController : IShaderController
    {
        public bool MatchShader(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName)) return false;
            else if (shaderName.Contains("PW_General")) return true;
            else if (shaderName.Contains("PW_General")) return true;
            return false;
        }

        public bool MatchBillboardShader(Material[] materials)
        {
            return false;
        }

        bool IsBarkShader(string shaderName)
        {
            if (shaderName.Contains("PW_General")) return true;
            return false;
        }

        public ShaderControllerSettings Settings { get; set; }

        public void CreateDefaultSettings(Material[] materials)
        {
            Settings = new ShaderControllerSettings
            {
                Heading = "PW Tree",
                Description = "",
                LODFadePercentage = false,
                LODFadeCrossfade = false,
                SampleWind = false,
                SupportsInstantIndirect = true,
                BillboardHDWind = true

            };
            Settings.AddLabelProperty(Settings.Description);
            Settings.AddBooleanProperty("ReplaceShader", "Replace shader", "This will replace the PW shader with a Vegetation Studio version that supports instanced indirect", true);
            Settings.AddLabelProperty("Foliage settings");
            Settings.AddColorProperty("HealtyColor", "Healthy Color", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_HealthyColor"));
            Settings.AddColorProperty("DryColor", "DryColor", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_DryColor"));

            Settings.AddLabelProperty("Bark settings");
            Settings.AddColorProperty("BarkColor", "Bark Color", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_Color"));

            Settings.AddLabelProperty("Wind settings");
            Settings.AddFloatProperty("InitialBend", "Initial Bend", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_InitialBend"), 0, 10);
            Settings.AddFloatProperty("Stiffness", "Stiffness", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_Stiffness"), 0, 10);
            Settings.AddFloatProperty("Drag", "Drag", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_Drag"), 0, 10);
            Settings.AddFloatProperty("ShiverDrag", "Shiver Drag", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_ShiverDrag"), 0, 10);
            Settings.AddFloatProperty("ShiverDirectionality", "Shiver Directionality", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_ShiverDirectionality"), 0, 1);

            Settings.AddFloatProperty("Smoothness", "Smoothness", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_Glossiness"), 0, 10);
            Settings.AddFloatProperty("Metallic", "Metallic", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_Metallic"), 0, 10);
        }

        public void UpdateMaterial(Material material, EnvironmentSettings environmentSettings)
        {

            if (Settings == null) return;
            bool replaceShader = Settings.GetBooleanPropertyValue("ReplaceShader");
            bool barkShader = IsBarkShader(material.shader.name);
            Texture t = material.GetTexture("_MetallicGlossMap");

            if (Settings.Heading == "Not Foliage")
            {
                if (replaceShader)
                {
                    if (Settings.Description == "Use NM")
                    {
                        if (Shader.Find("NatureManufacture Shaders/Standard Shaders/Standard Metalic Snow") != null)
                        {
                            material.shader = Shader.Find("NatureManufacture Shaders/Standard Shaders/Standard Metalic Snow");
                        }
                        else material.shader = Shader.Find("AwesomeTechnologies/Release/Standard/Standard Shader");
                    }
                    else material.shader = Shader.Find("AwesomeTechnologies/Release/Standard/Standard Shader");
                }
            }
            else if (Settings.Heading == "Plant")
            {
                if (replaceShader)
                {
                    if (barkShader)
                    {
                        if (replaceShader)
                        {
                            if (Settings.Description == "Use NM")
                            {
                                if (Shader.Find("NatureManufacture Shaders/Trees/Tree Bark Metalic") != null)
                                {
                                    material.shader = Shader.Find("NatureManufacture Shaders/Trees/Tree Bark Metalic");
                                }
                                else material.shader = Shader.Find("AwesomeTechnologies/Release/Grass/Grass");
                            }
                            else material.shader = Shader.Find("AwesomeTechnologies/Release/Trees/Tree_Bark_Metallic");

                            Color barkColor = Settings.GetColorPropertyValue("Bark Color");
                            material.SetColor("_Color", barkColor);
                        }
                    }
                    else
                    {
                        if (Shader.Find("NatureManufacture Shaders/Trees/Tree_Leaves_Specular") != null)
                        {
                            material.shader = Shader.Find("NatureManufacture Shaders/Trees/Tree_Leaves_Specular");
                        }
                        else material.shader = Shader.Find("AwesomeTechnologies/Release/Trees/Tree_Leaves_Specular");
                    }
                    Color healtyColor = Settings.GetColorPropertyValue("HealtyColor");
                    Color dryColor = Settings.GetColorPropertyValue("DryColor");

                    material.SetColor("_HealthyColor", healtyColor);
                    material.SetColor("_DryColor", dryColor);
                }
                else material.shader = Shader.Find("AwesomeTechnologies/Release/Trees/Tree_Leaves_Specular");
                float initialBend = Settings.GetFloatPropertyValue("InitialBend");
                float stiffness = Settings.GetFloatPropertyValue("Stiffness");
                float drag = Settings.GetFloatPropertyValue("Drag");
                float shiverDrag = .005f; //Settings.GetFloatPropertyValue("ShiverDrag");
                float shiverDirectionality = Settings.GetFloatPropertyValue("ShiverDirectionality");

                material.SetFloat("_ShiverDrag", shiverDrag);
                material.SetFloat("_ShiverDirectionality", shiverDirectionality);
                material.SetFloat("_InitialBend", initialBend);
                material.SetFloat("_Stiffness", stiffness);
                material.SetFloat("_Drag", drag);
            }

            else if (Settings.Heading == "Grass")
            {
                if (replaceShader)
                {
                    if (Settings.Description == "Use NM")
                    {
                        if (Shader.Find("NatureManufacture Shaders/Grass/Advanced Grass Standard") != null)
                        {
                            material.shader = Shader.Find("NatureManufacture Shaders/Grass/Advanced Grass Standard");
                        }
                    }
                    else material.shader = Shader.Find("AwesomeTechnologies/Release/Grass/Grass");
                }
                else material.shader = Shader.Find("AwesomeTechnologies/Release/Grass/Grass");
            }

            else
            {
                if (barkShader)
                {
                    if (replaceShader)
                    {
                        if (Settings.Description == "Use NM")
                        {
                            if (Shader.Find("NatureManufacture Shaders/Trees/Tree Bark Metalic") != null)
                            {
                                material.shader = Shader.Find("NatureManufacture Shaders/Trees/Tree Bark Metalic");
                            }
                            else material.shader = Shader.Find("AwesomeTechnologies/Release/Grass/Grass");
                        }
                        else material.shader = Shader.Find("AwesomeTechnologies/Release/Trees/Tree_Bark_Metallic");

                        Color barkColor = Settings.GetColorPropertyValue("Bark Color");
                        material.SetColor("_Color", barkColor);
                    }
                }
                else
                {
                    if (replaceShader)
                    {
                        if (Settings.Description == "Use NM")
                        {
                            if (Shader.Find("NatureManufacture Shaders/Trees/Tree_Leaves_Specular") != null)
                            {
                                material.shader = Shader.Find("NatureManufacture Shaders/Trees/Tree_Leaves_Specular");
                            }
                            else material.shader = Shader.Find("AwesomeTechnologies/Release/Trees/Tree_Leaves_Specular");
                        }
                        else material.shader = Shader.Find("AwesomeTechnologies/Release/Trees/Tree_Leaves_Specular");
                    }


                    Color healtyColor = Settings.GetColorPropertyValue("HealtyColor");
                    Color dryColor = Settings.GetColorPropertyValue("DryColor");

                    material.SetColor("_HealthyColor", healtyColor);
                    material.SetColor("_DryColor", dryColor);

                    float shiverDrag = .005f;// Settings.GetFloatPropertyValue("ShiverDrag");
                    float shiverDirectionality = Settings.GetFloatPropertyValue("ShiverDirectionality");

                    material.SetFloat("_ShiverDrag", shiverDrag);
                    material.SetFloat("_ShiverDirectionality", shiverDirectionality);
                }

                float initialBend = Settings.GetFloatPropertyValue("InitialBend");
                float stiffness = Settings.GetFloatPropertyValue("Stiffness");
                float drag = Settings.GetFloatPropertyValue("Drag");

                material.SetFloat("_InitialBend", initialBend);
                material.SetFloat("_Stiffness", stiffness);
                material.SetFloat("_Drag", drag);
            }
            float smoothness = Settings.GetFloatPropertyValue("Smoothness");
            float metallic = Settings.GetFloatPropertyValue("Metallic");
            material.SetFloat("_BumpScale", 1);
            material.SetFloat("_MetallicPower", metallic);
            material.SetFloat("_WetSmoothness", 0);
            material.SetFloat("_SmoothnessPower", smoothness);
            material.SetFloat("_Cover_Amount", 0);
            if (Settings.Description == "Use NM") material.SetTexture("_MetalicRAmbientOcclusionGSmoothnessA", t); // Reapply texture
        }
        
        public void UpdateWind(Material material, WindSettings windSettings)
        {

        }
    }
#endif
}
*/
