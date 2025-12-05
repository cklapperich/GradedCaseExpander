using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Reflection;
using TMPro;


// STEP 0 - DONE: GradingOverhaulCompat class provides soft dependency via reflection - DONE
// STEP 1: WE NEED A FUNCTION THAT READS IN AN ARBITRARY FOLDER FROM THE Pluginpath DIRECTORY, AND LOADS ALL THE CONFIGS FOUND THERE. - DONE
// STEP 2: ITERATE OVER EVERY FOLDER IN THE PLUGINS DIR AND LOAD THE SPRITES AND CONFIGS INTO DICTIONARIES. An image or config can be specific to a folder, a grade, or specific to both. - DONE

// STEP 3: Function to return correct sprites for simplicity.

// first we need 
// to get a company 'stamp' use this. 'company' is just an enum, need to use company.tostring()
// CompanyStampManager.TryGetCompany(cardData, out var company);
// Goal: make a new function, given a cardData and a grade, attempts to get the company. If it fails, return based on grade as in @card3duigrouppatch.cs
// the new function should return a tuple of the sprite and cropped sprite.
// see this old code as reference:
/*
                // Get both cropped sprite and full texture for this grade
                Sprite croppedSprite = null;
                Texture2D fullTexture = null;

                if (Plugin.GradeCroppedSprites.ContainsKey(grade))
                {
                    croppedSprite = Plugin.GradeCroppedSprites[grade];
                    fullTexture = Plugin.GradeTextures[grade];
                }
                else if (Plugin.DefaultLabelCroppedSprite != null)
                {
                    croppedSprite = Plugin.DefaultLabelCroppedSprite;
                    fullTexture = Plugin.DefaultLabelTexture;
                }
*/
// but now we want to FIRST attempt to return the 

namespace GradedCardExpander
{
    public static class MyPluginInfo
    {
        public const string PLUGIN_GUID = "GradedCaseExpander";
        public const string PLUGIN_NAME = "GradedCaseExpander";
        public const string PLUGIN_VERSION = "1.1.0";
    }

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Card Shop Simulator.exe")]
    [BepInDependency("shaklin.TextureReplacer", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("TCGCardShopSimulator.GradingOverhaul", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        internal static string PluginPath;
        internal static Dictionary<int, Sprite> GradeSprites = new Dictionary<int, Sprite>();
        internal static Dictionary<int, Texture2D> GradeTextures = new Dictionary<int, Texture2D>();
        internal static Dictionary<int, Sprite> GradeCroppedSprites = new Dictionary<int, Sprite>();
        internal static Dictionary<int, GradedCardGradeConfig> GradeConfigs = new Dictionary<int, GradedCardGradeConfig>();
        // Fonts now managed by FontLoader - this property provides backwards compatibility
        internal static Dictionary<string, TMP_FontAsset> LoadedFonts => FontLoader.LoadedFonts;

        // New folder-based asset storage: key is folder name ("" for root, "PSA", "BGS", etc.)
        internal static Dictionary<string, GradeAssets> FolderAssets = new Dictionary<string, GradeAssets>();

        // Convenience properties for accessing default (grade 0) assets
        internal static Texture2D DefaultLabelTexture => GradeTextures.ContainsKey(0) ? GradeTextures[0] : null;
        internal static Sprite DefaultLabelSprite => GradeSprites.ContainsKey(0) ? GradeSprites[0] : null;
        internal static Sprite DefaultLabelCroppedSprite => GradeCroppedSprites.ContainsKey(0) ? GradeCroppedSprites[0] : null;

        // Full textures for CardBackMeshBlocker (same as GradeTextures but more explicit naming)
        internal static Dictionary<int, Texture2D> GradeFullTextures => GradeTextures;
        internal static Texture2D DefaultLabelFullTexture => DefaultLabelTexture;
        private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            Logger = base.Logger;
            PluginPath = Path.GetDirectoryName(Info.Location);

            // Load root folder as "_default"
            FolderAssets["_default"] = GradeAssets.LoadFromFolder(PluginPath);

            // Load each subfolder
            foreach (string subdir in Directory.GetDirectories(PluginPath))
            {
                string folderName = Path.GetFileName(subdir);
                FolderAssets[folderName] = GradeAssets.LoadFromFolder(subdir);
            }

            harmony.PatchAll();
        }

        /// <summary>
        /// Gets the appropriate GradeAssets for a card based on company, expansion, or default.
        /// Priority: Company (from GradingOverhaul) → Expansion name → "_default"
        /// </summary>
        /// <param name="cardData">The card data object (can be null)</param>
        /// <param name="expansionName">Optional expansion name to check (e.g., from cardData.expansionType.ToString())</param>
        /// <returns>The matching GradeAssets, or null if none found</returns>
        public static GradeAssets GetAssetsForCard(object cardData, string expansionName = null)
        {
            // 1. Try company from GradingOverhaul
            if (cardData != null && GradingOverhaulCompat.IsAvailable)
            {
                if (GradingOverhaulCompat.TryGetCompany(cardData, out string company) && !string.IsNullOrEmpty(company))
                {
                    if (FolderAssets.TryGetValue(company, out var companyAssets) && companyAssets.HasAssets)
                    {
                        return companyAssets;
                    }
                }
            }

            // 2. Try expansion name
            if (!string.IsNullOrEmpty(expansionName))
            {
                if (FolderAssets.TryGetValue(expansionName, out var expansionAssets) && expansionAssets.HasAssets)
                {
                    return expansionAssets;
                }
            }

            // 3. Fall back to default
            if (FolderAssets.TryGetValue("_default", out var defaultAssets))
            {
                return defaultAssets;
            }

            return null;
        }

        /// <summary>
        /// Gets texture for a card and grade. Convenience method combining GetAssetsForCard + GetTexture.
        /// </summary>
        public static Texture2D GetTextureForCard(object cardData, int grade, string expansionName = null)
        {
            return GetAssetsForCard(cardData, expansionName)?.GetTexture(grade);
        }

        /// <summary>
        /// Gets sprite for a card and grade. Convenience method combining GetAssetsForCard + GetSprite.
        /// </summary>
        public static Sprite GetSpriteForCard(object cardData, int grade, string expansionName = null)
        {
            return GetAssetsForCard(cardData, expansionName)?.GetSprite(grade);
        }

        /// <summary>
        /// Gets cropped sprite for a card and grade. Convenience method combining GetAssetsForCard + GetCroppedSprite.
        /// </summary>
        public static Sprite GetCroppedSpriteForCard(object cardData, int grade, string expansionName = null)
        {
            return GetAssetsForCard(cardData, expansionName)?.GetCroppedSprite(grade);
        }

        /// <summary>
        /// Gets config for a card and grade. Convenience method combining GetAssetsForCard + GetConfig.
        /// </summary>
        public static GradedCardGradeConfig GetConfigForCard(object cardData, int grade, string expansionName = null)
        {
            return GetAssetsForCard(cardData, expansionName)?.GetConfig(grade);
        }
    }
}