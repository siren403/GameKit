// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using Void = GameKit.Common.Results.Void;

namespace GameKit.Navigation.Scenes
{
    public readonly struct ToScenePlanCommand : ICommand
    {
        public readonly string Label;
        public readonly IList<IResourceLocation> Locations;
        public ByteSize DownloadSize { get; init; }
        public bool IsDownloaded { get; init; }
        public int? TransitionIndex { get; init; }

        public IResourceLocation TransitionLocation =>
            TransitionIndex.HasValue ? Locations[TransitionIndex.Value] : null;

        public ToScenePlanCommand(
            string label,
            IList<IResourceLocation> locations
        )
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentException("Label cannot be null or whitespace.", nameof(label));
            }

            if (locations is { Count: 0 })
            {
                throw new ArgumentException("Locations cannot be empty.", nameof(locations));
            }

            Label = label;
            Locations = locations;
            DownloadSize = 0;
            IsDownloaded = false;
            TransitionIndex = null;
        }

        public void Deconstruct(
            out string label,
            out IList<IResourceLocation> locations,
            out bool isDownloaded,
            out IResourceLocation transitionLocation
        )
        {
            label = Label;
            locations = Locations;
            isDownloaded = IsDownloaded;
            transitionLocation = TransitionLocation;
        }
    }

    public readonly struct ToSceneCommand : ICommand
    {
        public string Label { get; init; }
    }

    public static class RouterExtensions
    {
        public static void ToScene(this Router router, string label)
        {
            if (router == null)
            {
                throw new ArgumentNullException(nameof(router));
            }

            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("Label cannot be null or empty.", nameof(label));
            }

            router.PublishAsync(new ToSceneCommand { Label = label });
        }
    }

    [Routes(CommandOrdering.Drop)]
    internal partial class SceneNavigator
    {
        private readonly NavigatorOptions _options;
        private readonly Router _router;
        private readonly HashSet<string> _loadedScenesCache = new();
        private readonly List<AsyncOperationHandle<SceneInstance>> _loadedSceneHandleCache = new();

        public string LoadedLocation { get; private set; }
        private Queue<IResourceLocation> _loadedLocations = new();

        public SceneNavigator(NavigatorOptions options, Router router)
        {
            _options = options;
            _router = router;
            LoadedLocation = _options.Root;
        }

        private async UniTask<FastResult<Void>> DownloadAsync(
            IList<IResourceLocation> locations,
            IProgress<DownloadStatus> progress = null,
            CancellationToken ct = default
        )
        {
            if (locations is { Count: 0 })
            {
                return FastResult<Void>.Fail("Download.EmptyLocations");
            }

            var handle = Addressables.DownloadDependenciesAsync(locations, autoReleaseHandle: true);
            var result = await handle.OrError(progress);
            return result.IsError(out FastResult<Void> fail) ? fail : FastResult.Ok;
        }

        private readonly struct TransitionScope : IAsyncDisposable
        {
            public static async UniTask CreateAsync(string label, CancellationToken ct = default)
            {
            }

            public async ValueTask DisposeAsync()
            {
            }
        }

        [Route]
        private async UniTask On(ToScenePlanCommand command, PublishContext context)
        {
            var (label, locations, downloaded, transitionLocation) = command;
            var ct = context.CancellationToken;
            await _router.PublishAsync(new NavigationStartedCommand()
            {
                Label = label
            }, ct);

            if (!downloaded)
            {
                var downloadResult = await AddressableOperations.DownloadLocationsAsync(locations, ct);
                if (downloadResult.IsError)
                {
                    // TODO: SceneErrorCommand
                    return;
                }
            }

            SceneInstance? transition = null;

            if (transitionLocation != null)
            {
                var handle = Addressables.LoadSceneAsync(
                    transitionLocation,
                    LoadSceneMode.Additive,
                    SceneReleaseMode.ReleaseSceneWhenSceneUnloaded,
                    false
                );
                await handle.Task.AsUniTask();
                await handle.Result.ActivateAsync();
                transition = handle.Result;
                await _router.PublishAsync(new TransitionStartedCommand()
                {
                    Label = label
                }, ct);
            }


            if (LoadedLocation != _options.Root)
            {
                await UnloadAsync();
                LoadedLocation = string.Empty;
            }

            await LoadAsync();
            LoadedLocation = label;

            if (transition.HasValue)
            {
                await _router.PublishAsync(new TransitionEndedCommand()
                {
                    Label = label
                }, ct);
                await SceneManager.UnloadSceneAsync(transition.Value.Scene);
                transition = null;
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
                foreach (IResourceLocation location in locations
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
        /// MainScene에서 시작했을때 다른씬 언로드에 사용
        /// </summary>
        [Obsolete]
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

        [Route]
        private async UniTask On(ToSceneCommand command, PublishContext context)
        {
            var label = command.Label;
            await _router.PublishAsync(new NavigationStartedCommand()
            {
                Label = label
            });


            IList<IResourceLocation> locations = await Addressables.LoadResourceLocationsAsync(label).Task.AsUniTask();

            if (locations.Count == 0)
            {
                LoadedLocation = label;
                return;
            }

            ByteSize size = await Addressables.GetDownloadSizeAsync(locations).Task.AsUniTask();
            Debug.Log($"Size of locations for path '{label}': {size} bytes");
            if (size > 0)
            {
                var result = await DownloadAsync(locations);
                if (result.IsError)
                {
                    Debug.LogError($"Failed to load location '{label}': {result}");
                    return;
                }
            }

            if (LoadedLocation != _options.Root)
            {
                await UnloadRouteAsync();
                LoadedLocation = string.Empty;
            }

            await LoadRouteAsync(locations);
            LoadedLocation = label;

            await _router.PublishAsync(new NavigationEndedCommand()
            {
                Label = label
            });
        }

        private async UniTask UnloadRouteAsync()
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

        private async UniTask LoadRouteAsync(IList<IResourceLocation> locations)
        {
            if (locations.Count == 0)
            {
                return;
            }

            GetLoadedScenes(_loadedScenesCache);

            _loadedSceneHandleCache.Clear();

            foreach (IResourceLocation location in locations
                         .Where(location => location.ResourceType == typeof(SceneInstance)))
            {
                string locationString = location.ToString();
                if (_loadedScenesCache.Contains(locationString))
                {
                    continue;
                }

                AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(location,
                    LoadSceneMode.Additive,
                    SceneReleaseMode.ReleaseSceneWhenSceneUnloaded,
                    false
                );
                await handle.Task;

                _loadedSceneHandleCache.Add(handle);
                _loadedLocations.Enqueue(location);
            }

            foreach (AsyncOperationHandle<SceneInstance> handle in _loadedSceneHandleCache)
            {
                await handle.Result.ActivateAsync();
            }

            _loadedSceneHandleCache.Clear();
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