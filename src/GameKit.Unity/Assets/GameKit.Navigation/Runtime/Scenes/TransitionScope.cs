using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameKit.Navigation.Scenes.Commands;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using VitalRouter;

namespace GameKit.Navigation.Scenes
{
    public readonly struct NavigationScope : IAsyncDisposable
    {
        private readonly Router _router;
        private readonly string _label;
        private readonly CancellationToken _ct;

        public static async UniTask<NavigationScope> CreateAsync(
            Router router,
            string label,
            CancellationToken ct = default
        )
        {
            await router.PublishAsync(new NavigationStartedCommand()
            {
                Label = label
            }, ct);

            return new NavigationScope(router, label, ct);
        }

        private NavigationScope(Router router, string label, CancellationToken ct)
        {
            _router = router;
            _label = label;
            _ct = ct;
        }

        public async ValueTask DisposeAsync()
        {
            await _router.PublishAsync(new NavigationEndedCommand()
            {
                Label = _label
            }, _ct);
        }
    }

    public readonly struct TransitionScope : IAsyncDisposable
    {
        private readonly string _loadingLabel;
        private readonly SceneInstance? _instance;
        private readonly Router _router;

        public static async UniTask<TransitionScope> CreateAsync(
            string loadingLabel,
            IResourceLocation location,
            Router router,
            CancellationToken ct = default
        )
        {
            var handle = Addressables.LoadSceneAsync(
                location,
                LoadSceneMode.Additive,
                SceneReleaseMode.ReleaseSceneWhenSceneUnloaded,
                false
            );
            await handle.Task.AsUniTask();
            await handle.Result.ActivateAsync();

            await router.PublishAsync(new TransitionStartedCommand()
            {
                Label = loadingLabel
            }, ct);

            return new(loadingLabel, handle.Result, router);
        }

        private TransitionScope(string loadingLabel, SceneInstance instance, Router router)
        {
            _loadingLabel = loadingLabel;
            _instance = instance;
            _router = router;
        }

        public async ValueTask DisposeAsync()
        {
            if (_instance.HasValue)
            {
                await _router.PublishAsync(new TransitionEndedCommand()
                {
                    Label = _loadingLabel
                });
                await SceneManager.UnloadSceneAsync(_instance.Value.Scene);
            }
        }
    }
}