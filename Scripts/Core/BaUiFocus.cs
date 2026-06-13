using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Capisoft.Lib.BaUnifiedUI.Core
{
    /// <summary>
    /// Clears UI EventSystem selection so WASD/arrows and click-to-move work again.
    /// GameManager.HasInputSelected blocks CityMapCam and MouseController while a UI button stays selected.
    /// </summary>
    public static class BaUiFocus
    {
        public static void ReleaseForMovement()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null || eventSystem.currentSelectedGameObject == null)
                return;

            eventSystem.SetSelectedGameObject(null);
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

