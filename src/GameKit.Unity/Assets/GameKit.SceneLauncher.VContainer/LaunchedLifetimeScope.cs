using VContainer.Unity;

namespace GameKit.SceneLauncher.VContainer
{
    public abstract class LaunchedLifetimeScope : LifetimeScope
    {
        public IInstaller? ExtraInstaller { set; protected get; }
    }
}