using System.Collections.ObjectModel;

namespace GameKit.Navigation.Scenes
{
    public record NavigatorOptions
    {
        public readonly string Root = "/";
        public ReadOnlyDictionary<string, int> BuiltInScenes { get; init; }
    }
}