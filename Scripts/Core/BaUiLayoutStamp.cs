using UnityEngine;

namespace Capisoft.Lib.BaUnifiedUI.Core
{
    /// <summary>Marks overlay roots built by LIB — used to invalidate stale UI when layout chrome changes.</summary>
    public sealed class BaUiLayoutStamp : MonoBehaviour
    {
        public int LayoutRevision;
    }
}
