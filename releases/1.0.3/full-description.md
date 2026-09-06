[b]LIB BA Unified UI 1.0.3[/b]

Independent shared user-interface library for Big Ambitions mod development.

LIB BA Unified UI does not add gameplay by itself. It provides reusable UI components and services for other mods.

[b]New in this update: reliable text input[/b]

Dynamically created widgets now inherit the same UI layer as their parent panel. While a BAUI search or text field is focused, gameplay shortcuts such as B and M stay suppressed instead of opening the phone or map. Focus transitions also avoid nested EventSystem selection. Existing public APIs and the stable assembly identity remain unchanged.

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
