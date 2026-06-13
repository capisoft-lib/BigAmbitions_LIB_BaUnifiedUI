using Capisoft.Lib.BaUi.Assets;
using Capisoft.Lib.BaUi.Core;
using Capisoft.Lib.BaUi.Layout;
using System;
using Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUi.Chrome
{
    public enum BaVanillaButtonStyle
    {
        Blue,
        Grey,
        Green,
        Red
    }

    public static class BaUiWidePanelChrome
    {
        /// <summary>Vanilla HUD reference width (same as VoogleRoute NavPanelLayout).</summary>
        public const float RefPanelWidth = 370f;
        public const float MainPanelWidth = RefPanelWidth * 2f;
        public const float MinFallbackPanelWidth = 540f;
        public const float FooterStatusVerticalSavings = 36f;
        public const float PanelTopClearanceSavings = 24f;
        public const float FooterTopContentHeight = 110f;
        public const float FooterStatusZoneHeight = 26f;
        public const float FooterHeight = FooterTopContentHeight + FooterStatusZoneHeight;
        public const float PrimaryButtonHeight = 46f;
        public const float HeaderBlockHeight = 48f;
        public const float ContentInset = 18f;
        public const float HudBodyTopPadding = 5f;
        public const float HudBodyBottomPadding = 8f;
        public const float HudButtonHeight = 40f;
        public const float HudButtonTextPaddingX = 12f;
        public const float HudButtonFontSize = 16f;
        public const float HudButtonGraphicBleedBottom = 2f;
        public const float HudButtonPixelsPerUnit = 2.5f;
        public const float HeaderTextPaddingX = 18f;
        public const float HeaderTextPaddingY = 7f;
        public const float HeaderTrimWidthBase = 11f;
        public const float HeaderTrimOffsetXBase = -0.5f;
        public const float HeaderLeftExtend = 2f;
        public const float BodyVisibleLeft = 26f;
        public const float BodyVisibleRight = 373f;
        public const float HeaderSliceBorderLeft = BodyVisibleLeft - 3f;
        public const float HeaderSliceBorderRight = 10f;
        public const float ToggleHudHeaderLeftAdjust = -9f;
        public const float ToggleHudHeaderRightAdjust = 1f;
        public static float FullWidthHeaderTrim => -(HeaderTrimWidthBase - HeaderLeftExtend);
        public const float FrameBleedWidth = 24f;
        public const float FramePixelsPerUnit = 2.45f;
        public const float FrameBleedHeight = 26f;
        public const float FrameOffsetX = -2f;
        public const float FrameOffsetY = -13f;
        public const float MainPanelHeaderTightenLeft = 3f;
        public const float MainPanelHeaderTightenRight = 5f;
        public const float ToggleHudButtonHeight = 32f;
        public const float ToggleHudButtonMarginX = 26f;
        public const float ToggleHudButtonLift = 6f;
        public const float ScreenMarginX = 16f;
        public const float ScreenMarginY = 36f;
        public const float TopScreenMargin = 36f;
        public const float PanelGapAboveCart = 20f;
        public const float FallbackCartHeight = 140f;
        public const float MinPanelHeight = 320f;

        public static readonly Color TitleColor = new Color(0.15f, 0.17f, 0.22f, 1f);
        public static readonly Color BodyTextColor = new Color(0.92f, 0.94f, 0.96f, 1f);
        public static readonly Color MutedTextColor = new Color(0.72f, 0.76f, 0.8f, 1f);
        public static readonly Color WarningTextColor = new Color(1f, 0.55f, 0.45f, 1f);
        public static readonly Color AccentGreenColor = new Color(0.35f, 0.95f, 0.45f, 1f);
        public static readonly Color ColumnHeaderColor = new Color(0.58f, 0.64f, 0.7f, 1f);
        public static readonly Color RowSeparatorColor = new Color(1f, 1f, 1f, 0.08f);
        public static readonly Color ListInsetColor = new Color(0f, 0f, 0f, 0.22f);
        public static readonly Color ButtonBlueTop = new Color(0.35f, 0.55f, 0.95f, 1f);
        public static readonly Color ButtonBlueBottom = new Color(0.2f, 0.38f, 0.78f, 1f);
        public static readonly Color ButtonGreenFallback = new Color(0.28f, 0.72f, 0.38f, 1f);
        public static readonly Color ButtonRedFallback = new Color(0.82f, 0.22f, 0.22f, 1f);
        public const float HeaderCloseButtonSize = 30f;
        public const float HeaderCloseButtonOffsetX = -5f;
        public const float FooterStatusVerticalNudge = 6f;
        public const float HeaderCloseButtonOffsetY = 1f;

        private static Sprite _panelBg;
        private static Sprite _headerBg;
        private static Sprite _btnGreen;
        private static Sprite _btnBlue;
        private static Sprite _btnGrey;
        private static Sprite _btnRed;
        private static TMP_FontAsset _fontRegular;
        private static TMP_FontAsset _fontBold;
        private static TMP_FontAsset _fontMedium;
        private static bool _discovered;

        public readonly struct HudPanelMetrics
        {
            public readonly float Scale;
            public readonly float PanelWidth;
            public readonly float PanelHeight;
            public readonly float ContentInset;
            public readonly float ContentWidth;
            public readonly float ButtonTopY;
            public readonly float FullButtonWidth;

            public HudPanelMetrics(float scale)
            {
                Scale = scale;
                PanelWidth = RefPanelWidth * scale;
                ContentInset = BaUiWidePanelChrome.ContentInset * scale;
                ContentWidth = PanelWidth - ContentInset * 2f;
                var bodyHeight = HudBodyTopPadding + HudButtonHeight + HudBodyBottomPadding;
                PanelHeight = HeaderBlockHeight + bodyHeight;
                ButtonTopY = -(HeaderBlockHeight + HudBodyTopPadding);
                FullButtonWidth = ContentWidth;
            }
        }

        public static void EnsureInitialized() => BaUiAssets.EnsureInitialized();

        public static void SetupOverlayCanvas(GameObject root, int sortingOrder, bool interactive)
        {
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            root.AddComponent<GraphicRaycaster>();

            var group = root.AddComponent<CanvasGroup>();
            group.interactable = interactive;
            group.blocksRaycasts = interactive;
        }

        /// <summary>Match vanilla UI layer so GameManager.HasInputSelected blocks hotkeys while typing.</summary>
        public static void ApplyUiLayer(GameObject root)
        {
            if (root == null)
                return;

            SetLayerRecursive(root, LayerHelper.UiLayerIndex);
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            var transform = go.transform;
            for (var i = 0; i < transform.childCount; i++)
                SetLayerRecursive(transform.GetChild(i).gameObject, layer);
        }

        /// <summary>Same chrome recipe as VoogleRoute RouteToggleHud / GameStylePanelChrome.</summary>
        public static RectTransform BuildToggleHudPanel(Transform parent, out RectTransform header, out HudPanelMetrics metrics)
        {
            metrics = new HudPanelMetrics(1f);
            return BuildHudPanel(parent, metrics.PanelWidth, metrics.PanelHeight, "ToggleHud", out header, useToggleHeader: true);
        }

        public static RectTransform BuildPanel(Transform parent, float panelWidth, float panelHeight, string panelName, out RectTransform header) =>
            BuildHudPanel(parent, panelWidth, panelHeight, panelName, out header, useToggleHeader: false);

        private static RectTransform BuildHudPanel(
            Transform parent,
            float panelWidth,
            float panelHeight,
            string panelName,
            out RectTransform header,
            bool useToggleHeader)
        {
            EnsureInitialized();

            var scale = panelWidth / RefPanelWidth;
            var panel = CreateRect(parent, panelName);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = new Vector2(panelWidth, panelHeight);

            var background = CreateRect(panel, "Background");
            ApplyBodyFrame(background, scale);
            var bgImage = background.gameObject.AddComponent<Image>();
            bgImage.raycastTarget = true;
            ApplyPanelBg(bgImage);

            header = CreateRect(panel, "Header");
            if (useToggleHeader)
                ApplyToggleHudHeaderFrame(header, panelWidth);
            else
                ApplyMainPanelHeaderFrame(header, panelWidth);
            var headerImage = header.gameObject.AddComponent<Image>();
            headerImage.raycastTarget = false;
            ApplyHeaderBg(headerImage);

            return panel;
        }

        public static void ApplyHeaderTitleInsets(RectTransform rect, float scale)
        {
            var padX = HeaderTextPaddingX * scale;
            var padY = HeaderTextPaddingY * scale;
            Stretch(rect);
            rect.offsetMin = new Vector2(padX, padY);
            rect.offsetMax = new Vector2(-padX, -padY);
        }

        /// <summary>
        /// Toggle HUD header â€” inset to visible frame borders (narrower than panel bleed rect).
        /// </summary>
        public static void ApplyToggleHudHeaderFrame(RectTransform header, float panelWidth)
        {
            var scale = Mathf.Max(0.01f, panelWidth / RefPanelWidth);
            var leftInset = HeaderSliceBorderLeft * scale + ToggleHudHeaderLeftAdjust;
            var rightInset = HeaderSliceBorderRight * scale + ToggleHudHeaderRightAdjust;

            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.anchoredPosition = Vector2.zero;
            header.sizeDelta = Vector2.zero;
            header.offsetMin = new Vector2(leftInset, -HeaderBlockHeight);
            header.offsetMax = new Vector2(-rightInset, 0f);
        }

        public static void StretchButtonGraphic(RectTransform rect, float scale)
        {
            var bleed = HudButtonGraphicBleedBottom * scale;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(0f, -bleed);
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>Same graphic child + bleed recipe as VoogleRoute GameUiStyle.CreateButtonGraphic.</summary>
        public static Image CreateButtonGraphic(
            RectTransform buttonRoot,
            float scale,
            BaVanillaButtonStyle style,
            bool bleedBottom = true,
            Action<Image> applyStyle = null)
        {
            void Apply(Image img)
            {
                if (applyStyle != null)
                    applyStyle(img);
                else
                    ApplyBaVanillaButtonStyle(img, style);
            }

            if (!bleedBottom)
            {
                var flat = buttonRoot.gameObject.AddComponent<Image>();
                flat.raycastTarget = true;
                Apply(flat);
                return flat;
            }

            var graphicGo = new GameObject("Graphic", typeof(RectTransform));
            graphicGo.transform.SetParent(buttonRoot, false);
            var rt = graphicGo.GetComponent<RectTransform>();
            StretchButtonGraphic(rt, scale);
            var img = graphicGo.AddComponent<Image>();
            img.raycastTarget = true;
            Apply(img);
            return img;
        }

        public static Button CreateVanillaButton(
            Transform parent,
            string label,
            float width,
            float height,
            float scale,
            UnityAction onClick,
            BaVanillaButtonStyle style = BaVanillaButtonStyle.Blue,
            float fontSize = 0f,
            bool bleedBottom = true,
            Action<Image> applyStyle = null)
        {
            if (fontSize <= 0f)
                fontSize = HudButtonFontSize;

            var rect = CreateRect(parent, "Button");
            rect.sizeDelta = new Vector2(width, height);

            var image = CreateButtonGraphic(rect, scale, style, bleedBottom, applyStyle);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ApplyVanillaButtonColors(button);
            if (onClick != null)
                button.onClick.AddListener(onClick);

            var labelGo = CreateRect(rect, "Label");
            labelGo.anchorMin = Vector2.zero;
            labelGo.anchorMax = Vector2.one;
            labelGo.offsetMin = new Vector2(HudButtonTextPaddingX * scale, 0f);
            labelGo.offsetMax = new Vector2(-HudButtonTextPaddingX * scale, 0f);
            var text = labelGo.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = fontSize * scale;
            text.fontStyle = FontStyles.UpperCase;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
            ApplyButtonFont(text);

            return button;
        }

        public static Button CreateHudActionButton(
            Transform parent,
            string label,
            float width,
            float height,
            float scale,
            UnityAction onClick,
            bool blue = true) =>
            CreateVanillaButton(
                parent,
                label,
                width,
                height,
                scale,
                onClick,
                blue ? BaVanillaButtonStyle.Blue : BaVanillaButtonStyle.Grey);

        public static void ApplyHudTitleStyle(TextMeshProUGUI text, float scale = 1f)
        {
            text.fontSize = 18f * scale;
            text.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            text.color = TitleColor;
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;
            ApplyTitleFont(text);
        }

        public static void UpdateToggleHudFrames(RectTransform panel, RectTransform header, float panelWidth)
        {
            var scale = GetScale(panelWidth);
            var background = panel.Find("Background") as RectTransform;
            if (background != null)
                ApplyBodyFrame(background, scale);

            ApplyToggleHudHeaderFrame(header, panelWidth);
        }

        /// <summary>Same trim recipe as VoogleRoute NavPanelLayout.ComputeHeaderRectHudTrim.</summary>
        public static void ComputeHeaderRectHudTrim(
            float panelWidth,
            float scale,
            float extraTrimWidth,
            out float sizeDeltaX,
            out float anchoredPositionX)
        {
            _ = panelWidth;
            var trimW = (HeaderTrimWidthBase - HeaderLeftExtend + extraTrimWidth) * scale;
            if (trimW <= 0f)
            {
                sizeDeltaX = 0f;
                anchoredPositionX = 0f;
                return;
            }

            var trimOffset = (HeaderTrimOffsetXBase - HeaderLeftExtend * 0.5f) * scale;
            sizeDeltaX = -trimW;
            anchoredPositionX = trimOffset;
        }

        /// <summary>
        /// Header edges aligned to the visible frame. Bleed uses ref-pixel constants (not panel-width scale)
        /// so a 2Ã—-wide panel does not over-stretch the title bar past the body frame.
        /// </summary>
        public static void ApplyMainPanelHeaderFrame(RectTransform header, float panelWidth)
        {
            _ = panelWidth;
            var leftExtend = FrameBleedWidth * 0.5f - FrameOffsetX - MainPanelHeaderTightenLeft;
            var rightExtend = FrameBleedWidth * 0.5f + FrameOffsetX - MainPanelHeaderTightenRight;

            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.anchoredPosition = Vector2.zero;
            header.sizeDelta = Vector2.zero;
            header.offsetMin = new Vector2(-leftExtend, -HeaderBlockHeight);
            header.offsetMax = new Vector2(rightExtend, 0f);
        }

        public static void ConfigureBottomLeftHudAnchor(RectTransform panel)
        {
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.zero;
            panel.pivot = Vector2.zero;
        }

        public static float GetScale(float panelWidth) => panelWidth / RefPanelWidth;

        public static void UpdatePanelFrames(RectTransform panel, RectTransform header, float panelWidth)
        {
            var scale = GetScale(panelWidth);
            var background = panel.Find("Background") as RectTransform;
            if (background != null)
                ApplyBodyFrame(background, scale);

            ApplyMainPanelHeaderFrame(header, panelWidth);
        }

        public static void ApplyHeaderCloseButtonLayout(RectTransform closeButton, float scale)
        {
            if (closeButton == null)
                return;

            closeButton.anchoredPosition = new Vector2(HeaderCloseButtonOffsetX * scale, HeaderCloseButtonOffsetY);
            closeButton.sizeDelta = new Vector2(HeaderCloseButtonSize, HeaderCloseButtonSize);
        }

        public static Button CreateCartChip(
            Transform parent,
            string label,
            bool selected,
            UnityAction onClick) =>
            CreateVanillaButton(
                parent,
                label,
                10f,
                28f,
                1f,
                onClick,
                selected ? BaVanillaButtonStyle.Green : BaVanillaButtonStyle.Grey,
                fontSize: 13f);

        public static Button CreateIconToolbarButton(Transform parent, string label, UnityAction onClick) =>
            CreateVanillaButton(parent, label, 36f, 28f, 1f, onClick, BaVanillaButtonStyle.Grey, fontSize: 13f);

        public static Button CreateButton(Transform parent, string label, float width, float height, UnityAction onClick) =>
            CreateVanillaButton(parent, label, width, height, 1f, onClick, BaVanillaButtonStyle.Blue, fontSize: 16f);

        public static Button CreateQtyButton(Transform parent, string label, float width, float height, UnityAction onClick) =>
            CreateVanillaButton(parent, label, width, height, 1f, onClick, BaVanillaButtonStyle.Green, fontSize: 16f);

        public static Button CreateHeaderCloseButton(Transform header, UnityAction onClick)
        {
            var rect = CreateRect(header, "CloseButton");
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(HeaderCloseButtonOffsetX, HeaderCloseButtonOffsetY);
            rect.sizeDelta = new Vector2(HeaderCloseButtonSize, HeaderCloseButtonSize);

            var image = CreateButtonGraphic(rect, 1f, BaVanillaButtonStyle.Red, bleedBottom: false);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ApplyVanillaButtonColors(button);
            if (onClick != null)
                button.onClick.AddListener(onClick);

            var labelGo = CreateRect(rect, "Label");
            Stretch(labelGo);
            var text = labelGo.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = "\u00d7";
            text.fontSize = 22f;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            ApplyButtonFont(text);

            return button;
        }

        public static Button CreateRedButton(Transform parent, string label, float width, float height, UnityAction onClick) =>
            CreateVanillaButton(parent, label, width, height, 1f, onClick, BaVanillaButtonStyle.Red, fontSize: 18f);

        public static Button CreatePrimaryButton(Transform parent, string label, UnityAction onClick) =>
            CreateVanillaButton(
                parent,
                label,
                10f,
                PrimaryButtonHeight,
                1f,
                onClick,
                BaVanillaButtonStyle.Blue,
                fontSize: 18f);

        public static void ApplyColumnHeaderStyle(TextMeshProUGUI text)
        {
            text.fontSize = 11f;
            text.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            text.color = ColumnHeaderColor;
            text.alignment = TextAlignmentOptions.Center;
            ApplyTitleFont(text);
        }

        public static void ApplyTotalValueStyle(TextMeshProUGUI text)
        {
            text.fontSize = 18f;
            text.fontStyle = FontStyles.Bold;
            text.color = AccentGreenColor;
            text.alignment = TextAlignmentOptions.MidlineRight;
            ApplyTitleFont(text);
        }

        private static void ApplyVanillaButtonColors(Button button)
        {
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.7f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
        }

        public static Image GetVanillaButtonImage(Button button)
        {
            if (button == null)
                return null;

            var image = button.GetComponent<Image>();
            if (image != null)
                return image;

            if (button.targetGraphic is Image target)
                return target;

            var graphic = button.transform.Find("Graphic");
            return graphic != null ? graphic.GetComponent<Image>() : null;
        }

        public static void ApplyVanillaButtonImageStyle(Image image, BaVanillaButtonStyle style)
        {
            if (image == null)
                return;

            EnsureInitialized();
            ApplyBaVanillaButtonStyle(image, style);
        }

        public static void SetBaVanillaButtonStyle(Button button, BaVanillaButtonStyle style) =>
            ApplyVanillaButtonImageStyle(GetVanillaButtonImage(button), style);

        private static void ApplyBaVanillaButtonStyle(Image image, BaVanillaButtonStyle style)
        {
            switch (style)
            {
                case BaVanillaButtonStyle.Grey:
                    ApplyHudButtonGrey(image);
                    break;
                case BaVanillaButtonStyle.Green:
                    ApplyHudButtonGreen(image);
                    break;
                case BaVanillaButtonStyle.Red:
                    ApplyHudButtonRed(image);
                    break;
                default:
                    ApplyHudButtonBlue(image);
                    break;
            }
        }

        private static void ApplyHudButtonBlue(Image image)
        {
            ApplySliced(image, _btnBlue, new Color(0.25f, 0.58f, 0.82f, 1f), Color.white);
            image.pixelsPerUnitMultiplier = HudButtonPixelsPerUnit;
        }

        private static void ApplyHudButtonGrey(Image image)
        {
            ApplySliced(image, _btnGrey != null ? _btnGrey : _btnBlue, new Color(0.36f, 0.41f, 0.46f, 1f), Color.white);
            image.pixelsPerUnitMultiplier = HudButtonPixelsPerUnit;
        }

        private static void ApplyHudButtonGreen(Image image)
        {
            var vanillaContinueGreen = new Color(0.47f, 0.73f, 0.38f, 1f);
            ApplySliced(image, _btnGreen != null ? _btnGreen : _btnGrey, vanillaContinueGreen, Color.white);
            image.pixelsPerUnitMultiplier = HudButtonPixelsPerUnit;
        }

        private static void ApplyHudButtonRed(Image image)
        {
            var vanillaRed = new Color(0.78f, 0.28f, 0.28f, 1f);
            ApplySliced(image, _btnRed, vanillaRed, Color.white);
            image.pixelsPerUnitMultiplier = HudButtonPixelsPerUnit;
        }

        public static void ApplyTitleStyle(TextMeshProUGUI text, float scale = 1f)
        {
            text.fontSize = 18f * Mathf.Clamp(scale, 0.85f, 1.15f);
            text.fontStyle = FontStyles.Bold;
            text.color = TitleColor;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;
            ApplyTitleFont(text);
        }

        public static void ApplyBodyStyle(TextMeshProUGUI text, float scale = 1f, bool muted = false)
        {
            text.fontSize = 14f * scale;
            text.fontStyle = FontStyles.Normal;
            text.color = muted ? MutedTextColor : BodyTextColor;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            ApplyTitleFont(text);
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void ApplyBodyFrame(RectTransform rect, float scale)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(FrameOffsetX * scale, FrameOffsetY * scale);
            rect.sizeDelta = new Vector2(FrameBleedWidth * scale, FrameBleedHeight * scale);
        }

        public static void ApplyHeaderFrame(RectTransform header, float scale) =>
            ApplyMainPanelHeaderFrame(header, RefPanelWidth * scale);

        private static void ApplyPanelBg(Image image)
        {
            ApplySliced(image, _panelBg, new Color(0.2f, 0.24f, 0.3f, 1f), Color.white);
            image.pixelsPerUnitMultiplier = FramePixelsPerUnit;
        }

        private static void ApplyHeaderBg(Image image)
        {
            ApplySliced(image, _headerBg, new Color(0.78f, 0.8f, 0.83f, 1f), Color.white);
            image.pixelsPerUnitMultiplier = FramePixelsPerUnit;
        }

        private static void ApplySliced(Image image, Sprite sprite, Color fallbackTint, Color spriteTint)
        {
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = spriteTint;
                var border = sprite.border;
                image.type = border.x > 0.01f || border.y > 0.01f || border.z > 0.01f || border.w > 0.01f
                    ? Image.Type.Sliced
                    : Image.Type.Simple;
            }
            else
            {
                image.color = fallbackTint;
            }

            image.preserveAspect = false;
        }

        private static void ApplyTitleFont(TextMeshProUGUI text)
        {
            var font = _fontRegular != null ? _fontRegular : _fontBold;
            if (font != null)
                text.font = font;
        }

        private static void ApplyButtonFont(TextMeshProUGUI text)
        {
            var font = _fontMedium != null ? _fontMedium : _fontBold;
            if (font != null)
                text.font = font;
        }

        private static RectTransform CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void DiscoverAssets()
        {
            try
            {
                foreach (var sprite in Resources.FindObjectsOfTypeAll<Sprite>())
                {
                    CaptureSprite(sprite);
                    if (HasAllAssets())
                        return;
                }
            }
            catch
            {
                // ignore
            }

            try
            {
                foreach (var font in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
                {
                    if (font == null)
                        continue;

                    if (font.name == "Rubik-Regular SDF" && _fontRegular == null)
                        _fontRegular = font;
                    else if (font.name == "Rubik-Bold SDF" && _fontBold == null)
                        _fontBold = font;
                    else if (font.name == "Rubik-Medium SDF" && _fontMedium == null)
                        _fontMedium = font;
                }
            }
            catch
            {
                // ignore
            }
        }

        private static bool HasAllAssets() =>
            _panelBg != null && _headerBg != null && (_fontRegular != null || _fontBold != null);

        private static void CaptureSprite(Sprite sprite)
        {
            if (sprite == null)
                return;

            if (sprite.name == "grey-round-bordered" && _panelBg == null)
                _panelBg = sprite;
            if (sprite.name == "darkgreybox-header@2x" && _headerBg == null)
                _headerBg = sprite;
            if (sprite.name == "Gradient-Green-Round" && _btnGreen == null)
                _btnGreen = sprite;
            if (sprite.name == "Gradient-Blue-Round" && _btnBlue == null)
                _btnBlue = sprite;
            if (sprite.name == "Gradient-Gray-Border-Round" && _btnGrey == null)
                _btnGrey = sprite;
            if (sprite.name == "Gradient-Red-Round" && _btnRed == null)
                _btnRed = sprite;
        }
    }
}

