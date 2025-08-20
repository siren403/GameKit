using GameKit.Navigation.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VitalRouter.VContainer;

namespace Samples.Navigation.SceneOverview
{
    public class MainSceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterSceneNavigator(navigator => { navigator.StartupRootOnlyMainScene("/intro"); });
            builder.RegisterVitalRouter(routing => { });
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
}