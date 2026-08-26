using System;
using System.Globalization;
using UnityEngine.InputSystem;

namespace Capisoft.Lib.BaUnifiedUI.Shortcuts
{
    /// <summary>Normalized modifier groups supported by a BA Unified UI keyboard shortcut.</summary>
    [Flags]
    public enum BaKeyModifiers
    {
        None = 0,
        Control = 1 << 0,
        Shift = 1 << 1,
        Alt = 1 << 2,
        Command = 1 << 3
    }

    /// <summary>An immutable keyboard chord made of one primary key and zero or more modifiers.</summary>
    public readonly struct BaKeybind : IEquatable<BaKeybind>
    {
        private const int SerializationVersion = 1;
        private const BaKeyModifiers AllModifiers = BaKeyModifiers.Control
                                                        | BaKeyModifiers.Shift
                                                        | BaKeyModifiers.Alt
                                                        | BaKeyModifiers.Command;

        public static BaKeybind Unbound => default;

        public Key PrimaryKey { get; }

        public BaKeyModifiers Modifiers { get; }

        public bool IsBound => PrimaryKey != Key.None;

        public BaKeybind(Key primaryKey, BaKeyModifiers modifiers = BaKeyModifiers.None)
        {
            if (!Enum.IsDefined(typeof(Key), primaryKey))
                throw new ArgumentOutOfRangeException(nameof(primaryKey), primaryKey, "Unknown Input System key.");
            if ((modifiers & ~AllModifiers) != 0)
                throw new ArgumentOutOfRangeException(nameof(modifiers), modifiers, "Unknown shortcut modifier.");

            if (primaryKey == Key.None)
            {
                PrimaryKey = Key.None;
                Modifiers = BaKeyModifiers.None;
                return;
            }

            if (IsModifierKey(primaryKey))
                throw new ArgumentException("A shortcut needs a non-modifier primary key.", nameof(primaryKey));

            PrimaryKey = primaryKey;
            Modifiers = modifiers;
        }

        public static BaKeybind Ctrl(Key primaryKey) =>
            new BaKeybind(primaryKey, BaKeyModifiers.Control);

        public static BaKeybind Shift(Key primaryKey) =>
            new BaKeybind(primaryKey, BaKeyModifiers.Shift);

        public static BaKeybind Alt(Key primaryKey) =>
            new BaKeybind(primaryKey, BaKeyModifiers.Alt);

        /// <summary>Stable PlayerPrefs representation. Do not use <see cref="ToString"/> for persistence.</summary>
        public string Serialize() =>
            SerializationVersion.ToString(CultureInfo.InvariantCulture)
            + "|" + ((int)Modifiers).ToString(CultureInfo.InvariantCulture)
            + "|" + ((int)PrimaryKey).ToString(CultureInfo.InvariantCulture);

        public static bool TryParse(string value, out BaKeybind binding)
        {
            binding = Unbound;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var parts = value.Split('|');
            if (parts.Length != 3
                || !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var version)
                || version != SerializationVersion
                || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var modifiersValue)
                || !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var keyValue))
                return false;

            var key = (Key)keyValue;
            var modifiers = (BaKeyModifiers)modifiersValue;
            try
            {
                binding = new BaKeybind(key, modifiers);
                return true;
            }
            catch (ArgumentException)
            {
                binding = Unbound;
                return false;
            }
        }

        public string ToDisplayString(string unboundText = "Unbound")
        {
            if (!IsBound)
                return unboundText ?? string.Empty;

            var prefix = string.Empty;
            if ((Modifiers & BaKeyModifiers.Control) != 0)
                prefix += "Ctrl + ";
            if ((Modifiers & BaKeyModifiers.Shift) != 0)
                prefix += "Shift + ";
            if ((Modifiers & BaKeyModifiers.Alt) != 0)
                prefix += "Alt + ";
            if ((Modifiers & BaKeyModifiers.Command) != 0)
                prefix += "Cmd + ";
            return prefix + GetPrimaryKeyDisplayName(PrimaryKey);
        }

        public override string ToString() => ToDisplayString();

        public bool Equals(BaKeybind other) =>
            PrimaryKey == other.PrimaryKey && Modifiers == other.Modifiers;

        public override bool Equals(object obj) => obj is BaKeybind other && Equals(other);

        public override int GetHashCode() => ((int)PrimaryKey * 397) ^ (int)Modifiers;

        public static bool operator ==(BaKeybind left, BaKeybind right) => left.Equals(right);

        public static bool operator !=(BaKeybind left, BaKeybind right) => !left.Equals(right);

        public static bool IsModifierKey(Key key) =>
            key == Key.LeftCtrl || key == Key.RightCtrl
            || key == Key.LeftShift || key == Key.RightShift
            || key == Key.LeftAlt || key == Key.RightAlt
            || key == Key.LeftMeta || key == Key.RightMeta;

        internal bool WasPressedThisFrame()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !IsBound)
                return false;

            var primary = keyboard[PrimaryKey];
            return primary != null
                   && primary.wasPressedThisFrame
                   && ReadCurrentModifiers(keyboard) == Modifiers;
        }

        internal static BaKeyModifiers ReadCurrentModifiers(Keyboard keyboard)
        {
            if (keyboard == null)
                return BaKeyModifiers.None;

            var result = BaKeyModifiers.None;
            if (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed)
                result |= BaKeyModifiers.Control;
            if (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed)
                result |= BaKeyModifiers.Shift;
            if (keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed)
                result |= BaKeyModifiers.Alt;
            if (keyboard.leftMetaKey.isPressed || keyboard.rightMetaKey.isPressed)
                result |= BaKeyModifiers.Command;
            return result;
        }

        private static string GetPrimaryKeyDisplayName(Key key)
        {
            try
            {
                var keyboard = Keyboard.current;
                var displayName = keyboard?[key]?.displayName;
                if (!string.IsNullOrWhiteSpace(displayName))
                    return displayName;
            }
            catch
            {
                // Fall through to the stable key name when the device is changing.
            }

            return key switch
            {
                Key.Digit0 => "0",
                Key.Digit1 => "1",
                Key.Digit2 => "2",
                Key.Digit3 => "3",
                Key.Digit4 => "4",
                Key.Digit5 => "5",
                Key.Digit6 => "6",
                Key.Digit7 => "7",
                Key.Digit8 => "8",
                Key.Digit9 => "9",
                Key.LeftArrow => "Left Arrow",
                Key.RightArrow => "Right Arrow",
                Key.UpArrow => "Up Arrow",
                Key.DownArrow => "Down Arrow",
                Key.PageDown => "Page Down",
                Key.PageUp => "Page Up",
                Key.CapsLock => "Caps Lock",
                Key.NumLock => "Num Lock",
                Key.ScrollLock => "Scroll Lock",
                Key.PrintScreen => "Print Screen",
                Key.ContextMenu => "Context Menu",
                Key.NumpadEnter => "Num Enter",
                Key.NumpadDivide => "Num /",
                Key.NumpadMultiply => "Num *",
                Key.NumpadPlus => "Num +",
                Key.NumpadMinus => "Num -",
                Key.NumpadPeriod => "Num .",
                Key.NumpadEquals => "Num =",
                Key.Numpad0 => "Num 0",
                Key.Numpad1 => "Num 1",
                Key.Numpad2 => "Num 2",
                Key.Numpad3 => "Num 3",
                Key.Numpad4 => "Num 4",
                Key.Numpad5 => "Num 5",
                Key.Numpad6 => "Num 6",
                Key.Numpad7 => "Num 7",
                Key.Numpad8 => "Num 8",
                Key.Numpad9 => "Num 9",
                _ => key.ToString()
            };
        }
    }
}
