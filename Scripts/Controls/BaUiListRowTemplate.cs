using Capisoft.Lib.BaUnifiedUI.Layout;
using UnityEngine;

namespace Capisoft.Lib.BaUnifiedUI.Controls
{
    public sealed class BaUiListRowTemplate
    {
        public BaUiListRowRecipe Recipe { get; set; }
        public float HorizontalInset { get; set; }
    }

    /// <summary>Fluent factory for map/history list row templates.</summary>
    public sealed class BaUiListRowTemplateBuilder
    {
        private readonly BaUiListRowRecipe _recipe;
        private float _horizontalInset;

        internal BaUiListRowTemplateBuilder(BaUiListRowRecipe recipe) => _recipe = recipe;

        public BaUiListRowTemplateBuilder InScroll()
        {
            _horizontalInset = 0f;
            return this;
        }

        public BaUiListRowTemplateBuilder OnPanel(float horizontalInset)
        {
            _horizontalInset = horizontalInset;
            return this;
        }

        public BaUiListRowTemplate Build() =>
            new BaUiListRowTemplate { Recipe = _recipe, HorizontalInset = _horizontalInset };

        public BaUiListRow Create(RectTransform parent, float textScale, string name, float rowTop = 0f) =>
            BaUiListRows.Create(parent, name, textScale, _recipe, _horizontalInset, rowTop);

        public BaUiListRow CreateAndBind(
            RectTransform parent,
            float textScale,
            string name,
            System.Action<BaUiListRowBinder> configure,
            float rowTop = 0f)
        {
            var row = Create(parent, textScale, name, rowTop);
            var binder = new BaUiListRowBinder(row);
            configure(binder);
            binder.Apply();
            return row;
        }
    }
}
