using VitalRouter;

namespace GameKit.Navigation.Scenes.Commands
{
    public readonly struct TransitionStartedCommand : ICommand
    {
        public readonly string Path { get; init; }
    }

}