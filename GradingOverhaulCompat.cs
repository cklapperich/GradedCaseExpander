using System;
using System.Reflection;

namespace GradedCardExpander
{
    internal static class GradingOverhaulCompat
    {
        private static bool? _isAvailable;
        private static Type _companyStampManagerType;
        private static MethodInfo _tryGetCompanyMethod;

        public static bool IsAvailable
        {
            get
            {
                if (_isAvailable == null)
                {
                    _companyStampManagerType = Type.GetType(
                        "TCGCardShopSimulator.GradingOverhaul.CompanyStampManager, TCGCardShopSimulator.GradingOverhaul");
                    _isAvailable = _companyStampManagerType != null;
                    if (_isAvailable.Value)
                    {
                        _tryGetCompanyMethod = _companyStampManagerType.GetMethod(
                            "TryGetCompany", BindingFlags.Public | BindingFlags.Static);
                    }
                }
                return _isAvailable.Value;
            }
        }

        public static bool TryGetCompany(object cardData, out string company)
        {
            company = null;
            if (!IsAvailable || _tryGetCompanyMethod == null)
                return false;

            object[] args = new object[] { cardData, null };
            bool result = (bool)_tryGetCompanyMethod.Invoke(null, args);
            if (result)
                company = args[1]?.ToString();
            return result;
        }
    }
}
