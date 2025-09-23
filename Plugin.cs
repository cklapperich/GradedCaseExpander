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
        internal static Sprite TransparentSprite;
        internal static Dictionary<string, TMP_FontAsset> LoadedFonts = new Dictionary<string, TMP_FontAsset>();

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

            LoadGradeSpecificAssets();
            // CreateTransparentSprite(); // No longer needed - using grade-specific sprites

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
                        }
                    }
                }
            }
        }

        private void CreateTransparentSprite()
        {
            // Create a 615x160 transparent texture (same size as expected label)
            // This matches the documented label size and ensures proper stretching behavior
            Texture2D transparentTexture = new Texture2D(615, 160);

            // Fill entire texture with transparent pixels
            Color[] transparentPixels = new Color[615 * 160];
            for (int i = 0; i < transparentPixels.Length; i++)
            {
                transparentPixels[i] = Color.clear; // Fully transparent
            }
            transparentTexture.SetPixels(transparentPixels);
            transparentTexture.Apply();
            transparentTexture.name = "TransparentTexture_615x160";

            // Create sprite from the transparent texture
            TransparentSprite = Sprite.Create(transparentTexture, new Rect(0, 0, 615, 160), new Vector2(0.5f, 0.5f));
            TransparentSprite.name = "TransparentSprite_615x160";

            Logger.LogInfo("Created 615x160 transparent sprite for LabelImageBack replacement");
        }

        public static Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh();

            // Vertices for a quad (rectangle)
            Vector3[] vertices = new Vector3[4]
            {
                new Vector3(-0.5f, -0.5f, 0f), // Bottom-left
                new Vector3(0.5f, -0.5f, 0f),  // Bottom-right
                new Vector3(-0.5f, 0.5f, 0f),  // Top-left
                new Vector3(0.5f, 0.5f, 0f)    // Top-right
            };

            // UV coordinates for texture mapping
            Vector2[] uv = new Vector2[4]
            {
                new Vector2(0, 0), // Bottom-left
                new Vector2(1, 0), // Bottom-right
                new Vector2(0, 1), // Top-left
                new Vector2(1, 1)  // Top-right
            };

            // Triangles (two triangles make a quad)
            int[] triangles = new int[6]
            {
                0, 2, 1, // First triangle
                2, 3, 1  // Second triangle
            };

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        public static Material CreateTransparentMaterial(Texture2D texture)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.SetFloat("_Mode", 3); // Transparent mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;

            if (texture != null)
            {
                material.mainTexture = texture;
            }

            return material;
        }
    }
}