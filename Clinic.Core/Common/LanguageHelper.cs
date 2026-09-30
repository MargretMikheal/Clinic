using System.Threading;

namespace Clinic.Core.Common
{
    public static class LanguageHelper
    {
        public static string GetCurrentLanguage()
        {
            string currentCulture = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant();
            if (currentCulture == "en" || currentCulture == "ar")
            {
                return currentCulture;
            }

            // Default
            return "ar";
        }
    }
}
