using UnityEngine;

namespace Capisoft.Lib.BaUnifiedUI.Core
{
    /// <summary>
    /// Hides a BAUI canvas while the vanilla Options screen is visible without
    /// changing the active state owned by the consumer mod.
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class BaUiOptionsVisibilityGuard : MonoBehaviour
    {
        private CanvasGroup _group;
        private bool _suppressed;
        private float _savedAlpha;
        private bool _savedInteractable;
        private bool _savedBlocksRaycasts;

        internal static void Attach(GameObject root)
        {
            if (root != null && root.GetComponent<BaUiOptionsVisibilityGuard>() == null)
                root.AddComponent<BaUiOptionsVisibilityGuard>();
        }

        private void Awake()
        {
            EnsureGroup();
            RefreshVisibility();
        }

        private void OnEnable()
        {
            EnsureGroup();
            RefreshVisibility();
        }

        private void LateUpdate() => RefreshVisibility();

        private void OnDisable() => Restore();

        private void OnDestroy() => Restore();

        private void RefreshVisibility()
        {
            if (IsOptionsVisible())
                Suppress();
            else
                Restore();
        }

        private void Suppress()
        {
            if (_suppressed)
                return;

            EnsureGroup();
            _savedAlpha = _group.alpha;
            _savedInteractable = _group.interactable;
            _savedBlocksRaycasts = _group.blocksRaycasts;
            _suppressed = true;

            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }

        private void Restore()
        {
            if (!_suppressed || _group == null)
                return;

            _group.alpha = _savedAlpha;
            _group.interactable = _savedInteractable;
            _group.blocksRaycasts = _savedBlocksRaycasts;
            _suppressed = false;
        }

        private void EnsureGroup()
        {
            if (_group == null)
                _group = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        }

        private static bool IsOptionsVisible()
        {
            try
            {
                return global::Scenes.MainMenu.Options.IsVisible;
            }
            catch
            {
                return false;
            }
        }
    }
}
