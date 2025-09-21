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
            // Disable any card back textures in the graded card group (removes front-face interference)
            if (__instance.m_GradedCardGrp != null)
            {
                Renderer[] renderers = __instance.m_GradedCardGrp.GetComponentsInChildren<Renderer>(true);
                UnityEngine.UI.Image[] images = __instance.m_GradedCardGrp.GetComponentsInChildren<UnityEngine.UI.Image>(true);
                //Logger.LogInfo($"EvaluateCardGrade called - Found {renderers.Length} renderers and {images.Length} UI Images to process");

                int totalMaterialsChecked = 0;
                int renderersDisabled = 0;

                foreach (Renderer renderer in renderers)
                {
                    //Logger.LogInfo($"Processing renderer: {renderer.name} (GameObject: {renderer.gameObject.name}) - Enabled: {renderer.enabled}");

                    if (renderer.materials != null)
                    {
                        totalMaterialsChecked += renderer.materials.Length;
                        //Logger.LogInfo($"  Found {renderer.materials.Length} materials on this renderer");

                        for (int i = 0; i < renderer.materials.Length; i++)
                        {
                            Material mat = renderer.materials[i];
                            if (mat != null)
                            {
                                string textureName = mat.mainTexture?.name ?? "null";
                                //Logger.LogInfo($"    Material[{i}]: {mat.name} - Texture: {textureName}");

                                if (mat.mainTexture != null && mat.mainTexture.name.Contains("CardBack"))
                                {
                                    //Logger.LogInfo($"    *** MATCH: Disabling renderer '{renderer.name}' due to CardBack texture '{mat.mainTexture.name}' ***");
                                    renderer.enabled = false;
                                    renderersDisabled++;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}