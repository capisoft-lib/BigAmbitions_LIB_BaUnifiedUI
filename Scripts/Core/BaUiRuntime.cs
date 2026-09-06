using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Capisoft.Lib.BaUnifiedUI.Core
{
    /// <summary>
    /// Marks runtime overlay objects so they are not written into a save, and
    /// destroys leftovers across city unload/reload.
    /// </summary>
    internal static class BaUiRuntime
    {
        internal const HideFlags Flags = HideFlags.DontSave;

        internal static void MarkRoot(GameObject go)
        {
            if (go == null)
                return;

            go.hideFlags = Flags;
            Object.DontDestroyOnLoad(go);
        }

        internal static void MarkHierarchy(GameObject go)
        {
            if (go == null)
                return;

            go.hideFlags = Flags;
            var transform = go.transform;
            for (var i = 0; i < transform.childCount; i++)
                MarkHierarchy(transform.GetChild(i).gameObject);
        }

        internal static void PurgeOverlayRoots()
        {
            var guards = Resources.FindObjectsOfTypeAll<BaUiOptionsVisibilityGuard>();
            var roots = new List<GameObject>(guards.Length);
            for (var i = 0; i < guards.Length; i++)
            {
                var guard = guards[i];
                if (guard == null)
                    continue;

                var go = guard.gameObject;
                if (go == null || go.transform.parent != null)
                    continue;

                roots.Add(go);
            }

            for (var i = 0; i < roots.Count; i++)
            {
                if (roots[i] != null)
                    Object.Destroy(roots[i]);
            }
        }

        internal static void DestroyEventSystem()
        {
            var systems = Resources.FindObjectsOfTypeAll<EventSystem>();
            for (var i = 0; i < systems.Length; i++)
            {
                var system = systems[i];
                if (system == null)
                    continue;

                var go = system.gameObject;
                if (go != null && go.name == BaUiBootstrap.RootName)
                    Object.Destroy(go);
            }
        }
    }
}
