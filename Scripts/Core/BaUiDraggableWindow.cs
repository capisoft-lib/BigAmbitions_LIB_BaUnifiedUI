using System;
using System.Collections;
using System.Reflection;
using UI;
using UI.DraggableWindows;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Core
{
    /// <summary>Runtime state exposed to consumers that need to preserve their automatic default layout.</summary>
    public sealed class BaUiDragState
    {
        private readonly BaUiDraggableWindowRegistration _registration;

        internal BaUiDragState(BaUiDraggableWindowRegistration registration) =>
            _registration = registration;

        public string PersistentId => _registration != null ? _registration.PersistentId : null;

        public bool IsDragging => _registration != null && _registration.IsDragging;

        public bool HasSavedPosition => _registration != null && _registration.HasSavedPosition;
    }

    /// <summary>
    /// Registers a runtime-built BAUI panel with Big Ambitions' native draggable-window service.
    /// Registration is deferred to LateUpdate so consumers can finish their initial positioning first.
    /// </summary>
    internal sealed class BaUiDraggableWindowRegistration : MonoBehaviour
    {
        private RectTransform _element;
        private DraggableWindowHandle _handle;
        private DraggableWindows _manager;
        private string _persistentId;
        private bool _registered;
        private bool _clampPending = true;

        internal string PersistentId => _persistentId;

        internal bool IsDragging => _handle != null && _handle.isDragging;

        internal bool HasSavedPosition
        {
            get
            {
                if (string.IsNullOrEmpty(_persistentId))
                    return false;

                var manager = _manager != null ? _manager : ResolveManager();
                return BaUiDraggableWindowsCompat.HasSavedPosition(manager, _persistentId);
            }
        }

        internal void Bind(RectTransform element, DraggableWindowHandle handle, string persistentId)
        {
            _element = element;
            _handle = handle;
            _persistentId = persistentId;
        }

        private void OnEnable() => _clampPending = true;

        private void OnRectTransformDimensionsChange() => _clampPending = true;

        private void LateUpdate()
        {
            if (!_registered)
                TryRegister();

            if (!_registered || !_clampPending || _manager == null || _element == null)
                return;

            _clampPending = false;
            _element.position = BaUiDraggableWindowsCompat.ClampPosition(
                _manager,
                _element,
                _element.position);
        }

        private void TryRegister()
        {
            if (_element == null || _handle == null)
                return;

            var manager = ResolveManager();
            if (manager == null)
                return;

            var defaultPosition = _element.position;
            var targetPosition = BaUiDraggableWindowsCompat.TryGetSavedPosition(
                manager,
                _persistentId,
                out var savedPosition)
                ? savedPosition
                : defaultPosition;
            _element.position = BaUiDraggableWindowsCompat.ClampPosition(
                manager,
                _element,
                targetPosition);

            if (!BaUiDraggableWindowsCompat.TryRegister(
                    manager,
                    _persistentId,
                    _element,
                    _handle,
                    defaultPosition))
                return;

            _manager = manager;
            _registered = true;
            _clampPending = false;
        }

        private static DraggableWindows ResolveManager()
        {
            var uis = InstanceBehavior<UIs>.Instance;
            return uis != null ? uis.draggableWindows : null;
        }
    }

    /// <summary>
    /// Feature-detected adapter for the native draggable-window API. Big Ambitions
    /// 0.11 and 1.0 expose the same types but different members and Data layouts,
    /// so direct calls to the 1.0-only members would fail before a version guard ran.
    /// </summary>
    internal static class BaUiDraggableWindowsCompat
    {
        private const BindingFlags InstanceFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags NestedFlags = BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Type ManagerType = typeof(DraggableWindows);
        private static readonly Type RegistrationDataType =
            ManagerType.GetNestedType("Data", NestedFlags);
        private static readonly MethodInfo GetDataMethod = ManagerType.GetMethod(
            "GetData",
            InstanceFlags,
            null,
            new[] { typeof(string) },
            null);
        private static readonly MethodInfo ClampPositionMethod = ManagerType.GetMethod(
            "ClampPosition",
            InstanceFlags,
            null,
            new[] { typeof(RectTransform), typeof(Vector3) },
            null);
        private static readonly MethodInfo RegisterMethod = FindRegisterMethod();
        private static readonly FieldInfo SavedDataField = ManagerType.GetField("data", InstanceFlags);
        private static readonly FieldInfo SavedIdField =
            typeof(DraggableWindowData).GetField("id", InstanceFlags);
        private static readonly FieldInfo SavedPositionField =
            typeof(DraggableWindowData).GetField("position", InstanceFlags);
        private static readonly FieldInfo RegistrationIdField =
            RegistrationDataType?.GetField("id", InstanceFlags);
        private static readonly FieldInfo RegistrationElementField =
            RegistrationDataType?.GetField("element", InstanceFlags);
        private static readonly FieldInfo RegistrationHandleField =
            RegistrationDataType?.GetField("handle", InstanceFlags);
        private static readonly FieldInfo RegistrationDefaultPositionField =
            RegistrationDataType?.GetField("defaultPosition", InstanceFlags);

        internal static bool HasSavedPosition(DraggableWindows manager, string persistentId) =>
            TryGetSavedPosition(manager, persistentId, out _);

        internal static bool TryGetSavedPosition(
            DraggableWindows manager,
            string persistentId,
            out Vector3 position)
        {
            position = default;
            if (manager == null || string.IsNullOrEmpty(persistentId))
                return false;

            try
            {
                object saved = null;
                if (GetDataMethod != null)
                    saved = GetDataMethod.Invoke(manager, new object[] { persistentId });

                saved ??= FindSavedData(manager, persistentId);
                if (saved == null || SavedPositionField == null)
                    return false;

                var serialized = SavedPositionField.GetValue(saved);
                if (serialized is not SerializableVector2 savedPosition)
                    return false;

                position = (Vector3)savedPosition;
                return true;
            }
            catch
            {
                position = default;
                return false;
            }
        }

        internal static Vector3 ClampPosition(
            DraggableWindows manager,
            RectTransform element,
            Vector3 position)
        {
            if (manager == null || element == null || ClampPositionMethod == null)
                return position;

            try
            {
                var result = ClampPositionMethod.Invoke(manager, new object[] { element, position });
                return result is Vector3 clamped ? clamped : position;
            }
            catch
            {
                return position;
            }
        }

        internal static bool TryRegister(
            DraggableWindows manager,
            string persistentId,
            RectTransform element,
            DraggableWindowHandle handle,
            Vector3 defaultPosition)
        {
            if (manager == null || element == null || handle == null ||
                RegistrationDataType == null || RegisterMethod == null ||
                RegistrationElementField == null || RegistrationHandleField == null)
                return false;

            try
            {
                // The 0.11 manager only updates pre-existing saved-data rows while
                // dragging. Seed one after the caller has checked HasSavedPosition.
                if (GetDataMethod == null)
                    EnsureLegacySavedData(manager, persistentId, defaultPosition);

                var registration = Activator.CreateInstance(RegistrationDataType);
                RegistrationIdField?.SetValue(registration, persistentId);
                RegistrationElementField.SetValue(registration, element);
                RegistrationHandleField.SetValue(registration, handle);
                RegistrationDefaultPositionField?.SetValue(registration, defaultPosition);
                RegisterMethod.Invoke(manager, new[] { registration });
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static MethodInfo FindRegisterMethod()
        {
            if (RegistrationDataType == null)
                return null;

            return ManagerType.GetMethod(
                "RegisterDraggableWindow",
                InstanceFlags,
                null,
                new[] { RegistrationDataType },
                null);
        }

        private static object FindSavedData(DraggableWindows manager, string persistentId)
        {
            if (SavedDataField?.GetValue(manager) is not IEnumerable entries)
                return null;

            foreach (var entry in entries)
            {
                if (entry != null &&
                    string.Equals(SavedIdField?.GetValue(entry) as string, persistentId, StringComparison.Ordinal))
                    return entry;
            }

            return null;
        }

        private static void EnsureLegacySavedData(
            DraggableWindows manager,
            string persistentId,
            Vector3 defaultPosition)
        {
            if (string.IsNullOrEmpty(persistentId) || FindSavedData(manager, persistentId) != null)
                return;

            if (SavedDataField?.GetValue(manager) is not IList entries ||
                SavedIdField == null || SavedPositionField == null)
                return;

            var saved = new DraggableWindowData();
            SavedIdField.SetValue(saved, persistentId);
            SavedPositionField.SetValue(saved, (SerializableVector2)defaultPosition);
            entries.Add(saved);
        }
    }

    /// <summary>Native Move cursor behavior for the transparent header drag surface.</summary>
    internal sealed class BaUiMoveCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ICursorHoverEvent
    {
        private bool _isHovering;

        public bool ChangedCursor { get; set; }

        public CursorType CursorType => global::CursorType.Move;

        private void OnEnable() => CursorHoverChangeEvent.OnFreezeCursorTypeChanged += OnFreezeCursorTypeChanged;

        private void OnDisable()
        {
            CursorHoverChangeEvent.OnFreezeCursorTypeChanged -= OnFreezeCursorTypeChanged;
            _isHovering = false;
            ResetCursor();
        }

        private void OnDestroy() => ResetCursor();

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            if (!CursorHoverChangeEvent.FreezeCursorType && MouseController.SetCursor(this))
                ChangedCursor = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            if (!CursorHoverChangeEvent.FreezeCursorType)
                ResetCursor();
        }

        private void OnFreezeCursorTypeChanged(bool isFrozen)
        {
            if (isFrozen)
                return;

            if (_isHovering)
                OnPointerEnter(null);
            else
                OnPointerExit(null);
        }

        private void ResetCursor()
        {
            if (!ChangedCursor)
                return;

            MouseController.SetCursor(null);
            ChangedCursor = false;
        }
    }

    internal static class BaUiDraggableWindow
    {
        internal static BaUiDragState Attach(RectTransform panel, RectTransform header, string persistentId)
        {
            var surfaceGo = new GameObject("DragSurface", typeof(RectTransform));
            surfaceGo.transform.SetParent(header, false);
            surfaceGo.transform.SetAsFirstSibling();

            var surface = surfaceGo.GetComponent<RectTransform>();
            surface.anchorMin = Vector2.zero;
            surface.anchorMax = Vector2.one;
            surface.offsetMin = Vector2.zero;
            surface.offsetMax = Vector2.zero;

            var image = surfaceGo.AddComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;

            var nativeHandle = surfaceGo.AddComponent<DraggableWindowHandle>();
            surfaceGo.AddComponent<BaUiMoveCursor>();

            var registration = panel.gameObject.AddComponent<BaUiDraggableWindowRegistration>();
            registration.Bind(panel, nativeHandle, persistentId);
            return new BaUiDragState(registration);
        }
    }
}
