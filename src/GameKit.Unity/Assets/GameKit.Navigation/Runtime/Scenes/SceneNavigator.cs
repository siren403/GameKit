// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Assets;
using GameKit.Assets.Extensions;
using GameKit.Navigation.Scenes.Commands;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using VitalRouter;

namespace GameKit.Navigation.Scenes
{
    [Routes(CommandOrdering.Drop)]
    internal partial class SceneNavigator
    {
        private readonly NavigatorOptions _options;
        private readonly Router _router;
        private readonly HashSet<string> _loadedScenesCache = new();
        private readonly List<AsyncOperationHandle<SceneInstance>> _loadedSceneHandleCache = new();

        private string _loadedLocation;
        private readonly Queue<IResourceLocation> _loadedLocations = new();

        public SceneNavigator(NavigatorOptions options, Router router)
        {
            _options = options;
            _router = router;
            _loadedLocation = _options.Root;
        }

        [Route]
        [Filter(typeof(CheckCatalog))]
        private async UniTask On(ToSceneCommand command, PublishContext context)
        {
            var label = command.Label;
            var ct = context.CancellationToken;

            var planResult = await ToScenePlanCommand.CreateUsingDownloadManifestAsync(label, ct: ct);
            if (planResult.IsError)
            {
                Debug.LogError($"Failed to create ToScenePlanCommand: {planResult}");
                return;
            }

            // TODO: ILogger<SceneNavigator>
            Debug.Log($"Plan Manifest: {planResult.Value.Manifest}");

            planResult = await ToScenePlanCommand.ToDownloadLocationsAsync(planResult.Value, ct: ct);
            if (planResult.IsError)
            {
                Debug.LogError($"Failed to download locations: {planResult}");
                return;
            }

            await InternalToScenePlanAsync(planResult.Value, ct);
        }


        [Route]
        private async UniTask On(ToScenePlanCommand command, PublishContext context)
        {
            await InternalToScenePlanAsync(command, context.CancellationToken);
        }

        private async UniTask InternalToScenePlanAsync(ToScenePlanCommand command, CancellationToken ct = default)
        {
            var (label, manifest, transitionLocation) = command;
            await _router.PublishAsync(new NavigationStartedCommand()
            {
                Label = label
            }, ct);

            if (!manifest.IsDownloaded)
            {
                var downloadResult = await AddressableOperations.DownloadAsync(manifest, ct: ct);
                if (downloadResult.IsError)
                {
                    // TODO: SceneErrorCommand
                    return;
                }
            }

#if UNITY_EDITOR
            if (_loadedLocation == _options.Root)
            {
                await UnloadUnmanagedScenes();
            }
#endif
            await using (await TransitionScope.CreateAsync(label, transitionLocation, _router, ct))
            {
                if (_loadedLocation != _options.Root)
                {
                    await UnloadAsync();
                }

                _loadedLocation = string.Empty;

                await LoadAsync();
                _loadedLocation = label;
            }

            await _router.PublishAsync(new NavigationEndedCommand()
            {
                Label = label
            }, ct);

            return;

            async UniTask UnloadAsync()
            {
                GetLoadedScenes(_loadedScenesCache);

                while (_loadedLocations.TryDequeue(out var location))
                {
                    string locationString = location.ToString();
                    if (!_loadedScenesCache.Contains(locationString))
                    {
                        continue;
                    }

                    var operation = SceneManager.UnloadSceneAsync(locationString);
                    if (operation == null)
                    {
                        continue;
                    }

                    await operation;
                }
            }

            async UniTask LoadAsync()
            {
                GetLoadedScenes(_loadedScenesCache);
                _loadedSceneHandleCache.Clear();
                // TODO: 리팩토링 필요
                foreach (IResourceLocation location in manifest.Locations
                             .Where(location => transitionLocation != location)
                             .Where(location => location.ResourceType == typeof(SceneInstance)))
                {
                    string locationString = location.ToString();
                    if (_loadedScenesCache.Contains(locationString))
                    {
                        continue;
                    }

                    AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(
                        location,
                        LoadSceneMode.Additive,
                        SceneReleaseMode.ReleaseSceneWhenSceneUnloaded,
                        false
                    );

                    var loadSceneResult = await handle.OrError();
                    if (loadSceneResult.IsError)
                    {
                        // TODO: SceneErrorCommand, Restore previous state
                        Debug.LogError($"Failed to load scene '{locationString}': {loadSceneResult}");
                        break;
                    }

                    _loadedSceneHandleCache.Add(handle);
                    _loadedLocations.Enqueue(location);
                }

                foreach (AsyncOperationHandle<SceneInstance> handle in _loadedSceneHandleCache)
                {
                    await handle.Result.ActivateAsync();
                }

                _loadedSceneHandleCache.Clear();
            }
        }


        /// <summary>
        /// MainScene에서 시작했을때 다른 씬 언로드에 사용
        /// </summary>
        private async UniTask UnloadUnmanagedScenes()
        {
            for (var i = 0; i < SceneManager.loadedSceneCount; ++i)
            {
                var unmanagedScene = SceneManager.GetSceneAt(i);
                if (unmanagedScene.buildIndex == 0)
                {
                    continue;
                }

                await SceneManager.UnloadSceneAsync(unmanagedScene);
            }
        }

        private void GetLoadedScenes(in HashSet<string> cache)
        {
            cache.Clear();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                cache.Add(SceneManager.GetSceneAt(i).path);
            }
        }
    }
}