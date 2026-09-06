# Changelog

## [1.0.4] - 2026-09-06

### Fixed

- `BaUiText.Loc` now resolves valid game localization keys through the actual
  Localizor API, whose context parameter is optional (issue #2).
- Missing keys keep the caller's fallback without triggering Localizor warnings.

### Compatibility

- Existing public APIs, assembly identity and layout revision 33 are unchanged.

## [1.0.3] - 2026-09-06

### Fixed

- Dynamically created widgets inherit their parent UI layer, including controls
  added after a shared modal is first built.
- Gameplay shortcuts remain suppressed while a BAUI text field is focused, so
  typing B or M cannot open the phone or map.
- Focus transitions no longer attempt a nested EventSystem selection while
  Unity is already changing the selected control.

### Compatibility

- Existing public APIs and assembly identity are unchanged.
- Layout revision 33 lets consumer panels detect the corrected UI-layer setup.

## [1.0.2] - 2026-08-30

### Fixed

- Controls created through `BaUiWidgets` now inherit their parent's UI layer,
  so game hotkeys remain suppressed when a late-created text field is focused.
- Native toggle/slider rows now convert vanilla design units to BAUI design units
  independently of live canvas scale. Their text and controls no longer grow
  inside a fixed-size window when playing at 4K or using a different UI zoom.
- Preserve text-input focus when another BAUI overlay releases gameplay input.

### Compatibility

- Existing HD row proportions, public APIs, canvas settings and panel/header
  geometry are unchanged; consumer mods do not need to be rebuilt for this fix.
- Layout revision 32 lets consumers detect the updated native-row layout.

## [1.0.1] - 2026-08-29

### Added

- `BaUiVanillaSettings.CreateToggle` and `CreateSlider`, which clone the exact
  native `Options > Mods` prefabs and bind consumer-owned values and callbacks
- Native label/value replacement without taking over a consumer's persistence

### Changed

- Layout revision 31 lets consumer panels rebuild with the corrected shared
  close-button geometry

### Fixed

- Header close buttons preserve the original 8-unit top inset and now reuse
  that exact inset on the right in both direct and fluent panel builders

## [1.0.0] - 2026-08-26

### Added

- Native `Options > Mods` color picker option with swatch/hex preview, per-mod
  persistence, Reset All support, and a runtime color handle
- Reusable scroll lists now keep a visible vertical rail; its draggable thumb
  shrinks when content exceeds the viewport

### Fixed

- Exact-name panel cleanup now snapshots active scene objects before destroying each match
  once, avoiding delayed-destroy lookup loops
- Custom color and shortcut options no longer notify consumers or rewrite values
  when an Options rebuild leaves the effective value unchanged
- Color-picker persistence and its consumer callback are committed once on close
  instead of once per HSV cursor movement
- Removed obsolete wide-panel sprite/font discovery and its unused caches

### Release

- First public standalone Steam Workshop release
- Stable `LIB_BaUnifiedUI` assembly identity for independent installation and updates
- Player-compatible packaging for the Mono runtime shared by Big Ambitions EA 0.11 and 1.0 experimental
- Runtime-compatible native draggable windows across the EA 0.11 and 1.0 experimental game APIs
- Vanilla-style panels, reusable controls, draggable windows, Options visibility handling, and configurable shortcuts

### Publishing scope

- This release publishes the shared library only
- Consumer Workshop items will declare this library as a required item when they are rebuilt and published separately
- Consumer packages must reference the stable assembly and must not embed a private BAUI DLL

## [0.2.0] - 2026-08-26

### Added

- Standalone Steam Workshop library distribution with stable `LIB_BaUnifiedUI` assembly identity
- Draggable windows backed by the game's native window-position service
- Automatic hiding and interaction suppression while vanilla Options is open
- Rebindable shortcut option with full keyboard chord conflict detection
- Width-aware panel chrome and auto-sized content composition

### Changed

- Consumer mods now reference one separately installed library instead of bundling private copies
- `LayoutRevision` remains a UI rebuild marker and no longer changes the runtime assembly name
- Official Big Ambitions Mod Builder is the primary release build path

### Migration

- Existing consumer releases containing versioned `LIB_BaUnifiedUI.rNN` assemblies remain isolated during the transition
- Rebuilt consumers require `LIB_BaUnifiedUI 0.2.0+` as a separate Workshop item and must not package a BAUI DLL

[0.2.0]: https://github.com/capisoft-lib/BigAmbitions_LIB_BaUnifiedUI/releases/tag/v0.2.0
[1.0.0]: https://github.com/capisoft-lib/BigAmbitions_LIB_BaUnifiedUI/releases/tag/v1.0.0
[1.0.1]: https://github.com/capisoft-lib/BigAmbitions_LIB_BaUnifiedUI/compare/v1.0.0...v1.0.1
[1.0.3]: https://github.com/capisoft-lib/BigAmbitions_LIB_BaUnifiedUI/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/capisoft-lib/BigAmbitions_LIB_BaUnifiedUI/compare/v1.0.1...v1.0.2
