using System;
using UnityEngine;

namespace Capisoft.Lib.BaUnifiedUI.Shortcuts
{
    /// <summary>
    /// Runtime access to a registered shortcut option. Poll <see cref="WasPressedThisFrame"/>
    /// from the consumer mod's Update method.
    /// </summary>
    public sealed class BaKeybindHandle : IDisposable
    {
        private readonly BaKeybindOption _option;
        private BaKeybind _binding;
        private string _loadedModId;
        private string _runtimeModId;
        private bool _hasLoaded;
        private bool _disposed;

        internal BaKeybindHandle(BaKeybindOption option)
        {
            _option = option ?? throw new ArgumentNullException(nameof(option));
            _binding = option.DefaultBinding;
            BaKeybindRegistry.Track(this);
        }

        public event Action<BaKeybind> BindingChanged;

        public string ModId => !string.IsNullOrEmpty(_runtimeModId) ? _runtimeModId : _option.ModId;

        public string OptionId => _option.Id;

        public BaKeybind DefaultBinding => _option.DefaultBinding;

        public BaKeybind Binding
        {
            get
            {
                EnsureLoaded();
                return _binding;
            }
        }

        public bool IsBound => Binding.IsBound;

        public bool IsRegistered
        {
            get
            {
                if (_disposed || !BaKeybindRegistry.IsActive(this))
                    return false;

                BaKeybindRegistry.Track(this);
                return true;
            }
        }

        public BaKeybindConflict Conflict =>
            _disposed ? BaKeybindConflict.None : BaKeybindRegistry.GetConflict(this, Binding);

        internal BaKeybindOption Option => _option;

        internal bool IsDisposed => _disposed;

        /// <summary>
        /// Returns true only on the chord's press frame. By default it yields to the game's
        /// menus/text fields, all BA Unified UI capture rows, and every known conflict.
        /// </summary>
        public bool WasPressedThisFrame(bool respectGameUi = true)
        {
            if (_disposed || !IsRegistered || BaKeybindRegistry.IsCaptureActive)
                return false;

            var binding = Binding;
            if (!binding.IsBound || BaKeybindRegistry.GetConflict(this, binding).HasConflict)
                return false;

            if (respectGameUi && ShouldYieldToGameUi())
                return false;

            return binding.WasPressedThisFrame();
        }

        /// <summary>Changes and persists the chord if it does not overlap a known binding.</summary>
        public bool TrySetBinding(BaKeybind binding) => TrySetBinding(binding, out _);

        /// <summary>Changes and persists the chord, returning the blocking owner on failure.</summary>
        public bool TrySetBinding(BaKeybind binding, out BaKeybindConflict conflict)
        {
            conflict = BaKeybindConflict.None;
            if (_disposed || !IsRegistered || string.IsNullOrEmpty(ModId))
                return false;

            EnsureLoaded();
            conflict = BaKeybindRegistry.GetConflict(this, binding);
            if (conflict.HasConflict)
                return false;

            SetBinding(binding, persist: true, notifyOption: true);
            return true;
        }

        public void Clear() => TrySetBinding(BaKeybind.Unbound);

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            BindingChanged = null;
            BaKeybindRegistry.Untrack(this);
        }

        internal BaKeybind GetBindingForRegistry()
        {
            EnsureLoaded();
            return _binding;
        }

        internal void ReloadForUi(string modId)
        {
            if (_disposed)
                return;

            if (!string.Equals(_runtimeModId, modId, StringComparison.Ordinal))
            {
                _runtimeModId = modId;
                _hasLoaded = false;
            }

            LoadFromPreferences(force: true);
            _option.NotifyValueChanged(_binding);
        }

        private void EnsureLoaded()
        {
            if (_disposed)
                return;

            var modId = ModId;
            if (string.IsNullOrEmpty(modId))
            {
                _binding = _option.DefaultBinding;
                return;
            }

            if (!_hasLoaded || !string.Equals(_loadedModId, modId, StringComparison.Ordinal))
                LoadFromPreferences(force: false);
        }

        private void LoadFromPreferences(bool force)
        {
            var modId = ModId;
            if (string.IsNullOrEmpty(modId))
                return;
            if (!force && _hasLoaded && string.Equals(_loadedModId, modId, StringComparison.Ordinal))
                return;

            var previous = _binding;
            var loaded = _option.DefaultBinding;
            var prefsKey = BuildPrefsKey(modId, OptionId);
            if (UnityEngine.PlayerPrefs.HasKey(prefsKey))
            {
                var serialized = UnityEngine.PlayerPrefs.GetString(prefsKey, string.Empty);
                if (!BaKeybind.TryParse(serialized, out loaded))
                {
                    loaded = _option.DefaultBinding;
                    Debug.LogWarning(
                        "[LIB_BaUnifiedUI] Ignoring invalid shortcut preference '" + prefsKey + "'.");
                }
            }

            _binding = loaded;
            _loadedModId = modId;
            _hasLoaded = true;
            if (previous != loaded)
            {
                RaiseBindingChanged(loaded);
                BaKeybindRegistry.NotifyBindingChanged();
            }
        }

        private void SetBinding(BaKeybind binding, bool persist, bool notifyOption)
        {
            var changed = _binding != binding;
            _binding = binding;
            _loadedModId = ModId;
            _hasLoaded = true;

            if (persist)
                UnityEngine.PlayerPrefs.SetString(BuildPrefsKey(ModId, OptionId), binding.Serialize());

            if (changed)
            {
                RaiseBindingChanged(binding);
                BaKeybindRegistry.NotifyBindingChanged();
            }

            if (notifyOption)
                _option.NotifyValueChanged(binding);
        }

        private void RaiseBindingChanged(BaKeybind binding)
        {
            var handlers = BindingChanged;
            if (handlers == null)
                return;

            foreach (Action<BaKeybind> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(binding);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private static bool ShouldYieldToGameUi()
        {
            try
            {
                return GameManager.ShouldBlockKeyboardShortcuts() || GameManager.HasInputSelected();
            }
            catch
            {
                // A city can be between managers for a frame; do not fire into that transition.
                return true;
            }
        }

        private static string BuildPrefsKey(string modId, string optionId) =>
            "m:" + modId + ":" + optionId;
    }
}
