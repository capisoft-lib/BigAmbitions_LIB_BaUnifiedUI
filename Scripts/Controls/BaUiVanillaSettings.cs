using System;
using System.Reflection;
using BigAmbitions.ModsInternal;
using Localizor.LanguageChangeEvent;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Controls
{
    /// <summary>
    /// Creates settings rows from Big Ambitions' own Options &gt; Mods prefabs.
    /// Consumers keep ownership of values and persistence; BAUI only binds the
    /// native visuals and interaction to the supplied callbacks.
    /// </summary>
    public static class BaUiVanillaSettings
    {
        // Native Options rows use 3840x2160 design units; BAUI panels use
        // 1920x1080 design units. Convert geometry, not current screen pixels.
        // Reading Canvas.scaleFactor here makes only the rows grow at 4K or
        // with UI zoom while their fixed-layout parent panel stays unchanged.
        private const float NativeRowScale = 0.5f;
        private const BindingFlags InstancePrivate = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo TogglePrefabField = RequireControllerField("modOptionsTogglePrefab");
        private static readonly FieldInfo SliderPrefabField = RequireControllerField("modOptionsSliderPrefab");
        private static readonly FieldInfo ToggleField = RequireControlField(typeof(ModOptionsToggleControl), "toggle");
        private static readonly FieldInfo ToggleLabelField = RequireControlField(typeof(ModOptionsToggleControl), "label");
        private static readonly FieldInfo SliderField = RequireControlField(typeof(ModOptionsSliderControl), "slider");
        private static readonly FieldInfo SliderLabelField = RequireControlField(typeof(ModOptionsSliderControl), "label");
        private static readonly FieldInfo SliderValueLabelField = RequireControlField(typeof(ModOptionsSliderControl), "valueLabel");

        /// <summary>Clone and bind the game's exact native toggle option row.</summary>
        public static Toggle CreateToggle(
            Transform parent,
            string label,
            bool initialValue,
            UnityAction<bool> onValueChanged,
            string name = "BaUiVanillaToggle")
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            var prefab = FindPrefab(TogglePrefabField, "toggle");
            var root = CreateNativeRow(parent, prefab, name);

            var control = root.GetComponent<ModOptionsToggleControl>()
                          ?? throw new InvalidOperationException("The native toggle prefab has no ModOptionsToggleControl.");
            var toggle = ToggleField.GetValue(control) as Toggle
                         ?? throw new InvalidOperationException("The native toggle prefab has no bound Toggle.");
            var text = ToggleLabelField.GetValue(control) as TextLocalizationComponent
                       ?? throw new InvalidOperationException("The native toggle prefab has no bound label.");

            control.enabled = false;
            text.SetValue(label ?? string.Empty, clearKey: true);
            toggle.onValueChanged.RemoveAllListeners();
            toggle.SetIsOnWithoutNotify(initialValue);
            if (onValueChanged != null)
                toggle.onValueChanged.AddListener(onValueChanged);
            return toggle;
        }

        /// <summary>Clone and bind the game's exact native integer-slider option row.</summary>
        public static Slider CreateSlider(
            Transform parent,
            string label,
            int min,
            int max,
            int initialValue,
            Func<int, string> formatValue,
            UnityAction<int> onValueChanged,
            string name = "BaUiVanillaSlider")
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (min > max)
                throw new ArgumentOutOfRangeException(nameof(min), "Slider minimum cannot exceed maximum.");

            var prefab = FindPrefab(SliderPrefabField, "slider");
            var root = CreateNativeRow(parent, prefab, name);

            var control = root.GetComponent<ModOptionsSliderControl>()
                          ?? throw new InvalidOperationException("The native slider prefab has no ModOptionsSliderControl.");
            var slider = SliderField.GetValue(control) as Slider
                         ?? throw new InvalidOperationException("The native slider prefab has no bound Slider.");
            var text = SliderLabelField.GetValue(control) as TextLocalizationComponent
                       ?? throw new InvalidOperationException("The native slider prefab has no bound label.");
            var valueText = SliderValueLabelField.GetValue(control) as TextLocalizationComponent
                            ?? throw new InvalidOperationException("The native slider prefab has no bound value label.");

            control.enabled = false;
            text.SetValue(label ?? string.Empty, clearKey: true);
            valueText.gameObject.SetActive(true);
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = true;
            slider.onValueChanged.RemoveAllListeners();

            var clamped = Mathf.Clamp(initialValue, min, max);
            slider.SetValueWithoutNotify(clamped);
            SetSliderValue(valueText, formatValue, clamped);
            slider.onValueChanged.AddListener(rawValue =>
            {
                var value = Mathf.RoundToInt(rawValue);
                SetSliderValue(valueText, formatValue, value);
                onValueChanged?.Invoke(value);
            });
            return slider;
        }

        private static void SetSliderValue(
            TextLocalizationComponent valueText,
            Func<int, string> formatValue,
            int value)
        {
            var formatted = formatValue == null ? value.ToString() : formatValue(value);
            valueText.SetValue(formatted ?? string.Empty, clearKey: true);
        }

        private static GameObject CreateNativeRow(
            Transform parent,
            GameObject prefab,
            string name)
        {
            var slot = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
            slot.transform.SetParent(parent, false);
            var slotRect = slot.GetComponent<RectTransform>();

            var root = UnityEngine.Object.Instantiate(prefab, slotRect, false);
            root.name = name + "Native";
            var rootRect = root.GetComponent<RectTransform>()
                           ?? throw new InvalidOperationException("The native option prefab has no RectTransform.");

            var sourceHeight = LayoutUtility.GetPreferredHeight(rootRect);
            if (sourceHeight <= 0f)
                sourceHeight = Mathf.Abs(rootRect.sizeDelta.y);
            if (sourceHeight <= 0f)
                sourceHeight = 160f;

            var slotLayout = slot.GetComponent<LayoutElement>();
            slotLayout.minHeight = sourceHeight * NativeRowScale;
            slotLayout.preferredHeight = sourceHeight * NativeRowScale;
            slotLayout.flexibleHeight = 0f;

            // Keep the full slot width after converting the prefab's design
            // units. The owning canvas may scale the entire window afterwards;
            // resolution/UI zoom must not independently resize these children.
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = new Vector2(1f / NativeRowScale, 1f / NativeRowScale);
            rootRect.pivot = Vector2.zero;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            rootRect.localScale = new Vector3(NativeRowScale, NativeRowScale, 1f);
            root.SetActive(true);
            return root;
        }

        private static GameObject FindPrefab(
            FieldInfo prefabField,
            string controlName)
        {
            var controllers = Resources.FindObjectsOfTypeAll<ModOptionsViewController>();
            for (var i = 0; i < controllers.Length; i++)
            {
                var controller = controllers[i];
                if (controller == null)
                    continue;

                if (prefabField.GetValue(controller) is GameObject prefab && prefab != null)
                {
                    return prefab;
                }
            }

            throw new InvalidOperationException(
                "Big Ambitions' native " + controlName + " option prefab is not loaded yet.");
        }

        private static FieldInfo RequireControllerField(string name) =>
            typeof(ModOptionsViewController).GetField(name, InstancePrivate)
            ?? throw new MissingFieldException(typeof(ModOptionsViewController).FullName, name);

        private static FieldInfo RequireControlField(Type type, string name) =>
            type.GetField(name, InstancePrivate)
            ?? throw new MissingFieldException(type.FullName, name);
    }
}
