using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Capisoft.Lib.BaUnifiedUI.Core
{
    /// <summary>
    /// Keeps the input field registered as the EventSystem selection while focused so
    /// GameManager.HasInputSelected blocks vanilla letter/space hotkeys.
    /// </summary>
    public sealed class BaUiInputGuard : MonoBehaviour
    {
        private TMP_InputField _field;

        public void Bind(TMP_InputField field) => _field = field;

        private void LateUpdate()
        {
            if (_field == null || !_field.isFocused)
                return;

            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return;

            if (eventSystem.currentSelectedGameObject != _field.gameObject)
                eventSystem.SetSelectedGameObject(_field.gameObject);
        }
    }
}

