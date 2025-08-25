// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Assets;
using GameKit.Assets.Extensions;
using GameKit.Common.Results;
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
        private readonly HashSet<Scene> _loadedScenesCache = new();
        private readonly List<AsyncOperationHandle<SceneInstance>> _loadedSceneHandleCache = new();

        private string _loadedLocation;
        private readonly Queue<Scene> _loadedScenes = new();

        public SceneNavigator(NavigatorOptions options, Router router)
        {
            _options = options;
            _router = router;
            _loadedLocation = _options.Root;
        }

        private async UniTask RunBuiltInProcessAsync(string label, int sceneIndex, CancellationToken ct)
        {
            await using (await NavigationScope.CreateAsync(_router, label, ct))
            {
                await UnloadScenesAsync();
                var operation = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
                if (operation == null)
                {
                    // TODO: SceneErrorCommand
                    Debug.LogError($"Failed to load built-in scene '{label}' with index {sceneIndex}");
                    return;
                }

                await operation;

                _loadedScenes.Enqueue(SceneManager.GetSceneByBuildIndex(sceneIndex));
                _loadedLocation = label;
            }
        }

        [Route]
        private async UniTask On(ToSceneCommand command, PublishContext context)
        {
            var label = command.Label;
            var ct = context.CancellationToken;

            #region BuiltIn

            if (_options.BuiltInScenes.TryGetValue(label, out var sceneIndex))
            {
                await RunBuiltInProcessAsync(label, sceneIndex, ct);
                return;
            }

            #endregion

            #region Remote Setup

            var planResult = await ToScenePlanCommand.CreateReadyAsync(command.Label, ct: ct);
            if (planResult.IsError)
            {
                // TODO: SceneErrorCommand
                Debug.LogError($"Failed to create scene plan for '{label}': {planResult}");
                return;
            }

            #endregion

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

            await using (await NavigationScope.CreateAsync(_router, label, ct))
            {
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
                    await CleanupScenes();
                }
#endif
                // TODO: 트랜지션 씬의 Router에만 이벤트 발생 처리
                await using (await TransitionScope.CreateAsync(label, transitionLocation, _router, ct))
                {
                    if (_loadedLocation != _options.Root)
                    {
                        await UnloadScenesAsync();
                    }

                    _loadedLocation = string.Empty;

                    await LoadAsync(manifest.Locations
                        .Where(location => transitionLocation != location)
                        .Where(location => location.ResourceType == typeof(SceneInstance))
                    );
                    _loadedLocation = label;
                }
            }
        }

        private async UniTask LoadAsync(IEnumerable<IResourceLocation> locations)
        {
            GetLoadedScenes(_loadedScenesCache);
            _loadedSceneHandleCache.Clear();

            foreach (IResourceLocation location in locations)
            {
                if (_loadedScenesCache.Any(scene => scene.path == location.ToString()))
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
                    Debug.LogError($"Failed to load scene '{location}': {loadSceneResult}");
                    break;
                }

                _loadedSceneHandleCache.Add(handle);
                _loadedScenes.Enqueue(loadSceneResult.Value.Scene);
            }

            foreach (AsyncOperationHandle<SceneInstance> handle in _loadedSceneHandleCache)
            {
                await handle.Result.ActivateAsync();
            }

            _loadedSceneHandleCache.Clear();
        }

        /// <summary>
        /// LoadedScenes에 있지만 실제 씬으로 로드되지 않은 씬들을 제외하고 언로드한다.
        /// TODO: LoadedScenes가 항상 유요한 상태를 유지 할 수 있도록 개선 필요
        /// </summary>
        private async UniTask UnloadScenesAsync()
        {
            GetLoadedScenes(_loadedScenesCache);

            while (_loadedScenes.TryDequeue(out var scene))
            {
                if (!_loadedScenesCache.Contains(scene))
                {
                    continue;
                }

                var operation = SceneManager.UnloadSceneAsync(scene);
                if (operation == null)
                {
                    continue;
                }

                await operation;
            }
        }

        /// <summary>
        /// MainScene에서 시작했을때 다른 씬 언로드에 사용
        /// </summary>
        private async UniTask CleanupScenes()
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

        private void GetLoadedScenes(in HashSet<Scene> cache)
        {
            cache.Clear();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                cache.Add(SceneManager.GetSceneAt(i));
            }
        }
    }
}