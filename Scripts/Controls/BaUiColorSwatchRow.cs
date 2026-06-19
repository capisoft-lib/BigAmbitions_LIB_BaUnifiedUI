using System.Collections.Generic;
using Capisoft.Lib.BaUnifiedUI.Assets;
using Capisoft.Lib.BaUnifiedUI.Layout;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Controls
{
    /// <summary>Horizontal color preset strip with per-swatch labels.</summary>
    public sealed class BaUiColorSwatchRow
    {
        private readonly List<Image> _swatches = new List<Image>();
        private readonly List<TextMeshProUGUI> _tips = new List<TextMeshProUGUI>();

        public IReadOnlyList<Image> Swatches => _swatches;
        public IReadOnlyList<TextMeshProUGUI> Tips => _tips;

        internal static BaUiColorSwatchRow Create(
            RectTransform parent,
            float rowHeight,
            Color[] colors,
            string[] tipTexts,
            UnityEngine.Events.UnityAction<Color> onSelected)
        {
            var row = new BaUiColorSwatchRow();
            var stripHeight = rowHeight - 4f;
            var strip = BaUiWidgets.CreateRect(parent, "SwatchStrip");
            strip.anchorMin = new Vector2(0f, 0.5f);
            strip.anchorMax = new Vector2(0f, 0.5f);
            strip.pivot = new Vector2(0f, 0.5f);
            strip.anchoredPosition = new Vector2(2f, 0f);
            strip.sizeDelta = new Vector2(
                colors.Length * BaUiSettingsMetrics.SwatchSize,
                rowHeight);

            var hLayout = strip.gameObject.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 0f;
            hLayout.childAlignment = TextAnchor.UpperLeft;
            hLayout.childControlWidth = false;
            hLayout.childControlHeight = false;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = false;

            for (var i = 0; i < colors.Length; i++)
                row.AddSwatch(strip, stripHeight, colors[i], tipTexts != null && i < tipTexts.Length ? tipTexts[i] : null, onSelected);

            return row;
        }

        public void SetTipText(int index, string text)
        {
            if (index < 0 || index >= _tips.Count || _tips[index] == null)
                return;

            _tips[index].text = text;
        }

        public void Highlight(Color current, float tolerance = 0.06f)
        {
            for (var i = 0; i < _swatches.Count; i++)
            {
                var swatch = _swatches[i];
                if (swatch == null)
                    continue;

                var selected = ColorsMatch(swatch.color, current, tolerance);
                swatch.transform.localScale = selected ? Vector3.one * 1.1f : Vector3.one;

                var outline = swatch.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = selected
                        ? new Color(1f, 1f, 1f, 0.95f)
                        : new Color(1f, 1f, 1f, 0.25f);
                }
            }
        }

        private void AddSwatch(
            RectTransform strip,
            float stripHeight,
            Color color,
            string tipText,
            UnityEngine.Events.UnityAction<Color> onSelected)
        {
            var cell = BaUiWidgets.CreateRect(strip, "SwatchCell");
            var cellLe = cell.gameObject.AddComponent<LayoutElement>();
            cellLe.preferredWidth = BaUiSettingsMetrics.SwatchSize;
            cellLe.preferredHeight = stripHeight;

            var column = BaUiWidgets.CreateRect(cell, "Column");
            BaUiWidgets.StretchFull(column);

            var swatchRt = BaUiWidgets.CreateRect(column, "Swatch");
            swatchRt.anchorMin = new Vector2(0.5f, 1f);
            swatchRt.anchorMax = new Vector2(0.5f, 1f);
            swatchRt.pivot = new Vector2(0.5f, 1f);
            swatchRt.sizeDelta = new Vector2(BaUiSettingsMetrics.SwatchSize, BaUiSettingsMetrics.SwatchSize);
            swatchRt.anchoredPosition = Vector2.zero;

            var img = swatchRt.gameObject.AddComponent<Image>();
            img.color = color;
            _swatches.Add(img);

            var outline = swatchRt.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.35f);
            outline.effectDistance = new Vector2(1f, -1f);

            var btn = swatchRt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var captured = color;
            btn.onClick.AddListener(() => onSelected?.Invoke(captured));

            var tipRt = BaUiWidgets.CreateRect(column, "Tip");
            tipRt.anchorMin = new Vector2(0f, 0f);
            tipRt.anchorMax = new Vector2(1f, 0f);
            tipRt.pivot = new Vector2(0.5f, 0f);
            tipRt.sizeDelta = new Vector2(0f, BaUiSettingsMetrics.SwatchTipHeight);
            tipRt.anchoredPosition = Vector2.zero;

            var tip = tipRt.gameObject.AddComponent<TextMeshProUGUI>();
            tip.text = tipText ?? string.Empty;
            tip.fontSize = BaUiSettingsMetrics.SwatchTipFontSize;
            tip.alignment = TextAlignmentOptions.Center;
            tip.color = new Color(0.8f, 0.85f, 0.95f, 1f);
            tip.raycastTarget = false;
            tip.enableWordWrapping = false;
            tip.overflowMode = TextOverflowModes.Overflow;
            BaUiAssets.ApplyButtonFont(tip);
            _tips.Add(tip);
        }

        private static bool ColorsMatch(Color a, Color b, float tolerance) =>
            Mathf.Abs(a.r - b.r) < tolerance &&
            Mathf.Abs(a.g - b.g) < tolerance &&
            Mathf.Abs(a.b - b.b) < tolerance;
    }
}
