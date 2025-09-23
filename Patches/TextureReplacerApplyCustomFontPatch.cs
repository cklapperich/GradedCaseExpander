using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;

namespace GradedCardExpander.Patches
{
    [HarmonyPatch(typeof(TextureReplacer.BepInExPlugin), "ApplyCustomFont")]
    public class TextureReplacerApplyCustomFontPatch
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("TextureReplacerFontPatch");

        [HarmonyPrefix]
        static bool Prefix(object __instance, GameObject cardFront)
        {
            // Block TextureReplacer from applying fonts to graded cards only
            if (cardFront != null && cardFront.name == "GradingSlabGrp")
            {
                return false; // Skip TextureReplacer's font application for graded cards
            }

            // Allow TextureReplacer to proceed for all other elements
            return true;
        }
    }
}