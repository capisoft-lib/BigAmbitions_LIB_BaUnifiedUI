using System;
using Capisoft.Lib.BaUnifiedUI.Assets;
using Capisoft.Lib.BaUnifiedUI.Chrome;
using Capisoft.Lib.BaUnifiedUI.Controls;
using Capisoft.Lib.BaUnifiedUI.Layout;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Fluent
{
    public sealed class BaBannerBuilt
    {
        public GameObject Root { get; internal set; }
        public Canvas Canvas { get; internal set; }
        public TextMeshProUGUI Label { get; internal set; }
    }

    public static partial class BaUi
    {
        public static class Colors
        {
            public static Color Title => BaUiAssets.TitleColor;
            public static Color Body => BaUiAssets.BodyTextColor;
            public static Color Muted => BaUiAssets.MutedBodyTextColor;
            public static Color CarPoiBackground => BaUiAssets.CarPoiBackgroundColor;
        }

        public static class Layout
        {
            public const float PanelWidth = BaUiLayout.PanelWidth;
            public const float ContentInset = BaUiLayout.ContentInset;
            public const float HeaderHeight = BaUiLayout.HeaderHeight;
            public const float BodyTopPadding = BaUiLayout.BodyTopPadding;
            public const float BodyBottomPadding = BaUiLayout.BodyBottomPadding;
            public const float ButtonHeight = BaUiLayout.ButtonHeight;
            public const float ButtonGap = BaUiLayout.ButtonGap;
            public const float ButtonTextPaddingX = BaUiLayout.ButtonTextPaddingX;
            public const float ButtonLabelBottomInset = BaUiLayout.ButtonLabelBottomInset;
            public const float ButtonFontSize = BaUiLayout.ButtonFontSize;
            public const float TitleFontSize = BaUiLayout.TitleFontSize;
            public const float HeaderTextPaddingX = BaUiLayout.HeaderTextPaddingX;
            public const float HeaderTextPaddingY = BaUiLayout.HeaderTextPaddingY;
            public const float HeaderIconButtonSize = BaUiLayout.HeaderIconButtonSize;
            public const float SettingsIconOffsetY = BaUiLayout.SettingsIconOffsetY;
            public const float ScreenMarginMinY = BaUiLayout.ScreenMarginMinY;
            public static float SettingsPanelHeaderWidenTrim => BaUiLayout.SettingsPanelHeaderWidenTrim;
            public const float SettingsHeaderLeftFlush = BaUiLayout.SettingsHeaderLeftFlush;

            public static BaUiLayout.Metrics CreateMetrics(float scale) => BaUiLayout.CreateMetrics(scale);

            public static Vector2 GetScreenPosition(float bottomMarginY) =>
                BaUiLayout.GetScreenPosition(bottomMarginY);

            public static float ComputeWideMapPanelHeaderWidenTrim(float panelWidth) =>
                BaUiLayout.ComputeWideMapPanelHeaderWidenTrim(panelWidth);

            public static float ComputeHeaderIconsTitleReserve(int iconCount, float scale) =>
                BaUiLayout.ComputeHeaderIconsTitleReserve(iconCount, scale);

            public static float ComputeHeaderIconRightInset(int slotFromRight, float scale) =>
                BaUiLayout.ComputeHeaderIconRightInset(slotFromRight, scale);

            public static float ComputeHeaderCloseTitleReserve(float scale, float extraOffsetX = 0f) =>
                BaUiAssets.ComputeHeaderCloseTitleReserve(scale, extraOffsetX);

            public static void ApplyHeaderTitleWithRightReserve(
                RectTransform titleRect,
                float scale,
                float rightReserve) =>
                BaUiLayout.ApplyHeaderTitleWithRightReserve(titleRect, scale, rightReserve);

            public static void ApplyHeaderTitleInsets(RectTransform titleRect, BaUiLayout.Metrics metrics) =>
                BaUiLayout.ApplyHeaderTitleInsets(titleRect, metrics);
        }

        public static void EnsureReady() => BaUiAssets.EnsureInitialized();

        public static bool ShouldRebuildHud => BaUiAssets.ShouldRebuildHud;

        public static void MarkRebuildHandled() => BaUiAssets.MarkRebuildHandled();

        public static void ApplyLayer(GameObject root) => BaUiChrome.ApplyUiLayer(root);

        public static void ApplyPanelBg(Image image) => BaUiAssets.ApplyPanelBg(image);

        public static void ApplyTitleFont(TextMeshProUGUI text) => BaUiAssets.ApplyTitleFont(text);

        public static void ApplyButtonFont(TextMeshProUGUI text) => BaUiAssets.ApplyButtonFont(text);

        public static void StyleButton(Image image, BaButtonStyle style) =>
            BaUiWidgets.ApplyButtonGraphic(image, style);

        public static void BindButtonClick(Button button, UnityAction onClick) =>
            BaUiAssets.BindButtonClick(button, onClick);

        public static Image CreateButtonGraphic(
            RectTransform parent,
            float scale,
            BaButtonStyle style,
            float widthFactor = 1f,
            bool bleedBottom = true) =>
            BaUiAssets.CreateButtonGraphic(parent, scale, StyleAction(style), widthFactor, bleedBottom);

        public static Image CreateButtonGraphic(
            RectTransform parent,
            float scale,
            Action<Image> style,
            float widthFactor = 1f,
            bool bleedBottom = true) =>
            BaUiAssets.CreateButtonGraphic(parent, scale, style, widthFactor, bleedBottom);

        public static bool TryApplyOverlayIcon(Image image, Action<Image> applyIcon) =>
            BaUiAssets.TryApplyOverlayIcon(image, applyIcon);

        public static void ApplyFocusIcon(Image image) => BaUiAssets.ApplyFocusIcon(image);

        public static void ApplyAddIcon(Image image) => BaUiAssets.ApplyAddIcon(image);

        public static void ApplyCarIcon(Image image) => BaUiAssets.ApplyCarIcon(image);

        public static void ApplyHistoryIcon(Image image) => BaUiAssets.ApplyHistoryIcon(image);

        public static void ApplySettingsIcon(Image image) => BaUiAssets.ApplySettingsIcon(image);

        public static bool TryGetCarIcon(out Sprite sprite) => BaUiWidgets.TryGetCarIcon(out sprite);

        public static Button CreateRowCloseButton(Transform parent, float scale, UnityAction onClick) =>
            BaUiAssets.CreateRowCloseButton(parent, scale, onClick);

        public static Button CreateHeaderCloseButton(
            Transform header,
            float scale,
            UnityAction onClick,
            float extraOffsetX = 0f) =>
            BaUiAssets.CreateHeaderCloseButton(header, scale, onClick, extraOffsetX);

        public static BaBannerBuilt Banner(
            string rootName,
            int sortOrder,
            float width,
            float height,
            float centerYOffset)
        {
            var (root, label) = BaUiWidgets.CreateBanner(rootName, sortOrder, width, height, centerYOffset);
            return new BaBannerBuilt
            {
                Root = root,
                Canvas = root.GetComponent<Canvas>(),
                Label = label
            };
        }

        private static Action<Image> StyleAction(BaButtonStyle style) => style switch
        {
            BaButtonStyle.Grey => BaUiAssets.ApplyButtonGrey,
            BaButtonStyle.Green => BaUiAssets.ApplyButtonGreen,
            BaButtonStyle.Red => BaUiAssets.ApplyButtonRed,
            _ => BaUiAssets.ApplyButtonBlue
        };
    }
}
