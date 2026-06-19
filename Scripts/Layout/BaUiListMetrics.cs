namespace Capisoft.Lib.BaUnifiedUI.Layout
{
    /// <summary>Shared list-row geometry (map bookmarks, visit history, etc.).</summary>
    public static class BaUiListMetrics
    {
        public const float RowHeight = 34f;
        public const float RowGap = 4f;
        public const float RowTypeIconSize = 22f;
        public const float RowActionButtonSize = 28f;
        public const float RowSetButtonWidth = 44f;
        public const float RowButtonGap = 2f;
        public const float RowButtonPadY = 3f;
        public const float RowDistanceWidth = 52f;
        public const float RowDistanceToCenterGap = 6f;
        public const float RowNameToDistanceGap = 6f;
        public const float SearchBarHeight = 28f;
        public const float SearchBarTopMargin = 8f;
        public const float HintHeight = 18f;
        public const float FooterTopMargin = 8f;
        public const float PickHintGap = 22f;

        public static float ContentRowsBlockHeight(int rowCount) =>
            rowCount <= 0 ? 0f : rowCount * RowHeight + (rowCount - 1) * RowGap;

        public static float ScrollViewportHeight(int visibleRowCount) =>
            visibleRowCount * RowHeight + (visibleRowCount - 1) * RowGap;
    }
}
