using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Assets;
using GameKit.Navigation.Scenes.Commands;
using GameKit.Navigation.Scenes.Extensions;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
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
            // Check catalog
            var catalogResult = await AddressableOperations.CheckCatalog(ct);
            if (catalogResult.IsError)
            {
                Debug.LogError($"Failed to check catalog: {catalogResult}");
                return;
            }

            Debug.Log("Successfully checked catalog.");

            var planResult = await ToScenePlanCommand.CreateUsingDownloadManifestAsync(label, ct: ct);
            if (planResult.IsError)
            {
                Debug.LogError($"Failed to create ToScenePlanCommand: {planResult}");
                return;
            }

            Debug.Log($"Plan Manifest: {planResult.Value.Manifest}");

            if (planResult.Value.Manifest.IsDownloaded == false)
            {
                Debug.Log($"Download? {planResult.Value.Manifest.Size}");
                await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Return), cancellationToken: ct);
            }

            planResult = await ToScenePlanCommand.ToDownloadLocationsAsync(planResult.Value, this, ct);
            if (planResult.IsError)
            {
                Debug.LogError($"Failed to download locations: {planResult}");
                return;
            }

            _ = _router.PublishAsync(planResult.Value, ct);
        }

        public void Report(DownloadStatus value)
        {
            ByteSize downloaded = value.DownloadedBytes;
            ByteSize total = value.TotalBytes;
            Debug.Log($"Download status: {downloaded}/{total} ({value.Percent:P2})");
        }
    }
}