using VContainer;
using VContainer.Unity;
using VitalRouter.VContainer;

namespace Samples.Navigation.SceneOverview.Scenes
{
    public class FadeScene : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<FadeCanvas>();
            builder.RegisterVitalRouter(routing => { routing.Map<FadePresenter>(); });
        }
    }
}