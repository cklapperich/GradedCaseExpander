using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Collections;
using TMPro;

namespace GradedCardExpander
{
    public class GradedCardTextConfig
    {
        public Color? Color { get; set; } = null;
        public float? FontSize { get; set; } = null;
        public Vector2? Position { get; set; } = null;
        public TMP_FontAsset Font { get; set; } = null;
        public string Text { get; set; } = null;
        public Color? OutlineColor { get; set; } = null;
        public float? OutlineWidth { get; set; } = null;
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
                Plugin.Logger.LogWarning($"DefaultLabel.txt file does not exist: {_configFilePath}");
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
        internal static Dictionary<int, Texture2D> GradeTextures = new Dictionary<int, Texture2D>();
        internal static Dictionary<int, GradedCardGradeConfig> GradeConfigs = new Dictionary<int, GradedCardGradeConfig>();
        internal static Dictionary<string, TMP_FontAsset> LoadedFonts = new Dictionary<string, TMP_FontAsset>();
        internal static Texture2D DefaultLabelTexture;
        internal static Sprite DefaultLabelSprite;

        // Debug config entries
        internal static ConfigEntry<bool> DebugCardBackMeshBlocker;
        internal static ConfigEntry<bool> DebugSlabBaseMesh;
        internal static ConfigEntry<bool> DebugSlabTopLayerMesh;

        private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            Logger = base.Logger;
            PluginPath = Path.GetDirectoryName(Info.Location);

            // Initialize debug config entries
            DebugCardBackMeshBlocker = Config.Bind("Debug", "Color CardBackMeshBlocker", false,
                "Color CardBackMeshBlocker bright red for debugging");
            DebugSlabBaseMesh = Config.Bind("Debug", "Color Slab_BaseMesh", false,
                "Color Slab_BaseMesh bright green for debugging");
            DebugSlabTopLayerMesh = Config.Bind("Debug", "Color Slab_TopLayerMesh", false,
                "Color Slab_TopLayerMesh bright blue for debugging");

            // Load fonts first before processing configuration files
            LoadCustomFonts();

            string gradedCardCaseFile = Path.Combine(PluginPath, "DefaultLabel.txt");
            GradedConfig.Initialize(gradedCardCaseFile);

            LoadDefaultLabelSprite();
            LoadGradeSpecificSprites();

            // Log summary of what was loaded
            Logger.LogInfo($"Loaded sprites for grades: {string.Join(", ", GradeSprites.Keys)}");
            Logger.LogInfo($"Loaded configs for grades: {string.Join(", ", GradeConfigs.Keys)}");
            Logger.LogInfo($"Available fonts: {string.Join(", ", LoadedFonts.Keys)}");

            harmony.PatchAll();
            Logger.LogInfo("GradedCardCaseExpander loaded successfully!");
        }

        private void LoadCustomFonts()
        {
            // Get all .ttf files in the plugin directory
            string[] fontFiles = Directory.GetFiles(PluginPath, "*.ttf", SearchOption.TopDirectoryOnly);

            if (fontFiles.Length == 0)
            {
                return;
            }

            foreach (string fontPath in fontFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(fontPath);
                string fontKey = fileName.ToLower();

                // Method: Use Unity's Font constructor with file path (the most reliable approach)
                Font unityFont = new Font(fontPath);
                if (unityFont != null)
                {
                    // Create TMP_FontAsset from the Unity Font
                    TMP_FontAsset tmpFontAsset = TMP_FontAsset.CreateFontAsset(unityFont);
                    if (tmpFontAsset != null)
                    {
                        tmpFontAsset.name = $"{fileName}_Font_FromFile";
                        // Isolate from system font changes by creating independent material
                        if (tmpFontAsset.material != null)
                        {
                            tmpFontAsset.material = new Material(tmpFontAsset.material);
                            tmpFontAsset.material.name = $"{fileName}_Material";
                        }
                        LoadedFonts[fontKey] = tmpFontAsset;
                    }
                }
            }
        }

        private void LoadDefaultLabelSprite()
        {
            string imagePath = Path.Combine(PluginPath, "DefaultLabel.png");
            if (File.Exists(imagePath))
            {
                byte[] imageData = File.ReadAllBytes(imagePath);
                DefaultLabelTexture = new Texture2D(2, 2);
                DefaultLabelTexture.LoadImage(imageData);

                // Set texture properties like TextureReplacer would
                DefaultLabelTexture.name = "DefaultLabelTexture";
                DefaultLabelTexture.filterMode = FilterMode.Bilinear;
                DefaultLabelTexture.wrapMode = TextureWrapMode.Clamp;
                DefaultLabelTexture.Apply();

                // Convert to sprite like TextureReplacer does with TextureToSprite()
                DefaultLabelSprite = Sprite.Create(DefaultLabelTexture, new Rect(0, 0, DefaultLabelTexture.width, DefaultLabelTexture.height), new Vector2(0.5f, 0.5f));
                DefaultLabelSprite.name = "DefaultLabelSprite";
            }
        }

