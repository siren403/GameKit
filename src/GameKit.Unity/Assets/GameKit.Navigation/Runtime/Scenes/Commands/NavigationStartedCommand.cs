using VitalRouter;

namespace GameKit.Navigation.Scenes.Commands
{
    public readonly struct NavigationStartedCommand : ICommand
    {
        public readonly string Label { get; init; }
    }

}