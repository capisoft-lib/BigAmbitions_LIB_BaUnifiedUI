using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Capisoft.Lib.BaUnifiedUI.Core
{
    /// <summary>
    /// Clears BaUi overlay selection so WASD/arrows and click-to-move work again.
    /// GameManager.HasInputSelected blocks CityMapCam and MouseController while a UI button stays selected.
    /// Only BaUi-owned overlays are cleared — never vanilla game UI.
    /// </summary>
    public static class BaUiFocus
    {
        public static void ReleaseForMovement()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return;

            var selected = eventSystem.currentSelectedGameObject;
            if (selected == null || !IsUnderBaUiOverlay(selected))
                return;

            eventSystem.SetSelectedGameObject(null);
        }

        static bool IsUnderBaUiOverlay(GameObject go)
        {
            if (go == null)
                return false;

            for (var t = go.transform; t != null; t = t.parent)
            {
                if (t.GetComponent<BaUiLayoutStamp>() != null)
                    return true;
            }

            return false;
        }

        public static UnityAction Wrap(UnityAction action)
        {
            if (action == null)
                return null;

            return () =>
            {
                action();
                ReleaseForMovement();
            };
        }
    }
}

