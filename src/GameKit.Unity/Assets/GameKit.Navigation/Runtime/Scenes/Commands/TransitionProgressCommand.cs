using VitalRouter;

namespace GameKit.Navigation.Scenes.Commands
{
    public readonly struct TransitionProgressCommand : ICommand
    {
        public readonly string Location { get; init; }
        public readonly float Progress { get; init; }

        public override string ToString()
        {
            return $"{Location} | {Progress}";
        }
    }
}