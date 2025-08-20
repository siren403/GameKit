using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Assets.Extensions;
using GameKit.Common.Results;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameKit.Assets
{
    public class AddressableOperations
    {
        public async UniTask<AsyncHandleSnapshot<IResourceLocator>> InitializeAsync(
            CancellationToken ct = default)
        {
            var handle = Addressables.InitializeAsync(false);
            var snapshot = await handle.CaptureWithRelease();
            return snapshot;
        }

        public async UniTask<CatalogResult> CheckForCatalogUpdatesAsync(CancellationToken ct = default)
        {
            var handle = Addressables.CheckForCatalogUpdates(false);
            var snapshot = await handle.CaptureWithRelease();
            if (snapshot.Status == AsyncOperationStatus.Failed)
            {
                return snapshot;
            }

            var ex = snapshot.OperationException;
            var error = ex.Message switch
            {
                { } msg when msg.Contains("RemoteProviderException") => new Error()
                {
                    Code = CatalogErrorCodes.RemoteError,
                    Description = ex.Message
                },
                { } msg when msg.Contains("ConnectionError") => new Error()
                {
                    Code = CatalogErrorCodes.ConnectionError,
                    Description = ex.Message
                },
                _ => new Error()
                {
                    Code = CatalogErrorCodes.UnknownError,
                    Description = ex.Message
                },
            };
            return error;
        }
    }
}