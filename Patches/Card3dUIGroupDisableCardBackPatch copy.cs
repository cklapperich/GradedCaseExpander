// using HarmonyLib;
// using UnityEngine;
// using BepInEx.Logging;

// namespace GradedCardExpander.Patches
// {
//     [HarmonyPatch(typeof(Card3dUIGroup))]
//     [HarmonyPatch("EvaluateCardGrade")]
//     public class Card3dUIGroupDisableCardBackPatch
//     {
//         private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Card3dUIGroupPatch");

//         static void Postfix(Card3dUIGroup __instance)
//         {
//             // Disable the CardBackMeshBlocker renderer directly
//             Transform cardBackBlocker = __instance.m_GradedCardGrp?.transform.Find("CardBackMeshBlocker");
//             if (cardBackBlocker?.GetComponent<Renderer>() is Renderer renderer)
//                 renderer.enabled = false;
//         }
//     }
// }