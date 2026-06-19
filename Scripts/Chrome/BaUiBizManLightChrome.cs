using Capisoft.Lib.BaUnifiedUI.Assets;
using Capisoft.Lib.BaUnifiedUI.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Chrome
{
    /// <summary>White BizMan-style modal chrome (in-game light theme).</summary>
    public static class BaUiBizManLightChrome
    {
        public static BaUiChrome.BuiltChrome Build(
            Transform parent,
            float panelWidth,
            float panelHeight,
            string panelName)
        {
            BaUiAssets.EnsureInitialized();
            var scale = BaUiBizManLightLayout.ComputeScale(panelWidth);
            var contentInset = BaUiBizManLightLayout.ContentInset * scale;
            var headerHeight = BaUiBizManLightLayout.HeaderHeight * scale;

            var panel = CreateRect(parent, panelName);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(panelWidth, panelHeight);

            var background = CreateRect(panel, "Background");
            Stretch(background);
            var backgroundImg = background.gameObject.AddComponent<Image>();
            backgroundImg.raycastTarget = true;
            BaUiAssets.ApplyBizManLightPanelBg(backgroundImg);

            var header = CreateRect(panel, "Header");
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.anchoredPosition = Vector2.zero;
            header.sizeDelta = new Vector2(0f, headerHeight);
            var headerImg = header.gameObject.AddComponent<Image>();
            headerImg.raycastTarget = false;
            BaUiAssets.ApplyBizManLightHeaderBg(headerImg);

            var metrics = BaUiLayout.CreateMetrics(scale);
            return new BaUiChrome.BuiltChrome
            {
                Panel = panel,
                Background = background,
                Header = header,
                Metrics = metrics,
                Scale = scale,
                ContentInset = contentInset
            };
        }

        public static RectTransform CreateBodyRect(RectTransform panel, float scale)
        {
            var go = new GameObject("Body", typeof(RectTransform));
            go.transform.SetParent(panel, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            var inset = BaUiBizManLightLayout.ContentInset * scale;
            var header = BaUiBizManLightLayout.HeaderHeight * scale;
            var bottom = BaUiBizManLightLayout.BodyBottomPadding * scale;
            rect.offsetMin = new Vector2(inset, bottom);
            rect.offsetMax = new Vector2(-inset, -header);
            return rect;
        }

        private static RectTransform CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
