using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Assets.Extensions;
using GameKit.Common.Results;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Void = GameKit.Common.Results.Void;

namespace GameKit.Assets
{
    public static class AddressableOperations
    {
        public static async UniTask<AsyncHandleSnapshot<IResourceLocator>> InitializeAsync(
            CancellationToken ct = default)
        {
            var handle = Addressables.InitializeAsync(false);
            var snapshot = await handle.CaptureWithRelease();
            return snapshot;
        }

        public static async UniTask<CatalogResult> CheckCatalog(CancellationToken ct = default)
        {
            var handle = Addressables.CheckForCatalogUpdates(false);
            var snapshot = await handle.CaptureWithRelease();
            if (snapshot.Status == AsyncOperationStatus.Succeeded)
            {
                return snapshot;
            }

            var ex = snapshot.OperationException;
            var error = ex.ToString() switch
            {
                { } msg when msg.Contains("ConnectionError") => new Error()
                {
                    Code = CatalogErrorCodes.ConnectionError,
                    Description = msg
                },
                { } msg when msg.Contains("RemoteProviderException") => new Error()
                {
                    Code = CatalogErrorCodes.RemoteError,
                    Description = msg
                },
                { } msg => new Error()
                {
                    Code = CatalogErrorCodes.UnknownError,
                    Description = msg
                },
            };
            return error;
        }

        public static async UniTask<FastResult<T>> InstantiateAsync<T>(
            string key,
            Transform parent = null,
            bool instantiateInWorldSpace = false
        )
        {
            var handle = Addressables.InstantiateAsync(key, parent, instantiateInWorldSpace);
            var result = await handle.OrError();
            if (result.IsError(out FastResult<T> fail))
            {
                return fail;
            }

            var instance = handle.Result;
            var component = instance.GetComponent<T>();
            return FastResult<T>.Ok(component);
        }

        public static async UniTask<FastResult<(ByteSize, IList<IResourceLocation>)>> GetDownloadSizeAsync(
            string key,
            CancellationToken ct = default
        )
        {
            // NOTE: fast 모드에서는 List, 아닐때는 [] 
            var locationsHandle = Addressables.LoadResourceLocationsAsync(key);
            var locationsResult = await locationsHandle.OrError();
            if (locationsResult.IsError(out FastResult<(ByteSize, IList<IResourceLocation>)> fail))
            {
                return fail;
            }

            var locations = locationsResult.Value;
            if (locations is {Count: 0})
            {
                return FastResult<(ByteSize, IList<IResourceLocation>)>.Fail("Download.EmptyLocations");
            }

            var sizeHandle = Addressables.GetDownloadSizeAsync(locations);
            var sizeResult = await sizeHandle.OrError();
            if (sizeResult.IsError(out fail))
            {
                return fail;
            }

            var size = sizeResult.Value;
            return FastResult<(ByteSize, IList<IResourceLocation>)>.Ok((size, locations));
        }

        public static async UniTask<FastResult<Void>> DownloadLocationsAsync(
            IList<IResourceLocation> locations,
            CancellationToken ct = default,
            IProgress<DownloadStatus> progress = null
        )
        {
            if (locations is {Count: 0})
            {
                return FastResult<Void>.Fail("Download.EmptyLocations");
            }

            var handle = Addressables.DownloadDependenciesAsync(locations);
            var result = await handle.OrError(progress);
            handle.Release();
            return result.IsError(out FastResult<Void> fail) ? fail : FastResult.Ok;
        }
    }
}