using Capisoft.Lib.BaUnifiedUI.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Controls
{
    public sealed class BaUiScrollList
    {
        private const float ScrollbarWidth = 10f;
        private const float ScrollbarInset = 2f;
        private const float ScrollbarSpacing = 4f;

        public RectTransform Rect { get; private set; }
        public RectTransform Content { get; private set; }
        public ScrollRect Scroll { get; private set; }
        public Scrollbar VerticalScrollbar { get; private set; }

        public static BaUiScrollList Create(Transform parent, string name = "ListScroll")
        {
            var scrollGo = BaUiWidgets.CreateRect(parent, name);
            var instance = new BaUiScrollList { Rect = scrollGo };

            var scroll = scrollGo.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;
            instance.Scroll = scroll;

            var viewport = BaUiWidgets.CreateRect(scrollGo, "Viewport");
            BaUiWidgets.StretchFull(viewport);
            viewport.offsetMax = new Vector2(
                -(ScrollbarInset + ScrollbarWidth + ScrollbarSpacing),
                0f);
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll.viewport = viewport;

            var content = BaUiWidgets.CreateRect(viewport, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            scroll.content = content;
            instance.Content = content;

            var scrollbarRect = BaUiWidgets.CreateRect(scrollGo, "VerticalScrollbar");
            scrollbarRect.anchorMin = new Vector2(1f, 0f);
            scrollbarRect.anchorMax = new Vector2(1f, 1f);
            scrollbarRect.pivot = new Vector2(1f, 0.5f);
            scrollbarRect.anchoredPosition = new Vector2(-ScrollbarInset, 0f);
            scrollbarRect.sizeDelta = new Vector2(ScrollbarWidth, -ScrollbarInset * 2f);

            var track = scrollbarRect.gameObject.AddComponent<Image>();
            track.color = new Color(0f, 0f, 0f, 0.28f);

            var handleRect = BaUiWidgets.CreateRect(scrollbarRect, "Handle");
            BaUiWidgets.StretchFull(handleRect);
            handleRect.offsetMin = new Vector2(2f, 2f);
            handleRect.offsetMax = new Vector2(-2f, -2f);

            var handle = handleRect.gameObject.AddComponent<Image>();
            handle.color = new Color(0.82f, 0.87f, 0.92f, 0.9f);

            var scrollbar = scrollbarRect.gameObject.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handle;
            scrollbar.value = 1f;
            scrollbar.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(1f, 1f, 1f, 1f),
                pressedColor = new Color(0.82f, 0.9f, 1f, 1f),
                selectedColor = Color.white,
                disabledColor = new Color(1f, 1f, 1f, 0.35f),
                colorMultiplier = 1f,
                fadeDuration = 0.08f
            };
            scrollbar.navigation = new Navigation { mode = Navigation.Mode.None };

            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            scroll.verticalScrollbarSpacing = ScrollbarSpacing;
            instance.VerticalScrollbar = scrollbar;

            return instance;
        }

        public void PlaceInStack(BaUiVerticalStack stack, float horizontalInset, float viewportHeight)
        {
            stack.PlaceTopBand(Rect, viewportHeight, horizontalInset);
        }

        public void LayoutRows(int activeRowCount, float rowHeight = BaUiListMetrics.RowHeight, float rowGap = BaUiListMetrics.RowGap)
        {
            Content.sizeDelta = new Vector2(0f, BaUiListMetrics.ContentRowsBlockHeight(activeRowCount));
        }

        public static void PositionRow(RectTransform rowRect, float rowTop, float horizontalInset, float rowHeight = BaUiListMetrics.RowHeight)
        {
            rowRect.anchoredPosition = new Vector2(0f, rowTop);
            rowRect.sizeDelta = new Vector2(-horizontalInset * 2f, rowHeight);
        }

        public static void PositionRowInContent(RectTransform rowRect, float rowTop, float rowHeight = BaUiListMetrics.RowHeight)
        {
            rowRect.anchoredPosition = new Vector2(0f, rowTop);
            rowRect.sizeDelta = new Vector2(0f, rowHeight);
        }
    }
}
