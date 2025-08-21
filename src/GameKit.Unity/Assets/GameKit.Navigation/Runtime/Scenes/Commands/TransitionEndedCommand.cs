using VitalRouter;

namespace GameKit.Navigation.Scenes.Commands
{
    public readonly struct TransitionEndedCommand : ICommand
    {
        public readonly string Label { get; init; }
    }
}