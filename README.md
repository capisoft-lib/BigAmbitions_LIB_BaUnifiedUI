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

## Chrome (all docked panels)

One header recipe for every docked panel (`ActionPanel`, `WideMapPanel`, etc.):

- Hud-trim on the panel root (`HeaderTrimWidthBase` / `HeaderTrimOffsetXBase` — legacy calibrated values)
- **Width-driven widen**: `ComputeDockedHeaderExtraTrim(panelWidth)` when width &gt; 370px (420 map panels, scaled action panel)
- Header background applied **once** after the final frame recipe
- `BaPanelRecipe` selects layout/modal/settings variants — not per-panel chrome hacks

```csharp
.Panel(BaPanelRecipe.WideMapPanel, 420f)  // widen trim is automatic from width
.Panel(BaPanelRecipe.ActionPanel, layout.PanelWidth, layout.PanelHeight)
```

```csharp
var panel = BaUi.Overlay("MyHud", sortOrder: 9000)
    .Dock(BaDock.BottomLeft)
    .Panel(BaPanelRecipe.ActionPanel, 370f, height)
    .Header(h => h.Title("VOOGLE ROUTE").Icon(BaIcons.Settings, OnSettings))
    .Body(b => b.VanillaButton("GO", BaButtonStyle.Blue, OnGo))
    .Build();
```

## Map panel content (search + scroll list)

Height is **derived from the composition** — pass width only; declare sections with fixed counts:

```csharp
BaUiScrollList scroll = null;
var built = BaUi.Overlay("MyPanel", sortOrder)
    .Panel(BaPanelRecipe.WideMapPanel, 420f)   // no height
    .Header(h => h.TitleLeft("BOOKMARKS", 1).CloseButton(Close))
    .Content(c => c
        .QuickRowStrip(slotCount: 3)
        .Search("Filter…", OnSearchChanged, out var search)
        .PickHint(out var pickHint)
        .ScrollList(visibleRowCount: 8, out scroll)
        .Footer(BaUi.Layout.ButtonHeight, h => h
            .ButtonsEqual(BaUi.Layout.ButtonGap,
                new BaHorizontalButtonSpec("ADD", BaButtonStyle.Blue, OnAdd),
                new BaHorizontalButtonSpec("CLEAR", BaButtonStyle.Red, OnClear))))
    .Build();

// built.PanelHeight == auto-summed chrome height
```

`HorizontalStack` / `Footer` accept any inner views left-to-right:

```csharp
.HorizontalStack(28f, h => h
    .Label("Status:", 60f)
    .Gap(8f)
    .View(120f, (rect, scale) => { /* custom widget */ })
    .Fill((rect, scale) => { /* takes remaining width */ }))
```

To preview height without building: `BaUi.Layout.ContentPanelHeight(c => c.ScrollList(8))`.

Explicit height still works: `.Panel(recipe, width, height)` overrides auto sizing (HUDs, modals).

## BaXaml (phase 2 pilot)

`BaUi.LoadFromBaXaml("GpsHud")` — see `Panels/GpsHud.baxaml` and `Scripts/BaXaml/Generated/GpsHud.g.cs`.
