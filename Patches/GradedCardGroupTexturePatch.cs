// using HarmonyLib;
// using UnityEngine;

// namespace GradedCardExpander.Patches
// {
//     [HarmonyPatch(typeof(Card3dUIGroup))]
//     [HarmonyPatch("EvaluateCardGrade")]
//     public class GradedCardGroupTexturePatch
//     {
//         static void Postfix(Card3dUIGroup __instance, CardData cardData)
//         {
//             if (cardData.cardGrade > 0 && __instance.m_GradedCardGrp != null)
//             {
//                 // Get the grade configuration for this card
//                 var gradeConfig = Plugin.GradedConfig.GetGradeConfig(cardData.cardGrade);

//                 if (gradeConfig.CaseTexture != null)
//                 {
//                     ApplyGradedCaseTexture(__instance.m_GradedCardGrp, gradeConfig.CaseTexture);
//                 }
//             }
//         }

//         private static void ApplyGradedCaseTexture(GameObject gradedCardGrp, Texture2D customTexture)
//         {
//             Renderer[] renderers = gradedCardGrp.GetComponentsInChildren<Renderer>();
//             foreach (var renderer in renderers)
//             {
//                 if (renderer.name == "Slab_BaseMesh" && renderer.materials != null)
//                 {
//                     for (int i = 0; i < renderer.materials.Length; i++)
//                     {
//                         var material = renderer.materials[i];
//                         if (material != null && material.mainTexture?.name == "trading_card_transparent")
//                         {
//                             material.mainTexture = customTexture;
//                         }
//                     }
//                 }
//             }
//         }
//     }
// }