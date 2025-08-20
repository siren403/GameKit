namespace GameKit.SceneLauncher.VContainer
{
    public static class LaunchOptionsExtensions
    {
        public const string VContainerKey = "VContainer";

        public static void SetExtension(this LaunchOptions options, StartupLifetimeScope lifetimeScope)
        {
            options.Extensions[VContainerKey] = lifetimeScope;
        }
    }
}
