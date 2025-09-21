using System;
using HarmonyLib;
using UnityEngine;

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

        static void Prefix(CardData cardData)
        {
            CardDataTracker.SetCurrentCard(cardData);
        }

        static void Postfix(CardUI __instance)
        {
            CardDataTracker.ClearCurrentCard();
        }
    }
}