using UnityEngine;
using UnityEngine.EventSystems;

namespace Capisoft.Lib.BaUnifiedUI.Core
{
    /// <summary>Ensures an EventSystem exists for mod overlay UI.</summary>
    public static class BaUiBootstrap
    {
        public static void EnsureEventSystem(string rootName = "LIB_BaUnifiedUI_EventSystem")
        {
            if (EventSystem.current != null)
                return;

            var go = new GameObject(rootName);
            Object.DontDestroyOnLoad(go);
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }
    }
}

