using System;
using BigAmbitions.Mods;
using UnityEngine;

namespace Capisoft.Lib.BaUnifiedUI.Shortcuts
{
    /// <summary>Visible strings used by the custom shortcut row.</summary>
    public sealed class BaKeybindUiText
    {
        public static BaKeybindUiText Default { get; } =
            new BaKeybindUiText("Unbound", "Press a key...", "Conflict");

        public string Unbound { get; }

        public string CapturePrompt { get; }

        public string ConflictPrefix { get; }

        public BaKeybindUiText(string unbound, string capturePrompt, string conflictPrefix)
        {
            Unbound = string.IsNullOrWhiteSpace(unbound) ? "Unbound" : unbound;
            CapturePrompt = string.IsNullOrWhiteSpace(capturePrompt) ? "Press a key..." : capturePrompt;
            ConflictPrefix = string.IsNullOrWhiteSpace(conflictPrefix) ? "Conflict" : conflictPrefix;
        }
    }

    /// <summary>A persistable custom option rendered by the game's Mods options view.</summary>
    public sealed class BaKeybindOption : ModOption, IPersistableOption
    {
        public BaKeybind DefaultBinding { get; }

        public Action<BaKeybind> OnValueChanged { get; }

        public BaKeybindUiText UiText { get; }

        public BaKeybindHandle Handle { get; }

        public BaKeybindOption(
            string id,
            string label,
            BaKeybind defaultBinding,
            Action<BaKeybind> onValueChanged = null,
            BaKeybindUiText uiText = null)
            : base(ValidateId(id), label)
        {
            DefaultBinding = defaultBinding;
            OnValueChanged = onValueChanged;
            UiText = uiText ?? BaKeybindUiText.Default;
            Handle = new BaKeybindHandle(this);
        }

        public override void SpawnUi(Transform parent, string modId)
        {
            if (parent == null)
            {
                Debug.LogError("[LIB_BaUnifiedUI] Cannot spawn shortcut option '" + Id + "': parent is null.");
                return;
            }

            var root = new GameObject("BaKeybindOption_" + Id, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var control = root.AddComponent<BaKeybindOptionControl>();
            control.Initialize(this, modId);
        }

        internal void NotifyValueChanged(BaKeybind binding)
        {
            try
            {
                OnValueChanged?.Invoke(binding);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static string ValidateId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("A shortcut option needs a non-empty persistent id.", nameof(id));
            return id;
        }
    }

    /// <summary>Fluent extensions for the official <see cref="ModOptions"/> API.</summary>
    public static class BaKeybindModOptionsExtensions
    {
        public static ModOptions AddKeybind(
            this ModOptions options,
            string id,
            string label,
            BaKeybind defaultBinding,
            Action<BaKeybind> onValueChanged = null,
            BaKeybindUiText uiText = null)
        {
            EnsureOptions(options);
            return options.AddCustom(
                new BaKeybindOption(id, label, defaultBinding, onValueChanged, uiText));
        }

        public static ModOptions AddKeybind(
            this ModOptions options,
            string id,
            string label,
            BaKeybind defaultBinding,
            out BaKeybindHandle handle,
            Action<BaKeybind> onValueChanged = null,
            BaKeybindUiText uiText = null)
        {
            EnsureOptions(options);
            var option = new BaKeybindOption(id, label, defaultBinding, onValueChanged, uiText);
            handle = option.Handle;
            return options.AddCustom(option);
        }

        public static ModOptions AddShortcut(
            this ModOptions options,
            string id,
            string label,
            BaKeybind defaultBinding,
            Action<BaKeybind> onValueChanged = null,
            BaKeybindUiText uiText = null) =>
            AddKeybind(options, id, label, defaultBinding, onValueChanged, uiText);

        public static ModOptions AddShortcut(
            this ModOptions options,
            string id,
            string label,
            BaKeybind defaultBinding,
            out BaKeybindHandle handle,
            Action<BaKeybind> onValueChanged = null,
            BaKeybindUiText uiText = null) =>
            AddKeybind(options, id, label, defaultBinding, out handle, onValueChanged, uiText);

        private static void EnsureOptions(ModOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
        }
    }
}
