using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Logging;

namespace GradedCardExpander.Patches
{
    [HarmonyPatch(typeof(CardUI))]
    [HarmonyPatch("ShowGradedCardCase")]
    public class ShowGradedCardCasePatch
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("ShowGradedCardCasePatch");

        // Uncomment this method if you need to debug the hierarchy
        /*
        static void Prefix(CardUI __instance, bool isShow)
        {
            if (__instance.GetCardData() != null)
            {
                int cardGrade = __instance.GetCardData().cardGrade;
                // Only dump hierarchy for graded cards that are being shown
                if (isShow && cardGrade > 0 && __instance.m_GradedCardCaseGrp != null)
                {
                    DebugUtils.DumpObjectHierarchyWithHeader(__instance.m_GradedCardCaseGrp, "m_GradedCardCaseGrp HIERARCHY");
                }
            }
        }
        */

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

                        Logger.LogInfo($"Applied grade {grade} sprite to CardUI GradedCardCase");
                    }
                    else
                    {
                        Logger.LogWarning($"No sprite available for grade {grade} and no DefaultLabel fallback");
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
            else
            {
                Logger.LogWarning($"No text configuration found for grade {grade} or fallback");
            }
        }
    }
}