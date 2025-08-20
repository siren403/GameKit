using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;

namespace GameKit.Assets
{
    public class AssetManager : IAssetManager
    {
        public async UniTask<AsyncHandleSnapshot<IResourceLocator>> InitializeAsync(
            CancellationToken ct = default
        )
        {
            var handle = Addressables.InitializeAsync(false);
            var snapshot = await handle.CaptureWithRelease();
            return snapshot;
        }

        public async UniTask<AsyncHandleSnapshot<List<string>>> CheckForCatalogUpdatesAsync(
            CancellationToken ct = default
        )
        {
            var handle = Addressables.CheckForCatalogUpdates(false);
            var snapshot = await handle.CaptureWithRelease();
            if (snapshot.Status == AsyncOperationStatus.Succeeded)
            {
                return snapshot;
            }

            var ex = snapshot.OperationException;
            var wrappedException = ex.Message switch
            {
                { } msg when msg.Contains("RemoteProviderException") => new RemoteProviderException(ex.Message),
                { } msg when msg.Contains("ConnectionError") => new CatalogUpdateConnectionException(ex),
                _ => ex,
            };
            // throw wrappedException;
            snapshot = new AsyncHandleSnapshot<List<string>>()
            {
                Status = snapshot.Status,
                Result = snapshot.Result,
                OperationException = wrappedException,
                DebugName = snapshot.DebugName
            };

            return snapshot;
        }
    }
}