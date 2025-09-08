using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Navigation.Screens.Core.Internal;
using GameKit.Navigation.Screens.Page;
using GameKit.Navigation.Screens.Page.Commands;
using VitalRouter;

namespace GameKit.Navigation.Screens.Sessions.Page.Internal
{
    internal sealed class PageSession<TPage> : IPageSession<TPage> where TPage : IPage
    {
        private readonly string _id;
        private readonly ScreenRegistry<IPage> _registry;
        private readonly Router _router;

        public PageSession(
            Router router,
            string id,
            ScreenRegistry<IPage> registry
        )
        {
            _router = router;
            _id = id;
            _registry = registry;
        }

        public async UniTask PushAsync(Action<TPage, PageBinder> binding, CancellationToken ct = default)
        {
            var screenResult = await _registry.GetScreenAsync(_id);
            if (screenResult.IsError || screenResult.Value.Screen is not TPage page)
            {
                return;
            }

            using var binder = new PageBinder(_router);
            binding.Invoke(page, binder);
            await _router.PublishAsync(new PushPageCommand(_id), ct);
        }

        public async UniTask PushAsync<TState>(TState state, Action<TPage, TState, PageBinder> binding,
            CancellationToken ct = default)
        {
            var screenResult = await _registry.GetScreenAsync(_id);
            if (screenResult.IsError || screenResult.Value.Screen is not TPage page)
            {
                return;
            }

            using var binder = new PageBinder(_router);
            binding.Invoke(page, state, binder);
            await _router.PublishAsync(new PushPageCommand(_id), ct);
        }
    }
}