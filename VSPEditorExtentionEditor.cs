#if UNITY_EDITOR
using UnityEngine;
using System;
using UnityEditor;
using UnityEditorInternal;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.Common;
using AwesomeTechnologies.VegetationStudio;


namespace VegetationStudioProEditorExtentions
{

    public partial class VSPEditorExtention : EditorWindow
    {
        static readonly string[] tabNames = { "Bulk Edit", "Import From Terrain", "Import From Hierarchy" };
        static readonly string[] bulkEditTabNames = { "Copy and Paste Settings", "Copy and Paste Items", "Single Package Delete", "Single Package Misc Editing" };
        string vegetationPackageName = "";
        readonly Color headerColor = new Color(.2f, .2f, .2f);
        int selectionCount = 0; // Unused at the moment

        // Formatting
        readonly int spacing = 15;
        readonly int smallSpacing = 5;
        GUIStyle headerStyle = new GUIStyle();
        GUIStyle tabStyle;

        bool Valid
        {
            get { return ValidityCheck(); }
        }
        bool ValidImporter
        {
            get { return ImporterValidityCheck(); }
        }
        [MenuItem("Window/VSP Editor Extention")]
        static void OpenWindow()
        {
            VSPEditorExtention window = (VSPEditorExtention)EditorWindow.GetWindow(typeof(VSPEditorExtention));
            window.Show();
        }
        private void Awake()
        {
            SetupHeaderStyle();
            vegetationTypes = Enum.GetNames(typeof(VegetationType));
        }
        private void OnEnable()
        {
            SetupHeaderStyle();
            vegetationTypes = Enum.GetNames(typeof(VegetationType));
        }

        /// <summary>
        /// Resets the current selected index 
        /// </summary>
        public void ResetSelectionValues()
        {
            currentSelectedVegetationIndex = 0;
            currentSelectedGridIndex = 0;
            // copiedItem = null;
        }

