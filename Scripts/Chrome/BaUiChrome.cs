using Capisoft.Lib.BaUnifiedUI.Assets;

using Capisoft.Lib.BaUnifiedUI.Layout;

using Helpers;

using UnityEngine;

using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Chrome

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

                root.AddComponent<GraphicRaycaster>();

            else

            {

                var group = root.AddComponent<CanvasGroup>();

                group.interactable = false;

                group.blocksRaycasts = false;

            }



            ApplyUiLayer(root);

        }



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

            header.SetAsLastSibling();



            var chrome = new BuiltChrome();

            chrome.Panel = panel;

            chrome.Background = background;

            chrome.Header = header;

            chrome.Metrics = metrics;

            chrome.Scale = scale;

            chrome.ContentInset = metrics.ContentInset;

            return chrome;

        }



        /// <summary>Re-apply docked header frame + bg after header children are built.</summary>
        public static void FinalizeDockedHeader(
            RectTransform panel,
            RectTransform header,
            float panelWidth,
            bool dockedPanel,
            float headerExtraTrim)
        {
            if (panel == null || header == null)
                return;

            if (!dockedPanel)
            {
                var scale = panelWidth / BaUiLayout.PanelWidth;
                BaUiAssets.BuildHeaderBackground(header, scale);
                return;
            }

            RestorePanelChrome(panel, panelWidth, headerExtraTrim);
        }

        /// <summary>Default docked header — hud-trim with explicit extraTrim only (no width inference).</summary>
        public static void ApplyDockedHeaderFrame(
            RectTransform header,
            in BaUiLayout.Metrics metrics,
            float headerExtraTrim = 0f)
        {
            ApplyHeaderFrame(header, metrics, headerExtraTrim);
        }



        public static void ApplyModalHeaderFrame(RectTransform header, float scale)

        {

            EnsureHeaderOnPanel(header);

            var leftExtend = BaUiLayout.SettingsHeaderLeftFlush * scale;



            header.anchorMin = new Vector2(0f, 1f);

            header.anchorMax = new Vector2(1f, 1f);

            header.pivot = new Vector2(0.5f, 1f);

            header.anchoredPosition = Vector2.zero;

            header.sizeDelta = Vector2.zero;

            header.offsetMin = new Vector2(-leftExtend, -BaUiLayout.HeaderHeight);

            header.offsetMax = Vector2.zero;

        }



        public static void RestorePanelChrome(RectTransform panel, float panelWidth, float headerExtraTrim = 0f)

        {

            var scale = panelWidth / BaUiLayout.PanelWidth;

            var background = panel.Find("Background") as RectTransform;

            if (background != null)

                BaUiLayout.ApplyBodyFrame(background, scale);



            var header = panel.Find("Header") as RectTransform;

            if (header == null)

                return;



            EnsureHeaderOnPanel(header);

            Canvas.ForceUpdateCanvases();

            var metrics = BaUiLayout.CreateMetrics(scale);

            ApplyHeaderFrame(header, metrics, headerExtraTrim);

            BaUiAssets.BuildHeaderBackground(header, scale);

        }



        public static void ApplyMainPanelHeaderFrame(RectTransform header)

        {

            EnsureHeaderOnPanel(header);

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

            ApplyDockedHeaderFrame(header, metrics);



        public static void ApplyHeaderFrame(

            RectTransform header,

            in BaUiLayout.Metrics metrics,

            float headerExtraTrim = 0f)

        {

            EnsureHeaderOnPanel(header);

            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

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



        private static void EnsureHeaderOnPanel(RectTransform header)

        {

            if (header.parent is RectTransform parent && parent.name == "Background")

                header.SetParent(parent.parent, false);

        }



        private static bool IsSettingsFullWidthHeader(float headerExtraTrim) =>

            Mathf.Abs(headerExtraTrim - BaUiLayout.SettingsPanelHeaderWidenTrim) < 0.01f;



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


