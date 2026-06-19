using Capisoft.Lib.BaUnifiedUI.Assets;
using Capisoft.Lib.BaUnifiedUI.Fluent;
using Capisoft.Lib.BaUnifiedUI.Layout;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Controls
{
    /// <summary>Settings modal body — fixed scroll or auto-height vertical stack.</summary>
    public sealed class BaSettingsModalBuilder
    {
        private readonly RectTransform _panel;
        private readonly float _scale;
        private readonly float _contentInset;
        private readonly RectTransform _scrollContent;

        internal BaSettingsModalBuilder(
            RectTransform panel,
            float scale,
            float contentInset,
            BaSettingsModalLayout layout)
        {
            _panel = panel;
            _scale = scale;
            _contentInset = contentInset;

            var footerReserve = layout.PinFooterClose
                ? ComputeFooterCloseReserve(scale)
                : BaUiLayout.BodyBottomPadding * scale + BaUiSettingsMetrics.ScrollContentPad + 8f;
            var scrollTop = (BaUiLayout.HeaderHeight + BaUiLayout.BodyTopPadding) * scale;

            if (layout.AutoHeight)
            {
                _scrollContent = BaUiWidgets.CreateRect(panel, "Content");
                _scrollContent.anchorMin = Vector2.zero;
                _scrollContent.anchorMax = Vector2.one;
                _scrollContent.offsetMin = new Vector2(contentInset, footerReserve);
                _scrollContent.offsetMax = new Vector2(-contentInset, -scrollTop);
                ConfigureContentLayout(_scrollContent);
                return;
            }

            var scrollGo = BaUiWidgets.CreateRect(panel, "Scroll");
            scrollGo.anchorMin = Vector2.zero;
            scrollGo.anchorMax = Vector2.one;
            scrollGo.offsetMin = new Vector2(contentInset, footerReserve);
            scrollGo.offsetMax = new Vector2(-contentInset, -scrollTop);

            var scroll = scrollGo.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var viewport = BaUiWidgets.CreateRect(scrollGo, "Viewport");
            BaUiWidgets.StretchFull(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll.viewport = viewport;

            _scrollContent = BaUiWidgets.CreateRect(viewport, "Content");
            _scrollContent.anchorMin = new Vector2(0f, 1f);
            _scrollContent.anchorMax = new Vector2(1f, 1f);
            _scrollContent.pivot = new Vector2(0.5f, 1f);
            _scrollContent.anchoredPosition = Vector2.zero;
            _scrollContent.sizeDelta = Vector2.zero;
            scroll.content = _scrollContent;

            ConfigureContentLayout(_scrollContent);
        }

        public static float ComputePanelHeight(BaSettingsModalLayout layout, float scale)
        {
            var rowHeight = BaUiSettingsMetrics.RowHeight + BaUiSettingsMetrics.CloseButtonExtraHeight;
            var rowCount = layout.ColorLineCount + layout.SectionLabelCount + layout.ButtonCount;
            var contentHeight = 6 + 10 + BaUiSettingsMetrics.ScrollContentPad * 2;
            contentHeight += layout.SectionLabelCount * BaUiSettingsMetrics.SectionLabelHeight;
            contentHeight += (layout.ColorLineCount + layout.ButtonCount) * rowHeight;
            if (rowCount > 1)
                contentHeight += (rowCount - 1) * BaUiSettingsMetrics.RowGap;

            var top = (BaUiLayout.HeaderHeight + BaUiLayout.BodyTopPadding) * scale;
            var bottom = layout.PinFooterClose
                ? ComputeFooterCloseReserve(scale)
                : BaUiLayout.BodyBottomPadding * scale + BaUiSettingsMetrics.ScrollContentPad + 8f;
            return top + contentHeight + bottom;
        }

        private static void ConfigureContentLayout(RectTransform contentRoot)
        {
            var layout = contentRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = BaUiSettingsMetrics.RowGap;
            layout.padding = new RectOffset(
                Mathf.RoundToInt(BaUiSettingsMetrics.ScrollContentPad),
                Mathf.RoundToInt(BaUiSettingsMetrics.ScrollContentPad),
                6,
                10);

            var fitter = contentRoot.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private static float ComputeFooterCloseReserve(float scale)        {
            var closeButtonHeight = BaUiSettingsMetrics.RowHeight + BaUiSettingsMetrics.CloseButtonExtraHeight;
            return closeButtonHeight
                   + BaUiLayout.BodyBottomPadding * scale
                   + BaUiSettingsMetrics.FooterBottomPad
                   + 12f;
        }

        public BaSettingsModalBuilder SectionLabel(string text, out TextMeshProUGUI label)
        {
            var row = CreateRow(BaUiSettingsMetrics.SectionLabelHeight);
            label = CreateRowLabel(row, 14f, FontStyles.Bold);
            label.color = new Color(0.85f, 0.9f, 1f, 1f);
            label.text = text;
            return this;
        }

        public BaSettingsModalBuilder ColorPresets(
            Color[] colors,
            string[] tipLabels,
            UnityAction<Color> onSelected,
            out BaUiColorSwatchRow row)
        {
            var rowHeight = BaUiSettingsMetrics.SwatchSize + BaUiSettingsMetrics.SwatchRowExtraHeight;
            var layoutRow = CreateRow(rowHeight);
            row = BaUiColorSwatchRow.Create(layoutRow, rowHeight, colors, tipLabels, onSelected);
            return this;
        }

        /// <summary>Label + current-color swatch + picker button on one row.</summary>
        public BaSettingsModalBuilder ColorLine(
            string labelText,
            Color initialColor,
            string buttonText,
            BaButtonStyle buttonStyle,
            UnityAction onPickColor,
            out TextMeshProUGUI label,
            out BaUiColorSwatchDisplay swatch,
            out TextMeshProUGUI buttonLabel)
        {
            var rowHeight = BaUiSettingsMetrics.RowHeight + BaUiSettingsMetrics.CloseButtonExtraHeight;
            var row = CreateRow(rowHeight);

            var hLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 10f;
            hLayout.childAlignment = TextAnchor.MiddleLeft;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = false;
            hLayout.padding = new RectOffset(2, 2, 0, 0);

            var labelRt = BaUiWidgets.CreateRect(row, "Label");
            var labelLe = labelRt.gameObject.AddComponent<LayoutElement>();
            labelLe.flexibleWidth = 1f;
            labelLe.minWidth = 72f;
            labelLe.preferredHeight = rowHeight;
            label = CreateRowLabel(labelRt, 14f, FontStyles.Bold);
            label.color = new Color(0.85f, 0.9f, 1f, 1f);
            label.text = labelText;

            var swatchSlot = BaUiWidgets.CreateRect(row, "SwatchSlot");
            swatch = BaUiColorSwatchDisplay.Create(swatchSlot, initialColor);

            var buttonRt = BaUiWidgets.CreateRect(row, "PickButton");
            var buttonLe = buttonRt.gameObject.AddComponent<LayoutElement>();
            buttonLe.flexibleWidth = 1.2f;
            buttonLe.minWidth = 130f;
            buttonLe.preferredHeight = rowHeight;

            var img = BaUiWidgets.CreateButtonGraphic(buttonRt, _scale, buttonStyle);
            var btn = buttonRt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            BaUiAssets.BindButtonClick(btn, onPickColor);

            buttonLabel = CreateRowLabel(buttonRt, 14f, FontStyles.UpperCase);
            buttonLabel.text = buttonText;
            buttonLabel.alignment = TextAlignmentOptions.Center;
            return this;
        }

        public BaSettingsModalBuilder Button(
            string text,
            BaButtonStyle style,
            UnityAction onClick,
            out TextMeshProUGUI label)
        {
            var row = CreateRow(BaUiSettingsMetrics.RowHeight + BaUiSettingsMetrics.CloseButtonExtraHeight);
            var img = BaUiWidgets.CreateButtonGraphic(row, _scale, style);
            var btn = row.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            BaUiAssets.BindButtonClick(btn, onClick);

            label = CreateRowLabel(row, 14f, FontStyles.UpperCase);
            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
            return this;
        }

        public BaSettingsModalBuilder FooterClose(string text, UnityAction onClick, out TextMeshProUGUI label)
        {
            var closeButtonHeight = BaUiSettingsMetrics.RowHeight + BaUiSettingsMetrics.CloseButtonExtraHeight;
            var closeRow = BaUiWidgets.CreateRect(_panel, "CloseRow");
            closeRow.anchorMin = new Vector2(0f, 0f);
            closeRow.anchorMax = new Vector2(1f, 0f);
            closeRow.pivot = new Vector2(0.5f, 0f);
            closeRow.anchoredPosition = new Vector2(0f, BaUiSettingsMetrics.FooterBottomPad);
            closeRow.sizeDelta = new Vector2(-_contentInset * 2f, closeButtonHeight);

            var closeImg = BaUiWidgets.CreateButtonGraphic(closeRow, _scale, BaButtonStyle.Blue);
            var closeBtn = closeRow.gameObject.AddComponent<Button>();
            closeBtn.targetGraphic = closeImg;
            BaUiAssets.BindButtonClick(closeBtn, onClick);

            var closeLabelGo = BaUiWidgets.CreateRect(closeRow, "Label");
            BaUiWidgets.Stretch(closeLabelGo);
            closeLabelGo.offsetMin = new Vector2(BaUiLayout.ButtonTextPaddingX * _scale, 0f);
            closeLabelGo.offsetMax = new Vector2(-BaUiLayout.ButtonTextPaddingX * _scale,
                -BaUiLayout.ButtonLabelBottomInset * _scale);
            label = closeLabelGo.gameObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = BaUiLayout.ButtonFontSize * _scale;
            label.fontStyle = FontStyles.UpperCase;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
            BaUiAssets.ApplyButtonFont(label);
            label.text = text;
            return this;
        }

        private RectTransform CreateRow(float height)
        {
            var row = BaUiWidgets.CreateRect(_scrollContent, "Row");
            var le = row.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
            return row;
        }

        private static TextMeshProUGUI CreateRowLabel(RectTransform parent, float fontSize, FontStyles style)
        {
            var rt = BaUiWidgets.CreateRect(parent, "Label");
            BaUiWidgets.Stretch(rt, 2f, 2f);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;
            tmp.margin = Vector4.zero;
            tmp.enableWordWrapping = true;
            BaUiAssets.ApplyButtonFont(tmp);
            return tmp;
        }
    }
}
