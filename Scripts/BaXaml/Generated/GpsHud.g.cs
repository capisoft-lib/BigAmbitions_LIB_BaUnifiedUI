using Capisoft.Lib.BaUi.Fluent;
using Capisoft.Lib.BaUi.Layout;
using Capisoft.Lib.BaUi.Localization;

namespace Capisoft.Lib.BaUi.BaXaml.Generated
{
    /// <summary>Precompiled from Panels/GpsHud.baxaml (pilot document).</summary>
    public static class GpsHudDocument
    {
        public const string DocumentName = "GpsHud";

        static GpsHudDocument()
        {
            BaUiXamlRegistry.Register(DocumentName, Build);
        }

        public static void EnsureRegistered() { }

        public static BaUiBuiltPanel Build()
        {
            var scale = 1f;
            var metrics = BaUiLayout.CreateMetrics(scale);
            return Fluent.BaUi.Overlay("BaXaml_GpsHud", sortOrder: 9000)
                .Dock(BaDock.BottomLeft)
                .Panel(BaPanelRecipe.DockedHud, BaUiLayout.PanelWidth, metrics.PanelHeight)
                .Header(h => h
                    .Title(BaUiText.Loc("voogle_route_panel_title", "VOOGLE ROUTE"), rightIconCount: 1)
                    .IconButton(BaIcons.Settings, 0, null, "⚙"))
                .Body(b => b
                    .VanillaButton(BaUiText.Loc("voogle_route_route_on", "ROUTE ON"), BaButtonStyle.Blue, null)
                    .Gap(8f)
                    .VanillaButton(BaUiText.Loc("voogle_route_autowalk", "AUTO-WALK"), BaButtonStyle.Grey, null))
                .Build();
        }
    }
}
