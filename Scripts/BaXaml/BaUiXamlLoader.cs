using System;
using System.Collections.Generic;
using Capisoft.Lib.BaUnifiedUI.Fluent;

namespace Capisoft.Lib.BaUnifiedUI.BaXaml
{
    /// <summary>Runtime registry for precompiled BaXaml documents (generated *.g.cs).</summary>
    public static class BaUiXamlRegistry
    {
        private static readonly Dictionary<string, Func<BaUiBuiltPanel>> Documents =
            new Dictionary<string, Func<BaUiBuiltPanel>>(StringComparer.OrdinalIgnoreCase);

        public static void Register(string name, Func<BaUiBuiltPanel> factory) =>
            Documents[name] = factory;

        public static bool TryLoad(string name, out BaUiBuiltPanel panel)
        {
            if (Documents.TryGetValue(name, out var factory))
            {
                panel = factory();
                return true;
            }

            panel = null;
            return false;
        }
    }

    public static class BaUiXamlLoader
    {
        public static BaUiBuiltPanel Load(string documentName)
        {
            if (BaUiXamlRegistry.TryLoad(documentName, out var panel))
                return panel;

            throw new InvalidOperationException("BaXaml document not found: " + documentName);
        }
    }
}