        /// <summary>
        /// Setup the default header style
        /// </summary>
        void SetupHeaderStyle()
        {
            // Header formatting
            headerStyle.normal.background = Texture2D.whiteTexture;
            headerStyle.border = new RectOffset(1, 1, 1, 1);
            headerStyle.fontStyle = FontStyle.Bold;
        }
        Vector2 scrollPosition;
        void OnGUI()
        {
            // GUI.skin can only be called from OnGUI... methods
            headerStyle.fontSize = GUI.skin.font.fontSize + 4;
            headerStyle.normal.textColor = GUI.skin.label.normal.textColor;
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUI.skin.scrollView);
            DrawSelectEditorTypeTab();
            switch (currentTab)
            {
                case 0:
                    {
                        if (Valid)
                        {
                            DrawBulkEditingTab();
                        }
                        else
                            EditorGUILayout.LabelField("Vegetation System Pro not found or no Vegetation Packages are added to the Vegetation System Pro component");
                        break;
                    }
                case 1:
                    {
                        if (ValidImporter)
                            DrawTerrainImporter();
                        else
                            EditorGUILayout.LabelField("Vegetation System Pro not found terrain does not exist");
                        break;
                    }
                case 2:
                    {
                        if (ValidImporter)
                        {
                            EditorGUILayout.Space(spacing);
                            EditorGUILayout.HelpBox("This takes objects in the hierarchy and adds them to the vegetation package and store the instances in persistent storage. This will DISABLE the objects that it converts to VSP", MessageType.Warning);
                            //EditorGUILayout.Space(spacing);
                            //CreateNewPrefabWith3LODs = GUILayout.Toggle(CreateNewPrefabWith3LODs, new GUIContent("Create a new asset and remove the 4rd LOD (Useful if 4rd LOD is cross)", "This will create a new prefab without the 4th LOD removed, if a 4th LOD exists."));
                            //EditorGUILayout.Space(spacing);
                            //TryToConvertToNMVSPrefabs = GUILayout.Toggle(TryToConvertToNMVSPrefabs, new GUIContent("Try to use Nature Manufacture Tree VSPrefabs (Experimental)", "Nature manufacture has specific prefabs that are designed to work well with VSP. Tested to work with Mountain Environment trees."));
                            EditorGUILayout.Space(spacing);
                            MinimumNumberOfInstancesRequiredToAdd = EditorGUILayout.IntField(new GUIContent("Minimum Instance Count", "The minimum number of instances of an object before it is considered to be added to VSP"), MinimumNumberOfInstancesRequiredToAdd);
                            EditorGUILayout.Space(spacing);

                            ScriptableObject target = this;
                            SerializedObject so = new SerializedObject(target);
                            SerializedProperty prop = so.FindProperty("hierarchyInformation");


                            EditorGUILayout.PropertyField(prop, true); // True means show children
                            so.ApplyModifiedProperties(); // Remember to apply modified properties

                            /*
                                                        hierarchyInformationList.DoLayoutList();

                                                        if (hierarchyInformationList.index != -1 && hierarchyInformationList.index < hierarchyInformationList.count)
                                                        {
                                                            if (hierarchyInformationList.serializedProperty.GetArrayElementAtIndex(hierarchyInformationList.index) != null)
                                                            {
                                                                if (hierarchyInformation != null)
                                                                {
                                                                    HierarchyProperties p = VSPHierarchyTools.ScanHierarchy(hierarchyInformationList.index, hierarchyInformation);

                                                                    GUILayout.Label("MeshRenderers : " + p.meshRenderers);
                                                                    GUILayout.Label("Lod Groups : " + p.lodGroups);
                                                                    GUILayout.Label("Total : " + p.total);
                                                                }
                                                            }
                                                        }
                            */
                            EditorGUILayout.BeginVertical("box");
                            createPackageAndAdd = GUILayout.Toggle(createPackageAndAdd, "Create new vegetation package?");
                            EditorGUILayout.Space(spacing);
                            if (createPackageAndAdd == true)
                            {
                                EditorGUILayout.HelpBox("A new vegetation package will be created and added to Vegetation Studio", MessageType.Warning);

                                EditorGUILayout.Space(spacing);
                                EditorGUILayout.LabelField("Biome type");
                                string[] enumNames = Enum.GetNames(typeof(BiomeType));

                                int value = (int)createPackageBiomeType;
                                value = EditorGUILayout.Popup(value, enumNames);
                                createPackageBiomeType = (BiomeType)value;

                                EditorGUILayout.LabelField("Package name");
                                vegetationPackageFileName = EditorGUILayout.TextArea(vegetationPackageFileName);
                                EditorGUILayout.Space(spacing);

                                // Create a new vegetation package and process the hierarchy 
                                if (DrawButton("Create package and import from hierarchy"))
                                {
                                    VegetationPackagePro package = VSPVegetationPackageTools.CreateNewVegetationPackage(
                                        VSP, vegetationPackageFileName, createPackageBiomeType);
                                    if (package != null)
                                    {
                                        for (int i = 0; i < hierarchyInformation.Count; i++)
                                        {
                                            AddAllHierarchyPrefabsToVSPItems(package);/*, i,
                                                (VegetationType)hierarchyInformationList.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("vegetationType").enumValueIndex);*/
                                        }
                                        VSPPersistentStorageTools.CreateOrLoadPersistentVegetationStorage(VSP, storageFileName);
                                        VSP.VegetationPackageIndex = VSP.VegetationPackageProList.Count - 1;
                                        Storage.SelectedVegetationPackageIndex = VSP.VegetationPackageIndex;
                                        ConvertHierarchyToVSPStorage();
                                        SceneUtility.SetSceneDirtySystemAndStoragenPackage(VSP);
                                        if (TryToConvertToNMVSPrefabs)
                                            for (int i = 0; i < VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList.Count; i++)
                                                ShaderControlOverride(VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[i]);
                                    }
                                }
                            }
                            else if (PackageNames.Length > 0 && VSP.PersistentVegetationStorage.PersistentVegetationStoragePackage != null)
                            {

                                VSP.VegetationPackageIndex = EditorGUILayout.Popup(VSP.VegetationPackageIndex, PackageNames);
                                if (DrawButton("Import from hierarchy into selected vegetation package"))
                                {
                                    if (VSP.VegetationPackageProList[VSP.VegetationPackageIndex] != null)
                                    {
                                        for (int i = 0; i < hierarchyInformation.Count; i++)
                                        {
                                            AddAllHierarchyPrefabsToVSPItems(VSP.VegetationPackageProList[VSP.VegetationPackageIndex]);/*, i,
                                                (VegetationType)hierarchyInformationList.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("vegetationType").enumValueIndex);
                                            */
                                        }
                                        if (!VSPPersistentStorageTools.IsPersistentStorageInitializedAndValid(VSP))
                                            Storage.InitializePersistentStorage();
                                        ConvertHierarchyToVSPStorage();
                                        SceneUtility.SetSceneDirtySystemAndStoragenPackage(VSP);
                                        if (TryToConvertToNMVSPrefabs)
                                            for (int i = 0; i < VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList.Count; i++)
                                                ShaderControlOverride(VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[i]);
                                    }
                                }
                            }
                            else
                                EditorGUILayout.HelpBox("There are no vegetation packages detected. You can choose to create and add a package.", MessageType.Error);
                        }
                        EditorGUILayout.EndVertical();
                        break;
                    }
            }
            EditorGUILayout.EndScrollView();
        }

