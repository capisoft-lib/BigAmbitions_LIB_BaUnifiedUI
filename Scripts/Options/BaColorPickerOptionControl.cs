using System;
using Capisoft.Lib.BaUnifiedUI.Assets;
using Localizor.LanguageChangeEvent;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Options
{
    internal sealed class BaColorPickerOptionControl : MonoBehaviour
    {
        // Mods options prefabs are authored at twice their final on-screen size.
        private const float RowHeight = 100f;
        private const float ButtonHeight = 60f;
        private const float ColorButtonWidth = 620f;
        private const float ResetButtonWidth = 60f;
        private const float ResetIconSize = 38f;
        private const float OptionLabelMinWidth = 280f;
        private const float OptionLabelFontSize = 36f;
        private const float ValueLabelFontSize = 30f;
        private const float SwatchFrameSize = 46f;
        private const float SwatchInset = 4f;
        private const int HorizontalPadding = 8;
        private const int VerticalPadding = 20;
        private const float ControlSpacing = 12f;
        private const float FieldHorizontalPadding = 16f;
        private const int PickerCanvasSortOrder = 12000;

        private static BaColorPickerOptionControl _activePickerControl;

        private BaColorPickerOption _option;
        private Button _colorButton;
        private Image _colorButtonGraphic;
        private Image _swatch;
        private TextMeshProUGUI _valueLabel;
        private bool _initialized;
        private bool _pickerOpen;
        private float _unavailableUntil;
        private CustomColorPicker _nativePicker;
        private Canvas _pickerCanvas;
        private int _pickerCanvasOriginalSortOrder;
        private bool _pickerCanvasOrderChanged;

        internal void Initialize(BaColorPickerOption option, string modId)
        {
            _option = option;
            BaUiAssets.EnsureInitialized();
            BuildUi();

            _option.Handle.ColorChanged += OnColorChanged;
            _initialized = true;
            _option.Handle.ReloadForUi(modId);
            UpdateVisual();
        }

        private void BuildUi()
        {
            var root = (RectTransform)transform;
            root.sizeDelta = new Vector2(0f, RowHeight);

            var rootLayout = gameObject.AddComponent<LayoutElement>();
            rootLayout.minHeight = RowHeight;
            rootLayout.preferredHeight = RowHeight;
            rootLayout.flexibleWidth = 1f;

            var row = gameObject.AddComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(
                HorizontalPadding,
                HorizontalPadding,
                VerticalPadding,
                VerticalPadding);
            row.spacing = ControlSpacing;
            row.childAlignment = TextAnchor.MiddleLeft;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            var optionLabelGo = CreateRect(transform, "OptionLabel");
            var optionLabelLayout = optionLabelGo.gameObject.AddComponent<LayoutElement>();
            optionLabelLayout.minWidth = OptionLabelMinWidth;
            optionLabelLayout.flexibleWidth = 1f;

            var optionLabel = optionLabelGo.gameObject.AddComponent<TextMeshProUGUI>();
            optionLabel.text = _option.Label;
            optionLabel.fontSize = OptionLabelFontSize;
            optionLabel.fontStyle = FontStyles.Bold;
            optionLabel.color = BaUiAssets.BodyTextColor;
            optionLabel.alignment = TextAlignmentOptions.MidlineLeft;
            optionLabel.enableWordWrapping = false;
            optionLabel.overflowMode = TextOverflowModes.Ellipsis;
            optionLabel.raycastTarget = false;
            BaUiAssets.ApplyButtonFont(optionLabel);
            var localization = optionLabelGo.gameObject.AddComponent<TextLocalizationComponent>();
            localization.Key = _option.Label;

            _colorButton = CreateColorButton(transform);
            _colorButton.onClick.AddListener(OpenNativePicker);

            var resetButton = CreateResetButton(transform);
            resetButton.onClick.AddListener(ResetColor);
        }

        private Button CreateColorButton(Transform parent)
        {
            var root = CreateRect(parent, "ColorButton");
            var layout = root.gameObject.AddComponent<LayoutElement>();
            layout.minWidth = ColorButtonWidth;
            layout.preferredWidth = ColorButtonWidth;
            layout.minHeight = ButtonHeight;
            layout.preferredHeight = ButtonHeight;

            _colorButtonGraphic = BaUiAssets.CreateButtonGraphic(
                root,
                1f,
                BaUiAssets.ApplyKeybindField,
                bleedBottom: false);
            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = _colorButtonGraphic;

            var swatchFrame = CreateRect(root, "SwatchFrame");
            swatchFrame.anchorMin = new Vector2(0f, 0.5f);
            swatchFrame.anchorMax = new Vector2(0f, 0.5f);
            swatchFrame.pivot = new Vector2(0f, 0.5f);
            swatchFrame.anchoredPosition = new Vector2(FieldHorizontalPadding, 0f);
            swatchFrame.sizeDelta = new Vector2(SwatchFrameSize, SwatchFrameSize);
            var frameImage = swatchFrame.gameObject.AddComponent<Image>();
            frameImage.color = Color.white;
            frameImage.raycastTarget = false;

            var swatchRoot = CreateRect(swatchFrame, "Swatch");
            swatchRoot.anchorMin = Vector2.zero;
            swatchRoot.anchorMax = Vector2.one;
            swatchRoot.offsetMin = new Vector2(SwatchInset, SwatchInset);
            swatchRoot.offsetMax = new Vector2(-SwatchInset, -SwatchInset);
            _swatch = swatchRoot.gameObject.AddComponent<Image>();
            _swatch.raycastTarget = false;

            var labelRoot = CreateRect(root, "Value");
            labelRoot.anchorMin = Vector2.zero;
            labelRoot.anchorMax = Vector2.one;
            labelRoot.offsetMin = new Vector2(
                FieldHorizontalPadding + SwatchFrameSize + FieldHorizontalPadding,
                0f);
            labelRoot.offsetMax = new Vector2(-FieldHorizontalPadding, 0f);
            _valueLabel = labelRoot.gameObject.AddComponent<TextMeshProUGUI>();
            _valueLabel.fontSize = ValueLabelFontSize;
            _valueLabel.fontStyle = FontStyles.Bold;
            _valueLabel.color = BaUiAssets.KeybindFieldTextColor;
            _valueLabel.alignment = TextAlignmentOptions.MidlineLeft;
            _valueLabel.enableWordWrapping = false;
            _valueLabel.overflowMode = TextOverflowModes.Ellipsis;
            _valueLabel.raycastTarget = false;
            BaUiAssets.ApplyButtonFont(_valueLabel);
            return button;
        }

        private static Button CreateResetButton(Transform parent)
        {
            var root = CreateRect(parent, "ResetButton");
            var layout = root.gameObject.AddComponent<LayoutElement>();
            layout.minWidth = ResetButtonWidth;
            layout.preferredWidth = ResetButtonWidth;
            layout.minHeight = ButtonHeight;
            layout.preferredHeight = ButtonHeight;

            var graphic = BaUiAssets.CreateButtonGraphic(
                root,
                1f,
                BaUiAssets.ApplyButtonRed,
                bleedBottom: false);
            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = graphic;

            var iconRoot = CreateRect(root, "ResetIcon");
            iconRoot.anchorMin = new Vector2(0.5f, 0.5f);
            iconRoot.anchorMax = new Vector2(0.5f, 0.5f);
            iconRoot.pivot = new Vector2(0.5f, 0.5f);
            iconRoot.anchoredPosition = Vector2.zero;
            iconRoot.sizeDelta = new Vector2(ResetIconSize, ResetIconSize);

            var icon = iconRoot.gameObject.AddComponent<Image>();
            BaUiAssets.ApplyResetIcon(icon);
            BaUiAssets.ConfigureOverlayIcon(icon, graphic);
            return button;
        }

        private void Update()
        {
            if (_unavailableUntil > 0f && Time.unscaledTime >= _unavailableUntil)
            {
                _unavailableUntil = 0f;
                UpdateVisual();
            }

            if (!_pickerOpen)
                return;

            try
            {
                if (CustomColorPicker.isOpen)
                    return;
            }
            catch
            {
                // A scene transition can temporarily unload the native picker.
            }

            ReleasePicker();
        }

        private void OpenNativePicker()
        {
            _unavailableUntil = 0f;
            UpdateVisual();

            try
            {
                if (!UIs.IsInitialized || UIs.Instance?.customColorPicker == null)
                {
                    ShowPickerUnavailable();
                    return;
                }

                if (CustomColorPicker.isOpen)
                    return;

                _nativePicker = UIs.Instance.customColorPicker;
                _activePickerControl = this;
                _pickerOpen = true;
                _option.Handle.BeginPreview();
                _nativePicker.Open(OnNativeColorChanged, _option.Handle.Color);
                BringPickerToFront(_nativePicker);
            }
            catch (Exception ex)
            {
                ReleasePicker();
                Debug.LogError(
                    "[LIB_BaUnifiedUI] Native color picker failed for option '" +
                    _option.Id + "': " + ex);
                ShowPickerUnavailable(logWarning: false);
            }
        }

        private void OnNativeColorChanged(Color color)
        {
            _option.Handle.PreviewColor(color);
        }

        private void ResetColor()
        {
            CancelPicker();
            _option.Handle.ResetToDefault();
            UpdateVisual();
        }

        private void OnColorChanged(Color _) => UpdateVisual();

        private void UpdateVisual()
        {
            if (!_initialized || _valueLabel == null || _swatch == null)
                return;

            var color = _option.Handle.Color;
            _swatch.color = color;
            if (_unavailableUntil > Time.unscaledTime)
            {
                _valueLabel.text = _option.UiText.Unavailable;
                _valueLabel.color = Color.white;
                BaUiAssets.ApplyButtonRed(_colorButtonGraphic);
                return;
            }

            _valueLabel.text = BaColorPreference.ToDisplayString(color);
            _valueLabel.color = BaUiAssets.KeybindFieldTextColor;
            BaUiAssets.ApplyKeybindField(_colorButtonGraphic);
        }

        private void ShowPickerUnavailable(bool logWarning = true)
        {
            if (logWarning)
            {
                Debug.LogWarning(
                    "[LIB_BaUnifiedUI] Native color picker is unavailable for option '" +
                    _option.Id + "'.");
            }

            _unavailableUntil = Time.unscaledTime + 1.5f;
            UpdateVisual();
        }

        private void BringPickerToFront(CustomColorPicker picker)
        {
            picker.transform.SetAsLastSibling();
            _pickerCanvas = picker.GetComponentInParent<Canvas>(true);
            if (_pickerCanvas == null)
                return;

            _pickerCanvasOriginalSortOrder = _pickerCanvas.sortingOrder;
            _pickerCanvasOrderChanged = _pickerCanvasOriginalSortOrder < PickerCanvasSortOrder;
            if (_pickerCanvasOrderChanged)
                _pickerCanvas.sortingOrder = PickerCanvasSortOrder;
        }

        private void CancelPicker()
        {
            if (!_pickerOpen || _activePickerControl != this)
                return;

            try
            {
                if (_nativePicker != null && CustomColorPicker.isOpen)
                    _nativePicker.Cancel();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    "[LIB_BaUnifiedUI] Could not cancel native color picker cleanly: " + ex.Message);
            }
            finally
            {
                ReleasePicker();
            }
        }

        private void ReleasePicker()
        {
            var wasOpen = _pickerOpen;
            if (_pickerCanvasOrderChanged
                && _pickerCanvas != null
                && _pickerCanvas.sortingOrder == PickerCanvasSortOrder)
            {
                _pickerCanvas.sortingOrder = _pickerCanvasOriginalSortOrder;
            }

            _pickerCanvas = null;
            _pickerCanvasOrderChanged = false;
            _pickerOpen = false;
            _nativePicker = null;
            if (_activePickerControl == this)
                _activePickerControl = null;

            if (wasOpen && _option != null)
                _option.Handle.CommitPreview();
        }

        private void OnDisable()
        {
            CancelPicker();
        }

        private void OnDestroy()
        {
            CancelPicker();
            if (_initialized && _option != null)
                _option.Handle.ColorChanged -= OnColorChanged;
        }

        private static RectTransform CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }
    }
}
