# LIB_BaUnifiedUI

Shared Big Ambitions mod UI library — vanilla chrome, fluent builders, BaXaml pilot.

Repository: https://github.com/capisoft-lib/BigAmbitions_LIB_BaUnifiedUI

## Namespace

`Capisoft.Lib.BaUnifiedUI.*` — `.Assets`, `.Chrome`, `.Layout`, `.Controls`, `.Fluent`, `.Localization`, `.BaXaml`

## Deploy

```powershell
.\sdk-overlay\bigambitions\scripts\compile-install-lib-ba-unified-ui.ps1
```

Deploy **before** consumer mods (VoogleRoute, BetterFines, AutoShopping, CasinoAutoPlay).

## Fluent API (phase 1)

```csharp
var panel = BaUi.Overlay("MyHud", sortOrder: 9000)
    .Dock(BaDock.BottomLeft)
    .Panel(BaPanelRecipe.DockedHud, 370f, height)
    .Header(h => h.Title("GPS").IconButton(BaIcons.Settings, 0, OnSettings))
    .Body(b => b.VanillaButton("GO", BaButtonStyle.Blue, OnGo))
    .Build();
```

## BaXaml (phase 2 pilot)

`BaUi.LoadFromBaXaml("GpsHud")` — see `Panels/GpsHud.baxaml` and `Scripts/BaXaml/Generated/GpsHud.g.cs`.
