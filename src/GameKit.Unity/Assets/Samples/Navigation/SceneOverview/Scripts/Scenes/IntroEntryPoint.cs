using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Assets;
using GameKit.Common.Results;
using GameKit.Navigation.Scenes;
using GameKit.Navigation.Scenes.Commands;
using GameKit.Navigation.Scenes.Extensions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using VContainer.Unity;
using VitalRouter;

namespace Samples.Navigation.SceneOverview.Scenes
{
    // https://docs.unity3d.com/Packages/com.unity.addressables@2.6/manual/remote-content-assetbundle-cache.html
    // 참조 되지 않는 캐시 항목 제거
    // var result = await Addressables.CleanBundleCache().Task.AsUniTask();
    // Debug.Log($"Addressables.CleanBundleCache: {result}");
    public class IntroEntryPoint : ITickable, IProgress<DownloadStatus>
    {
        private readonly Router _router;

        public IntroEntryPoint(Router router)
        {
            _router = router;
        }

        public void Tick()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Debug.Log("Enter to async");
                _router.ToScene("/title");
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ManualToSceneAsync("/title", Application.exitCancellationToken).Forget();
            }
        }

        private async UniTaskVoid ManualToSceneAsync(string label, CancellationToken ct)
        {
            // {
            //     var plan = await new ToScenePlanBuilder(label)
            //         .CheckCatalog()
            //         .GetDownloadSize(new { }, static async (state, ct) =>
            //         {
            //             return FastResult.Ok;
            //         })
            //         .AppendTransition()
            //         .DownloadLocations(new { }, static async (state, ct) =>
            //         {
            //             return FastResult.Ok;
            //         })
            //         .BuildAsync(ct);
            //     Debug.Log(plan);
            // }

            // Addressable로 ToScene에 필요한 흐름제어 구성 후
            // ToScenePlan을 사용하여 씬 전환을 구현
            // 추후 ToScenePlanBuilder를 통해 흐름제어와 ToScene 연동

            ByteSize downloadSize = 0;
            List<IResourceLocation> downloadLocations = new List<IResourceLocation>();

            // Check catalog
            if (true)
            {
                var catalogResult = await AddressableOperations.CheckCatalog(ct);
                if (catalogResult.IsError)
                {
                    Debug.LogError($"Failed to check catalog: {catalogResult}");
                    return;
                }

                if (catalogResult.Value.Any())
                {
                    Debug.LogWarning("Catalog is up to date.");
                    return;
                }

                Debug.Log("Successfully checked catalog.");
            }

            // Check label
            {
                var sizeResult = await AddressableOperations.GetDownloadSizeAsync(label, ct);
                if (sizeResult.IsError)
                {
                    Debug.LogError($"Failed to get download size for '{label}': {sizeResult}");
                    return;
                }

                var (size, locations) = sizeResult.Value;

                if (!locations.Any())
                {
                    Debug.LogWarning($"No locations found for '{label}'.");
                    return;
                }

                if (size > 0)
                {
                    Debug.LogWarning("Download size is greater than 0. question: Do you want to download?");
                    await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Return), cancellationToken: ct);
                }

                downloadSize = size;
                downloadLocations.AddRange(locations);
                Debug.Log($"valid label. '{label}': {size} bytes");
                Debug.Log($"Locations: {string.Join(", ", downloadLocations.Select(loc => loc.PrimaryKey))}");
            }

            // Transition
            int? transitionIndex = null;
            {
                var transitionLabel = $"{label}:transition";
                IResourceLocation transitionLocation = null;
                var transitionSizeResult = await AddressableOperations.GetDownloadSizeAsync(transitionLabel, ct);
                if (!transitionSizeResult.IsError)
                {
                    transitionLocation = transitionSizeResult.Value.Item2.FirstOrDefault();
                }

                if (transitionLocation is null)
                {
                    transitionLabel = "/:transition";
                    transitionSizeResult = await AddressableOperations.GetDownloadSizeAsync(transitionLabel, ct);
                    if (!transitionSizeResult.IsError)
                    {
                        transitionLocation = transitionSizeResult.Value.Item2.FirstOrDefault();
                    }
                }

                if (transitionLocation is not null)
                {
                    Debug.Log("Found transition label");
                    downloadLocations.Add(transitionLocation);
                    transitionIndex = downloadLocations.Count - 1;
                }
            }

            Debug.Log($"Final Locations: {string.Join(", ", downloadLocations.Select(loc => loc.PrimaryKey))}");

            var isDownloaded = false;
            // Download
            {
                var downloadResult =
                    await AddressableOperations.DownloadLocationsAsync(downloadLocations, ct, this);
                if (downloadResult.IsError)
                {
                    Debug.LogError($"Failed to download locations for '{label}': {downloadResult}");
                    return;
                }

                Debug.Log($"Successfully downloaded locations for '{label}'.");
                isDownloaded = true;
            }

            // Present
            {
                var plan = new ToScenePlanCommand(label, downloadLocations)
                {
                    DownloadSize = downloadSize,
                    IsDownloaded = true,
                    TransitionIndex = transitionIndex
                };
                _ = _router.PublishAsync(plan, ct);
            }
        }

        public void Report(DownloadStatus value)
        {
            ByteSize downloaded = value.DownloadedBytes;
            ByteSize total = value.TotalBytes;
            Debug.Log($"Download status: {downloaded}/{total} ({value.Percent:P2})");
        }
    }
}