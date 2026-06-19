using UnityEngine;

namespace Capisoft.Lib.BaUnifiedUI.Layout
{
    /// <summary>Top-down Y placement for panel sections (header-relative).</summary>
    public sealed class BaUiVerticalStack
    {
        private float _y;

        public BaUiVerticalStack(float startY) => _y = startY;

        public float CursorY => _y;

        /// <param name="horizontalInset">Per-side inset in screen pixels (total shrink is ×2).</param>
        public float PlaceTopBand(RectTransform rect, float height, float horizontalInset, float gapAfter = 0f)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -_y);
            rect.sizeDelta = new Vector2(-horizontalInset * 2f, height);
            _y += height + gapAfter;
            return _y;
        }

        public float Advance(float height, float gapAfter = 0f)
        {
            _y += height + gapAfter;
            return _y;
        }
    }
}
