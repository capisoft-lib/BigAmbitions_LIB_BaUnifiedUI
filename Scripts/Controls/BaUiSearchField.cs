using Capisoft.Lib.BaUnifiedUI.Assets;
using Capisoft.Lib.BaUnifiedUI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Controls
{
    public sealed class BaUiSearchField
    {
        public RectTransform Rect { get; private set; }
        public TMP_InputField Field { get; private set; }
        public TextMeshProUGUI Placeholder { get; private set; }
        public BaUiInputGuard Guard { get; private set; }

        public static BaUiSearchField Create(Transform parent, float textScale, string name = "SearchBar")
        {
            var instance = new BaUiSearchField();
            instance.Rect = BaUiWidgets.CreateRect(parent, name);

            var bgGo = BaUiWidgets.CreateRect(instance.Rect, "Background");
            BaUiWidgets.StretchFull(bgGo);
            bgGo.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);

            var textAreaGo = BaUiWidgets.CreateRect(instance.Rect, "TextArea");
            textAreaGo.anchorMin = Vector2.zero;
            textAreaGo.anchorMax = Vector2.one;
            textAreaGo.offsetMin = new Vector2(8f, 4f);
            textAreaGo.offsetMax = new Vector2(-8f, -4f);

            var placeholderGo = BaUiWidgets.CreateRect(textAreaGo, "Placeholder");
            BaUiWidgets.StretchFull(placeholderGo);
            instance.Placeholder = placeholderGo.gameObject.AddComponent<TextMeshProUGUI>();
            instance.Placeholder.fontSize = 14f * textScale;
            instance.Placeholder.color = new Color(1f, 1f, 1f, 0.45f);
            instance.Placeholder.fontStyle = FontStyles.Italic;
            instance.Placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            BaUiAssets.ApplyButtonFont(instance.Placeholder);

            var textGo = BaUiWidgets.CreateRect(textAreaGo, "Text");
            BaUiWidgets.StretchFull(textGo);
            var textLabel = textGo.gameObject.AddComponent<TextMeshProUGUI>();
            textLabel.fontSize = 14f * textScale;
            textLabel.color = BaUiAssets.BodyTextColor;
            textLabel.alignment = TextAlignmentOptions.MidlineLeft;
            BaUiAssets.ApplyButtonFont(textLabel);

            instance.Field = instance.Rect.gameObject.AddComponent<TMP_InputField>();
            instance.Field.textViewport = textAreaGo;
            instance.Field.textComponent = textLabel;
            instance.Field.placeholder = instance.Placeholder;
            instance.Field.lineType = TMP_InputField.LineType.SingleLine;

            instance.Guard = BaUiWidgets.AttachInputGuard(instance.Field);
            return instance;
        }

        public void Wire(UnityAction<string> onChanged, UnityAction onSelected = null)
        {
            Field.onValueChanged.RemoveAllListeners();
            Field.onValueChanged.AddListener(onChanged);
            Field.onSelect.RemoveAllListeners();
            if (onSelected != null)
                Field.onSelect.AddListener(_ => onSelected());
        }

        public void SetPlaceholder(string text)
        {
            if (Placeholder != null)
                Placeholder.text = text;
        }
    }
}
