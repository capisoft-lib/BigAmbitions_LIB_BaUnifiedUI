using UnityEngine;

namespace Capisoft.Lib.BaUnifiedUI.Layout
{
    /// <summary>Row counts for <see cref="Controls.BaSettingsModalBuilder"/> auto-height measurement.</summary>
    public readonly struct BaSettingsModalLayout
    {
        public int ColorLineCount { get; }
        public int SectionLabelCount { get; }
        public int ButtonCount { get; }
        public bool PinFooterClose { get; }
        public bool AutoHeight { get; }

        public BaSettingsModalLayout(
            int colorLineCount = 0,
            int sectionLabelCount = 0,
            int buttonCount = 0,
            bool pinFooterClose = true,
            bool autoHeight = false)
        {
            ColorLineCount = Mathf.Max(0, colorLineCount);
            SectionLabelCount = Mathf.Max(0, sectionLabelCount);
            ButtonCount = Mathf.Max(0, buttonCount);
            PinFooterClose = pinFooterClose;
            AutoHeight = autoHeight;
        }

        public static BaSettingsModalLayout ColorLines(int count, bool pinFooterClose = false, bool autoHeight = true) =>
            new BaSettingsModalLayout(count, pinFooterClose: pinFooterClose, autoHeight: autoHeight);
    }

    /// <summary>Layout constants for modal settings panels (route color picker, etc.).</summary>
    public static class BaUiSettingsMetrics
    {
        public const float DefaultPanelScale = 1.5f;
        public const float DefaultPanelHeight = 290f;
        public const float FooterBottomPad = 18f;
        public const float RowHeight = 44f;
        public const float RowGap = 8f;
        public const float SwatchSize = 40f;
        public const float SectionLabelHeight = 26f;
        public const float SwatchRowExtraHeight = 30f;
        public const float CloseButtonExtraHeight = 4f;
        public const float ScrollContentPad = 4f;
        public const float SwatchTipHeight = 18f;
        public const float SwatchTipFontSize = 10f;
    }
}
