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
    [BepInDependency("munch.gradingoverhaul", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        internal static string PluginPath;
        internal static Dictionary<int, Sprite> GradeSprites = new Dictionary<int, Sprite>();
        internal static Dictionary<int, Texture2D> GradeTextures = new Dictionary<int, Texture2D>();
        internal static Dictionary<int, Sprite> GradeCroppedSprites = new Dictionary<int, Sprite>();
        internal static Dictionary<int, GradedCardGradeConfig> GradeConfigs = new Dictionary<int, GradedCardGradeConfig>();

        // folder-based asset storage: key is folder name ("" for root, "PSA", "BGS", etc.)
        internal static Dictionary<string, GradeAssets> FolderAssets = new Dictionary<string, GradeAssets>();
        private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            Logger = base.Logger;
            PluginPath = Path.GetDirectoryName(Info.Location);

            // Load fonts FIRST before processing configuration files
            FontLoader.LoadFonts(PluginPath);

            // Load root folder as "_default"
            FolderAssets["_default"] = GradeAssets.LoadFromFolder(PluginPath);

            // Load each subfolder
            foreach (string subdir in Directory.GetDirectories(PluginPath))
            {
                string folderName = Path.GetFileName(subdir);
                FolderAssets[folderName] = GradeAssets.LoadFromFolder(subdir);
            }

            // Wire up parent relationships: all expansion folders inherit from _default
            if (FolderAssets.TryGetValue("_default", out var defaultAssets))
            {
                foreach (var kvp in FolderAssets)
                {
                    if (kvp.Key != "_default")
                        kvp.Value.Parent = defaultAssets;
                }
            }

            // Write asset log for debugging
            //string logPath = Path.Combine(PluginPath, "GradedCaseExpander_Assets.log");
            //if (File.Exists(logPath)) File.Delete(logPath);
            // foreach (var kvp in FolderAssets)
            // {
            //     kvp.Value.LogToFile(logPath, kvp.Key);
            // }

            harmony.PatchAll();
        }

        /// <summary>
        /// Gets the appropriate GradeAssets for a card based on company, expansion, or default.
        /// Priority: Company (from GradingOverhaul) → Expansion name → "_default"
        /// Returns null if no matching assets found (patches should not modify anything).
        /// </summary>
        /// <param name="cardData">The card data object (can be null)</param>
        /// <param name="expansionName">Optional expansion name to check (e.g., from cardData.expansionType.ToString())</param>
        /// <returns>The matching GradeAssets with assets, or null if none found</returns>
        public static GradeAssets GetAssetsForCard(object cardData, string expansionName = null)
        {
            if (cardData==null) return null;
            string company = GradingOverhaulCompat.TryGetCompany(cardData);
            if (company!="Vanilla" && FolderAssets.TryGetValue(company, out var companyAssets) && companyAssets.HasAssets)
            {
                return companyAssets;
            }

            // 2. Try expansion name
            if (!string.IsNullOrEmpty(expansionName))
            {
                if (FolderAssets.TryGetValue(expansionName, out var expansionAssets) && expansionAssets.HasAssets)
                {
                    return expansionAssets;
                }
            }

            // 3. Fall back to default - but ONLY if it has assets
            if (FolderAssets.TryGetValue("_default", out var defaultAssets) && defaultAssets.HasAssets)
            {
                return defaultAssets;
            }

            // No matching assets found - patches should not modify anything
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