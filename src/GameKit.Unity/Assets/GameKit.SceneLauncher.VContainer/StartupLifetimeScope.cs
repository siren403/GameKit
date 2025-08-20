using VContainer.Unity;

namespace GameKit.SceneLauncher.VContainer
{
    public class StartupLifetimeScope : LaunchedLifetimeScope
    {
        protected override void Awake()
        {
            autoRun = false;
            parentReference = new ParentReference();
            base.Awake();

            var options = LaunchOptions.Create();
            options.SetExtension(this);
            StartupLauncher.Launch(options, () =>
            {
                if (ExtraInstaller != null)
                {
                    using (Enqueue(ExtraInstaller))
                    {
                        Build();
                    }
                }
                else
                {
                    Build();
                }

                ExtraInstaller = null;
            });
        }
    }
}