using UnityEngine;
using UnityEngine.EventSystems;

namespace Capisoft.Lib.BaUnifiedUI.Core
{
    /// <summary>Ensures an EventSystem exists for mod overlay UI.</summary>
    public static class BaUiBootstrap
    {
        internal const string RootName = "LIB_BaUnifiedUI_EventSystem";

        public static void EnsureEventSystem(string rootName = RootName)
        {
            if (EventSystem.current != null)
                return;

            var go = new GameObject(rootName);
            BaUiRuntime.MarkRoot(go);
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }
    }
}
