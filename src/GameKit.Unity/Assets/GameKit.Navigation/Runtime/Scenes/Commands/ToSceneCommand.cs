using VitalRouter;

namespace GameKit.Navigation.Scenes.Commands
{
    public readonly struct ToSceneCommand : ICommand
    {
        public string Label { get; init; }
    }
}