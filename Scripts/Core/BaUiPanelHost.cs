using System;
using Capisoft.Lib.BaUnifiedUI.Fluent;
using UnityEngine;

namespace Capisoft.Lib.BaUnifiedUI.Core
{
    /// <summary>Versioned overlay root lifecycle shared by consumer mods.</summary>
    public static class BaUiPanelHost
    {
        public static bool ShouldRecreate(GameObject root, string rootName)
        {
            if (root == null)
                return false;

            if (root.name != rootName)
                return true;

            var stamp = root.GetComponent<BaUiLayoutStamp>();
            if (stamp == null)
                return true;

            if (stamp.LayoutRevision != BaUiVersion.LayoutRevision)
                return true;

            // A stamped panel with the current layout revision is valid even when the global
            // assets-rebuild flag is still set — otherwise lazy-built HUD panels (e.g. Better
            // Fines status panel on first ticket) destroy/recreate every consumer tick.
            return false;
        }

        public static string DescribeRecreateReason(GameObject root, string rootName)
        {
            if (root == null)
                return "no-root";

            if (root.name != rootName)
                return "name:" + root.name;

            var stamp = root.GetComponent<BaUiLayoutStamp>();
            if (stamp == null)
                return BaUi.ShouldRebuildChrome ? "no-stamp+assets-rebuild" : "no-stamp";

            if (stamp.LayoutRevision != BaUiVersion.LayoutRevision)
                return "layout-rev:" + stamp.LayoutRevision + "!=" + BaUiVersion.LayoutRevision;

            if (BaUi.ShouldRebuildChrome)
                return "assets-rebuild-ignored-stamped";

            return "current";
        }

        public static void DestroyIfStale(ref GameObject root, string rootName, Action destroy)
        {
            if (root == null)
                return;

            if (!ShouldRecreate(root, rootName))
                return;

            destroy();
            root = null;
        }

        public static void PurgeOrphanRoots(params string[] rootNamePrefixes)
        {
            if (rootNamePrefixes == null || rootNamePrefixes.Length == 0)
                return;

            var roots = Resources.FindObjectsOfTypeAll<GameObject>();
            for (var i = 0; i < roots.Length; i++)
            {
                var go = roots[i];
                if (go == null || go.transform.parent != null)
                    continue;

                var objectName = go.name;
                for (var p = 0; p < rootNamePrefixes.Length; p++)
                {
                    if (!objectName.StartsWith(rootNamePrefixes[p], StringComparison.Ordinal))
                        continue;

                    UnityEngine.Object.Destroy(go);
                    break;
                }
            }
        }

        public static void PurgeNamedRoots(params string[] exactNames)
        {
            if (exactNames == null || exactNames.Length == 0)
                return;

            for (var i = 0; i < exactNames.Length; i++)
            {
                var name = exactNames[i];
                GameObject go;
                while ((go = GameObject.Find(name)) != null)
                    UnityEngine.Object.Destroy(go);
            }
        }
    }
}
