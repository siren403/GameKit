using VContainer;
using VContainer.Unity;

namespace Samples.Navigation.SceneOverview.Scenes
{
    public class IntroScene : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<IntroSceneEntryPoint>();
        }
    }
}