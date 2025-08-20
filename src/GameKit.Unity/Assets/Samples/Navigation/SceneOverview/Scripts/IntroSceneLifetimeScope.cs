using VContainer;
using VContainer.Unity;

namespace Samples.Navigation.SceneOverview
{
    public class IntroSceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<IntroSceneEntryPoint>();
        }
    }
}