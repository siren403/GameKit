namespace GameKit.SceneLauncher
{
    public interface IAliases
    {
        string this[string key] { get; set; }
        void Clear();
    }
}
