using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using VitalRouter.VContainer;

namespace Samples.Navigation.SceneOverview
{
    public class TransitionSceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<Image>();
            builder.RegisterVitalRouter(routing =>
            {
                routing.MapEntryPoint<TransitionPresenter>();
            });
        }
    }
}