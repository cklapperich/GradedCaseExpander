using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;

namespace GradedCardExpander.Patches
{
    [HarmonyPatch(typeof(Card3dUIGroup))]
    [HarmonyPatch("EvaluateCardGrade")]
    public class Card3dUIGroupDisableCardBackPatch
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Card3dUIGroupPatch");

        static void Postfix(Card3dUIGroup __instance, CardData cardData)
        {
            // Disable the CardBackMeshBlocker renderer to prevent card back texture interference
            Transform cardBackBlocker = __instance.m_GradedCardGrp?.transform.Find("CardBackMeshBlocker");
            if (cardBackBlocker?.GetComponent<Renderer>() is Renderer renderer)
                renderer.enabled = false;

            // Apply grade-specific customization
            if (__instance.m_GradedCardGrp != null && cardData != null)
            {
                int grade = cardData.cardGrade;

                var labelBack = __instance.m_GradedCardGrp.transform.Find("LabelImageBack")?.GetComponent<UnityEngine.UI.Image>();
                var labelImage = __instance.m_GradedCardGrp.transform.Find("LabelImage")?.GetComponent<UnityEngine.UI.Image>();

                // Apply grade-specific sprite or fallback
                if (labelBack != null)
                {
                    if (Plugin.GradeSprites.ContainsKey(grade))
                    {
                        labelBack.sprite = Plugin.GradeSprites[grade];
                    }
                    else if (Plugin.GradeSprites.ContainsKey(0)) // Fallback to GradedCardCase.png
                    {
                        labelBack.sprite = Plugin.GradeSprites[0];
                    }
                    else
                    {
                        labelBack.enabled = false;
                        return;
                    }

                    labelBack.color = Color.white;
                    labelBack.type = UnityEngine.UI.Image.Type.Simple;
                    labelBack.preserveAspect = false;
                }

                // Disable unused label
                if (labelImage != null)
                    labelImage.enabled = false;

                // Apply text configuration
                if (Plugin.GradeConfigs.ContainsKey(grade))
                {
                    ApplyTextConfiguration(__instance, Plugin.GradeConfigs[grade]);
                }
                else if (Plugin.GradeConfigs.ContainsKey(0)) // Fallback to GradedCardCase.txt
                {
                    ApplyTextConfiguration(__instance, Plugin.GradeConfigs[0]);
                }
            }
        }

        private static void ApplyTextConfiguration(Card3dUIGroup instance, GradedCardGradeConfig config)
        {
            ApplyTextConfig(instance.m_GradeNumberText, config.GradeNumberText);
            ApplyTextConfig(instance.m_GradeDescriptionText, config.GradeDescriptionText);
            ApplyTextConfig(instance.m_GradeNameText, config.GradeNameText);
            ApplyTextConfig(instance.m_GradeExpansionRarityText, config.GradeExpansionRarityText);
        }

        private static void ApplyTextConfig(TMPro.TextMeshProUGUI text, GradedCardTextConfig config)
        {
            if (text == null) return;

            if (config.Color.HasValue)
            {
                text.color = config.Color.Value;
                Logger.LogInfo($"Applied color {config.Color.Value} to {text.name}");
            }
            if (config.FontSize.HasValue)
            {
                text.fontSize = config.FontSize.Value;
                Logger.LogInfo($"Applied font size {config.FontSize.Value} to {text.name}");
            }
            if (config.Position.HasValue)
            {
                var originalPos = text.rectTransform.anchoredPosition;
                text.rectTransform.anchoredPosition = originalPos + config.Position.Value;
                Logger.LogInfo($"Applied offset {config.Position.Value} to {text.name} (original: {originalPos}, new: {originalPos + config.Position.Value})");
            }
        }
    }
}