// using HarmonyLib;
// using UnityEngine;
// using BepInEx.Logging;
// using System.Linq;

// namespace GradedCardExpander.Patches
// {
//     [HarmonyPatch(typeof(Card3dUIGroup))]
//     [HarmonyPatch("EvaluateCardGrade")]
//     public class Card3dUIGroupDiagnostic
//     {
//         private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Card3dUIGroupDiagnostic");

//         static void Postfix(Card3dUIGroup __instance, CardData cardData)
//         {
//             return;
//             if (__instance.m_GradedCardGrp == null)
//             {
//                 Logger.LogWarning("m_GradedCardGrp is null");
//                 return;
//             }

//             // Log the entire hierarchy
//             Logger.LogInfo("=== m_GradedCardGrp Hierarchy ===");
//             LogHierarchy(__instance.m_GradedCardGrp.transform, 0);
//             Logger.LogInfo("=== End Hierarchy ===");

//             // Find all Image components recursively and log them
//             var allImages = __instance.m_GradedCardGrp.GetComponentsInChildren<UnityEngine.UI.Image>(true);
//             Logger.LogInfo($"Found {allImages.Length} Image components:");
//             foreach (var img in allImages)
//             {
//                 Logger.LogInfo($"  - {GetFullPath(img.transform)} (enabled: {img.enabled}, sprite: {img.sprite?.name ?? "null"})");
//             }
//         }

//         private static void LogHierarchy(Transform t, int depth)
//         {
//             string indent = new string(' ', depth * 2);
//             string components = string.Join(", ", t.GetComponents<Component>().Select(c => c.GetType().Name));
//             Logger.LogInfo($"{indent}{t.name} [{components}]");

//             foreach (Transform child in t)
//             {
//                 LogHierarchy(child, depth + 1);
//             }
//         }

//         private static string GetFullPath(Transform t)
//         {
//             string path = t.name;
//             Transform current = t.parent;
//             while (current != null)
//             {
//                 path = current.name + "/" + path;
//                 current = current.parent;
//             }
//             return path;
//         }
//     }
// }