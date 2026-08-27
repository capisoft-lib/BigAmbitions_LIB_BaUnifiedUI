using System;
using BigAmbitions.Mods;
using UnityEngine;

namespace Capisoft.Lib.BaUnifiedUI.Options
{
    /// <summary>Visible strings used by the custom color picker row.</summary>
    public sealed class BaColorPickerUiText
    {
        public static BaColorPickerUiText Default { get; } =
            new BaColorPickerUiText("Color picker unavailable");

        public string Unavailable { get; }

        public BaColorPickerUiText(string unavailable)
        {
            Unavailable = string.IsNullOrWhiteSpace(unavailable)
                ? "Color picker unavailable"
                : unavailable;
        }
    }

    /// <summary>
    /// A persistable custom option rendered by the game's Mods options view and
    /// edited through Big Ambitions' native HSV color picker.
    /// </summary>
    public sealed class BaColorPickerOption : ModOption, IPersistableOption
    {
        public Color DefaultColor { get; }

        public Action<Color> OnValueChanged { get; }

        public BaColorPickerUiText UiText { get; }

        public BaColorPickerHandle Handle { get; }

        public BaColorPickerOption(
            string id,
            string label,
            Color defaultColor,
            Action<Color> onValueChanged = null,
            BaColorPickerUiText uiText = null)
            : base(ValidateId(id), label)
        {
            DefaultColor = BaColorPreference.Canonicalize(defaultColor);
            OnValueChanged = onValueChanged;
            UiText = uiText ?? BaColorPickerUiText.Default;
            Handle = new BaColorPickerHandle(this);
        }

        public override void SpawnUi(Transform parent, string modId)
        {
            if (parent == null)
            {
                Debug.LogError(
                    "[LIB_BaUnifiedUI] Cannot spawn color option '" + Id + "': parent is null.");
                return;
            }

            var root = new GameObject("BaColorPickerOption_" + Id, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var control = root.AddComponent<BaColorPickerOptionControl>();
            control.Initialize(this, modId);
        }

        internal void NotifyValueChanged(Color color)
        {
            try
            {
                OnValueChanged?.Invoke(color);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static string ValidateId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(
                    "A color picker option needs a non-empty persistent id.",
                    nameof(id));
            return id;
        }
    }

    /// <summary>Fluent color picker extensions for the official <see cref="ModOptions"/> API.</summary>
    public static class BaColorPickerModOptionsExtensions
    {
        public static ModOptions AddColorPicker(
            this ModOptions options,
            string id,
            string label,
            Color defaultColor,
            Action<Color> onValueChanged = null,
            BaColorPickerUiText uiText = null)
        {
            EnsureOptions(options);
            return options.AddCustom(
                new BaColorPickerOption(id, label, defaultColor, onValueChanged, uiText));
        }

        public static ModOptions AddColorPicker(
            this ModOptions options,
            string id,
            string label,
            Color defaultColor,
            out BaColorPickerHandle handle,
            Action<Color> onValueChanged = null,
            BaColorPickerUiText uiText = null)
        {
            EnsureOptions(options);
            var option = new BaColorPickerOption(
                id,
                label,
                defaultColor,
                onValueChanged,
                uiText);
            handle = option.Handle;
            return options.AddCustom(option);
        }

        public static ModOptions AddColor(
            this ModOptions options,
            string id,
            string label,
            Color defaultColor,
            Action<Color> onValueChanged = null,
            BaColorPickerUiText uiText = null) =>
            AddColorPicker(options, id, label, defaultColor, onValueChanged, uiText);

        public static ModOptions AddColor(
            this ModOptions options,
            string id,
            string label,
            Color defaultColor,
            out BaColorPickerHandle handle,
            Action<Color> onValueChanged = null,
            BaColorPickerUiText uiText = null) =>
            AddColorPicker(options, id, label, defaultColor, out handle, onValueChanged, uiText);

        private static void EnsureOptions(ModOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
        }
    }
}
