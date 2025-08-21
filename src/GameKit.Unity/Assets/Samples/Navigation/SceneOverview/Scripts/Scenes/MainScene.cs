using GameKit.Navigation.Scenes;
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
            resolver.Register("Assets/Samples/Navigation/SceneOverview/Scenes/Local/IntroScene.unity",
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
            });
            builder.RegisterVitalRouter(routing => { });
            builder.RegisterEntryPoint<MainEntryPoint>();
            // builder.RegisterComponentInHierarchy<FadeCanvas>();
            // builder.RegisterBuildCallback(container =>
            // {
            //     var router = container.Resolve<Router>();
            //     var fade = container.Resolve<FadeCanvas>();
            //     var ct = Application.exitCancellationToken;
            //     router.SubscribeAwait<NavigationStartedCommand>(async (command, ctx) =>
            //     {
            //         await fade.InAsync(ct: ctx.CancellationToken);
            //     }).AddTo(ct);
            //     router.SubscribeAwait<NavigationEndedCommand>(async (command, ctx) =>
            //     {
            //         await fade.OutAsync(ct: ctx.CancellationToken);
            //     }).AddTo(ct);
            // });
            // 전체 번들 캐시 제거
            var result = Caching.ClearCache();
            Debug.Log($"Caching.ClearCache: {result}");
        }
    }

    public class MainEntryPoint : IInitializable
    {
        private readonly Router _router;

        public MainEntryPoint(Router router)
        {
            _router = router;
        }

        public void Initialize()
        {
            var fromMainScene = SceneManager.GetActiveScene().buildIndex == 0;
            if (fromMainScene)
            {
                _router.ToScene("/intro");
            }
        }
    }
}