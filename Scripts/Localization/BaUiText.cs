using System;
using Localizor;
using UnityEngine;

namespace Capisoft.Lib.BaUnifiedUI.Localization
{
    /// <summary>Shared localization helper for mod UI strings.</summary>
    public static class BaUiText
    {
        public static string Loc(string key, string fallback)
        {
            if (string.IsNullOrWhiteSpace(key))
                return fallback ?? string.Empty;

            try
            {
                // Avoid Localizor warnings when consumers probe missing keys.
                if (LocalizorManager.IsLocalizedKey(key))
                {
                    var text = key.GetLocalization();
                    if (!string.IsNullOrWhiteSpace(text) && text != key)
                        return text;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[BaUiText] Loc failed for '" + key + "': " + ex.Message);
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
