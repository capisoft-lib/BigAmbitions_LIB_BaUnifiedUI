using System;
using System.Collections.Generic;
using BigAmbitions.Mods;
using Helpers;
using NWH.VehiclePhysics2.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Capisoft.Lib.BaUnifiedUI.Shortcuts
{
    /// <summary>
    /// Coordinates participating BA Unified UI shortcuts. It only observes game bindings;
    /// it never changes or disables an InputAction.
    /// </summary>
    internal static class BaKeybindRegistry
    {
        private static readonly HashSet<BaKeybindHandle> Handles = new HashSet<BaKeybindHandle>();
        private static readonly HashSet<BaKeybind> GameKeyboardBindings = new HashSet<BaKeybind>();

        private static InputActionAsset _playerAsset;
        private static InputActionAsset _vehicleAsset;
        private static bool _gameBindingsScanned;
        private static bool _initialized;

        internal static event Action ConflictsChanged;

        internal static bool IsCaptureActive => BaKeybindCaptureCoordinator.IsCaptureActive;

        internal static void Initialize()
        {
            if (!_initialized)
            {
                _initialized = true;
                OptionsService.OnChanged -= OnOptionsChanged;
                OptionsService.OnChanged += OnOptionsChanged;
                RefreshGameBindings();
            }

            EnsureGameBindingSubscription();
        }

        internal static void Shutdown()
        {
            if (_initialized)
            {
                GlobalEvents.onBindingsChanged -= OnGameBindingsChanged;
                OptionsService.OnChanged -= OnOptionsChanged;
            }

            _initialized = false;
            _gameBindingsScanned = false;
            _playerAsset = null;
            _vehicleAsset = null;
            GameKeyboardBindings.Clear();
            Handles.Clear();
            BaKeybindCaptureCoordinator.CancelActive();
            ConflictsChanged = null;
        }

        internal static void Track(BaKeybindHandle handle)
        {
            if (handle == null)
                return;

            Initialize();
            if (Handles.Add(handle) && IsActive(handle))
                RaiseConflictsChanged();
        }

        internal static void Untrack(BaKeybindHandle handle)
        {
            if (handle == null)
                return;

            if (Handles.Remove(handle))
                RaiseConflictsChanged();
        }

        internal static BaKeybindConflict GetConflict(BaKeybindHandle self, BaKeybind candidate)
        {
            if (!candidate.IsBound)
                return BaKeybindConflict.None;

            EnsureGameBindingsCurrent();
            if (GameKeyboardBindings.Contains(candidate))
            {
                return new BaKeybindConflict(
                    BaKeybindConflictKind.GameInput,
                    "Big Ambitions",
                    string.Empty);
            }

            Handles.RemoveWhere(handle => handle == null || handle.IsDisposed);
            foreach (var other in Handles)
            {
                if (ReferenceEquals(other, self) || !IsActive(other))
                    continue;

                if (other.GetBindingForRegistry() != candidate)
                    continue;

                return new BaKeybindConflict(
                    BaKeybindConflictKind.BaUnifiedUiOption,
                    other.ModId,
                    other.OptionId);
            }

            return BaKeybindConflict.None;
        }

        internal static void NotifyBindingChanged()
        {
            RaiseConflictsChanged();
        }

        internal static bool IsActive(BaKeybindHandle handle)
        {
            var modId = handle.ModId;
            if (string.IsNullOrEmpty(modId)
                || !OptionsService.RegisteredEntries.TryGetValue(modId, out var registered))
                return false;

            foreach (var option in registered.Options)
            {
                if (ReferenceEquals(option, handle.Option))
                    return true;
            }

            return false;
        }

        private static void OnGameBindingsChanged()
        {
            RefreshGameBindings();
            RaiseConflictsChanged();
        }

        private static void OnOptionsChanged()
        {
            var removed = Handles.RemoveWhere(handle =>
                handle == null
                || handle.IsDisposed
                || (!string.IsNullOrEmpty(handle.ModId) && !IsActive(handle)));
            if (removed > 0)
                RaiseConflictsChanged();
        }

        private static void EnsureGameBindingsCurrent()
        {
            Initialize();
            GetCurrentAssets(out var playerAsset, out var vehicleAsset);
            if (!_gameBindingsScanned
                || !ReferenceEquals(playerAsset, _playerAsset)
                || !ReferenceEquals(vehicleAsset, _vehicleAsset))
                RefreshGameBindings(playerAsset, vehicleAsset);
        }

        private static void EnsureGameBindingSubscription()
        {
            var expected = (Action)OnGameBindingsChanged;
            var current = GlobalEvents.onBindingsChanged;
            if (current != null)
            {
                foreach (var registered in current.GetInvocationList())
                {
                    if (registered.Equals(expected))
                        return;
                }
            }

            // GlobalEvents.Init clears its public delegates during startup. Reattach lazily
            // if this registry happened to initialize before that reset.
            GlobalEvents.onBindingsChanged += OnGameBindingsChanged;
            RefreshGameBindings();
        }

        private static void RefreshGameBindings()
        {
            GetCurrentAssets(out var playerAsset, out var vehicleAsset);
            RefreshGameBindings(playerAsset, vehicleAsset);
        }

        private static void RefreshGameBindings(InputActionAsset playerAsset, InputActionAsset vehicleAsset)
        {
            _playerAsset = playerAsset;
            _vehicleAsset = vehicleAsset;
            _gameBindingsScanned = true;
            GameKeyboardBindings.Clear();
            CollectKeyboardBindings(playerAsset);
            if (!ReferenceEquals(vehicleAsset, playerAsset))
                CollectKeyboardBindings(vehicleAsset);
        }

        private static void GetCurrentAssets(out InputActionAsset playerAsset, out InputActionAsset vehicleAsset)
        {
            playerAsset = null;
            vehicleAsset = null;
            try
            {
                playerAsset = InputHelper.playerInput?.asset;
            }
            catch
            {
                // Input setup can replace/dispose its generated wrapper during a scene transition.
            }

            try
            {
                vehicleAsset = InputSystemVehicleInputProvider.vehicleInputActions?.asset;
            }
            catch
            {
                // The vehicle provider is optional until its first instance is awake.
            }
        }

        private static void CollectKeyboardBindings(InputActionAsset asset)
        {
            if (asset == null)
                return;

            try
            {
                var bindings = new List<InputBinding>(asset.bindings);
                for (var i = 0; i < bindings.Count; i++)
                {
                    var binding = bindings[i];
                    if (binding.isComposite)
                    {
                        if (IsModifierCompositePath(binding.effectivePath, binding.path))
                            i = CollectModifierComposite(bindings, i);
                        continue;
                    }

                    var path = binding.effectivePath;
                    if (IsAnyKeyboardKeyPath(path))
                        continue;

                    if (!TryResolveKeyboardKey(path, out var key))
                        continue;

                    // A standalone modifier is not a shortcut chord. Treating it as a
                    // global claim made every Ctrl/Shift/Alt combination conflict merely
                    // because the game uses that modifier somewhere (run, handbrake, etc.).
                    if (BaKeybind.IsModifierKey(key))
                        continue;

                    // Vanilla bindings are compared as complete chords. A plain Y
                    // therefore conflicts with Y, but not with Ctrl+Y or Ctrl+Shift+Y.
                    GameKeyboardBindings.Add(new BaKeybind(key));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LIB_BaUnifiedUI] Could not inspect a game input asset: " + ex.Message);
            }
        }

        private static int CollectModifierComposite(
            List<InputBinding> bindings,
            int compositeIndex)
        {
            var modifiers = BaKeyModifiers.None;
            var primaryKey = Key.None;
            var lastPartIndex = compositeIndex;
            var valid = true;

            for (var i = compositeIndex + 1; i < bindings.Count && bindings[i].isPartOfComposite; i++)
            {
                lastPartIndex = i;
                var part = bindings[i];
                if (!TryResolveKeyboardKey(part.effectivePath, out var key))
                    continue;

                var partName = part.name ?? string.Empty;
                if (partName.StartsWith("modifier", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryGetModifier(key, out var modifier))
                        valid = false;
                    else
                        modifiers |= modifier;
                }
                else if (string.Equals(partName, "button", StringComparison.OrdinalIgnoreCase)
                         || string.Equals(partName, "binding", StringComparison.OrdinalIgnoreCase))
                {
                    if (BaKeybind.IsModifierKey(key))
                        valid = false;
                    else
                        primaryKey = key;
                }
            }

            if (valid && primaryKey != Key.None && modifiers != BaKeyModifiers.None)
                GameKeyboardBindings.Add(new BaKeybind(primaryKey, modifiers));

            return lastPartIndex;
        }

        private static bool IsModifierCompositePath(string effectivePath, string fallbackPath)
        {
            var path = string.IsNullOrWhiteSpace(effectivePath) ? fallbackPath : effectivePath;
            return !string.IsNullOrWhiteSpace(path)
                   && path.IndexOf("Modifier", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool TryResolveKeyboardKey(string path, out Key key)
        {
            key = Key.None;
            if (string.IsNullOrWhiteSpace(path)
                || path.IndexOf("Keyboard", StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            var slash = path.LastIndexOf('/');
            var controlName = slash >= 0 ? path.Substring(slash + 1) : path;
            var brace = controlName.IndexOf('}');
            if (brace >= 0 && brace + 1 < controlName.Length)
                controlName = controlName.Substring(brace + 1);
            controlName = controlName.Trim('<', '>', '{', '}', ' ');

            if (controlName.Length == 1 && char.IsDigit(controlName[0]))
                controlName = "Digit" + controlName;

            switch (controlName.ToLowerInvariant())
            {
                case "ctrl":
                case "control":
                    key = Key.LeftCtrl;
                    return true;
                case "shift":
                    key = Key.LeftShift;
                    return true;
                case "alt":
                    key = Key.LeftAlt;
                    return true;
                case "meta":
                case "command":
                    key = Key.LeftMeta;
                    return true;
            }

            if (Enum.TryParse(controlName, true, out key)
                && key != Key.None
                && Enum.IsDefined(typeof(Key), key))
                return true;

            try
            {
                if (UnityEngine.InputSystem.InputSystem.FindControl(path) is KeyControl control)
                {
                    key = control.keyCode;
                    return key != Key.None;
                }
            }
            catch
            {
                // A keyboard device is not guaranteed to exist while the city is loading.
            }

            key = Key.None;
            return false;
        }

        private static bool TryGetModifier(Key key, out BaKeyModifiers modifier)
        {
            if (key == Key.LeftCtrl || key == Key.RightCtrl)
            {
                modifier = BaKeyModifiers.Control;
                return true;
            }
            if (key == Key.LeftShift || key == Key.RightShift)
            {
                modifier = BaKeyModifiers.Shift;
                return true;
            }
            if (key == Key.LeftAlt || key == Key.RightAlt)
            {
                modifier = BaKeyModifiers.Alt;
                return true;
            }
            if (key == Key.LeftMeta || key == Key.RightMeta)
            {
                modifier = BaKeyModifiers.Command;
                return true;
            }

            modifier = BaKeyModifiers.None;
            return false;
        }

        private static bool IsAnyKeyboardKeyPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)
                || path.IndexOf("Keyboard", StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            var slash = path.LastIndexOf('/');
            var controlName = slash >= 0 ? path.Substring(slash + 1) : path;
            return string.Equals(controlName.Trim(), "anyKey", StringComparison.OrdinalIgnoreCase);
        }

        private static void RaiseConflictsChanged()
        {
            var handlers = ConflictsChanged;
            if (handlers == null)
                return;

            foreach (Action handler in handlers.GetInvocationList())
            {
                try
                {
                    handler();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }
}
