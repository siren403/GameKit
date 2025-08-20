namespace GameKit.Navigation.Scenes
{
    public record NavigatorOptions
    {
        public readonly string Root = "/";
        public bool StartupRoot { get; init; } = true;
        public string EntryPath { get; init; } = string.Empty;
    }
}