using Capisoft.Lib.BaUi.Assets;
using Capisoft.Lib.BaUi.Layout;
using Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUi.Chrome
{
    public static class BaUiChrome
    {
        public struct BuiltChrome
        {
            public RectTransform Panel;
            public RectTransform Background;
            public RectTransform Header;
            public BaUiLayout.Metrics Metrics;
            public float Scale;
            public float ContentInset;
        }

        public static void SetupOverlayCanvas(GameObject root, int sortingOrder, bool interactive = true)
        {
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            if (interactive)
            {
                root.AddComponent<GraphicRaycaster>();
            }
            else
            {
                var group = root.AddComponent<CanvasGroup>();
                group.interactable = false;
                group.blocksRaycasts = false;
            }

            ApplyUiLayer(root);
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

        public static BuiltChrome Build(
            Transform parent,
            float panelWidth,
            float panelHeight,
            string panelName,
            float headerExtraTrim = 0f)
        {
            BaUiAssets.EnsureInitialized();

            var scale = panelWidth / BaUiLayout.PanelWidth;
            var metrics = BaUiLayout.CreateMetrics(scale);

            var panel = CreateRect(parent, panelName);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(panelWidth, panelHeight);

            var background = CreateRect(panel, "Background");
            BaUiLayout.ApplyBodyFrame(background, scale);
            var backgroundImg = background.gameObject.AddComponent<Image>();
            backgroundImg.raycastTarget = true;
            BaUiAssets.ApplyPanelBg(backgroundImg);

            var header = CreateRect(panel, "Header");
            ApplyHeaderFrame(header, metrics, headerExtraTrim);
            var headerImg = header.gameObject.AddComponent<Image>();
            headerImg.raycastTarget = false;
            BaUiAssets.ApplyHeaderBg(headerImg);

            var chrome = new BuiltChrome();
            chrome.Panel = panel;
            chrome.Background = background;
            chrome.Header = header;
            chrome.Metrics = metrics;
            chrome.Scale = scale;
            chrome.ContentInset = metrics.ContentInset;
            return chrome;
        }

        /// <summary>Centered Voogle modal popup â€” right edge fixed, left-only flush extension.</summary>
        public static void ApplyModalHeaderFrame(RectTransform header, float scale)
        {
            var leftExtend = BaUiLayout.SettingsHeaderLeftFlush * scale;

            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.anchoredPosition = Vector2.zero;
            header.sizeDelta = Vector2.zero;
            header.offsetMin = new Vector2(-leftExtend, -BaUiLayout.HeaderHeight);
            header.offsetMax = Vector2.zero;
        }

        public static void ApplyHudTrimHeader(
            RectTransform header,
            float panelWidth,
            float headerExtraTrim = 0f)
        {
            var scale = panelWidth / BaUiLayout.PanelWidth;
            ApplyHeaderFrame(header, BaUiLayout.CreateMetrics(scale), headerExtraTrim);
        }

        /// <summary>Reapply body bleed + default hud-trim header together (RouteToggleHud recipe).</summary>
        public static void RestorePanelChrome(RectTransform panel, float panelWidth, float headerExtraTrim = 0f)
        {
            var scale = panelWidth / BaUiLayout.PanelWidth;
            var background = panel.Find("Background") as RectTransform;
            if (background != null)
                BaUiLayout.ApplyBodyFrame(background, scale);

            var header = panel.Find("Header") as RectTransform;
            if (header == null)
                return;

            if (header.parent != panel)
                header.SetParent(panel, false);

            ApplyHudTrimHeader(header, panelWidth, headerExtraTrim);
        }

        /// <summary>
        /// Header aligned to BaUiLayout visible body frame edges (ref panel 370).
        /// Uses BodyVisibleLeft/Right â€” the calibrated reference, not bleed or toggle guesses.
        /// </summary>
        public static void ApplyVisibleFrameHeader(RectTransform header, float scale)
        {
            if (header.parent is RectTransform parent && parent.name == "Background")
                header.SetParent(parent.parent, false);

            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.anchoredPosition = Vector2.zero;
            header.sizeDelta = Vector2.zero;

            var leftInset = BaUiLayout.BodyVisibleLeft * scale;
            var rightExtend = (BaUiLayout.BodyVisibleRight - BaUiLayout.PanelWidth) * scale;
            header.offsetMin = new Vector2(leftInset, -BaUiLayout.HeaderHeight);
            header.offsetMax = new Vector2(rightExtend, 0f);
        }

        /// <summary>
        /// Parents the header on the bled body frame and insets it to the sprite corners.
        /// Guarantees horizontal alignment between header bar and panel frame on tall docked panels.
        /// </summary>
        public static void ApplyHeaderOnBodyFrame(RectTransform header, RectTransform background, float scale)
        {
            header.SetParent(background, false);
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.anchoredPosition = Vector2.zero;
            header.sizeDelta = Vector2.zero;
            var leftInset = BaUiLayout.MainPanelHeaderTightenLeft * scale;
            var rightInset = BaUiLayout.MainPanelHeaderTightenRight * scale;
            header.offsetMin = new Vector2(leftInset, -BaUiLayout.HeaderHeight);
            header.offsetMax = new Vector2(-rightInset, 0f);
        }

        /// <summary>Toggle / docked HUD â€” header inset to visible frame borders (same as RouteToggleHud).</summary>
        public static void ApplyToggleHudHeaderFrame(RectTransform header, float scale)
        {
            var leftInset = BaUiLayout.HeaderSliceBorderLeft * scale + BaUiLayout.ToggleHudHeaderLeftAdjust;
            var rightInset = BaUiLayout.HeaderSliceBorderRight * scale + BaUiLayout.ToggleHudHeaderRightAdjust;

            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.anchoredPosition = Vector2.zero;
            header.sizeDelta = Vector2.zero;
            header.offsetMin = new Vector2(leftInset, -BaUiLayout.HeaderHeight);
            header.offsetMax = new Vector2(-rightInset, 0f);
        }

        /// <summary>Main panel or wide HUD â€” header edges align with visible body frame (ref-pixel constants).</summary>
        public static void ApplyMainPanelHeaderFrame(RectTransform header)
        {
            var leftExtend = BaUiLayout.FrameBleedWidth * 0.5f - BaUiLayout.FrameOffsetX -
                             BaUiLayout.MainPanelHeaderTightenLeft;
            var rightExtend = BaUiLayout.FrameBleedWidth * 0.5f + BaUiLayout.FrameOffsetX -
                              BaUiLayout.MainPanelHeaderTightenRight;

            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.anchoredPosition = Vector2.zero;
            header.sizeDelta = Vector2.zero;
            header.offsetMin = new Vector2(-leftExtend, -BaUiLayout.HeaderHeight);
            header.offsetMax = new Vector2(rightExtend, 0f);
        }

        public static void ApplyHeaderFrameAligned(RectTransform header, in BaUiLayout.Metrics metrics) =>
            ApplyHeaderFrame(header, metrics);

        public static void ApplyHeaderFrame(
            RectTransform header,
            in BaUiLayout.Metrics metrics,
            float headerExtraTrim = 0f)
        {
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);

            BaUiLayout.ComputeHeaderRectHudTrim(
                metrics.PanelWidth,
                metrics.Scale,
                headerExtraTrim,
                out var sizeDeltaX,
                out var posX);
            header.anchoredPosition = new Vector2(posX, 0f);
            header.sizeDelta = new Vector2(sizeDeltaX, metrics.HeaderHeight);

            if (IsSettingsFullWidthHeader(headerExtraTrim))
                ApplySettingsHeaderLeftFlush(header, metrics);
        }

        private static bool IsSettingsFullWidthHeader(float headerExtraTrim) =>
            Mathf.Abs(headerExtraTrim - BaUiLayout.SettingsPanelHeaderWidenTrim) < 0.01f;

        /// <summary>Colle le bord gauche du header sans dÃ©placer le bord droit (validÃ© en jeu).</summary>
        private static void ApplySettingsHeaderLeftFlush(RectTransform header, in BaUiLayout.Metrics metrics)
        {
            var extend = BaUiLayout.SettingsHeaderLeftFlush * metrics.Scale;
            header.anchoredPosition = Vector2.zero;
            header.sizeDelta = Vector2.zero;
            header.offsetMin = new Vector2(-extend, -metrics.HeaderHeight);
            header.offsetMax = Vector2.zero;
        }

        private static RectTransform CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<RectTransform>();
        }
    }
}

