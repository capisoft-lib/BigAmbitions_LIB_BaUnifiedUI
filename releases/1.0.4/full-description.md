[b]LIB BA Unified UI 1.0.4[/b]

Independent shared user-interface library for Big Ambitions mod development.

LIB BA Unified UI does not add gameplay by itself. It provides reusable UI components and services for other mods.

[b]New in this update: game localization fix[/b]

BaUiText.Loc now resolves valid game keys through the actual Localizor API. Missing keys keep the caller's fallback without flooding the log with missing-translation warnings. Existing public APIs, assembly identity and layout revision 33 remain unchanged.

[b]Included UI services[/b]

[list]
[*]Native toggle and slider rows cloned from the game's Options prefabs
[*]Native persisted color options under Options > Mods using the game's HSV picker
[*]Vanilla-style panels, headers, buttons, lists, search fields and popups
[*]Reusable scroll lists with visible rails and draggable thumbs
[*]Reusable fluent layout builders with automatic panel sizing
[*]Draggable windows with native cursor behavior and saved positions
[*]Automatic hiding while the game's Options screen is open
[*]Configurable keyboard shortcuts with complete chord conflict detection
[*]Stable public assembly and namespace for mod integrations
[/list]

[b]Installation[/b]

[list]
[*]Subscribe to LIB BA Unified UI on Steam Workshop
[*]Enable it in the Big Ambitions Mods menu
[*]Restart the game after an update
[/list]

[b]For mod developers[/b]

Reference the stable [code]LIB_BaUnifiedUI[/code] assembly from your asmdef. Do not embed a private copy of the DLL in another package. Existing integrations do not require a rebuild for this update.

Source code and integration documentation are available in the [url=https://github.com/capisoft-lib/BigAmbitions_LIB_BaUnifiedUI]GitHub repository[/url].

Community library by capisoft-lib. Not affiliated with Hovgaard Games.

[b]Support the developer ☕[/b]

[url=https://buymeacoffee.com/capitaine]☕ Buy me a coffee[/url]
