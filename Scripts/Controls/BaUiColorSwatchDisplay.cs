using Capisoft.Lib.BaUnifiedUI.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Controls
{
    /// <summary>Single read-only color square for settings rows.</summary>
    public sealed class BaUiColorSwatchDisplay
    {
        private Image _image;

        public Image Image => _image;

        private BaUiColorSwatchDisplay()
        {
        }

        internal static BaUiColorSwatchDisplay Create(RectTransform parent, Color color)
        {
            var display = new BaUiColorSwatchDisplay();
            var swatchRt = BaUiWidgets.CreateRect(parent, "Swatch");
            BaUiWidgets.StretchFull(swatchRt);

            display._image = swatchRt.gameObject.AddComponent<Image>();
            display._image.color = color;
            display._image.raycastTarget = false;

            var outline = swatchRt.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.55f);
            outline.effectDistance = new Vector2(1f, -1f);

            var le = parent.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = BaUiSettingsMetrics.SwatchSize;
            le.preferredHeight = BaUiSettingsMetrics.SwatchSize;
            le.minWidth = BaUiSettingsMetrics.SwatchSize;
            le.minHeight = BaUiSettingsMetrics.SwatchSize;

            return display;
        }

        public void SetColor(Color color)
        {
            if (_image != null)
                _image.color = color;
        }
    }
}
