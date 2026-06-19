using System;
using System.Collections.Generic;
using Capisoft.Lib.BaUnifiedUI.Layout;
using UnityEngine;

namespace Capisoft.Lib.BaUnifiedUI.Controls
{
    /// <summary>Dynamic list rows sharing one template (scroll content or panel strip).</summary>
    public sealed class BaUiListRowPool
    {
        private readonly RectTransform _parent;
        private readonly float _textScale;
        private readonly BaUiListRowTemplate _template;
        private readonly string _namePrefix;
        private readonly List<BaUiListRow> _rows = new List<BaUiListRow>();

        public IReadOnlyList<BaUiListRow> Rows => _rows;

        public BaUiListRowPool(
            RectTransform parent,
            float textScale,
            BaUiListRowTemplate template,
            string namePrefix = "Row")
        {
            _parent = parent;
            _textScale = textScale;
            _template = template;
            _namePrefix = namePrefix;
        }

        public void Sync(int needed, Action<int, BaUiListRow> onCreate = null)
        {
            while (_rows.Count < needed)
            {
                var index = _rows.Count;
                var row = BaUiListRows.Create(
                    _parent,
                    _namePrefix + index,
                    _textScale,
                    _template.Recipe,
                    _template.HorizontalInset);
                _rows.Add(row);
                onCreate?.Invoke(index, row);
            }

            while (_rows.Count > needed)
            {
                var last = _rows.Count - 1;
                if (_rows[last]?.Root != null)
                    UnityEngine.Object.Destroy(_rows[last].Root);
                _rows.RemoveAt(last);
            }
        }

        public int LayoutInScroll(BaUiScrollList scroll, Func<BaUiListRow, bool> isVisible, ref float y)
        {
            if (scroll?.Content == null)
                return 0;

            var active = 0;
            for (var i = 0; i < _rows.Count; i++)
            {
                var row = _rows[i];
                if (row?.Root == null || !isVisible(row))
                    continue;

                BaUiListRows.PositionInScroll(row, -y);
                y += BaUiListMetrics.RowHeight + BaUiListMetrics.RowGap;
                active++;
            }

            return active;
        }

        public void PlaceOnPanelStrip(float startY, int count)
        {
            for (var i = 0; i < count && i < _rows.Count; i++)
            {
                var row = _rows[i];
                if (row?.Rect == null)
                    continue;

                var rowTop = startY - i * (BaUiListMetrics.RowHeight + BaUiListMetrics.RowGap);
                row.Rect.anchoredPosition = new Vector2(0f, rowTop);
            }
        }

        public void Clear()
        {
            for (var i = 0; i < _rows.Count; i++)
            {
                if (_rows[i]?.Root != null)
                    UnityEngine.Object.Destroy(_rows[i].Root);
            }

            _rows.Clear();
        }
    }

    public static class BaUiListRowPools
    {
        public static void SyncHolders<THolder>(
            List<THolder> holders,
            RectTransform parent,
            int needed,
            float textScale,
            BaUiListRowTemplate template,
            string namePrefix,
            Func<int, BaUiListRow, THolder> createHolder,
            Func<THolder, BaUiListRow> selectUi)
        {
            while (holders.Count < needed)
            {
                var index = holders.Count;
                var ui = BaUiListRows.Create(
                    parent,
                    namePrefix + index,
                    textScale,
                    template.Recipe,
                    template.HorizontalInset);
                holders.Add(createHolder(index, ui));
            }

            while (holders.Count > needed)
            {
                var last = holders.Count - 1;
                var ui = selectUi(holders[last]);
                if (ui?.Root != null)
                    UnityEngine.Object.Destroy(ui.Root);
                holders.RemoveAt(last);
            }
        }

        public static int LayoutHoldersInScroll<THolder>(
            BaUiScrollList scroll,
            IReadOnlyList<THolder> holders,
            Func<THolder, BaUiListRow> selectUi,
            Func<THolder, bool> isVisible,
            ref float y)
        {
            if (scroll?.Content == null)
                return 0;

            var active = 0;
            for (var i = 0; i < holders.Count; i++)
            {
                var holder = holders[i];
                var ui = selectUi(holder);
                if (ui?.Root == null || !isVisible(holder))
                    continue;

                BaUiListRows.PositionInScroll(ui, -y);
                y += BaUiListMetrics.RowHeight + BaUiListMetrics.RowGap;
                active++;
            }

            return active;
        }
    }
}
