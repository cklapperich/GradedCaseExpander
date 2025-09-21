using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

namespace GradedCardExpander
{
    public class GradedCardTextConfig
    {
        public Color? Color { get; set; } = null;
        public float? FontSize { get; set; } = null;
        public Vector2? Position { get; set; } = null;
    }

    public class GradedCardGradeConfig
    {
        public GradedCardTextConfig GradeNumberText { get; set; } = new GradedCardTextConfig();
        public GradedCardTextConfig GradeDescriptionText { get; set; } = new GradedCardTextConfig();
        public GradedCardTextConfig GradeNameText { get; set; } = new GradedCardTextConfig();
        public GradedCardTextConfig GradeExpansionRarityText { get; set; } = new GradedCardTextConfig();
    }

    public class GradedCardConfig
    {
        private GradedCardGradeConfig _defaultConfig = new GradedCardGradeConfig();
        private string _configFilePath;

        public void Initialize(string configFilePath)
        {
            _configFilePath = configFilePath;
            LoadConfiguration();
        }

        public GradedCardGradeConfig GetGradeConfig(int grade)
        {
            // All grades use the same configuration from the single file
            return _defaultConfig;
        }

        private void LoadConfiguration()
        {
            if (!File.Exists(_configFilePath))
            {
                Plugin.Logger.LogWarning($"GradedCardCase.txt file does not exist: {_configFilePath}");
                return;
            }

            // Note: LoadTextConfig moved to Plugin class for grade-specific loading
            Plugin.Logger.LogInfo($"Loaded graded card text config from: {_configFilePath}");
        }
    }

    public static class MyPluginInfo
    {
        public const string PLUGIN_GUID ="com.snowbloom.cardshopsim.gradedcardcaseexpander";
        public const string PLUGIN_NAME = "GradedCardCaseExpander";
        public const string PLUGIN_VERSION = "1.0.0";
    }

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Card Shop Simulator.exe")]
    [BepInDependency("shaklin.TextureReplacer", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        internal static string PluginPath;
        internal static GradedCardConfig GradedConfig = new GradedCardConfig();
        internal static Dictionary<int, Sprite> GradeSprites = new Dictionary<int, Sprite>();
        internal static Dictionary<int, GradedCardGradeConfig> GradeConfigs = new Dictionary<int, GradedCardGradeConfig>();

        private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            Logger = base.Logger;
            PluginPath = Path.GetDirectoryName(Info.Location);

            string gradedCardCaseFile = Path.Combine(PluginPath, "GradedCardCase.txt");
            GradedConfig.Initialize(gradedCardCaseFile);

            LoadGradeSpecificAssets();

            harmony.PatchAll();
            Logger.LogInfo("GradedCardCaseExpander loaded successfully!");
        }

        private void LoadGradeSpecificAssets()
        {
            // Load fallback assets first (GradedCardCase.png and GradedCardCase.txt)
            LoadGradeAssets(0, "GradedCardCase");

            // Load grade-specific sprites (1.png to 10.png)
            for (int grade = 1; grade <= 10; grade++)
            {
                LoadGradeAssets(grade, grade.ToString());
            }

            Logger.LogInfo($"Loaded {GradeSprites.Count} grade sprites and {GradeConfigs.Count} grade configs");
        }

        private void LoadGradeAssets(int gradeKey, string baseName)
        {
            // Load sprite
            string imagePath = Path.Combine(PluginPath, $"{baseName}.png");
            if (File.Exists(imagePath))
            {
                try
                {
                    byte[] imageData = File.ReadAllBytes(imagePath);
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(imageData);

                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    GradeSprites[gradeKey] = sprite;
                    Logger.LogInfo($"Loaded {baseName} sprite: {texture.width}x{texture.height}");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to load {baseName} image: {ex.Message}");
                }
            }

            // Load config
            string configPath = Path.Combine(PluginPath, $"{baseName}.txt");
            if (File.Exists(configPath))
            {
                try
                {
                    var gradeConfig = new GradedCardGradeConfig();
                    LoadTextConfig(configPath, gradeConfig);
                    GradeConfigs[gradeKey] = gradeConfig;
                    Logger.LogInfo($"Loaded {baseName} config from: {configPath}");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to load {baseName} config: {ex.Message}");
                }
            }
        }

        private void LoadTextConfig(string configFile, GradedCardGradeConfig gradeConfig)
        {
            string[] lines = File.ReadAllLines(configFile);
            GradedCardTextConfig currentTextConfig = null;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    string sectionName = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    currentTextConfig = sectionName switch
                    {
                        "GradeNumberText" => gradeConfig.GradeNumberText,
                        "GradeDescriptionText" => gradeConfig.GradeDescriptionText,
                        "GradeNameText" => gradeConfig.GradeNameText,
                        "GradeExpansionRarityText" => gradeConfig.GradeExpansionRarityText,
                        _ => null
                    };
                    Logger.LogInfo($"Found section [{sectionName}] in {configFile}, valid: {currentTextConfig != null}");
                    continue;
                }

                if (currentTextConfig != null && trimmedLine.Contains("="))
                {
                    string[] parts = trimmedLine.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();

                        switch (key)
                        {
                            case "Color":
                                if (ColorUtility.TryParseHtmlString(value, out Color color))
                                {
                                    currentTextConfig.Color = color;
                                    Logger.LogInfo($"Loaded color {color} for current section in {configFile}");
                                }
                                else
                                {
                                    Logger.LogWarning($"Failed to parse color '{value}' in {configFile}");
                                }
                                break;
                            case "FontSize":
                                if (float.TryParse(value, out float fontSize))
                                {
                                    currentTextConfig.FontSize = fontSize;
                                }
                                break;
                            case "OffsetX":
                                if (float.TryParse(value, out float offsetX))
                                {
                                    var currentOffset = currentTextConfig.Position ?? Vector2.zero;
                                    currentTextConfig.Position = new Vector2(offsetX, currentOffset.y);
                                    Logger.LogInfo($"Loaded OffsetX {offsetX} for current section in {configFile}");
                                }
                                break;
                            case "OffsetY":
                                if (float.TryParse(value, out float offsetY))
                                {
                                    var currentOffset = currentTextConfig.Position ?? Vector2.zero;
                                    currentTextConfig.Position = new Vector2(currentOffset.x, offsetY);
                                    Logger.LogInfo($"Loaded OffsetY {offsetY} for current section in {configFile}");
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
}