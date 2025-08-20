using VitalRouter;

namespace GameKit.Navigation.Scenes.Commands
{
    public readonly struct NavigationEndedCommand : ICommand
    {
        public readonly string Path { get; init; }
    }

}