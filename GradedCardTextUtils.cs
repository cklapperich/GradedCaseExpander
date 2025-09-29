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
            if (config.OutlineColor.HasValue)
            {
                text.outlineColor = config.OutlineColor.Value;
            }
            if (config.OutlineWidth.HasValue)
            {
                // TextMeshPro outline width should be between 0.0 and 1.0 typically
                // Values outside this range can cause unexpected behavior
                float clampedWidth = Mathf.Clamp(config.OutlineWidth.Value, 0.0f, 1.0f);

                // Create a new material instance to avoid affecting other text components
                if (text.fontMaterial != null)
                {
                    text.fontMaterial = new Material(text.fontMaterial);
                    text.fontMaterial.SetFloat("_OutlineWidth", clampedWidth);
                }

                // Also set the component property as backup
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
            var type = instance.GetType();

            var gradeNumberText = type.GetField("m_GradeNumberText")?.GetValue(instance) as TextMeshProUGUI;
            var gradeDescriptionText = type.GetField("m_GradeDescriptionText")?.GetValue(instance) as TextMeshProUGUI;
            var gradeNameText = type.GetField("m_GradeNameText")?.GetValue(instance) as TextMeshProUGUI;
            var gradeExpansionRarityText = type.GetField("m_GradeExpansionRarityText")?.GetValue(instance) as TextMeshProUGUI;
            var gradeSerialText = type.GetField("m_GradeSerialText")?.GetValue(instance) as TextMeshProUGUI; // "NumberText"

            ApplyTextConfig(gradeNumberText, config.GradeNumberText, is3D);
            ApplyTextConfig(gradeDescriptionText, config.GradeDescriptionText, is3D);
            ApplyTextConfig(gradeNameText, config.GradeNameText, is3D);
            ApplyTextConfig(gradeExpansionRarityText, config.GradeExpansionRarityText, is3D);
            ApplyTextConfig(gradeSerialText, config.GradeSerialText, is3D);
        }
    }
}