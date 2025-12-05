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
            string expansionName = cardData.expansionType.ToString();

            // Get assets using new helper (checks company, expansion, then default)
            Sprite spriteToApply = Plugin.GetSpriteForCard(cardData, grade, expansionName);
            GradedCardGradeConfig config = Plugin.GetConfigForCard(cardData, grade, expansionName);

            // Find the main GradedCardCase image component
            Transform gradedCardCaseTransform = __instance.m_GradedCardCaseGrp.transform;
            if (gradedCardCaseTransform != null)
            {
                Image gradedCardCaseImage = gradedCardCaseTransform.GetComponent<Image>();
                if (gradedCardCaseImage != null && spriteToApply != null)
                {
                    gradedCardCaseImage.sprite = spriteToApply;
                    gradedCardCaseImage.color = Color.white;
                }
            }

            // Apply text configuration
            if (config != null)
            {
                GradedCardTextUtils.ApplyTextConfiguration(__instance, config, is3D: false);
            }
        }
    }
}