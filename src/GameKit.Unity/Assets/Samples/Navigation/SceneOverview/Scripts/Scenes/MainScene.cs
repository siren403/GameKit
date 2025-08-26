using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Navigation.Scenes.Commands;
using GameKit.Navigation.VContainer;
using GameKit.SceneLauncher.VContainer;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            var resolver = new SceneInstallerResolver();
            const string sampleMainScenePath = "Assets/Samples/Navigation/SceneOverview/Scenes/MainScene.unity";
            resolver.Register(sampleMainScenePath, new MainScene());

            resolver.Register("Assets/Samples/Navigation/SceneOverview/Scenes/Local/FadeScene.unity",
                new FadeScene());
            resolver.Register("Assets/Samples/Navigation/SceneOverview/Scenes/Local/InitializeScene.unity",
                new InitializeScene());
            resolver.Register("Assets/Samples/Navigation/SceneOverview/Scenes/IntroScene.unity",
                new IntroScene());
            resolver.Register("Assets/Samples/Navigation/SceneOverview/Scenes/Local/TransitionScene.unity",
                new TransitionScene());
            resolver.Register("Assets/Samples/Navigation/SceneOverview/Scenes/Remote/TitleScene.unity",
                new TitleScene());
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