using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Logging;
/* THIS METHOD PATCHES THE 2D GRADED CARDS (ONLY SEEN ON PRICE DISPLAY SCREEN) */

namespace GradedCardExpander.Patches
{
    [HarmonyPatch(typeof(CardUI))]
    [HarmonyPatch("ShowGradedCardCase")]
    public class ShowGradedCardCasePatch
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("ShowGradedCardCasePatch");

        static void Postfix(CardUI __instance, bool isShow)
        {
            if (!isShow || __instance.m_GradedCardCaseGrp == null)
                return;

            // Get current card data from tracker
            var cardData = CardUISetCardPatch.CardDataTracker.GetCurrentCardInfo();
            if (cardData == null || cardData.cardGrade <= 0)
                return;

            int grade = cardData.cardGrade;

            // Find the main GradedCardCase image component
            Transform gradedCardCaseTransform = __instance.m_GradedCardCaseGrp.transform;
            if (gradedCardCaseTransform != null)
            {
                Image gradedCardCaseImage = gradedCardCaseTransform.GetComponent<Image>();
                if (gradedCardCaseImage != null)
                {
                    // Apply grade-specific non-cropped sprite or fallback to DefaultLabel
                    Sprite spriteToApply = null;
                    if (Plugin.GradeSprites.ContainsKey(grade))
                    {
                        spriteToApply = Plugin.GradeSprites[grade];
                    }
                    else if (Plugin.DefaultLabelSprite != null)
                    {
                        spriteToApply = Plugin.DefaultLabelSprite;
                    }

                    if (spriteToApply != null)
                    {
                        gradedCardCaseImage.sprite = spriteToApply;
                        gradedCardCaseImage.color = Color.white;

                        // Logger.LogInfo($"Applied grade {grade} sprite to CardUI GradedCardCase");
                    }
                }
            }

            // Apply text configuration
            if (Plugin.GradeConfigs.ContainsKey(grade))
            {
                GradedCardTextUtils.ApplyTextConfiguration(__instance, Plugin.GradeConfigs[grade], is3D: false);
            }
            else if (Plugin.GradeConfigs.ContainsKey(0)) // Fallback to DefaultLabel.txt
            {
                GradedCardTextUtils.ApplyTextConfiguration(__instance, Plugin.GradeConfigs[0], is3D: false);
            }
        }
    }
}