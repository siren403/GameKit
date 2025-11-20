using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using VitalRouter.VContainer;

namespace Samples.Navigation.SceneOverview.Scenes
{
    public class TransitionScene : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<Image>();
            builder.RegisterVitalRouter(routing =>
            {
                routing.MapEntryPoint<TransitionPresenter>();
            });
            builder.RegisterComponentInHierarchy<Slider>();
        }
    }
}