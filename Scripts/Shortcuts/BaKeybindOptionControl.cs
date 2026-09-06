using Capisoft.Lib.BaUnifiedUI.Assets;
using Localizor.LanguageChangeEvent;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Shortcuts
{
    internal sealed class BaKeybindOptionControl : MonoBehaviour
    {
        // The Mods options prefabs are authored at twice their final on-screen size.
        // Custom options share the same canvas, so use the vanilla reference scale here
        // instead of scaling the transform (which would break the parent layout group).
        private const float RowHeight = 100f;
        private const float ButtonHeight = 60f;
        private const float BindingButtonWidth = 620f;
        private const float ResetButtonWidth = 60f;
        private const float ResetIconSize = 38f;
        private const float OptionLabelMinWidth = 280f;
        private const float OptionLabelFontSize = 36f;
        private const float ButtonLabelFontSize = 30f;
        private const float ButtonLabelMinFontSize = 20f;
        private const int HorizontalPadding = 8;
        private const int VerticalPadding = 20;
        private const float ControlSpacing = 12f;
        private const float ButtonLabelHorizontalPadding = 16f;

        private BaKeybindOption _option;
        private Button _bindingButton;
        private Button _resetButton;
        private Image _bindingGraphic;
        private TextMeshProUGUI _bindingLabel;
        private bool _capturing;
        private bool _initialized;
        private int _captureStartedFrame;
        private float _rejectedUntil;
        private BaKeybind _rejectedBinding;

        internal void Initialize(BaKeybindOption option, string modId)
        {
            _option = option;
            BaUiAssets.EnsureInitialized();
            BuildUi();

            _option.Handle.BindingChanged += OnBindingChanged;
            BaKeybindRegistry.ConflictsChanged += OnConflictsChanged;
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

            _bindingButton = CreateButton(
                transform,
                "BindingButton",
                BindingButtonWidth,
                ButtonHeight,
                out _bindingGraphic,
                out _bindingLabel);
            _bindingButton.onClick.AddListener(BeginCapture);
            _bindingLabel.enableAutoSizing = true;
            _bindingLabel.fontSizeMin = ButtonLabelMinFontSize;
            _bindingLabel.fontSizeMax = ButtonLabelFontSize;
            _bindingLabel.alignment = TextAlignmentOptions.MidlineLeft;

            _resetButton = CreateButton(
                transform,
                "ResetButton",
                ResetButtonWidth,
                ButtonHeight,
                out var resetGraphic,
                out var resetLabel);
            BaUiAssets.ApplyButtonRed(resetGraphic);
            resetLabel.gameObject.SetActive(false);
            CreateResetIcon(_resetButton.transform, resetGraphic);
            _resetButton.onClick.AddListener(ResetBinding);
        }

        private void Update()
        {
            if (!_capturing)
                return;

            if (_rejectedUntil > 0f && Time.unscaledTime >= _rejectedUntil)
            {
                _rejectedUntil = 0f;
                UpdateVisual();
            }

            if (Time.frameCount <= _captureStartedFrame)
                return;

            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                StopCapture();
                return;
            }

            if (keyboard.backspaceKey.wasPressedThisFrame || keyboard.deleteKey.wasPressedThisFrame)
            {
                if (_option.Handle.TrySetBinding(BaKeybind.Unbound))
                    StopCapture();
                return;
            }

            foreach (var keyControl in keyboard.allKeys)
            {
                if (keyControl == null)
                    continue;
                var key = keyControl.keyCode;
                if (!keyControl.wasPressedThisFrame
                    || key == Key.None
                    || key == Key.Escape
                    || key == Key.Backspace
                    || key == Key.Delete
                    || BaKeybind.IsModifierKey(key))
                    continue;

                var candidate = new BaKeybind(key, BaKeybind.ReadCurrentModifiers(keyboard));
                if (_option.Handle.TrySetBinding(candidate, out var conflict))
                {
                    StopCapture();
                }
                else if (conflict.HasConflict)
                {
                    _rejectedBinding = candidate;
                    _rejectedUntil = Time.unscaledTime + 1.5f;
                    UpdateVisual();
                }
                return;
            }
        }

        private void BeginCapture()
        {
            if (_capturing)
            {
                StopCapture();
                return;
            }

            BaKeybindCaptureCoordinator.Begin(this);
            _capturing = true;
            _captureStartedFrame = Time.frameCount;
            _rejectedUntil = 0f;
            UpdateVisual();
        }

        private void ResetBinding()
        {
            StopCapture();
            _option.Handle.TrySetBinding(_option.DefaultBinding);
            UpdateVisual();
        }

        private void StopCapture()
        {
            if (!_capturing)
                return;

            _capturing = false;
            _rejectedUntil = 0f;
            BaKeybindCaptureCoordinator.Release(this);
            UpdateVisual();
        }

        internal void CancelCaptureFromCoordinator()
        {
            _capturing = false;
            _rejectedUntil = 0f;
            UpdateVisual();
        }

        private void OnBindingChanged(BaKeybind _) => UpdateVisual();

        private void OnConflictsChanged() => UpdateVisual();

        private void UpdateVisual()
        {
            if (!_initialized || _bindingLabel == null)
                return;

            if (_capturing)
            {
                if (_rejectedUntil > Time.unscaledTime)
                {
                    _bindingLabel.text = _option.UiText.ConflictPrefix + ": "
                                         + _rejectedBinding.ToDisplayString(_option.UiText.Unbound);
                    _bindingLabel.color = Color.white;
                    BaUiAssets.ApplyButtonRed(_bindingGraphic);
                }
                else
                {
                    _bindingLabel.text = _option.UiText.CapturePrompt;
                    _bindingLabel.color = Color.white;
                    BaUiAssets.ApplyButtonBlue(_bindingGraphic);
                }
                return;
            }

            var binding = _option.Handle.Binding;
            var conflict = _option.Handle.Conflict;
            if (conflict.HasConflict)
            {
                _bindingLabel.text = _option.UiText.ConflictPrefix + ": "
                                     + binding.ToDisplayString(_option.UiText.Unbound);
                _bindingLabel.color = Color.white;
                BaUiAssets.ApplyButtonRed(_bindingGraphic);
            }
            else
            {
                _bindingLabel.text = binding.ToDisplayString(_option.UiText.Unbound);
                _bindingLabel.color = BaUiAssets.KeybindFieldTextColor;
                BaUiAssets.ApplyKeybindField(_bindingGraphic);
            }
        }

        private void OnDisable()
        {
            if (_capturing)
                StopCapture();
        }

        private void OnDestroy()
        {
            BaKeybindCaptureCoordinator.Release(this);
            if (!_initialized || _option == null)
                return;

            _option.Handle.BindingChanged -= OnBindingChanged;
            BaKeybindRegistry.ConflictsChanged -= OnConflictsChanged;
        }

        private static RectTransform CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            float width,
            float height,
            out Image graphic,
            out TextMeshProUGUI label)
        {
            var root = CreateRect(parent, name);
            var layout = root.gameObject.AddComponent<LayoutElement>();
            layout.minWidth = width;
            layout.preferredWidth = width;
            layout.minHeight = height;
            layout.preferredHeight = height;

            graphic = BaUiAssets.CreateButtonGraphic(root, 1f, BaUiAssets.ApplyButtonGrey, bleedBottom: false);
            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = graphic;

            var labelRoot = CreateRect(root, "Label");
            labelRoot.anchorMin = Vector2.zero;
            labelRoot.anchorMax = Vector2.one;
            labelRoot.offsetMin = new Vector2(ButtonLabelHorizontalPadding, 0f);
            labelRoot.offsetMax = new Vector2(-ButtonLabelHorizontalPadding, 0f);
            label = labelRoot.gameObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = ButtonLabelFontSize;
            label.fontStyle = FontStyles.Bold;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.raycastTarget = false;
            BaUiAssets.ApplyButtonFont(label);
            return button;
        }

        private static void CreateResetIcon(Transform parent, Image buttonGraphic)
        {
            var iconRoot = CreateRect(parent, "ResetIcon");
            iconRoot.anchorMin = new Vector2(0.5f, 0.5f);
            iconRoot.anchorMax = new Vector2(0.5f, 0.5f);
            iconRoot.pivot = new Vector2(0.5f, 0.5f);
            iconRoot.anchoredPosition = Vector2.zero;
            iconRoot.sizeDelta = new Vector2(ResetIconSize, ResetIconSize);

            var icon = iconRoot.gameObject.AddComponent<Image>();
            BaUiAssets.ApplyResetIcon(icon);
            BaUiAssets.ConfigureOverlayIcon(icon, buttonGraphic);
        }
    }

    internal static class BaKeybindCaptureCoordinator
    {
        private static BaKeybindOptionControl _active;

        internal static bool IsCaptureActive => _active != null;

        internal static void Begin(BaKeybindOptionControl control)
        {
            if (_active == control)
                return;

            var previous = _active;
            _active = control;
            previous?.CancelCaptureFromCoordinator();
        }

        internal static void Release(BaKeybindOptionControl control)
        {
            if (_active == control)
                _active = null;
        }

        internal static void CancelActive()
        {
            var active = _active;
            _active = null;
            active?.CancelCaptureFromCoordinator();
        }
    }
}
