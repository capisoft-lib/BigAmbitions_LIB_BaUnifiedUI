# Publishing LIB BA Unified UI

## Release boundary

This repository prepares the release but does not upload it. Creating the Steam Workshop item and changing its visibility require an explicit publication action.

## Build and validate

1. Use Unity `2022.3.62f2`, matching `ProjectSettings/ProjectVersion.txt`.
2. In Unity, open **Big Ambitions → Mod Builder**, select `LIB_BaUnifiedUI`, then click **Build + Install**. No manual framework conversion or Mono command is required.
3. For an automated release recheck, close this Unity project and run the equivalent headless command:

   ```powershell
   .\Assets\Mods\LIB_BaUnifiedUI\tools\build-official.ps1
   ```

4. Build the current first-party consumers after the library.
5. Run the full dependency check:

   ```powershell
   .\Assets\Mods\LIB_BaUnifiedUI\tools\validate-workshop-release.ps1 -RequireConsumerOutputs -RequireInstalled
   ```

6. Confirm the final runtime package is `Output/LIB_BaUnifiedUI/` and that its only managed library is `LIB_BaUnifiedUI.dll`.

## Steam Workshop fields

- Title: `LIB BA Unified UI`
- Existing Workshop item: `3790426259`
- In-game uploader: select the installed `LIB_BaUnifiedUI` folder and confirm the thumbnail is visible in the preview before submitting
- Content folder: `Output/LIB_BaUnifiedUI/` (contains both the DLL and `Thumbnail.png` after the official build)
- Preview source: `Assets/Mods/LIB_BaUnifiedUI/Thumbnail.png`
- Summary: `releases/1.0.0/short-description.txt`
- Description: `releases/1.0.0/full-description.md`
- Change notes: `releases/1.0.0/Steam_ChangeLog.md`

Publish the library by itself first. Do not update consumer Workshop items as part of this release. Once Steam assigns the library item ID, add it to each consumer's **Required Items** when that consumer is rebuilt and published later.

## Post-upload smoke test

Subscribe through Steam, remove or disable the local developer copy, enable the Workshop library and one updated consumer, then restart the game. Verify one library load line, open a consumer panel, use a button and draggable header, and confirm there are no `TypeLoadException`, `FileNotFoundException`, or `MissingMethodException` entries for BAUI.