        private void LoadGradeSpecificSprites()
        {
            // Load grade-specific sprites (1.png to 10.png) using the same format as DefaultLabel
            for (int grade = 1; grade <= 10; grade++)
            {
                string imagePath = Path.Combine(PluginPath, $"{grade}.png");
                if (File.Exists(imagePath))
                {
                    byte[] imageData = File.ReadAllBytes(imagePath);
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(imageData);

                    // Set texture properties like TextureReplacer would
                    texture.name = $"Grade{grade}Texture";
                    texture.filterMode = FilterMode.Bilinear;
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.Apply();

                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    sprite.name = $"Grade{grade}Sprite";

                    GradeSprites[grade] = sprite;
                    GradeTextures[grade] = texture;
                }
            }

            // Also load corresponding .txt files for grade-specific text configs
            LoadGradeSpecificConfigs();
        }

        private void LoadGradeSpecificConfigs()
        {
            // Load DefaultLabel.txt first as fallback (grade 0)
            string defaultConfigPath = Path.Combine(PluginPath, "DefaultLabel.txt");
            if (File.Exists(defaultConfigPath))
            {
                var defaultConfig = new GradedCardGradeConfig();
                LoadTextConfig(defaultConfigPath, defaultConfig);
                GradeConfigs[0] = defaultConfig;
            }

            // Load grade-specific configs (1.txt to 10.txt)
            for (int grade = 1; grade <= 10; grade++)
            {
                string configPath = Path.Combine(PluginPath, $"{grade}.txt");
                if (File.Exists(configPath))
                {
                    var gradeConfig = new GradedCardGradeConfig();
                    LoadTextConfig(configPath, gradeConfig);
                    GradeConfigs[grade] = gradeConfig;
                }
            }
        }

        private void LoadGradeSpecificAssets()
        {
            // Load fallback assets first (DefaultLabel.png and DefaultLabel.txt)
            LoadGradeAssets(0, "DefaultLabel");

            // Load grade-specific sprites (1.png to 10.png)
            for (int grade = 1; grade <= 10; grade++)
            {
                LoadGradeAssets(grade, grade.ToString());
            }


            // Log which grades have sprites loaded for debugging
            foreach (var kvp in GradeSprites)
            {
                string gradeName = kvp.Key == 0 ? "DefaultLabel" : kvp.Key.ToString();
            }
        }

        private void LoadGradeAssets(int gradeKey, string baseName)
        {
            // Load sprite
            string imagePath = Path.Combine(PluginPath, $"{baseName}.png");
            if (File.Exists(imagePath))
            {
                byte[] imageData = File.ReadAllBytes(imagePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageData);

                // Set texture properties
                texture.name = $"GradeTexture_{baseName}";
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.Apply(); // Apply texture changes

                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                sprite.name = $"GradeSprite_{baseName}";

                GradeSprites[gradeKey] = sprite;
                GradeTextures[gradeKey] = texture;
            }

            // Load config
            string configPath = Path.Combine(PluginPath, $"{baseName}.txt");
            if (File.Exists(configPath))
            {
                var gradeConfig = new GradedCardGradeConfig();
                LoadTextConfig(configPath, gradeConfig);
                GradeConfigs[gradeKey] = gradeConfig;
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
                            case "Font":
                                
                                if (LoadedFonts.ContainsKey(value.ToLower()))
                                {
                                    currentTextConfig.Font = LoadedFonts[value.ToLower()];
                                }
                                else
                                {
                                    Logger.LogWarning($"Font '{value}' not found in loaded fonts. Available fonts: {string.Join(", ", LoadedFonts.Keys)}");
                                }
                                break;
                            case "OffsetX":
                                if (float.TryParse(value, out float offsetX))
                                {
                                    var currentOffset = currentTextConfig.Position ?? Vector2.zero;
                                    currentTextConfig.Position = new Vector2(offsetX, currentOffset.y);
                                }
                                break;
                            case "OffsetY":
                                if (float.TryParse(value, out float offsetY))
                                {
                                    var currentOffset = currentTextConfig.Position ?? Vector2.zero;
                                    currentTextConfig.Position = new Vector2(currentOffset.x, offsetY);
                                }
                                break;
                            case "Text":
                                currentTextConfig.Text = value;
                                break;
                            case "OutlineColor":
                                if (ColorUtility.TryParseHtmlString(value, out Color outlineColor))
                                {
                                    currentTextConfig.OutlineColor = outlineColor;
                                }
                                else
                                {
                                    Logger.LogWarning($"Failed to parse outline color '{value}' in {configFile}");
                                }
                                break;
                            case "OutlineWidth":
                                if (float.TryParse(value, out float outlineWidth))
                                {
                                    currentTextConfig.OutlineWidth = outlineWidth;
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
}