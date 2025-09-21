using HarmonyLib;
using TMPro;
using UnityEngine;

namespace GradedCardExpander.Patches
{
    [HarmonyPatch(typeof(Card3dUIGroup))]
    [HarmonyPatch("EvaluateCardGrade")]
    public class Card3dUIGroupEvaluateCardGradePatch
    {
        static void Postfix(Card3dUIGroup __instance, CardData cardData)
        {
            if (cardData.cardGrade <= 0)
                return;

            // Get the grade configuration
            var gradeConfig = Plugin.GradedConfig.GetGradeConfig(cardData.cardGrade);

            // Apply text styling
            ApplyTextStyling(__instance.m_GradeNumberText, gradeConfig.GradeNumberText);
            ApplyTextStyling(__instance.m_GradeDescriptionText, gradeConfig.GradeDescriptionText);
            ApplyTextStyling(__instance.m_GradeNameText, gradeConfig.GradeNameText);
            ApplyTextStyling(__instance.m_GradeExpansionRarityText, gradeConfig.GradeExpansionRarityText);
        }

        private static void ApplyTextStyling(TextMeshProUGUI textComponent, GradedCardTextConfig textConfig)
        {
            if (textComponent == null || textConfig == null)
                return;

            // Apply color
            textComponent.color = textConfig.Color;

            // Apply font size
            textComponent.fontSize = textConfig.FontSize;

            // Apply position offset
            if (textConfig.Position != Vector2.zero)
            {
                RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Vector2 currentPos = rectTransform.anchoredPosition;
                    rectTransform.anchoredPosition = currentPos + textConfig.Position;
                }
            }
        }
    }
}