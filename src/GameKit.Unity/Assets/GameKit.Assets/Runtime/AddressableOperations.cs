using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Assets.Extensions;
using GameKit.Common.Results;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameKit.Assets
{
    public static class AddressableOperations
    {
        public static async UniTask<FastResult<IResourceLocator>> InitializeAsync(
            CancellationToken ct = default)
        {
            var handle = Addressables.InitializeAsync(false);
            var snapshot = await handle.CaptureWithRelease();
            return snapshot.IsSuccess
                ? FastResult<IResourceLocator>.Ok(snapshot.Result)
                : FastResult<IResourceLocator>.Fail("Addressables.InitializeFailed");
        }

        public static async UniTask<FastResult<List<string>>> CheckCatalog(
            CancellationToken ct = default)
        {
            var handle = Addressables.CheckForCatalogUpdates(false);
            var snapshot = await handle.CaptureWithRelease();
            if (snapshot.Status == AsyncOperationStatus.Succeeded)
            {
                return snapshot.Result.Any()
                    ? FastResult<List<string>>.Fail("Catalog.UpToDate")
                    : FastResult<List<string>>.Ok(snapshot.Result);
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
            return FastResult<List<string>>.Fail(error);
        }

        public static async UniTask<FastResult<T>> InstantiateAsync<T>(
            string key,
            Transform parent = null,
            bool instantiateInWorldSpace = false
        )
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return FastResult<T>.Fail("Download.InvalidKey");
            }

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

        public static async UniTask<FastResult<DownloadManifest>> GetDownloadManifestAsync(
            string key,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return FastResult<DownloadManifest>.Fail("DownloadManifest.InvalidKey");
            }

            // NOTE: fast 모드에서는 List, 아닐때는 [] 
            var locationsHandle = Addressables.LoadResourceLocationsAsync(key);
            var locationsResult = await locationsHandle.OrError();
            if (locationsResult.IsError(out FastResult<DownloadManifest> fail))
            {
                return fail;
            }

            var locations = locationsResult.Value;
            if (locations is { Count: 0 })
            {
                return FastResult<DownloadManifest>.Fail("DownloadManifest.EmptyLocations");
            }

            var sizeHandle = Addressables.GetDownloadSizeAsync(locations);
            var sizeResult = await sizeHandle.OrError();
            if (sizeResult.IsError(out fail))
            {
                return fail;
            }

            var size = sizeResult.Value;
            return FastResult<DownloadManifest>.Ok(new(size, locations));
        }

        public static async UniTask<FastResult<DownloadManifest>> DownloadAsync(
            DownloadManifest manifest,
            IProgress<DownloadStatus> progress = null,
            CancellationToken ct = default
        )
        {
            var handle = Addressables.DownloadDependenciesAsync(manifest.Locations);
            var result = await handle.OrError(progress);
            handle.Release();
            if (result.IsError(out FastResult<DownloadManifest> fail))
            {
                return fail;
            }

            return FastResult<DownloadManifest>.Ok(manifest with
            {
                Size = 0
            });
        }
    }
}