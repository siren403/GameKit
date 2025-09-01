using System;
using VitalRouter;

namespace GameKit.Navigation.Screens.Core
{
    public readonly struct ScreenEntry<TScreen> : IDisposable where TScreen : IScreen
    {
        public readonly string Id;
        public readonly TScreen Screen;
        public readonly Router Router;

        public ScreenEntry(string id, TScreen screen, Router router)
        {
            Id = id;
            Screen = screen ?? throw new ArgumentNullException(nameof(screen), "Screen cannot be null.");
            Router = router ?? throw new ArgumentNullException(nameof(router), "Router cannot be null.");
        }

        public void Deconstruct(out string id, out IScreen screen, out Router router)
        {
            id = Id;
            screen = Screen;
            router = Router;
        }

        public void Dispose()
        {
            Router?.Dispose();
        }
    }
}