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
        public Color Color { get; set; } = Color.white;
        public float FontSize { get; set; } = 16f;
        public Vector2 Position { get; set; } = Vector2.zero;
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

            try
            {
                LoadTextConfig(_configFilePath, _defaultConfig);
                Plugin.Logger.LogInfo($"Loaded graded card text config from: {_configFilePath}");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Failed to load text config: {ex.Message}");
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
                                break;
                            case "FontSize":
                                if (float.TryParse(value, out float fontSize))
                                {
                                    currentTextConfig.FontSize = fontSize;
                                }
                                break;
                            case "PositionX":
                                if (float.TryParse(value, out float posX))
                                {
                                    currentTextConfig.Position = new Vector2(posX, currentTextConfig.Position.y);
                                }
                                break;
                            case "PositionY":
                                if (float.TryParse(value, out float posY))
                                {
                                    currentTextConfig.Position = new Vector2(currentTextConfig.Position.x, posY);
                                }
                                break;
                        }
                    }
                }
            }
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

        private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            Logger = base.Logger;
            PluginPath = Path.GetDirectoryName(Info.Location);

            string gradedCardCaseFile = Path.Combine(PluginPath, "GradedCardCase.txt");
            GradedConfig.Initialize(gradedCardCaseFile);

            harmony.PatchAll();
            Logger.LogInfo("GradedCardCaseExpander loaded successfully!");
        }
    }
}