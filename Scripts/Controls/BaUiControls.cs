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
            return PopulateHeaderIconButton(rect, scale, applyStyle, tryApplyIcon, onClick, fallbackGlyph);
        }

        /// <summary>Right-aligned strip — icons stack via <see cref="HorizontalLayoutGroup"/> (first added = rightmost).</summary>
        public static RectTransform CreateHeaderIconStrip(Transform header, float scale)
        {
            var strip = BaUiWidgets.CreateRect(header, "HeaderIcons");
            strip.anchorMin = new Vector2(1f, 0f);
            strip.anchorMax = new Vector2(1f, 1f);
            strip.pivot = new Vector2(1f, 0.5f);
            strip.anchoredPosition = new Vector2(0f, BaUiLayout.SettingsIconOffsetY * scale);
            strip.sizeDelta = Vector2.zero;

            var pad = Mathf.RoundToInt(BaUiLayout.HeaderIconButtonPad * scale);
            var hlg = strip.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(pad, pad, 0, 0);
            hlg.spacing = BaUiLayout.HeaderIconButtonGap * scale;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.reverseArrangement = true;

            var fitter = strip.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            return strip;
        }

        public static Button CreateHeaderIconInStrip(
            Transform iconStrip,
            float scale,
            Action<Image> applyStyle,
            Func<Image, bool> tryApplyIcon,
            UnityAction onClick,
            string name = null,
            string fallbackGlyph = null)
        {
            var go = new GameObject(string.IsNullOrEmpty(name) ? "HeaderIcon" : name, typeof(RectTransform));
            go.transform.SetParent(iconStrip, false);
            var rect = go.GetComponent<RectTransform>();
            var size = BaUiLayout.HeaderIconButtonSize * scale;
            var layout = go.AddComponent<LayoutElement>();
            layout.preferredWidth = layout.preferredHeight = size;
            layout.minWidth = layout.minHeight = size;
            return PopulateHeaderIconButton(rect, scale, applyStyle, tryApplyIcon, onClick, fallbackGlyph);
        }

        /// <summary>Action panel buttons anchored from the panel top-center (legacy RouteToggleHud layout).</summary>
        public static Button CreatePanelTopActionButton(
            RectTransform panel,
            string name,
            Vector2 topAnchoredPos,
            float width,
            float height,
            float scale,
            BaButtonStyle style,
            UnityAction onClick,
            out Image graphic,
            out TextMeshProUGUI label)
        {
            var rect = BaUiWidgets.CreateRect(panel, name);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = topAnchoredPos;
            rect.sizeDelta = new Vector2(width, height);

            graphic = BaUiWidgets.CreateButtonGraphic(rect, scale, style);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = graphic;
            BaUiAssets.BindButtonClick(button, onClick);

            var labelGo = BaUiWidgets.CreateRect(rect, "Label");
            BaUiWidgets.Stretch(labelGo);
            labelGo.offsetMin = new Vector2(BaUiLayout.ButtonTextPaddingX * scale, 0f);
            labelGo.offsetMax = new Vector2(-BaUiLayout.ButtonTextPaddingX * scale,
                -BaUiLayout.ButtonLabelBottomInset * scale);
            label = labelGo.gameObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = BaUiLayout.ButtonFontSize * scale;
            label.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
            BaUiAssets.ApplyButtonFont(label);
            return button;
        }

        private static Button PopulateHeaderIconButton(
            RectTransform rect,
            float scale,
            Action<Image> applyStyle,
            Func<Image, bool> tryApplyIcon,
            UnityAction onClick,
            string fallbackGlyph)
        {
            var graphic = BaUiAssets.CreateButtonGraphic(rect, scale, applyStyle, 1f, bleedBottom: false);
            var button = rect.gameObject.AddComponent<Button>();
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
