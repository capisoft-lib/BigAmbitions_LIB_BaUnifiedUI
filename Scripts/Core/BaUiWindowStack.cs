using System.Collections.Generic;
using Capisoft.Lib.BaUnifiedUI.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Core
{
    /// <summary>
    /// Layered overlay windows: each child window sorts above its parent; popups sort above panel content.
    /// </summary>
    public static class BaUiWindowStack
    {
        private const int SortStep = 40;
        private const int PopupLayerOffset = 80;
        private static int _stackDepth;
        private static readonly Dictionary<int, Transform> PopupLayers = new();

        public static int PushSortOrder(int baseSortOrder)
        {
            _stackDepth++;
            return baseSortOrder + _stackDepth * SortStep;
        }

        public static void PopSortOrder()
        {
            if (_stackDepth > 0)
                _stackDepth--;
        }

        public static int ChildSortOrder(int parentSortOrder) => parentSortOrder + SortStep;

        /// <summary>
        /// Popup layer for dropdowns / pickers opened from a modal. Sorts above the parent canvas.
        /// </summary>
        public static Transform GetOrCreatePopupLayer(GameObject root)
        {
            if (root == null)
                return null;

            var id = root.GetInstanceID();
            if (PopupLayers.TryGetValue(id, out var existing) && existing != null)
                return existing;

            var rootCanvas = root.GetComponent<Canvas>();
            if (rootCanvas == null)
                return root.transform;

            var go = new GameObject("BaUi_PopupLayer", typeof(RectTransform));
            go.transform.SetParent(root.transform, false);
            var rect = go.GetComponent<RectTransform>();
            BaUiWidgets.StretchFull(rect);

            var popupCanvas = go.AddComponent<Canvas>();
            popupCanvas.overrideSorting = true;
            popupCanvas.sortingOrder = rootCanvas.sortingOrder + PopupLayerOffset;
            go.AddComponent<GraphicRaycaster>();

            PopupLayers[id] = go.transform;
            return go.transform;
        }

        public static void ReleasePopupLayer(GameObject root)
        {
            if (root == null)
                return;

            var id = root.GetInstanceID();
            if (!PopupLayers.TryGetValue(id, out var layer))
                return;

            PopupLayers.Remove(id);
            if (layer != null)
                Object.Destroy(layer.gameObject);
        }

        public static void BringToPopupLayer(Transform target, GameObject root)
        {
            if (target == null || root == null)
                return;

            var layer = GetOrCreatePopupLayer(root);
            if (layer == null)
                return;

            target.SetParent(layer, true);
            target.SetAsLastSibling();
        }
    }
}
