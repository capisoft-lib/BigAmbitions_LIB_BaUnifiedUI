namespace Capisoft.Lib.BaUnifiedUI.Controls
{
    /// <summary>Action strip presets for map list rows.</summary>
    public enum BaUiListRowRecipe
    {
        /// <summary>Center, add-bookmark, set-destination, go (visit history on city map).</summary>
        VisitHistoryMap,

        /// <summary>Go only (visit history from HUD).</summary>
        VisitHistoryHud,

        /// <summary>Center, drive, set-destination, delete (saved bookmarks).</summary>
        MapBookmark,

        /// <summary>Center, drive, set-destination (quick rows, vehicles).</summary>
        MapActions,
    }
}
