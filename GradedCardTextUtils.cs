using UnityEngine;
using BepInEx.Logging;
using TMPro;
using System.Collections.Generic;

namespace GradedCardExpander
{
    public static class GradedCardTextUtils
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("GradedCardTextUtils");

        private static readonly Dictionary<string, Vector2> Original3DPositions = new Dictionary<string, Vector2>
        {
            { "GradeNumberText", new Vector2(218.01f, 594.00f) },
            { "GradeText", new Vector2(218.00f, 541.00f) },
            { "NameText", new Vector2(42.00f, 590.00f) },
            { "RarityText", new Vector2(0.00f, 550.00f) },
            { "NumberText", new Vector2(218.00f, 517.00f) }
        };

        private static readonly Dictionary<string, Vector2> Original2DPositions = new Dictionary<string, Vector2>
        {
            { "GradeNumberText", new Vector2(150.19f, 338.23f) },
            { "GradeText", new Vector2(150.19f, 301.71f) },
            { "NameText", new Vector2(28.94f, 335.47f) },
            { "RarityText", new Vector2(0.00f, 307.91f) },
            { "NumberText", new Vector2(150.1878f, 285.1793f) }
        };

        /// <summary>
        /// Applies text configuration to a TextMeshProUGUI component
        /// </summary>
        public static void ApplyTextConfig(TextMeshProUGUI text, GradedCardTextConfig config, bool is3D = false)
        {
            // Null check - critical for binder view where text components may not exist
            if (text == null)
            {
                Logger.LogWarning($"ApplyTextConfig called with null text component");
                return;
            }

            if (config == null)
            {
                Logger.LogWarning($"ApplyTextConfig called with null config for {text.name}");
                return;
            }

            // Enable rich text and word wrapping for all text components
            text.richText = true;
            //text.enableWordWrapping = true;

            if (config.Color.HasValue)
            {
                text.color = config.Color.Value;
            }
            if (config.FontSize.HasValue)
            {
                // Disable auto-sizing if it's enabled, which could override manual font size
                text.enableAutoSizing = false;
                text.fontSize = config.FontSize.Value;
            }
            if (config.Font != null)
            {
                text.font = config.Font;

                // Fix: Mobile shader doesn't support outlines properly
                // Force the standard Distance Field shader
                if (text.fontMaterial.shader.name.Contains("Mobile"))
                {
                    var standardShader = Shader.Find("TextMeshPro/Distance Field");
                    if (standardShader != null)
                    {
                        text.fontMaterial.shader = standardShader;
                    }
                }
            }

            // Select the appropriate position offset based on is3D flag
            Vector2? offsetToUse = is3D ? config.Position : config.Position2D;
            if (offsetToUse.HasValue)
            {
                var positionDict = is3D ? Original3DPositions : Original2DPositions;
                if (positionDict.TryGetValue(text.name, out var originalPos))
                {
                    Vector2 finalPosition = originalPos + offsetToUse.Value;
                    text.rectTransform.anchoredPosition = finalPosition;
                }
            }
            if (config.Text != null)
            {
                text.text = config.Text;
            }
            if (config.MaxTextWidthPixels.HasValue)
            {
                var sizeDelta = text.rectTransform.sizeDelta;
                text.rectTransform.sizeDelta = new Vector2(config.MaxTextWidthPixels.Value, sizeDelta.y);
            }
            // Handle outline settings - must set on component properties, not material
            if (config.OutlineColor.HasValue)
            {
                text.outlineColor = config.OutlineColor.Value;
            }

            if (config.OutlineWidth.HasValue)
            {
                float clampedWidth = Mathf.Clamp(config.OutlineWidth.Value, 0.0f, 1.0f);
                text.outlineWidth = clampedWidth;
            }

            // Force mesh update to ensure all changes are applied immediately
            text.ForceMeshUpdate();
        }

        /// <summary>
        /// Applies text configuration to any object with graded card text components using reflection
        /// Works with both Card3dUIGroup and CardUI since they have the same property names
        /// </summary>
        public static void ApplyTextConfiguration(object instance, GradedCardGradeConfig config, bool is3D = false)
        {
            if (instance == null)
            {
                Logger.LogWarning($"ApplyTextConfiguration called with null instance");
                return;
            }

            if (config == null)
            {
                Logger.LogWarning($"ApplyTextConfiguration called with null config");
                return;
            }

            Logger.LogInfo($"=== ApplyTextConfiguration on {instance.GetType().Name} (is3D={is3D}) ===");

            var type = instance.GetType();

            var gradeNumberText = type.GetField("m_GradeNumberText")?.GetValue(instance) as TextMeshProUGUI;
            var gradeDescriptionText = type.GetField("m_GradeDescriptionText")?.GetValue(instance) as TextMeshProUGUI;
            var gradeNameText = type.GetField("m_GradeNameText")?.GetValue(instance) as TextMeshProUGUI;
            var gradeExpansionRarityText = type.GetField("m_GradeExpansionRarityText")?.GetValue(instance) as TextMeshProUGUI;
            var gradeSerialText = type.GetField("m_GradeSerialText")?.GetValue(instance) as TextMeshProUGUI; // "NumberText"

            Logger.LogInfo($"Text components found: GradeNumber={gradeNumberText != null}, GradeDesc={gradeDescriptionText != null}, GradeName={gradeNameText != null}, Rarity={gradeExpansionRarityText != null}, Serial={gradeSerialText != null}");

            // Only apply if component exists - critical for binder view
            if (gradeNumberText != null)
                ApplyTextConfig(gradeNumberText, config.GradeNumberText, is3D);
            if (gradeDescriptionText != null)
                ApplyTextConfig(gradeDescriptionText, config.GradeDescriptionText, is3D);
            if (gradeNameText != null)
                ApplyTextConfig(gradeNameText, config.GradeNameText, is3D);
            if (gradeExpansionRarityText != null)
                ApplyTextConfig(gradeExpansionRarityText, config.GradeExpansionRarityText, is3D);
            if (gradeSerialText != null)
                ApplyTextConfig(gradeSerialText, config.GradeSerialText, is3D);
        }
    }
}