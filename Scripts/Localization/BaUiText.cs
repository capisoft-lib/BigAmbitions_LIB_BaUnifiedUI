using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Localizor;
using UnityEngine;

namespace Capisoft.Lib.BaUnifiedUI.Localization
{
    /// <summary>Shared localization helper for mod UI strings.</summary>
    public static class BaUiText
    {
        private static Func<string, string> _localizeKey;
        private static bool _localizeProbeDone;

        public static string Loc(string key, string fallback)
        {
            if (string.IsNullOrWhiteSpace(key))
                return fallback ?? string.Empty;

            var localize = ResolveLocalizeFunc();
            if (localize != null)
            {
                try
                {
                    var text = localize(key);
                    if (!string.IsNullOrWhiteSpace(text) && text != key)
                        return text;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[BaUiText] Loc failed for '" + key + "': " + ex.Message);
                }
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

        private static Func<string, string> ResolveLocalizeFunc()
        {
            if (_localizeProbeDone)
                return _localizeKey;

            _localizeProbeDone = true;
            _localizeKey = TryCreateLocalizeFunc();
            if (_localizeKey == null)
                Debug.LogWarning("[BaUiText] No runtime GetLocalization(string) resolver found; using fallbacks only.");
            return _localizeKey;
        }

        private static Func<string, string> TryCreateLocalizeFunc()
        {
            foreach (var name in new[] { "GetLocalization", "GetLocalizedString", "GetText", "Localize", "Translate" })
            {
                if (TryGetStaticStringMethod(typeof(LocalizorManager), name, out var method))
                    return key => InvokeStringMethod(method, key);
            }

            if (TryFindExtensionGetLocalization(out var extension))
                return key => InvokeStringMethod(extension, key);

            return null;
        }

        private static bool TryGetStaticStringMethod(Type type, string name, out MethodInfo method)
        {
            method = type.GetMethod(
                name,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(string) },
                null);
            return method != null && method.ReturnType == typeof(string);
        }

        private static bool TryFindExtensionGetLocalization(out MethodInfo method)
        {
            method = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types ?? Array.Empty<Type>();
                }

                foreach (var type in types)
                {
                    if (type == null || !type.IsSealed || !type.IsAbstract)
                        continue;

                    var candidate = type.GetMethod(
                        "GetLocalization",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(string) },
                        null);
                    if (candidate == null || candidate.ReturnType != typeof(string))
                        continue;

                    if (!candidate.IsDefined(typeof(ExtensionAttribute), false))
                        continue;

                    method = candidate;
                    return true;
                }
            }

            return false;
        }

        private static string InvokeStringMethod(MethodInfo method, string key)
        {
            try
            {
                return method.Invoke(null, new object[] { key }) as string;
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        }
    }
}
