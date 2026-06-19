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
    /// <summary>Left-to-right placement inside a horizontal band (footer, toolbar, etc.).</summary>
    public sealed class BaHorizontalStackBuilder
    {
        private readonly RectTransform _row;
        private readonly float _scale;
        private readonly float _textScale;
        private readonly float _bandHeight;
        private readonly float _bandWidth;
        private float _cursorX;
        private int _childIndex;

        internal BaHorizontalStackBuilder(
            RectTransform row,
            float scale,
            float textScale,
            float bandHeight,
            float bandWidth)
        {
            _row = row;
            _scale = scale;
            _textScale = textScale;
            _bandHeight = bandHeight;
            _bandWidth = bandWidth;
        }

        public float CursorX => _cursorX;
        public float RemainingWidth => Mathf.Max(0f, _bandWidth - _cursorX);

        public BaHorizontalStackBuilder Gap(float pixels)
        {
            _cursorX += pixels;
            return this;
        }

        public BaHorizontalStackBuilder Button(
            string text,
            BaButtonStyle style,
            UnityAction onClick,
            float width,
            string name = null)
        {
            var rect = CreateSlot(name ?? $"Button{_childIndex++}", width);
            var img = BaUiWidgets.CreateButtonGraphic(rect, _scale, style);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            BaUiAssets.BindButtonClick(button, onClick);

            var labelGo = BaUiWidgets.CreateRect(rect, "Label");
            BaUiWidgets.Stretch(labelGo);
            var label = labelGo.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 14f * _textScale;
            label.fontStyle = FontStyles.UpperCase;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
            BaUiAssets.ApplyButtonFont(label);
            return this;
        }

        /// <summary>Action panel primary button (bold uppercase, vanilla button graphic).</summary>
        public BaHorizontalStackBuilder ActionButton(
            string name,
            BaButtonStyle style,
            UnityAction onClick,
            float width,
            out Image graphic,
            out TextMeshProUGUI label)
        {
            var rect = CreateSlot(name, width);
            graphic = BaUiWidgets.CreateButtonGraphic(rect, _scale, style);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = graphic;
            BaUiAssets.BindButtonClick(button, onClick);

            var labelGo = BaUiWidgets.CreateRect(rect, "Label");
            BaUiWidgets.Stretch(labelGo);
            labelGo.offsetMin = new Vector2(BaUiLayout.ButtonTextPaddingX * _scale, 0f);
            labelGo.offsetMax = new Vector2(-BaUiLayout.ButtonTextPaddingX * _scale,
                -BaUiLayout.ButtonLabelBottomInset * _scale);
            label = labelGo.gameObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = BaUiLayout.ButtonFontSize * _scale;
            label.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
            BaUiAssets.ApplyButtonFont(label);
            return this;
        }

        [System.Obsolete("Use ActionButton.")]
        public BaHorizontalStackBuilder HudButton(
            string name,
            BaButtonStyle style,
            UnityAction onClick,
            float width,
            out Image graphic,
            out TextMeshProUGUI label) =>
            ActionButton(name, style, onClick, width, out graphic, out label);

        public BaHorizontalStackBuilder Label(
            string text,
            float width,
            BaTextStyle style = BaTextStyle.Body,
            TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft)
        {
            var rect = CreateSlot($"Label{_childIndex++}", width);
            var label = BaUiControls.CreateBodyLabel(rect, text, _textScale, style);
            label.alignment = alignment;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            return this;
        }

        /// <summary>Custom child view (icon, input, nested widget, etc.).</summary>
        public BaHorizontalStackBuilder View(
            float width,
            Action<RectTransform, float> build,
            string name = null)
        {
            var rect = CreateSlot(name ?? $"View{_childIndex++}", width);
            build?.Invoke(rect, _textScale);
            return this;
        }

        /// <summary>Child using all remaining row width.</summary>
        public BaHorizontalStackBuilder Fill(Action<RectTransform, float> build, string name = null)
        {
            var width = RemainingWidth;
            if (width <= 0f)
                return this;

            var rect = CreateSlot(name ?? $"Fill{_childIndex++}", width);
            build?.Invoke(rect, _textScale);
            return this;
        }

        /// <summary>Equal-width buttons with gaps — common footer shortcut.</summary>
        public BaHorizontalStackBuilder ButtonsEqual(
            float gap,
            params BaHorizontalButtonSpec[] buttons)
        {
            if (buttons == null || buttons.Length == 0)
                return this;

            var totalGap = gap * (buttons.Length - 1);
            var width = (RemainingWidth - totalGap) / buttons.Length;
            for (var i = 0; i < buttons.Length; i++)
            {
                if (i > 0)
                    Gap(gap);
                var spec = buttons[i];
                Button(spec.Text, spec.Style, spec.OnClick, width, spec.Name);
            }

            return this;
        }

        private RectTransform CreateSlot(string name, float width)
        {
            var rect = BaUiWidgets.CreateRect(_row, name);
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(_cursorX, 0f);
            rect.sizeDelta = new Vector2(width, _bandHeight);
            _cursorX += width;
            return rect;
        }
    }

    public readonly struct BaHorizontalButtonSpec
    {
        public string Text { get; }
        public BaButtonStyle Style { get; }
        public UnityAction OnClick { get; }
        public string Name { get; }

        public BaHorizontalButtonSpec(string text, BaButtonStyle style, UnityAction onClick, string name = null)
        {
            Text = text;
            Style = style;
            OnClick = onClick;
            Name = name;
        }
    }
}
