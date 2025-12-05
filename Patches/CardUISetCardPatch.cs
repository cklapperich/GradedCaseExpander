using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using TMPro;

namespace GradedCardExpander.Patches
{
    [HarmonyPatch(typeof(CardUI))]
    [HarmonyPatch("SetCardUI")]
    [HarmonyPatch(new Type[] { typeof(CardData) })]
    public class CardUISetCardPatch
    {
        // Made internal so it's accessible within the assembly but not exposed publicly
        internal static class CardDataTracker
        {
            private static CardData currentCardData;

            public static void SetCurrentCard(CardData data)
            {
                currentCardData = data;
            }

            public static void ClearCurrentCard()
            {
                currentCardData = null;
            }

            public static CardData GetCurrentCardInfo()
            {   
                return currentCardData;
            }
        }

        static void Prefix(CardData cardData, CardUI __instance)
        {
            // Skip processing for nested FullArt cards to prevent double-processing and sprite contamination
            var isNestedField = typeof(CardUI).GetField("m_IsNestedFullArt", BindingFlags.NonPublic | BindingFlags.Instance);
            if (isNestedField != null)
            {
                bool isNested = (bool)isNestedField.GetValue(__instance);
                if (isNested)
                {
                    // Skip setting current card data for nested calls
                    return;
                }
            }

            CardDataTracker.SetCurrentCard(cardData);
        }

        static void Postfix(CardUI __instance)
        {
            // Only clear if this wasn't a nested call
            var isNestedField = typeof(CardUI).GetField("m_IsNestedFullArt", BindingFlags.NonPublic | BindingFlags.Instance);
            if (isNestedField != null)
            {
                bool isNested = (bool)isNestedField.GetValue(__instance);
                if (isNested)
                {
                    // Skip clearing for nested calls
                    return;
                }
            }

            CardDataTracker.ClearCurrentCard();
        }
    }
}