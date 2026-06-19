using System;
using Capisoft.Lib.BaUnifiedUI.Assets;
using Capisoft.Lib.BaUnifiedUI.Fluent;
using Capisoft.Lib.BaUnifiedUI.Layout;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Controls
{
    /// <summary>
    /// Fluent body sections for panels built with <see cref="BaOverlayBuilder.Content"/>.
    /// Pass only fixed counts (visible rows, quick slots); total panel height is derived from the stack.
    /// </summary>
    public sealed class BaPanelContentBuilder
    {
        private readonly RectTransform _panel;
        private readonly float _scale;
        private readonly float _textScale;
        private readonly float _horizontalInset;
        private readonly float _contentWidth;
        private readonly bool _measureOnly;
        private BaUiVerticalStack _stack;

        public RectTransform Panel => _panel;
        public float Scale => _scale;
        public float TextScale => _textScale;
        /// <summary>Per-side horizontal inset passed to <see cref="BaUiVerticalStack"/>.</summary>
        public float HorizontalInset => _horizontalInset;
        public float StackY => _stack.CursorY;

        /// <summary>Total panel height (header + composed sections + body bottom padding).</summary>
        public float PanelHeight => _stack.CursorY + BaUiLayout.BodyBottomPadding;

        internal BaPanelContentBuilder(RectTransform panel, float scale, float contentInset, bool measureOnly = false)
        {
            _panel = panel;
            _scale = scale;
            _textScale = Mathf.Clamp(scale, 0.85f, 1.15f);
            _horizontalInset = contentInset;
            _contentWidth = BaUiLayout.PanelWidth * scale - contentInset * 2f;
            _measureOnly = measureOnly;
            _stack = new BaUiVerticalStack(BaUiLayout.HeaderHeight + BaUiLayout.BodyTopPadding);
        }

        /// <summary>Measure height for a composition without building UI.</summary>
        public static float ComputePanelHeight(Action<BaPanelContentBuilder> compose)
        {
            var planner = new BaPanelContentBuilder(null, 1f, BaUiLayout.ContentInset, measureOnly: true);
            compose(planner);
            return planner.PanelHeight;
        }

        public BaPanelContentBuilder Gap(float pixels)
        {
            _stack.Advance(pixels);
            return this;
        }

        /// <summary>Reserve space for fixed quick rows above search (rows are still created by the consumer).</summary>
        public BaPanelContentBuilder QuickRowStrip(int slotCount, float gapAfter = 6f)
        {
            _stack.Advance(BaUiListMetrics.ContentRowsBlockHeight(slotCount), gapAfter);
            return this;
        }

        /// <summary>Quick row strip on the panel — uses a list row template.</summary>
        public BaPanelContentBuilder QuickRows(
            int slotCount,
            BaUiListRowTemplate template,
            Action<int, BaUiListRow> onCreate,
            out BaUiListRowPool pool,
            float gapAfter = 6f)
        {
            if (_measureOnly)
            {
                pool = null;
                QuickRowStrip(slotCount, gapAfter);
                return this;
            }

            pool = new BaUiListRowPool(_panel, _textScale, template, "QuickRow");
            pool.Sync(slotCount, onCreate);
            pool.PlaceOnPanelStrip(-_stack.CursorY, slotCount);
            _stack.Advance(BaUiListMetrics.ContentRowsBlockHeight(slotCount), gapAfter);
            return this;
        }

        public BaPanelContentBuilder Search(
            string placeholder,
            UnityAction<string> onChanged,
            out BaUiSearchField field,
            UnityAction onSelected = null)
        {
            _stack.Advance(BaUiListMetrics.SearchBarTopMargin);
            if (_measureOnly)
            {
                field = null;
                _stack.Advance(BaUiListMetrics.SearchBarHeight);
                return this;
            }

            field = BaUiSearchField.Create(_panel, _textScale);
            _stack.PlaceTopBand(field.Rect, BaUiListMetrics.SearchBarHeight, _horizontalInset);
            field.SetPlaceholder(placeholder);
            field.Wire(onChanged, onSelected);
            return this;
        }

        /// <summary>Pick-mode hint band — reserves <see cref="BaUiListMetrics.PickHintGap"/> below search.</summary>
        public BaPanelContentBuilder PickHint(out TextMeshProUGUI label, bool hiddenByDefault = true)
        {
            if (_measureOnly)
            {
                label = null;
                _stack.Advance(BaUiListMetrics.PickHintGap);
                return this;
            }

            var hintGo = BaUiWidgets.CreateRect(_panel, "PickHint");
            label = hintGo.gameObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = 13f * _textScale;
            label.color = new Color(0.9f, 0.75f, 0.35f, 1f);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.fontStyle = FontStyles.Italic;
            BaUiAssets.ApplyButtonFont(label);
            _stack.PlaceTopBand(hintGo, BaUiListMetrics.HintHeight, _horizontalInset, BaUiListMetrics.PickHintGap - BaUiListMetrics.HintHeight);
            if (hiddenByDefault)
                label.gameObject.SetActive(false);
            return this;
        }

        public BaPanelContentBuilder Hint(out TextMeshProUGUI label, bool hiddenByDefault = true, float gapAfter = 4f)
        {
            if (_measureOnly)
            {
                label = null;
                _stack.Advance(BaUiListMetrics.HintHeight, gapAfter);
                return this;
            }

            var hintGo = BaUiWidgets.CreateRect(_panel, "Hint");
            label = hintGo.gameObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = 13f * _textScale;
            label.color = new Color(0.9f, 0.75f, 0.35f, 1f);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.fontStyle = FontStyles.Italic;
            BaUiAssets.ApplyButtonFont(label);
            _stack.PlaceTopBand(hintGo, BaUiListMetrics.HintHeight, _horizontalInset, gapAfter);
            if (hiddenByDefault)
                label.gameObject.SetActive(false);
            return this;
        }

        public BaPanelContentBuilder ScrollList(int visibleRowCount, out BaUiScrollList list, float gapAfter = 0f) =>
            AddScrollList(visibleRowCount, gapAfter, out list);

        public BaPanelContentBuilder AddScrollList(int visibleRowCount, float gapAfter, out BaUiScrollList list)
        {
            list = ScrollList(visibleRowCount, gapAfter);
            return this;
        }

        public BaUiScrollList ScrollList(int visibleRowCount, float gapAfter = 0f)
        {
            var height = BaUiListMetrics.ScrollViewportHeight(visibleRowCount);
            if (_measureOnly)
            {
                _stack.Advance(height, gapAfter);
                return null;
            }

            var list = BaUiScrollList.Create(_panel);
            _stack.PlaceTopBand(list.Rect, height, _horizontalInset, gapAfter);
            return list;
        }

        /// <summary>Horizontal band in the vertical stack — add buttons, labels, or custom views left-to-right.</summary>
        public BaPanelContentBuilder HorizontalStack(
            float height,
            Action<BaHorizontalStackBuilder> configure,
            float topMargin = 0f,
            float gapAfter = 0f)
        {
            if (topMargin > 0f)
                _stack.Advance(topMargin);

            if (_measureOnly)
            {
                _stack.Advance(height, gapAfter);
                return this;
            }

            var rowGo = BaUiWidgets.CreateRect(_panel, "HorizontalStack");
            _stack.PlaceTopBand(rowGo, height, _horizontalInset, gapAfter);
            var bandWidth = Mathf.Max(0f, _contentWidth);
            configure(new BaHorizontalStackBuilder(rowGo, _scale, _textScale, height, bandWidth));
            return this;
        }

        /// <summary>Action panel button row — two primary actions below the header (e.g. WAY OUT / WALK).</summary>
        public BaPanelContentBuilder ActionButtonRow(Action<BaHorizontalStackBuilder> configure) =>
            HorizontalStack(
                BaUiLayout.ButtonHeight * _scale,
                configure,
                gapAfter: BaUiLayout.BodyBottomPadding * _scale);

        [System.Obsolete("Use ActionButtonRow.")]
        public BaPanelContentBuilder HudButtonRow(Action<BaHorizontalStackBuilder> configure) =>
            ActionButtonRow(configure);

        /// <summary>Footer shortcut — horizontal stack with default top margin.</summary>
        public BaPanelContentBuilder Footer(
            float buttonHeight,
            Action<BaHorizontalStackBuilder> configure,
            float gapAfter = 0f) =>
            HorizontalStack(buttonHeight, configure, BaUiListMetrics.FooterTopMargin, gapAfter);

        /// <summary>
        /// Settings modal body. Pass <see cref="BaSettingsModalLayout.AutoHeight"/> to size the panel from row counts.
        /// </summary>
        public BaPanelContentBuilder SettingsModal(
            BaSettingsModalLayout layout,
            Action<BaSettingsModalBuilder> configure)
        {
            if (_measureOnly)
            {
                var total = layout.AutoHeight
                    ? BaSettingsModalBuilder.ComputePanelHeight(layout, _scale)
                    : BaUiSettingsMetrics.DefaultPanelHeight;
                var stackStart = BaUiLayout.HeaderHeight + BaUiLayout.BodyTopPadding;
                _stack.Advance(Mathf.Max(0f, total - stackStart - BaUiLayout.BodyBottomPadding));
                return this;
            }

            configure(new BaSettingsModalBuilder(_panel, _scale, _horizontalInset, layout));
            return this;
        }

        [Obsolete("Use Footer or HorizontalStack — add any number of inner views via BaHorizontalStackBuilder.")]
        public BaPanelContentBuilder FooterPair(
            float buttonWidth,
            float buttonHeight,
            float leftX,
            float rightX,
            string leftText,
            BaButtonStyle leftStyle,
            UnityAction leftClick,
            out Button leftButton,
            out TextMeshProUGUI leftLabel,
            string rightText,
            BaButtonStyle rightStyle,
            UnityAction rightClick,
            out Button rightButton,
            out TextMeshProUGUI rightLabel,
            float topMargin = BaUiListMetrics.FooterTopMargin)
        {
            if (_measureOnly)
            {
                leftButton = null;
                rightButton = null;
                leftLabel = null;
                rightLabel = null;
                _stack.Advance(topMargin + buttonHeight);
                return this;
            }

            var footerY = -(_stack.CursorY + topMargin);
            leftButton = CreateFooterButton("FooterLeft", leftX, footerY, buttonWidth, buttonHeight, leftStyle, leftClick, out leftLabel);
            leftLabel.text = leftText;
            rightButton = CreateFooterButton("FooterRight", rightX, footerY, buttonWidth, buttonHeight, rightStyle, rightClick, out rightLabel);
            rightLabel.text = rightText;
            _stack.Advance(topMargin + buttonHeight);
            return this;
        }

        public BaUiListRow ListRow(
            BaUiListRowTemplate template,
            string name,
            float rowTop = 0f,
            Action<BaUiListRowBinder> bind = null)
        {
            var builder = BaUiListRows.Template(template.Recipe);
            if (template.HorizontalInset > 0f)
                builder.OnPanel(template.HorizontalInset);
            else
                builder.InScroll();

            if (bind != null)
                return builder.CreateAndBind(_panel, _textScale, name, bind, rowTop);

            return builder.Create(_panel, _textScale, name, rowTop);
        }

        [Obsolete("Use ListRow(BaUiListRowTemplate, ...) or BaUiListRows.Template.")]
        public BaUiListRow ListRow(
            Transform parent,
            string name,
            BaUiListRowRecipe recipe,
            float rowHorizontalInset = 0f,
            float rowTop = 0f) =>
            BaUiListRows.Create(parent as RectTransform ?? parent.GetComponent<RectTransform>(), name, _textScale, recipe, rowHorizontalInset, rowTop);

        [Obsolete("Use ComputePanelHeight(Action<BaPanelContentBuilder>) with the same Content lambda.")]
        public static float ComputeMapPanelHeight(
            bool hasSearch,
            bool hasHint,
            bool hintVisible,
            int visibleScrollRows,
            bool hasFooter) =>
            ComputePanelHeight(c =>
            {
                if (hasSearch)
                    c.Search("", _ => { }, out _);
                if (hasHint)
                {
                    if (hintVisible)
                        c.Hint(out _);
                    else
                        c.PickHint(out _);
                }
                c.ScrollList(visibleScrollRows);
                if (hasFooter)
                    c.Footer(BaUiLayout.ButtonHeight, h => h.Button("", BaButtonStyle.Blue, null, 1f));
            });

        [Obsolete("Use ComputePanelHeight(c => c.ScrollList(rows))")]
        public static float ComputeHistoryPanelHeight(int visibleScrollRows) =>
            ComputePanelHeight(c => c.ScrollList(visibleScrollRows));

        private Button CreateFooterButton(
            string name,
            float x,
            float y,
            float width,
            float height,
            BaButtonStyle style,
            UnityAction onClick,
            out TextMeshProUGUI label)
        {
            var rect = BaUiWidgets.CreateRect(_panel, name);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);

            var img = BaUiWidgets.CreateButtonGraphic(rect, _scale, style);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            BaUiAssets.BindButtonClick(button, onClick);

            var labelGo = BaUiWidgets.CreateRect(rect, "Label");
            BaUiWidgets.Stretch(labelGo);
            label = labelGo.gameObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = 14f * _textScale;
            label.fontStyle = FontStyles.UpperCase;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
            BaUiAssets.ApplyButtonFont(label);
            return button;
        }
    }
}
