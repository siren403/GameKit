using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Navigation.Scenes.Commands;
using GameKit.Navigation.VContainer;
using GameKit.SceneLauncher.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VitalRouter;
using VitalRouter.VContainer;

namespace Samples.Navigation.SceneOverview.Scenes
{
    public class MainScene : IInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            var resolver = new SceneInstallerResolver(new MainScene());
            resolver.RegisterBuiltIn(1, new IntroScene());

            resolver.RegisterName("FadeScene", new FadeScene());
            resolver.RegisterName("InitializeScene", new InitializeScene());
            resolver.RegisterName("TransitionScene", new TransitionScene());
            resolver.RegisterName("TitleScene", new TitleScene());

            SceneScopeInitializer.Initialize(resolver);
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterSceneNavigator(navigator =>
            {
                // AutoStartup
                // navigator.AutoStartupFromMainScene("/intro");
                navigator.RegisterBuiltInScene("/intro", 1);
            });
            builder.RegisterVitalRouter(routing => { });
            builder.RegisterEntryPoint<MainEntryPoint>();
            builder.RegisterComponentInHierarchy<FadeCanvas>();
            // 전체 번들 캐시 제거
            var result = Caching.ClearCache();
            Debug.Log($"Caching.ClearCache: {result}");
        }
    }

    public class MainEntryPoint : IAsyncStartable
    {
        private readonly Router _router;
        private readonly FadeCanvas _fade;

        public MainEntryPoint(Router router, FadeCanvas fade)
        {
            _router = router;
            _fade = fade;
        }

        public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
        {
            if (SceneScopeInitializer.IsStartedFromMainScene)
            {
                await _router.PublishAsync(new ToSceneCommand()
                {
                    Label = "/intro"
                }, cancellation);
            }

            await _fade.OutAsync(ct: cancellation);
        }
    }
}