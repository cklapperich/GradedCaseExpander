using System;
using System.Reflection;
using HarmonyLib;

namespace GradedCardExpander
{
    internal static class GradingOverhaulCompat
    {
        public static int GetDisplayGrade(object cardData)
        {
            if (cardData == null) return 0;

            // 1. Get the Type (Same class as GetGradingCompany now!)
            Type stampManager = AccessTools.TypeByName("TCGCardShopSimulator.GradingOverhaul.CompanyStampManager");
            if (stampManager == null) return 0;

            // 2. Get the Method
            MethodInfo getGradeMethod = AccessTools.Method(stampManager, "GetDisplayGrade", new Type[] { cardData.GetType() });
            if (getGradeMethod == null) return 0;

            // 3. Invoke
            try 
            {
                return (int)getGradeMethod.Invoke(null, new object[] { cardData });
            }
            catch 
            {
                return 0;
            }
        }
        public static string TryGetCompany(object cardData)
        {
            if (cardData == null) return "Vanilla";

            // 1. Get the Type
            Type stampManager = AccessTools.TypeByName("TCGCardShopSimulator.GradingOverhaul.CompanyStampManager");
            if (stampManager == null) return "Vanilla";

            // 2. Get the Method (No Enums or 'out' params needed now)
            MethodInfo getMethod = AccessTools.Method(stampManager, "GetCompanyName", new Type[] { cardData.GetType() });
            if (getMethod == null) return "Vanilla";

            // 3. Invoke and return the result
            // This will now return "PSA", "Beckett", etc., or "Vanilla" - never null.
            try 
            {
                return (string)getMethod.Invoke(null, new object[] { cardData });
            }
            catch 
            {
                return "Vanilla";
            }
        }
    }
}
