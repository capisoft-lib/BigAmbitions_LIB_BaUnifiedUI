using Capisoft.Lib.BaUnifiedUI.Assets;
using Capisoft.Lib.BaUnifiedUI.Layout;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Controls
{
    /// <summary>Itemized bill block: label left, amount right — for modals and receipts.</summary>
    public sealed class BaUiBillPanel
    {
        private const float LineHeight = 28f;
        private const float HostPadX = 12f;
        private const float HostPadTop = 10f;

        private readonly RectTransform _host;
        private readonly float _textScale;
        private float _lineY;

        public RectTransform Host => _host;

        private BaUiBillPanel(RectTransform host, float textScale)
        {
            _host = host;
            _textScale = textScale;
            _lineY = HostPadTop * textScale;
        }

        public static BaUiBillPanel Place(
            Transform parent,
            BaUiVerticalStack stack,
            float height,
            float textScale,
            float horizontalInset = 0f)
        {
            var host = BaUiWidgets.CreateRect(parent, "BillPanel");
            stack.PlaceTopBand(host, height, horizontalInset, gapAfter: BaUiBizManLightLayout.RowGap);

            var bg = host.gameObject.AddComponent<Image>();
            bg.color = new Color(0.945f, 0.95f, 0.965f, 1f);
            bg.raycastTarget = false;

            return new BaUiBillPanel(host, textScale);
        }

        public void Clear()
        {
            for (var i = _host.childCount - 1; i >= 0; i--)
                Object.Destroy(_host.GetChild(i).gameObject);

            _lineY = HostPadTop * _textScale;
        }

        public void AddLine(string label, string amount, bool emphasize = false)
        {
            var rowH = LineHeight * _textScale;
            var row = BaUiWidgets.CreateRect(_host, "Line");
            row.anchorMin = new Vector2(0f, 1f);
            row.anchorMax = new Vector2(1f, 1f);
            row.pivot = new Vector2(0.5f, 1f);
            row.anchoredPosition = new Vector2(0f, -_lineY);
            row.sizeDelta = new Vector2(-HostPadX * 2f, rowH);
            _lineY += rowH;

            var labelRect = BaUiWidgets.CreateRect(row, "Label");
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = new Vector2(0.68f, 1f);
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;

            var labelTmp = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
            labelTmp.text = label ?? string.Empty;
            labelTmp.fontSize = BaUiBizManLightLayout.BodyFontSize * _textScale;
            labelTmp.fontStyle = emphasize ? FontStyles.Bold : FontStyles.Normal;
            labelTmp.color = emphasize
                ? BaUiAssets.BizManLightBodyTextColor
                : BaUiAssets.BizManLightMutedTextColor;
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
            labelTmp.overflowMode = TextOverflowModes.Ellipsis;
            labelTmp.enableWordWrapping = false;
            labelTmp.raycastTarget = false;
            BaUiAssets.ApplyButtonFont(labelTmp);

            var amountRect = BaUiWidgets.CreateRect(row, "Amount");
            amountRect.anchorMin = new Vector2(0.68f, 0f);
            amountRect.anchorMax = Vector2.one;
            amountRect.offsetMin = amountRect.offsetMax = Vector2.zero;

            var amountTmp = amountRect.gameObject.AddComponent<TextMeshProUGUI>();
            amountTmp.text = amount ?? string.Empty;
            amountTmp.fontSize = BaUiBizManLightLayout.BodyFontSize * _textScale;
            amountTmp.fontStyle = emphasize ? FontStyles.Bold : FontStyles.Normal;
            amountTmp.color = BaUiAssets.BizManLightBodyTextColor;
            amountTmp.alignment = TextAlignmentOptions.MidlineRight;
            amountTmp.overflowMode = TextOverflowModes.Overflow;
            amountTmp.raycastTarget = false;
            BaUiAssets.ApplyButtonFont(amountTmp);
        }

        public void AddSpacer(float height = 8f) => _lineY += height * _textScale;
    }
}
