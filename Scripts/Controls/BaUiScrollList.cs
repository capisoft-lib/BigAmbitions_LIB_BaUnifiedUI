using Capisoft.Lib.BaUnifiedUI.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Controls
{
    public sealed class BaUiScrollList
    {
        public RectTransform Rect { get; private set; }
        public RectTransform Content { get; private set; }
        public ScrollRect Scroll { get; private set; }

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
