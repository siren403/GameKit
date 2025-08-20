namespace GameKit.SceneLauncher
{
    public sealed record LaunchedContext
    {
        private LaunchedContext()
        {
            Options = LaunchOptions.Create();
        }

        public LaunchOptions Options { get; private init; }

        internal static LaunchedContext FromOptions(LaunchOptions options)
        {
            return new LaunchedContext { Options = options };
        }
    }
}
