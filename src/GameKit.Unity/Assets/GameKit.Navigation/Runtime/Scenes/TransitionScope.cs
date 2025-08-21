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