        #region VSP Extention terrain conversion methods
        /// <summary>
        /// Draws the terrain importer tab
        /// </summary>
        void DrawTerrainImporter()
        {
            EditorGUILayout.Space(spacing);

            ConvertTerrainTrees = GUILayout.Toggle(ConvertTerrainTrees, "Convert terrain trees ");
            ConvertTerrainDetails = GUILayout.Toggle(ConvertTerrainDetails, "Convert terrain details");
            //CreateNewPrefabWith3LODs = GUILayout.Toggle(CreateNewPrefabWith3LODs, "Create a new prefab assets and remove the 4rd LOD (Useful if 4rd LOD is cross)");
            //TryToConvertToNMVSPrefabs = GUILayout.Toggle(TryToConvertToNMVSPrefabs, "Try to use Nature Manufacture VSPrefabs (Experimental)");

            if (ConvertTerrainTrees == false && ConvertTerrainDetails == false)
                EditorGUILayout.HelpBox("You must select to import either terrain trees, details or both", MessageType.Info);
            else
            {
                DrawImportFromTerrainToExistingVegetationPackage();
                DrawCreateNewVegetationPackageAndImportFromTerrain();
            }
        }
        /// <summary>
        /// Draws terrain import to vegetation existing package inspector
        /// </summary>
        void DrawImportFromTerrainToExistingVegetationPackage()
        {
            if (VSP.VegetationPackageProList.Count > 0)
            {
                DrawSelectBiomeDropDown();
                EditorGUILayout.Space(spacing);

                if (DrawButton("Import trees and details from terrain to selected package"))
                {
                    if (Storage.PersistentVegetationStoragePackage != null)
                        VSPPersistentStorageTools.CreateOrLoadPersistentVegetationStorage(VSP, storageFileName);
                    else
                    {
                        ClearStoredVegetationFromImportSource(2);
                        ClearStoredVegetationFromImportSource(4);
                    }
                    ConvertTerrainTreesAndDetails();
                }
            }
            else EditorGUILayout.HelpBox("There are no vegetation packages associated with the vegetation system. You can specify a name and one will be created", MessageType.Warning);
        }
        /// <summary>
        /// Draws terrain import to new vegetation package instpector
        /// </summary>
        void DrawCreateNewVegetationPackageAndImportFromTerrain()
        {
            GUILayout.Space(spacing);
            GUILayout.BeginHorizontal();
            GUILayout.Label("New package name : ");
            vegetationPackageFileName = GUILayout.TextArea(vegetationPackageFileName);
            GUILayout.EndHorizontal();

            if (DrawButton("Create package and import trees and details from terrain"))
            {
                VegetationPackagePro package = VSPVegetationPackageTools.CreateNewVegetationPackage(
                    VSP, vegetationPackageFileName, createPackageBiomeType);
                VSP.VegetationPackageIndex = VSP.VegetationPackageProList.Count - 1;
                vegetationPackageName = VSP.VegetationPackageProList[VSP.VegetationPackageIndex].name;

                ConvertTerrainTreesAndDetails();
            }
        }
        /// <summary>
        /// Converts terrain prototypes to vegetation package and storage package
        /// </summary>
        void ConvertTerrainTreesAndDetails()
        {
            VSPPersistentStorageTools.CreateOrLoadPersistentVegetationStorage(VSP, storageFileName);
            if (ConvertTerrainTrees)
            {
                AddAllTerrainTreesToVSPItems();
                SceneUtility.SetSceneDirtyVegetationSystemOnly(VSP);
                ConvertUnityTreeInstanceToVSP();
            }
            if (ConvertTerrainDetails)
            {
                ConvertTerrainDetailsToVSP(Terrain.activeTerrain);
                SceneUtility.SetSceneDirtySystemAndStoragenPackage(VSP);
                if (TryToConvertToNMVSPrefabs)
                    for (int i = 0; i < VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList.Count; i++)
                        ShaderControlOverride(VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[i]);
            }
        }
        #endregion

