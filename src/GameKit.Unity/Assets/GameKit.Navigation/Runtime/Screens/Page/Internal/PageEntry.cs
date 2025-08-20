using System;
using VitalRouter;

namespace GameKit.Navigation.Screens.Page.Internal
{
    public readonly struct PageEntry : IDisposable
    {
        public readonly string Id;
        public readonly IPage Page;
        public readonly Router Router;

        public PageEntry(string id, IPage page, Router router)
        {
            Id = id;
            Page = page ?? throw new ArgumentNullException(nameof(page), "Page cannot be null.");
            Router = router ?? throw new ArgumentNullException(nameof(router), "Router cannot be null.");
        }

        public void Deconstruct(out string id, out IPage page, out Router router)
        {
            id = Id;
            page = Page;
            router = Router;
        }

        public void Dispose()
        {
            Router?.Dispose();
        }
    }
}