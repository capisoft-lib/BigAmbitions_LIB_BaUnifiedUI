using System;
using Capisoft.Lib.BaUnifiedUI.Assets;
using Capisoft.Lib.BaUnifiedUI.Chrome;
using Capisoft.Lib.BaUnifiedUI.Fluent;
using Capisoft.Lib.BaUnifiedUI.Layout;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Controls
{
    public static class BaUiControls
    {
        public static TextMeshProUGUI CreateTitleLabel(
            Transform parent,
            string text,
            float scale,
            float rightReserve = 0f)
        {
            var go = new GameObject("Title", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            if (rightReserve > 0f)
                BaUiLayout.ApplyHeaderTitleWithRightReserve(rect, scale, rightReserve);

            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = BaUiLayout.TitleFontSize * scale;
            label.fontStyle = FontStyles.Bold;
            label.color = BaUiAssets.TitleColor;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.raycastTarget = false;
            BaUiAssets.ApplyTitleFont(label);
            return label;
        }

        public static TextMeshProUGUI CreateBodyLabel(Transform parent, string text, float scale, BaTextStyle style = BaTextStyle.Body)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 24f * scale);

            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = BaUiLayout.ButtonFontSize * scale;
            label.color = style switch
            {
                BaTextStyle.Muted => BaUiAssets.MutedBodyTextColor,
                BaTextStyle.Warning => new Color(1f, 0.55f, 0.45f, 1f),
                _ => BaUiAssets.BodyTextColor
            };
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.raycastTarget = false;
            BaUiAssets.ApplyButtonFont(label);
            return label;
        }

        public static (Button Button, Image Graphic, TextMeshProUGUI Label) CreateVanillaButton(
            Transform parent,
            string text,
            BaButtonStyle style,
            float scale,
            float width,
            float height,
            UnityAction onClick)
        {
            BaUiAssets.EnsureInitialized();
            var go = new GameObject("Button", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);

            Action<Image> apply = style switch
            {
                BaButtonStyle.Grey => BaUiAssets.ApplyButtonGrey,
                BaButtonStyle.Green => BaUiAssets.ApplyButtonGreen,
                BaButtonStyle.Red => BaUiAssets.ApplyButtonRed,
                _ => BaUiAssets.ApplyButtonBlue
            };

            var graphic = BaUiAssets.CreateButtonGraphic(rect, scale, apply);
            var button = go.AddComponent<Button>();
            button.targetGraphic = graphic;
            BaUiAssets.BindButtonClick(button, onClick);

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(rect, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(BaUiLayout.ButtonTextPaddingX * scale, 0f);
            labelRect.offsetMax = new Vector2(-BaUiLayout.ButtonTextPaddingX * scale,
                -BaUiLayout.ButtonLabelBottomInset * scale);

            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = BaUiLayout.ButtonFontSize * scale;
            label.fontStyle = FontStyles.Bold;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            BaUiAssets.ApplyButtonFont(label);
            return (button, graphic, label);
        }

        public static Button CreateHeaderIconButton(
            Transform header,
            int slotFromRight,
            float scale,
            Action<Image> applyStyle,
            Func<Image, bool> tryApplyIcon,
            UnityAction onClick,
            string fallbackGlyph = null)
        {
            var go = new GameObject("HeaderIcon", typeof(RectTransform));
            go.transform.SetParent(header, false);
            var rect = go.GetComponent<RectTransform>();
            var size = BaUiLayout.HeaderIconButtonSize * scale;
            var rightInset = BaUiLayout.ComputeHeaderIconRightInset(slotFromRight, scale);
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = new Vector2(-rightInset, BaUiLayout.SettingsIconOffsetY * scale);

            var graphic = BaUiAssets.CreateButtonGraphic(rect, scale, applyStyle, 1f, bleedBottom: false);
            var button = go.AddComponent<Button>();
            button.targetGraphic = graphic;
            BaUiAssets.BindButtonClick(button, onClick);

            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(rect, false);
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            var iconPad = BaUiLayout.HeaderIconInnerPad * scale;
            iconRect.offsetMin = new Vector2(iconPad, iconPad);
            iconRect.offsetMax = new Vector2(-iconPad, -iconPad);
            var icon = iconGo.AddComponent<Image>();
            icon.raycastTarget = false;

            if (!tryApplyIcon(icon) && !string.IsNullOrEmpty(fallbackGlyph))
            {
                var fb = new GameObject("Fallback", typeof(RectTransform));
                fb.transform.SetParent(rect, false);
                var fbRect = fb.GetComponent<RectTransform>();
                fbRect.anchorMin = Vector2.zero;
                fbRect.anchorMax = Vector2.one;
                fbRect.offsetMin = new Vector2(iconPad, iconPad);
                fbRect.offsetMax = new Vector2(-iconPad, -iconPad);
                var tmp = fb.AddComponent<TextMeshProUGUI>();
                tmp.text = fallbackGlyph;
                tmp.fontSize = 13f * scale;
                tmp.color = BaUiAssets.TitleColor;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                BaUiAssets.ApplyTitleFont(tmp);
            }

            return button;
        }

        public static void ApplyWarningBannerLabel(TextMeshProUGUI text, float scale = 1f)
        {
            text.fontSize = BaUiLayout.TitleFontSize * scale;
            text.fontStyle = FontStyles.Bold;
            text.color = new Color(1f, 0.55f, 0.45f, 1f);
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Overflow;
            text.margin = Vector4.zero;
            BaUiAssets.ApplyTitleFont(text);
        }

        public static void ApplyBodyLabelStyle(TextMeshProUGUI text, float scale, bool muted = false)
        {
            text.fontSize = 14f * scale;
            text.fontStyle = FontStyles.Normal;
            text.color = muted ? BaUiAssets.MutedBodyTextColor : BaUiAssets.BodyTextColor;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;
            BaUiAssets.ApplyTitleFont(text);
        }

        public static GameObject CreateBannerRoot(string name, int sortOrder, float width, float height)
        {
            BaUiAssets.EnsureInitialized();
            var root = new GameObject(name);
            UnityEngine.Object.DontDestroyOnLoad(root);
            BaUiChrome.SetupOverlayCanvas(root, sortOrder);

            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(root.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(width, height);

            var bg = panel.AddComponent<Image>();
            BaUiAssets.ApplyPanelBg(bg);
            return root;
        }
    }
}
