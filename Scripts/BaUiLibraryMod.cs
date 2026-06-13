using System.Threading.Tasks;
using BAModAPI;
using Capisoft.Lib.BaUi.BaXaml.Generated;
using Capisoft.Lib.BaUi.Core;
using UnityEngine;

[assembly: RegisterModClass(typeof(Capisoft.Lib.BaUi.BaUiLibraryMod))]

namespace Capisoft.Lib.BaUi
{
    [ModEntryOnCityLoad]
    public sealed class BaUiLibraryMod : IModBigAmbitions
    {
        public string[] RelativeAssetBundlePaths => System.Array.Empty<string>();

        public Task OnLoadAsync(ModContext context)
        {
            BaUiBootstrap.EnsureEventSystem();
            GpsHudDocument.EnsureRegistered();
            Debug.Log("[LIB_BaUnifiedUI] UI library " + BaUiVersion.Version + " loaded.");
            return Task.CompletedTask;
        }

        public Task OnUnloadAsync()
        {
            Debug.Log("[LIB_BaUnifiedUI] UI library unloaded.");
            return Task.CompletedTask;
        }
    }
}

