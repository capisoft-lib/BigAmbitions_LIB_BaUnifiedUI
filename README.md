# LIB_BaUnifiedUI

Standalone Big Ambitions library mod for vanilla-style UI chrome, fluent builders, reusable controls, draggable windows, and configurable shortcuts.

> ☕ If this library has saved your mods from reinventing every button, [buy me a coffee](https://buymeacoffee.com/capitaine). The UI stays unified without caffeine; its maintainer is less certain.

Repository: https://github.com/capisoft-lib/BigAmbitions_LIB_BaUnifiedUI

| Property | Value |
|---|---|
| **Version** | `1.0.1` |
| **Game** | Big Ambitions **EA 0.11** and **1.0 experimental** |
| **Mod ID** | `LIB_BaUnifiedUI` |
| **Assembly** | `LIB_BaUnifiedUI` (stable across updates) |
| **Unity** | `2022.3.62f2` with the Big Ambitions Modding SDK |
| **Distribution** | Separate Steam Workshop dependency |

## Player installation

This library does not add gameplay by itself. Install it when another mod lists **LIB BA Unified UI** as required:

1. Subscribe to **LIB BA Unified UI** and the consumer mod on Steam Workshop.
2. Enable both entries in Big Ambitions' **Mods** menu.
3. Restart the game after either Workshop item updates.

Consumer mods must not bundle their own copy of this DLL. A single enabled Workshop installation supplies the shared assembly.

Version 1.0.1 adds settings rows cloned directly from the game's own Options prefabs, alongside the native color picker, visible scroll rails, cross-version draggable-window integration, and corrected close-button spacing.

## Namespace

`Capisoft.Lib.BaUnifiedUI.*` — `.Assets`, `.Chrome`, `.Layout`, `.Controls`, `.Fluent`, `.Localization`, `.Shortcuts`, `.BaXaml`

## Install into the SDK

Copy this repository to:

```text
<BigAmbitionsSdk>/Assets/Mods/LIB_BaUnifiedUI/
```

Consumer asmdefs reference this library by GUID:

```text
f1e2d3c4b5a6478890ab1c2d3e4f5a6b
```

Build and install the library before its consumers. Do not add `LIB_BaUnifiedUI.dll` to a consumer's `Dependencies/` folder.

## Build

Normal workflow: open **Big Ambitions → Mod Builder** in the matching Unity Editor and choose **Build + Install** for `LIB_BaUnifiedUI`. That is all a mod author needs to do; the SDK's packaging step applies the required player-runtime compatibility automatically.

For repeatable release checks or CI, the headless equivalent is:

```powershell
.\Assets\Mods\LIB_BaUnifiedUI\tools\build-official.ps1
```

The repository also retains this maintenance fallback for diagnosing an unavailable Unity Editor:

```powershell
.\scripts\compile-install-lib-ba-unified-ui.ps1
```

These paths must produce `Output/LIB_BaUnifiedUI/LIB_BaUnifiedUI.dll`. Run `tools/validate-workshop-release.ps1` after building the library and its consumers.

## Compatibility policy

The public assembly identity stays `LIB_BaUnifiedUI`; it is never suffixed with the layout revision. `BaUiVersion.LayoutRevision` remains an internal signal that tells consumer panels when to rebuild. New releases should preserve existing public members where practical and document the minimum library version when a consumer uses a newer API.

## Draggable windows

Interactive panels can opt into the game's native draggable-window service.
Dragging starts only on the free header surface, keeps header buttons clickable,
uses the native Move cursor, clamps the panel to the safe area, and stores its
position in the player's window settings.

```csharp
var panel = BaUi.Overlay("MyHud", sortOrder: 9000)
    .Dock(BaDock.BottomLeft)
    .Panel(BaPanelRecipe.ActionPanel, 370f, height)
    .Draggable("my-mod:main-hud")
    .Header(h => h.Title("MY HUD"))
    .Build();

if (panel.Drag.HasSavedPosition || panel.Drag.IsDragging)
{
    // Do not re-apply an automatic dock while the user owns the position.
}
```

Panels built manually with `BaUiWidePanelChrome.BuildPanel` can attach the same
native behavior through `BaUiWidePanelChrome.AttachDraggableWindow`.

## Vanilla Options visibility

Every BAUI canvas is automatically hidden and made non-interactive while the
game's Options screen is visible. The library does not change the root's active
state, so the consumer window returns exactly as it was when Options closes.

## Native settings rows

`BaUiVanillaSettings` clones Big Ambitions' own `Options > Mods` toggle and
slider prefabs. The game therefore supplies the real sprites, spacing,
transitions, fonts, pill toggles, slider tracks, and circular handles; the
consumer supplies only its current value, display text, and callback.

```csharp
using Capisoft.Lib.BaUnifiedUI.Controls;

BaUiVanillaSettings.CreateToggle(
    content,
    "Enable deliveries",
    deliveriesEnabled,
    value => SetDeliveriesEnabled(value));

BaUiVanillaSettings.CreateSlider(
    content,
    "Delivery capacity",
    1,
    100,
    capacity,
    value => value + " items",
    value => SetCapacity(value));
```

These controls deliberately do not write PlayerPrefs. This keeps persistence
and immediate-apply behavior under the consumer mod's ownership.

## Keyboard shortcuts in Options > Mods

`AddKeybind` (also available as `AddShortcut`) is a custom official `ModOption`.
It stores its value under the same `m:{modId}:{optionId}` PlayerPrefs namespace as
the game controls, so **Reset All** restores the declared default.

```csharp
using BigAmbitions.Mods;
using Capisoft.Lib.BaUnifiedUI.Shortcuts;
using UnityEngine.InputSystem;

private BaKeybindHandle _togglePanelShortcut;

var options = new ModOptions()
    .AddKeybind(
        "toggle_panel_shortcut",
        "my_mod_option_toggle_panel_shortcut",
        new BaKeybind(Key.F8),
        out _togglePanelShortcut);

OptionsService.Register(context.ModId, options);

// In the consumer mod's Update method:
if (_togglePanelShortcut.WasPressedThisFrame())
    TogglePanel();
```

The row accepts one non-modifier key plus Ctrl/Shift/Alt/Cmd. Escape cancels
capture and Backspace/Delete clears the shortcut. The handle automatically yields
while the game blocks keyboard shortcuts, a text/UI control is selected, another
BAUILib row is capturing, or any key in the chord conflicts with a current game binding or
another active BAUILib shortcut. Game bindings are observed but never modified.

Conflict discovery covers the game's current player and vehicle Input System
assets plus participating BAUILib options. A mod that polls raw keyboard state
without registering through BAUILib cannot be discovered; `Unbound` is therefore
the only universally conflict-free value.

## Color picker in Options > Mods

`AddColorPicker` (also available as `AddColor`) adds a native-style row with a
live swatch, hexadecimal value, and reset button. Clicking the field opens Big
Ambitions' own HSV color picker. The selected `Color` is stored as `#RRGGBBAA`
under `m:{modId}:{optionId}`, so the game's **Reset All** action restores the
declared default.

```csharp
using BigAmbitions.Mods;
using Capisoft.Lib.BaUnifiedUI.Options;
using UnityEngine;

private BaColorPickerHandle _routeColor;

var options = new ModOptions()
    .AddColorPicker(
        "route_color",
        "my_mod_option_route_color",
        new Color(0.1f, 0.75f, 1f, 1f),
        out _routeColor,
        color => ApplyRouteColor(color));

OptionsService.Register(context.ModId, options);

// Available anywhere after registration:
ApplyRouteColor(_routeColor.Color);
```

Picker movements update the handle's `ColorChanged` event and the row preview
live. Persistence and the option's `onValueChanged` callback occur once when the
picker closes with a different final color. Native Cancel restores the opening
color without writing or notifying the consumer callback.

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

## License

MIT — see [LICENSE](LICENSE).
