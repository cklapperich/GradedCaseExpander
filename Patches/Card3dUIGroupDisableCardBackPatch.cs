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

            // Inspect and disable label background images for transparent graded case support
            if (__instance.m_GradedCardGrp != null)
            {
                var labelBack = __instance.m_GradedCardGrp.transform.Find("LabelImageBack")?.GetComponent<UnityEngine.UI.Image>();
                var labelImage = __instance.m_GradedCardGrp.transform.Find("LabelImage")?.GetComponent<UnityEngine.UI.Image>();

                if (labelBack != null)
                {
                    var rect = labelBack.rectTransform;
                    Logger.LogInfo($"LabelImageBack - Size: {rect.sizeDelta}, Position: {rect.anchoredPosition}, Sprite: {labelBack.sprite?.name} ({labelBack.sprite?.texture?.width}x{labelBack.sprite?.texture?.height})");

                    // Apply custom sprite to the visible label (LabelImageBack)
                    if (Plugin.CustomLabelSprite != null)
                    {
                        labelBack.sprite = Plugin.CustomLabelSprite;
                        labelBack.color = Color.white;
                        labelBack.type = UnityEngine.UI.Image.Type.Simple;
                        labelBack.preserveAspect = false;
                        Logger.LogInfo("Applied custom sprite to LabelImageBack (the visible one)");
                    }
                    else
                    {
                        labelBack.enabled = false;
                    }
                }

                if (labelImage != null)
                {
                    // Disable the hidden/unused label
                    labelImage.enabled = false;
                }
            }
        }
    }
}