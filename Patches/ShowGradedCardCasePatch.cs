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
            if (__instance.m_GradedCardCaseGrp == null){
                return;
            }

            // Start a coroutine to wait for the shield to lift
            __instance.StartCoroutine(ApplyAssetsNextFrame(__instance, isShow));
        }
        static System.Collections.IEnumerator ApplyAssetsNextFrame(CardUI __instance, bool isShow)
        {
                // Wait for end of frame (Ensures Grading Overhaul Postfix has restored the data)
                yield return new WaitForEndOfFrame(); 

                var cardData = __instance.GetCardData();
                
                // NOW this will return 106 (PSA), not 10 (Vanilla)
                // Get card data directly from the instance
                int grade = GradingOverhaulCompat.GetDisplayGrade(cardData);
                if (cardData == null || grade <= 0){
                    yield break;
                }
                string expansionName = cardData.expansionType.ToString();
                GradeAssets assets = Plugin.GetAssetsForCard(cardData,expansionName);

                if (assets == null)
                {
                    // No assets for this expansion/company - leave card unmodified
                    yield break;
                }

                Sprite spriteToApply = assets.GetSprite(grade);
                GradedCardGradeConfig config = assets.GetConfig(grade);

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