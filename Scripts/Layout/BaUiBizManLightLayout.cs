using UnityEngine;

namespace Capisoft.Lib.BaUnifiedUI.Layout
{
    /// <summary>
    /// BizMan in-game light modals (employee assign, factory dialogs).
    /// Reference: ~62% screen width, ~72% height, white body, grey header strip.
    /// </summary>
    public static class BaUiBizManLightLayout
    {
        public const float ReferencePanelWidth = 1180f;
        public const float ReferencePanelHeight = 780f;
        public const float ScreenWidthRatio = 0.62f;
        public const float ScreenHeightRatio = 0.72f;
        public const float MinPanelWidth = 960f;
        public const float MaxPanelWidth = 1320f;
        public const float MinPanelHeight = 640f;
        public const float MaxPanelHeight = 860f;

        public const float ContentInset = 28f;
        public const float HeaderHeight = 52f;
        public const float BodyTopPadding = 16f;
        public const float BodyBottomPadding = 20f;
        public const float TitleFontSize = 20f;
        public const float BodyFontSize = 16f;
        public const float LabelFontSize = 15f;
        public const float ControlFontSize = 14f;
        public const float FooterButtonHeight = 44f;
        public const float FooterBottomInset = 20f;
        public const float FooterButtonGap = 16f;
        public const float RowHeight = 48f;
        public const float RowGap = 10f;
        public const float LabelColumnWidth = 260f;
        public const float HeaderTextPaddingX = 24f;
        public const float HeaderTextPaddingY = 8f;
        public const float DimmerAlpha = 0.35f;

        public static float ResolvePanelWidth()
        {
            var w = Screen.width * ScreenWidthRatio;
            return Mathf.Clamp(w, MinPanelWidth, MaxPanelWidth);
        }

        public static float ResolvePanelHeight()
        {
            var h = Screen.height * ScreenHeightRatio;
            return Mathf.Clamp(h, MinPanelHeight, MaxPanelHeight);
        }

        public static float ComputeScale(float panelWidth) =>
            panelWidth / ReferencePanelWidth;
    }
}
