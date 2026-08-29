# Changelog

## [1.1.0] - 2026-08-29

### Added

- `BaUiVanillaSettings.CreateToggle` and `CreateSlider`, which clone the exact
  native `Options > Mods` prefabs and bind consumer-owned values and callbacks
- Native label/value replacement without taking over a consumer's persistence

### Changed

- Layout revision 28 lets consumer panels rebuild for the new settings controls

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
[1.1.0]: https://github.com/capisoft-lib/BigAmbitions_LIB_BaUnifiedUI/compare/v1.0.0...v1.1.0
