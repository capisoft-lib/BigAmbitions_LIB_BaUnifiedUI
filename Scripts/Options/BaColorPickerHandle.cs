using System;
using System.Globalization;
using UnityEngine;

namespace Capisoft.Lib.BaUnifiedUI.Options
{
    /// <summary>Runtime access to a registered color picker option.</summary>
    public sealed class BaColorPickerHandle
    {
        private readonly BaColorPickerOption _option;
        private Color _color;
        private string _loadedModId;
        private string _runtimeModId;
        private bool _hasLoaded;
        private bool _previewActive;
        private Color _previewInitialColor;

        internal BaColorPickerHandle(BaColorPickerOption option)
        {
            _option = option ?? throw new ArgumentNullException(nameof(option));
            _color = option.DefaultColor;
        }

        public event Action<Color> ColorChanged;

        public string ModId => !string.IsNullOrEmpty(_runtimeModId)
            ? _runtimeModId
            : _option.ModId;

        public string OptionId => _option.Id;

        public Color DefaultColor => _option.DefaultColor;

        /// <summary>The persisted color, or the declared default before registration.</summary>
        public Color Color
        {
            get
            {
                EnsureLoaded();
                return _color;
            }
        }

        /// <summary>Alias suitable for generic settings code.</summary>
        public Color Value => Color;

        /// <summary>Changes and persists the selected color.</summary>
        public bool SetColor(Color color)
        {
            if (string.IsNullOrEmpty(ModId))
                return false;

            EnsureLoaded();
            SetColorCore(color, persist: true, notifyOption: true);
            return true;
        }

        public bool SetValue(Color color) => SetColor(color);

        public bool ResetToDefault() => SetColor(DefaultColor);

        internal void ReloadForUi(string modId)
        {
            if (!string.Equals(_runtimeModId, modId, StringComparison.Ordinal))
            {
                _runtimeModId = modId;
                _hasLoaded = false;
            }

            if (LoadFromPreferences(force: true))
                _option.NotifyValueChanged(_color);
        }

        internal void BeginPreview()
        {
            EnsureLoaded();
            if (_previewActive)
                return;

            _previewInitialColor = _color;
            _previewActive = true;
        }

        internal void PreviewColor(Color color)
        {
            if (!_previewActive)
                BeginPreview();

            SetColorCore(color, persist: false, notifyOption: false);
        }

        internal void CommitPreview()
        {
            if (!_previewActive)
                return;

            var changed = !BaColorPreference.AreEqual(_previewInitialColor, _color);
            _previewActive = false;
            if (!changed)
                return;

            PersistColor(_color);
            _option.NotifyValueChanged(_color);
        }

        private void EnsureLoaded()
        {
            var modId = ModId;
            if (string.IsNullOrEmpty(modId))
            {
                _color = _option.DefaultColor;
                return;
            }

            if (!_hasLoaded || !string.Equals(_loadedModId, modId, StringComparison.Ordinal))
                LoadFromPreferences(force: false);
        }

        private bool LoadFromPreferences(bool force)
        {
            var modId = ModId;
            if (string.IsNullOrEmpty(modId))
                return false;
            if (!force && _hasLoaded && string.Equals(_loadedModId, modId, StringComparison.Ordinal))
                return false;

            var previous = _color;
            var loaded = _option.DefaultColor;
            var prefsKey = BuildPrefsKey(modId, OptionId);
            if (UnityEngine.PlayerPrefs.HasKey(prefsKey))
            {
                var serialized = UnityEngine.PlayerPrefs.GetString(prefsKey, string.Empty);
                if (!BaColorPreference.TryDeserialize(serialized, out loaded))
                {
                    loaded = _option.DefaultColor;
                    Debug.LogWarning(
                        "[LIB_BaUnifiedUI] Ignoring invalid color preference '" + prefsKey + "'.");
                }
            }

            _color = loaded;
            _loadedModId = modId;
            _hasLoaded = true;
            var changed = !BaColorPreference.AreEqual(previous, loaded);
            if (changed)
                RaiseColorChanged(loaded);
            return changed;
        }

        private bool SetColorCore(Color color, bool persist, bool notifyOption)
        {
            var canonical = BaColorPreference.Canonicalize(color);
            var changed = !BaColorPreference.AreEqual(_color, canonical);
            if (!changed)
                return false;

            _color = canonical;
            _loadedModId = ModId;
            _hasLoaded = true;

            if (persist)
                PersistColor(canonical);

            RaiseColorChanged(canonical);

            if (notifyOption)
                _option.NotifyValueChanged(canonical);

            return true;
        }

        private void PersistColor(Color color)
        {
            var modId = ModId;
            if (string.IsNullOrEmpty(modId))
                return;

            UnityEngine.PlayerPrefs.SetString(
                BuildPrefsKey(modId, OptionId),
                BaColorPreference.Serialize(color));
        }

        private void RaiseColorChanged(Color color)
        {
            var handlers = ColorChanged;
            if (handlers == null)
                return;

            foreach (Action<Color> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(color);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private static string BuildPrefsKey(string modId, string optionId) =>
            "m:" + modId + ":" + optionId;
    }

    internal static class BaColorPreference
    {
        internal static Color Canonicalize(Color color) => (Color)(Color32)color;

        internal static bool AreEqual(Color left, Color right)
        {
            var leftBytes = (Color32)left;
            var rightBytes = (Color32)right;
            return leftBytes.r == rightBytes.r
                   && leftBytes.g == rightBytes.g
                   && leftBytes.b == rightBytes.b
                   && leftBytes.a == rightBytes.a;
        }

        internal static string Serialize(Color color)
        {
            var bytes = (Color32)Canonicalize(color);
            return string.Format(
                CultureInfo.InvariantCulture,
                "#{0:X2}{1:X2}{2:X2}{3:X2}",
                bytes.r,
                bytes.g,
                bytes.b,
                bytes.a);
        }

        internal static string ToDisplayString(Color color)
        {
            var bytes = (Color32)Canonicalize(color);
            return bytes.a == byte.MaxValue
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "#{0:X2}{1:X2}{2:X2}",
                    bytes.r,
                    bytes.g,
                    bytes.b)
                : Serialize(bytes);
        }

        internal static bool TryDeserialize(string serialized, out Color color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(serialized))
                return false;

            var hex = serialized.Trim();
            if (hex.StartsWith("#", StringComparison.Ordinal))
                hex = hex.Substring(1);
            if (hex.Length != 6 && hex.Length != 8)
                return false;

            if (!TryParseByte(hex, 0, out var red)
                || !TryParseByte(hex, 2, out var green)
                || !TryParseByte(hex, 4, out var blue))
                return false;

            var alpha = byte.MaxValue;
            if (hex.Length == 8 && !TryParseByte(hex, 6, out alpha))
                return false;

            color = (Color)new Color32(red, green, blue, alpha);
            return true;
        }

        private static bool TryParseByte(string hex, int offset, out byte value)
        {
            return byte.TryParse(
                hex.Substring(offset, 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out value);
        }
    }
}
