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

        static void Postfix(Card3dUIGroup __instance)
        {
            // Disable the CardBackMeshBlocker renderer to prevent card back texture interference
            Transform cardBackBlocker = __instance.m_GradedCardGrp?.transform.Find("CardBackMeshBlocker");
            if (cardBackBlocker?.GetComponent<Renderer>() is Renderer renderer)
                renderer.enabled = false;

            // Disable label background images for transparent graded case support
            if (__instance.m_GradedCardGrp != null)
            {
                var labelBack = __instance.m_GradedCardGrp.transform.Find("LabelImageBack")?.GetComponent<UnityEngine.UI.Image>();
                var labelImage = __instance.m_GradedCardGrp.transform.Find("LabelImage")?.GetComponent<UnityEngine.UI.Image>();

                if (labelBack != null) labelBack.enabled = false;
                if (labelImage != null) labelImage.enabled = false;
            }
        }
    }
}