        #region VSP Extention bulk editing methods
        /// <summary>
        /// Entry point to draw the bulk editing tools
        /// </summary>
        void DrawBulkEditingTab()
        {
            GUIContent content;
            EditorGUILayout.Space(smallSpacing);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bulk edit type : ", GUILayout.Width((EditorGUIUtility.currentViewWidth / 3) - 15));
            GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Button"));
            style.fontSize += 4;
            currentBulkEditingTab = EditorGUILayout.Popup(currentBulkEditingTab, bulkEditTabNames, style);
            EditorGUILayout.EndHorizontal();
            //currentBulkEditingTab = GUILayout.SelectionGrid(currentBulkEditingTab, bulkEditTabNames, 1, GUILayout.Width((EditorGUIUtility.currentViewWidth * .66f) - 20));
            if (VSP.VegetationPackageProList.Count < 2) currentBulkEditingTab = 0;

            DrawSelectBiomeDropDown();
            DrawVegetationItemSelectorDropDown();

            // Check if this package contains any items, if not return
            if (!CheckVegetationPackageContainsItems()) return;

            if (currentBulkEditingTab != 3)
            {
                VegetationPackageEditorTools.DrawVegetationItemSelector(VSP,
                    VSP.VegetationPackageProList[VSP.VegetationPackageIndex],
                    ref currentSelectedGridIndex,
                    ref currentSelectedVegetationIndex,
                    ref selectionCount,
                    VegetationItemTypeSelection(),
                    64);

                EditorGUILayout.Space(spacing);
            }
            // Pasting copied settings section
            //
            if (currentBulkEditingTab == 0 || currentBulkEditingTab == 1)
            {
                content = new GUIContent("  Copy and Paste Item Settings ", "Pastes the currently copied item settings to item or all items of a type.");
                LayoutBox(headerColor, content, headerStyle);
                EditorGUILayout.Space(smallSpacing);
                allowDuplicateItems = GUILayout.Toggle(allowDuplicateItems, "Allow duplication of pasted items?");
                copyAllSettings = GUILayout.Toggle(copyAllSettings, "Copy all item rules regardless of selection?");
                alwaysEnableRuntimeSpawnOnPastedItems = GUILayout.Toggle(alwaysEnableRuntimeSpawnOnPastedItems, "Always enable runtime spawn on pasted and duplicated items?");
                DrawCopySettings();
            }

            if (currentBulkEditingTab == 0) // Single package copy/paste
            {

                if (DrawButton("Copy selected object settings")) copiedItem = VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[currentSelectedVegetationIndex];
                if (copiedItem != null && copiedItem.VegetationPrefab != null)
                {
                    DrawItemInformation(copiedItem, true);
                    DrawPasteCopiedItemSettings();
                    DrawPasteCopiedItemSettingsToAllOfType();
                }
                else EditorGUILayout.HelpBox("You must copy an item in order to select and paste its settings", MessageType.Warning);
            }
            // Package to package copy section
            //
            else if (currentBulkEditingTab == 1) // 2 package copy/paste
            {

                GUILayout.BeginVertical();
                if (VSP.VegetationPackageProList.Count > 1)
                {
                    if (VSP.VegetationPackageIndex == packageToPasteTo)
                    {
                        EditorGUILayout.Space(spacing);
                        EditorGUILayout.HelpBox("Source and destination package are the same, select a different destination package",
                            MessageType.Error, true);
                    }
                    else
                    {
                        DrawItemInformation(VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[currentSelectedVegetationIndex]);
                        EditorGUILayout.Space(spacing);
                        DrawPasteCopiedItemToAnotherPackage();

                        DrawPasteAllItemsOfSelectedTypeFromPackageToPackage();

                    }
                }
                GUILayout.EndVertical();
            }

            // Deleting section
            //
            if (currentBulkEditingTab == 2)
            {
                EditorGUILayout.BeginVertical("box");
                content = new GUIContent("  Delete Items", "Delete the selected item or all of an item type.");
                LayoutBox(headerColor, content, headerStyle);
                DrawDelectCurrentItem();
                DrawDeleteAllItemsOfType();

                EditorGUILayout.EndVertical();
            }
            // Misc editing section
            //
            if (currentBulkEditingTab == 3)
            {

                EditorGUILayout.BeginVertical("box");
                content = new GUIContent("  Enable / Disable Runtime Spawn", "Enable and disable runtime spawn for all items of the selected type.");
                LayoutBox(headerColor, content, headerStyle);
                DrawRuntimeSpawnTypeSelector();
                DrawEnableRuntimeSpawningOnAllItemsOfType();
                DrawDisableRuntimeSpawningOnAllItemsOfType();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(spacing);
            }
        }
        void DrawRuntimeSpawnTypeSelector()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Type :");
            bulkEnableRuntimeSpawn = EditorGUILayout.Popup(bulkEnableRuntimeSpawn, VegetationTypeNames);
            GUILayout.EndHorizontal();
        }
        void DrawEnableRuntimeSpawningOnAllItemsOfType()
        {
            EditorGUILayout.Space(spacing);
            if (DrawButton("Enable runtime spawn on all items of type"))
            {
                if (EditorUtility.DisplayDialog("Enable runtime spawn on all items of type : " + VegetationTypeNames[bulkEnableRuntimeSpawn] + "?",
                "Do you want to enable runtime spawn on all items of type : " + VegetationTypeNames[bulkEnableRuntimeSpawn] + "?", "Enable Runtime Spawn", "Cancel"))
                {
                    // Bulk enable runtime

                }
                GUIUtility.ExitGUI();
            }
        }
        void DrawDisableRuntimeSpawningOnAllItemsOfType()
        {
            EditorGUILayout.Space(spacing);
            if (DrawButton("Disable runtime spawn on all items of type"))
            {
                if (EditorUtility.DisplayDialog("Disable runtime spawn on all items of type : " + VegetationTypeNames[bulkEnableRuntimeSpawn] + "?",
                    "Do you want to disable runtime spawn on all items of type : " + VegetationTypeNames[bulkEnableRuntimeSpawn] + "?", "Disable Runtime Spawn", "Cancel"))
                {
                    // Bulk disable runtime

                }
                GUIUtility.ExitGUI();
            }


        }
        void DrawDelectCurrentItem()
        {
            DrawItemInformation(VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[currentSelectedVegetationIndex]);
            if (DrawButton("Delete current selected item"))
            {
                if (EditorUtility.DisplayDialog("Delete current selected item : " + VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[currentSelectedVegetationIndex].Name + "?",
                "Do you want to delete the current selected item : " + VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[currentSelectedVegetationIndex].Name + "?", "Delete", "Cancel"))
                {
                    // Single delete
                    DeleteVegetationItem();
                    currentSelectedVegetationIndex = 0;
                }
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.Space(spacing);
        }
        void DrawDeleteAllItemsOfType()
        {
            if (DrawButton("Delete all items of type : "))
            {
                if (EditorUtility.DisplayDialog("Delete all items of type : " + VegetationTypeNames[bulkItemsToDelete] + "?",
                "Do you want to delete all of the items of type : " + VegetationTypeNames[bulkItemsToDelete] + "?", "Delete", "Cancel"))
                {
                    // Bulk delete
                    DeleteVegetationItems();
                    currentSelectedVegetationIndex = 0;
                }
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type :");
            bulkItemsToDelete = EditorGUILayout.Popup(bulkItemsToDelete, VegetationTypeNames);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(spacing);
        }
        void DrawPasteCopiedItemToAnotherPackage()
        {
            if (DrawButton("Paste selected item to another package"))
            {
                if (EditorUtility.DisplayDialog($"Paste copied item : " + copiedItem.Name + " to package : " + PackageNames[packageToPasteTo] + "?",
                "Do you want to paste the copied item : " + copiedItem.Name + " to package : " + PackageNames[packageToPasteTo] + "?", "Paste Item to Package", "Cancel"))
                {
                    copiedItem = VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[currentSelectedVegetationIndex];
                    PasteCurrentCopiedItemToAnotherPackage();
                }
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.Space(spacing);
        }
        void DrawPasteAllItemsOfSelectedTypeFromPackageToPackage()
        {
            if (DrawButton("Paste all items of type to another package : "))
            {
                if (EditorUtility.DisplayDialog("Paste all items of type : " + VegetationTypeNames[packageToPasteAllOfType] + " to package : " + PackageNames[packageToPasteTo] + "?",
                "Do you want to paste all items of type: " + VegetationTypeNames[packageToPasteAllOfType] + " to package: " + PackageNames[packageToPasteTo] + " ? ", "Paste All Items to Package", "Cancel"))
                {
                    PasteAllItemsOfTypeToAnotherPackage();
                }
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type :");
            packageToPasteAllOfType = EditorGUILayout.Popup(packageToPasteAllOfType, VegetationTypeNames);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(spacing);
        }
        void DrawPasteCopiedItemSettings()
        {
            VegetationItemInfoPro pasteToItem = VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[currentSelectedVegetationIndex];

            if (pasteToItem == copiedItem)
            {
                EditorGUILayout.HelpBox("The copied item and the selected item to paste onto must be different for single item settings pasting. To paste item settings to another item, select another item. You can still paste the copied settings to all items of a specific type.", MessageType.Warning);
                return;
            }

            if (DrawButton("Paste copied item settings to the selected object"))
            {
                if (EditorUtility.DisplayDialog("Paste copied settings?",
                "Do you want to paste the copied settings from : " + copiedItem.Name + "\nTo selected item : " +
                pasteToItem.Name + "?",
                "Paste", "Cancel"))
                {
                    // Undo
                    Undo.SetCurrentGroupName("Paste vegetation item");
                    Undo.RecordObject(VSP.VegetationPackageProList[VSP.VegetationPackageIndex], "Paste Settings");
                    // Copy the current selected item info settings
                    if (copyAllSettings) PasteAllItemSettingsToAnotherItem(pasteToItem, copiedItem);
                    else if (copiedItem.VegetationPrefab != null) PasteItemSettingsToAnotherItem(pasteToItem, copiedItem);
                    // refresh
                    SceneUtility.SetSceneDirtySystemAndVegetaionPackage(VSP);
                }
                GUIUtility.ExitGUI();
            }
            DrawItemInformation(pasteToItem);

        }
        void DrawPasteCopiedItemSettingsToAllOfType()
        {
            if (DrawButton("Paste copied item settings to ALL items of type"))
            {
                if (EditorUtility.DisplayDialog("Paste copied settings to all of type : " + VegetationTypeNames[bulkCopyToSettingsType] + "?",
                "Do you want to paste the copied settings from : " + copiedItem.Name + "\nTo all items of type : " +
                VegetationTypeNames[bulkCopyToSettingsType] + "?", "Paste", "Cancel"))
                {
                    //Undo
                    Undo.SetCurrentGroupName("Paste vegetation item");
                    Undo.RecordObject(VSP.VegetationPackageProList[VSP.VegetationPackageIndex], "Paste Settings");

                    for (int i = 0; i < VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList.Count; i++)
                    {
                        VegetationItemInfoPro pasteToItem = VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList[i];

                        // see if this item matches the selected paste-to type or if the type is all
                        if (pasteToItem.VegetationType == (VegetationType)VSPVegetationPackageTools.ConvertStringToVegetationType(
                            VegetationTypeNames[bulkCopyToSettingsType]) ||
                            VegetationTypeNames[bulkCopyToSettingsType] == "All")
                        {
                            if (copyAllSettings) PasteAllItemSettingsToAnotherItem(pasteToItem, copiedItem);
                            else if (copiedItem.VegetationPrefab != null) PasteItemSettingsToAnotherItem(pasteToItem, copiedItem);
                        }
                    }
                    //refresh
                    SceneUtility.SetSceneDirtySystemAndVegetaionPackage(VSP);
                }
                GUIUtility.ExitGUI();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Type :");
            bulkCopyToSettingsType = EditorGUILayout.Popup(bulkCopyToSettingsType, VegetationTypeNames);
            GUILayout.EndHorizontal();
            GUILayout.Space(spacing);
        }
        #endregion

        #region VSP specific helper functions
        /// <summary>
        /// Convert type of vegetation to edit in this package to something internally useful
        /// Internally useful
        /// </summary>
        /// <returns> VegetationItemTypeSelection based on integer value </returns>
        VegetationPackageEditorTools.VegetationItemTypeSelection VegetationItemTypeSelection()
        {
            return VegetationPackageEditorTools.GetVegetationItemTypeSelection(currentSelectedVegetationType);
        }

        /// <summary>
        /// Checks that the current selected vegetation package contains vegetation items
        /// Displays warning if no items are present in the package
        /// </summary>
        /// <returns>false if no items exist in the currently selected vegetation package</returns>
        bool CheckVegetationPackageContainsItems()
        {
            if (VSP.VegetationPackageProList[VSP.VegetationPackageIndex].VegetationInfoList.Count == 0)
            {
                EditorGUILayout.HelpBox("The current biome package does not contain vegetation items.", MessageType.Error);
                ResetSelectionValues();
                return false;
            }
            return true;
        }

        #endregion

        #region Inspector drawing helpers
        /// <summary>
        /// Draws the editor extention type tab
        /// </summary>
        void DrawSelectEditorTypeTab()
        {
            LayoutBox(headerColor, new GUIContent("  Select Editor Extention ", "Choose between different editor extentions."), headerStyle);
            //EditorGUILayout.HelpBox("Select editing mode", MessageType.None, true);
            EditorGUILayout.Space(spacing);
            currentTab = GUILayout.SelectionGrid(currentTab, tabNames, 3);
        }
        /// <summary>
        /// Draws a dropdown for the selected vegetation type
        /// </summary>
        void DrawVegetationItemSelectorDropDown()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space(spacing);
            if (currentBulkEditingTab == 0 || currentBulkEditingTab == 1)
            {
                LayoutBox(headerColor,
                    new GUIContent("  Select Vegetation Item", "The item selected can be copied and pasted to, copy an item and select another item to paste settings onto."),
                    headerStyle);
            }
            else if (currentBulkEditingTab == 2)
            {
                LayoutBox(headerColor,
                    new GUIContent("  Select Vegetation Item To Delete ", "Selects an item to delete."),
                    headerStyle);
            }
            if (currentBulkEditingTab != 3)
            {
                EditorGUILayout.Space(spacing);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Sort by type : ");
                currentSelectedVegetationType = EditorGUILayout.Popup(currentSelectedVegetationType, VegetationTypeNames);
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck()) ResetSelectionValues();
                EditorGUILayout.Space(spacing);
            }
        }
        /// <summary>
        /// Draws a dropdown for selecting biome package
        /// </summary>
        void DrawSelectBiomeDropDown()
        {
            GUIStyle style = new GUIStyle(GUI.skin.GetStyle("DropDown"));
            style.fontSize += 4;
            EditorGUI.BeginChangeCheck();

            if (currentBulkEditingTab != 1) // All single package edit types
            {
                EditorGUILayout.Space(spacing);
                LayoutBox(headerColor,
                    new GUIContent("  Select a Biome Package To Edit ", "The biome package to edit."),
                    headerStyle);
                EditorGUILayout.Space(smallSpacing);
                VSP.VegetationPackageIndex = EditorGUILayout.Popup(VSP.VegetationPackageIndex, PackageNames, style);
                EditorGUILayout.LabelField("Biome : " + VSP.VegetationPackageProList[VSP.VegetationPackageIndex].BiomeType.ToString());
            }
            else if (currentBulkEditingTab == 1) // Multi package copy
            {
                EditorGUILayout.Space(spacing);
                LayoutBox(headerColor,
                    new GUIContent("  Select a Biome Package To Copy From ", "The source biome package to copy items from."),
                    headerStyle);
                EditorGUILayout.Space(smallSpacing);
                VSP.VegetationPackageIndex = EditorGUILayout.Popup(VSP.VegetationPackageIndex, PackageNames, style);
                EditorGUILayout.LabelField("Biome : " + VSP.VegetationPackageProList[VSP.VegetationPackageIndex].BiomeType.ToString());

                EditorGUILayout.Space(spacing);
                LayoutBox(headerColor,
                    new GUIContent("  Select a Biome Package To Paste Into "),
                    headerStyle);
                EditorGUILayout.Space(smallSpacing);
                packageToPasteTo = EditorGUILayout.Popup(packageToPasteTo, PackageNames, style);
                EditorGUILayout.LabelField("Biome : " + VSP.VegetationPackageProList[packageToPasteTo].BiomeType.ToString());
            }
            if (EditorGUI.EndChangeCheck()) ResetSelectionValues();
        }
        /// <summary>
        /// Draws the copy item settings inspector
        /// </summary>
        bool toggle = false;
        void DrawCopySettings()
        {
            EditorGUILayout.Space(smallSpacing);
            EditorGUILayout.HelpBox("Select individual rules / settings that will be copied.", MessageType.Info);
            EditorGUILayout.Space(smallSpacing);
            showPositionRules = VegetationPackageEditorTools.DrawHeader("Show position rules", showPositionRules);
            float originalLableWidth = EditorGUIUtility.labelWidth;
            // EditorGUIUtility.labelWidth = (EditorGUIUtility.currentViewWidth/2) - 30;
            if (showPositionRules)
            {
                EditorGUILayout.Space(smallSpacing);
                EditorGUILayout.BeginHorizontal();
                copySamplePosition = EditorGUILayout.ToggleLeft("Sample position", copySamplePosition);
                copyDensity = EditorGUILayout.ToggleLeft("Density", copyDensity);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                copyRotationSettings = EditorGUILayout.ToggleLeft("Rotation rules", copyRotationSettings);
                copyScaleSettings = EditorGUILayout.ToggleLeft("Scale rules", copyScaleSettings);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                copyHeightSettings = EditorGUILayout.ToggleLeft("Height rules", copyHeightSettings);
                copySteepnessSettings = EditorGUILayout.ToggleLeft("Steepness rules", copySteepnessSettings);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                copyPositionOffsetSettings = EditorGUILayout.ToggleLeft("Position offset rules", copyPositionOffsetSettings);
                copyHeightOffset = EditorGUILayout.ToggleLeft("Height offset rules", copyHeightOffset);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(smallSpacing);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select all position rules"))
                {
                    copySamplePosition = true;
                    copyDensity = true;
                    copyRotationSettings = true;
                    copyScaleSettings = true;
                    copyHeightSettings = true;
                    copySteepnessSettings = true;
                    copyPositionOffsetSettings = true;
                    copyHeightOffset = true;
                }
                if (GUILayout.Button("Deselect all position rules"))
                {
                    copySamplePosition = false;
                    copyDensity = false;
                    copyRotationSettings = false;
                    copyScaleSettings = false;
                    copyHeightSettings = false;
                    copySteepnessSettings = false;
                    copyPositionOffsetSettings = false;
                    copyHeightOffset = false;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space(smallSpacing);

            showRuleListToCopy = VegetationPackageEditorTools.DrawHeader("Show placement rules", showRuleListToCopy);
            if (showRuleListToCopy)
            {
                EditorGUILayout.Space(smallSpacing);
                EditorGUILayout.BeginHorizontal();
                copyNoiseCutoffRules = EditorGUILayout.ToggleLeft("Noise cutoff rules", copyNoiseCutoffRules);
                copyNoiseDensityRules = EditorGUILayout.ToggleLeft("Noise density rules", copyNoiseDensityRules);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                copyNoiseScaleRules = EditorGUILayout.ToggleLeft("Noise scale rules", copyNoiseScaleRules);
                copyBiomeEdgeScaleRules = EditorGUILayout.ToggleLeft("Biome edge exclude rules", copyBiomeEdgeScaleRules);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                copyBiomeEdgeIncludeRules = EditorGUILayout.ToggleLeft("Biome edge include rules", copyBiomeEdgeIncludeRules);
                copyConcaveLocationRules = EditorGUILayout.ToggleLeft("Concave location rules", copyConcaveLocationRules);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                copyTerrainTextureExcludeRules = EditorGUILayout.ToggleLeft("Terrain texture exclude rules", copyTerrainTextureExcludeRules);
                copyTerrainTextureIncludeRules = EditorGUILayout.ToggleLeft("Terrain texture include rules", copyTerrainTextureIncludeRules);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                copyTextureMaskExcludeRules = EditorGUILayout.ToggleLeft("Texture mask exclude rules", copyTextureMaskExcludeRules);
                copyTextureMaskIncludeRules = EditorGUILayout.ToggleLeft("Texture mask include rules", copyTextureMaskIncludeRules);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                copyVegetationMaskRule = EditorGUILayout.ToggleLeft("Vegetation mask rules", copyVegetationMaskRule);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(smallSpacing);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select all placement rules"))
                {
                    copyNoiseCutoffRules = true;
                    copyNoiseDensityRules = true;
                    copyNoiseScaleRules = true;
                    copyBiomeEdgeScaleRules = true;
                    copyBiomeEdgeIncludeRules = true;
                    copyConcaveLocationRules = true;
                    copyTerrainTextureExcludeRules = true;
                    copyTerrainTextureIncludeRules = true;
                    copyTextureMaskExcludeRules = true;
                    copyTextureMaskIncludeRules = true;
                    copyVegetationMaskRule = true;
                }
                if (GUILayout.Button("Deselect all placement rules"))
                {
                    copyNoiseCutoffRules = false;
                    copyNoiseDensityRules = false;
                    copyNoiseScaleRules = false;
                    copyBiomeEdgeScaleRules = false;
                    copyBiomeEdgeIncludeRules = false;
                    copyConcaveLocationRules = false;
                    copyTerrainTextureExcludeRules = false;
                    copyTerrainTextureIncludeRules = false;
                    copyTextureMaskExcludeRules = false;
                    copyTextureMaskIncludeRules = false;
                    copyVegetationMaskRule = false;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(smallSpacing);


            showMiscSettings = VegetationPackageEditorTools.DrawHeader("Show misc rules", showMiscSettings);
            if (showMiscSettings)
            {
                EditorGUILayout.Space(smallSpacing);
                EditorGUILayout.BeginHorizontal();
                copybillboardSettings = EditorGUILayout.ToggleLeft(new GUIContent("Billboard settings", "Trees only"), copybillboardSettings);
                copyColliderType = EditorGUILayout.ToggleLeft(new GUIContent("Collider type", "Trees only"), copyColliderType);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                copyLodSettings = EditorGUILayout.ToggleLeft(new GUIContent("LOD settings", "Doesnt apply to trees"), copyLodSettings);
                copyDistanceFalloff = EditorGUILayout.ToggleLeft(new GUIContent("Distance falloff", "Doesn't apply to trees"), copyDistanceFalloff);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                copyFoliageSettings = EditorGUILayout.ToggleLeft(new GUIContent("Foliage settings", "Only copies settings from same shader type."), copyFoliageSettings);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(smallSpacing);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select all misc rules"))
                {
                    copybillboardSettings = true;
                    copyColliderType = true;
                    copyLodSettings = true;
                    copyDistanceFalloff = true;
                    copyFoliageSettings = true;
                }
                if (GUILayout.Button("Deselect all misc rules"))
                {
                    copybillboardSettings = false;
                    copyColliderType = false;
                    copyLodSettings = false;
                    copyDistanceFalloff = false;
                    copyFoliageSettings = false;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUIUtility.labelWidth = originalLableWidth;
            EditorGUILayout.Space(smallSpacing);

        }
        /// <summary>
        /// Draws the item name and vegetation type
        /// </summary>
        /// <param name="item"></param>
        /// <param name="copied"></param>
        void DrawItemInformation(VegetationItemInfoPro item, bool copied = false)
        {
            Color guiBackGroundColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(.2f, .2f, .5f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");
            if (copied)
                EditorGUILayout.LabelField("Copied Prefab : " + item.Name, GUILayout.Width(EditorGUIUtility.currentViewWidth - 100));
            else EditorGUILayout.LabelField("Selected Prefab : " + item.Name, GUILayout.Width(EditorGUIUtility.currentViewWidth - 100));
            EditorGUILayout.LabelField("Type : " + item.VegetationType, GUILayout.Width(EditorGUIUtility.currentViewWidth - 100));
            //EditorGUILayout.Space(spacing);
            EditorGUILayout.EndVertical();
            EditorGUILayout.LabelField(new GUIContent(AssetPreviewCache.GetAssetPreview(item.VegetationPrefab)), GUILayout.MaxWidth(64), GUILayout.MaxHeight(64));
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = guiBackGroundColor;
        }
        /// <summary>
        /// Helper to draw a colored header
        /// </summary>
        /// <param name="color"></param>
        /// <param name="content"></param>
        /// <param name="style"></param>
        public static void LayoutBox(Color color, GUIContent content, GUIStyle style)
        {
            Color backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;

            GUILayout.Box(content, style);
            GUI.backgroundColor = backgroundColor;
        }
        /// <summary>
        /// Helper to draw a colored label with a larger text than the standard gui skin text
        /// </summary>
        /// <param name="labelText"></param>
        /// <param name="textColor"></param>
        /// <param name="additionalSize"></param>
        public static void LabelWithLargerColoredText(string labelText, Color textColor, int additionalSize)
        {
            GUIStyle labelStyle = new GUIStyle("Label")
            {
                normal = new GUIStyleState { textColor = textColor },
                fontSize = GUI.skin.font.fontSize + additionalSize,
            };
            GUILayout.Label(labelText, labelStyle);
        }

        // Button style
        GUIStyle buttonStyle = new GUIStyle();
        /// <summary>
        /// Draws the button with a different style and color
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        bool DrawButton(string text)
        {
            Color guiBackGroundColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(.5f, .5f, .8f);
            buttonStyle.fontSize = GUI.skin.font.fontSize + 3;
            buttonStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
            buttonStyle.fontSize += 3;
            if (GUILayout.Button(text, buttonStyle))
            {
                GUI.backgroundColor = guiBackGroundColor;
                return true;
            }
            GUI.backgroundColor = guiBackGroundColor;
            return false;
        }

        #endregion
    }
}
#endif
