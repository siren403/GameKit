using System.Linq;
using System.Threading.Tasks;
using GameKit.Assets;
using UnityEngine;
using VitalRouter;

namespace GameKit.Navigation.Scenes
{
    class CheckCatalog : ICommandInterceptor
    {
        public async ValueTask InvokeAsync<T>(T command, PublishContext context, PublishContinuation<T> next)
            where T : ICommand
        {
            var catalogResult = await AddressableOperations.CheckCatalog(context.CancellationToken);
            if (catalogResult.IsError)
            {
                // TODO: SceneErrorCommand
                Debug.LogError($"Failed to check catalog: {catalogResult}");
                return;
            }
            
            var manifest = catalogResult.Value;
            if (manifest.HasUpdate)
            {
                Debug.LogWarning("There are updates available in the catalog. Please update the assets.");
                return;
            }

            Debug.Log("Successfully checked catalog.");
            await next(command, context);
        }
    }
}