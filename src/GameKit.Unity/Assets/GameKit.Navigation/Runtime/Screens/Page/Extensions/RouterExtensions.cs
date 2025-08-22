using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameKit.Navigation.Screens.Page.Commands;
using VitalRouter;

namespace GameKit.Navigation.Screens.Page.Extensions
{
    public static class RouterExtensions
    {
        public static UniTaskVoid ToPage(this Router router, string pageId)
        {
            router.ToPageAsync(pageId);
            return new UniTaskVoid();
        }
        
        public static UniTaskVoid PushPage(this Router router, string pageId)
        {
            router.PushPageAsync(pageId);
            return new UniTaskVoid();
        }
        
        public static UniTaskVoid ReplacePage(this Router router, string pageId)
        {
            router.ReplacePageAsync(pageId);
            return new UniTaskVoid();
        }

        public static ValueTask ToPageAsync(
            this Router router,
            string pageId,
            CancellationToken ct = default
        )
        {
            return router.PublishAsync(new ToPageCommand(pageId), ct);
        }

        public static ValueTask PushPageAsync(this Router router, string pageId, CancellationToken ct = default)
        {
            return router.PublishAsync(new PushPageCommand(pageId), ct);
        }

        public static ValueTask ReplacePageAsync(this Router router, string pageId, CancellationToken ct = default)
        {
            return router.PublishAsync(new ReplacePageCommand(pageId), ct);
        }

        public static ValueTask BackPageAsync(this Router router, CancellationToken ct = default)
        {
            return router.PublishAsync(new BackPageCommand(), ct);
        }
    }
}