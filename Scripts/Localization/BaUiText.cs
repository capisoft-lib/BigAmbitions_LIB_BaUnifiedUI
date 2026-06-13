using Localizor;
using UnityEngine;

namespace Capisoft.Lib.BaUi.Localization
{
    /// <summary>Shared localization helper for mod UI strings.</summary>
    public static class BaUiText
    {
        public static string Loc(string key, string fallback)
        {
            try
            {
                var text = key.GetLocalization();
                if (!string.IsNullOrWhiteSpace(text) && text != key)
                    return text;
            }
            catch
            {
                // mod key not registered yet
            }

            return fallback;
        }

        public static string ResolveLoadedLocale()
        {
            try
            {
                var locale = LocalizorManager.LoadedLocale;
                if (!string.IsNullOrWhiteSpace(locale))
                    return locale.Trim().Replace('_', '-');
            }
            catch
            {
                // Localizor not ready
            }

            return "en";
        }
    }
}
