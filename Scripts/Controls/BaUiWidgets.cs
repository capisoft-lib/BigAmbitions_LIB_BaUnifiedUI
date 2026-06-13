using System;
using Capisoft.Lib.BaUnifiedUI.Assets;
using Capisoft.Lib.BaUnifiedUI.Chrome;
using Capisoft.Lib.BaUnifiedUI.Core;
using Capisoft.Lib.BaUnifiedUI.Fluent;
using Capisoft.Lib.BaUnifiedUI.Layout;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Controls
{
    /// <summary>Shared uGUI widgets — sole low-level UI entry for consumer mods.</summary>
    public static class BaUiWidgets
    {
        public static RectTransform CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        public static void Stretch(RectTransform rect, float padX = 0f, float padY = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padX, padY);
            rect.offsetMax = new Vector2(-padX, -padY);
        }

        public static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        public static Button CreateModalDimmer(Transform parent, float alpha, UnityAction onClick)
        {
            var dim = CreateRect(parent, "Dimmer");
            StretchFull(dim);
            var dimImg = dim.gameObject.AddComponent<Image>();
            dimImg.color = new Color(0f, 0f, 0f, alpha);
            dimImg.raycastTarget = true;
            var dimBtn = dim.gameObject.AddComponent<Button>();
            dimBtn.targetGraphic = dimImg;
            dimBtn.onClick.AddListener(BaUiFocus.Wrap(onClick));
            return dimBtn;
        }

        public static TextMeshProUGUI CreateHeaderTitleLeft(
            Transform header,
            string text,
            float scale,
            int rightIconCount,
            bool upperCase = false)
        {
            var titleGo = CreateRect(header, "Title");
            StretchFull(titleGo);
            var reserve = rightIconCount > 0
                ? BaUiLayout.ComputeHeaderIconsTitleReserve(rightIconCount, scale)
                : BaUiLayout.HeaderTextPaddingX * scale;
            BaUiLayout.ApplyHeaderTitleWithRightReserve(titleGo, scale, reserve);
            var label = titleGo.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = BaUiLayout.TitleFontSize * scale;
            label.fontStyle = (upperCase ? FontStyles.UpperCase : FontStyles.Normal) | FontStyles.Bold;
            label.color = BaUiAssets.TitleColor;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.raycastTarget = false;
            BaUiAssets.ApplyTitleFont(label);
            return label;
        }

        public static TextMeshProUGUI CreateHeaderTitleCenter(Transform header, float scale)
        {
            var titleGo = CreateRect(header, "Title");
            StretchFull(titleGo);
            BaUiLayout.ApplyHeaderTitleInsets(titleGo, BaUiLayout.CreateMetrics(scale));
            var label = titleGo.gameObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = BaUiLayout.TitleFontSize * scale;
            label.fontStyle = FontStyles.Bold;
            label.color = BaUiAssets.TitleColor;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            BaUiAssets.ApplyTitleFont(label);
            return label;
        }

        public static (Image Graphic, TextMeshProUGUI Label) CreateHudActionButton(
            Transform panel,
            string name,
            Vector2 topAnchoredPos,
            float width,
            float height,
            float scale,
            BaButtonStyle style,
            UnityAction onClick)
        {
            var rect = CreateRect(panel, name);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = topAnchoredPos;
            rect.sizeDelta = new Vector2(width, height);

            Action<Image> apply = StyleAction(style);
            var graphic = BaUiAssets.CreateButtonGraphic(rect, scale, apply);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = graphic;
            BaUiAssets.BindButtonClick(button, onClick);

            var labelGo = CreateRect(rect, "Label");
            Stretch(labelGo);
            labelGo.offsetMin = new Vector2(BaUiLayout.ButtonTextPaddingX * scale, 0f);
            labelGo.offsetMax = new Vector2(-BaUiLayout.ButtonTextPaddingX * scale,
                -BaUiLayout.ButtonLabelBottomInset * scale);
            var label = labelGo.gameObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = BaUiLayout.ButtonFontSize * scale;
            label.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            BaUiAssets.ApplyButtonFont(label);
            return (graphic, label);
        }

        public static Button CreateFooterButton(
            Transform panel,
            string name,
            Vector2 anchoredPos,
            Vector2 size,
            float scale,
            string labelText,
            BaButtonStyle style,
            UnityAction onClick)
        {
            var rect = CreateRect(panel, name);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var img = BaUiAssets.CreateButtonGraphic(rect, scale, StyleAction(style));
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            BaUiAssets.BindButtonClick(button, onClick);

            var labelGo = CreateRect(rect, "Label");
            Stretch(labelGo);
            labelGo.offsetMin = new Vector2(BaUiLayout.ButtonTextPaddingX * scale, 0f);
            labelGo.offsetMax = new Vector2(-BaUiLayout.ButtonTextPaddingX * scale, 0f);
            var tmp = labelGo.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = labelText;
            tmp.fontSize = BaUiLayout.ButtonFontSize * scale;
            tmp.fontStyle = FontStyles.UpperCase;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            BaUiAssets.ApplyButtonFont(tmp);
            return button;
        }

        public static TextMeshProUGUI CreateCenteredBodyLabel(Transform parent, float scale, bool wordWrap = true)
        {
            var body = CreateRect(parent, "BodyLabel");
            StretchFull(body);
            var label = body.gameObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = BaUiLayout.ButtonFontSize * scale;
            label.color = BaUiAssets.BodyTextColor;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = wordWrap;
            label.raycastTarget = false;
            BaUiAssets.ApplyButtonFont(label);
            return label;
        }

        public static (GameObject Root, TextMeshProUGUI Label) CreateBanner(
            string rootName,
            int sortOrder,
            float width,
            float height,
            float centerYOffset)
        {
            BaUiAssets.EnsureInitialized();
            var root = new GameObject(rootName);
            UnityEngine.Object.DontDestroyOnLoad(root);
            BaUiChrome.SetupOverlayCanvas(root, sortOrder, interactive: false);

            var panel = CreateRect(root.transform, "Panel");
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = new Vector2(0f, centerYOffset);
            panel.sizeDelta = new Vector2(width, height);

            var bg = panel.gameObject.AddComponent<Image>();
            bg.raycastTarget = false;
            BaUiAssets.ApplyPanelBg(bg);

            var labelGo = CreateRect(panel, "Label");
            Stretch(labelGo, 18f, 12f);
            var label = labelGo.gameObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = BaUiLayout.TitleFontSize;
            label.fontStyle = FontStyles.Bold;
            label.color = BaUiAssets.TitleColor;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            BaUiAssets.ApplyTitleFont(label);

            BaUiChrome.ApplyUiLayer(root);
            return (root, label);
        }

        public static void ApplyButtonGraphic(Image image, BaButtonStyle style) =>
            StyleAction(style)(image);

        public static Image CreateButtonGraphic(
            RectTransform parent,
            float scale,
            BaButtonStyle style,
            bool bleedBottom = true) =>
            BaUiAssets.CreateButtonGraphic(parent, scale, StyleAction(style), bleedBottom: bleedBottom);

        public static void RestoreWideMapChrome(RectTransform panel, float panelWidth) =>
            BaUiChrome.RestorePanelChrome(
                panel,
                panelWidth,
                BaUiLayout.ComputeWideMapPanelHeaderWidenTrim(panelWidth));

        public static bool TryGetCarIcon(out Sprite sprite) => BaUiAssets.TryGetCarIcon(out sprite);

        public static BaUiInputGuard AttachInputGuard(TMP_InputField field)
        {
            var guard = field.gameObject.AddComponent<BaUiInputGuard>();
            guard.Bind(field);
            return guard;
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
