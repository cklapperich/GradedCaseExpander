using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Logging;

/* 
    if ((Object)(object)CardCropGrading == (Object)null)
    {
        Texture2D cachedTexture = GetCachedTexture("GradedCardCase");
        if ((Object)(object)cachedTexture != (Object)null)
        {
            int num3 = 271;
            int num4 = 484;
            int num5 = 128;
            int num6 = ((Texture)cachedTexture).height - 208;
            Texture2D val2 = new Texture2D(num4, num5, (TextureFormat)4, false);
            Graphics.CopyTexture((Texture)(object)cachedTexture, 0, 0, num3, num6, num4, num5, (Texture)(object)val2, 0, 0, 0, 0);
            Sprite val3 = TextureToSprite(val2);
            if ((Object)(object)val3 != (Object)null)
            {
                CardCropGrading = val3;
            }
        }
    }
*/
namespace GradedCardExpander.Patches
{
    [HarmonyPatch(typeof(Card3dUIGroup))]
    [HarmonyPatch("EvaluateCardGrade")]
    public class Card3dUIGroupDisableCardBackPatch
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Card3dUIGroupPatch");

        static void Postfix(Card3dUIGroup __instance, CardData cardData)
        {
            // Only apply to graded cards (grade > 0)
            if (__instance.m_GradedCardGrp != null && cardData != null && cardData.cardGrade > 0)
            {
                // Log the actual GameObject name for debugging
                //Logger.LogInfo($"GradedCardGrp GameObject name: '{__instance.m_GradedCardGrp.name}'");
                int grade = cardData.cardGrade;

                // Apply grade-specific sprite to LabelImageBack
                //ApplyGradeSpecificSprite(__instance, grade);

                // Clear CardBackMesh to prevent black rectangle
                //ClearCardBackMesh(__instance);

                // Apply text configuration
                if (Plugin.GradeConfigs.ContainsKey(grade))
                {
                    ApplyTextConfiguration(__instance, Plugin.GradeConfigs[grade]);
                }
                else if (Plugin.GradeConfigs.ContainsKey(0)) // Fallback to DefaultLabel.txt
                {
                    ApplyTextConfiguration(__instance, Plugin.GradeConfigs[0]);
                }
            }
        }

        private static void ApplyGradeSpecificSprite(Card3dUIGroup instance, int grade)
        {
            // Find LabelImageBack UI component and apply grade-specific sprite
            var labelBack = instance.m_GradedCardGrp?.transform.Find("LabelImageBack")?.GetComponent<Image>();
            var labelImage = instance.m_GradedCardGrp?.transform.Find("LabelImage")?.GetComponent<Image>();

            // Apply grade-specific sprite or fallback
            if (labelImage != null)
            {
                if (Plugin.GradeSprites.ContainsKey(grade))
                {
                    labelImage.sprite = Plugin.GradeSprites[grade];
                    Logger.LogInfo($"Applied grade {grade} sprite to LabelImageBack");
                }
                else if (Plugin.GradeSprites.ContainsKey(0)) // Fallback to DefaultLabel.png
                {
                    labelImage.sprite = Plugin.GradeSprites[0];
                    Logger.LogInfo($"Applied fallback sprite (DefaultLabel.png) to LabelImageBack for grade {grade}");
                }
                else
                {
                    labelImage.enabled = false;
                    Logger.LogWarning("No grade sprites loaded, disabling LabelImageBack");
                    return;
                }

                labelImage.color = Color.white;
                labelImage.type = Image.Type.Simple;
                labelImage.preserveAspect = false;
            }

        }

        private static void ClearCardBackMesh(Card3dUIGroup instance)
        {
            // Find and modify CardBackMesh material color
            Transform cardBackMesh = instance.m_GradedCardGrp?.transform.Find("CardBackMeshBlocker");
            Renderer cardBackRenderer = cardBackMesh?.GetComponent<Renderer>();

            if (cardBackRenderer != null && cardBackRenderer.material != null)
            {   // important! T_CardBackMesh.png is replaced with a transparent PNG, for testing, outside of this mode.
                // disabling this causes REALLY weird issues with transparency. Setting this to false does cause the right parts of the label to be transparent, but if you have multiple overlapping cards in your hand, the labels render through each other!
                cardBackRenderer.enabled=true; 
                cardBackRenderer.material.color = Color.clear; // BUT this doesnt seem to do anything? You still get a black rectangle.
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
            }
            if (config.FontSize.HasValue)
            {
                text.fontSize = config.FontSize.Value;
            }
            if (config.Font != null)
            {
                text.font = config.Font;
            }
            if (config.Position.HasValue)
            {
                var originalPos = text.rectTransform.anchoredPosition;
                text.rectTransform.anchoredPosition = originalPos + config.Position.Value;
            }
        }
    }
}