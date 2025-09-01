using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using VitalRouter;

namespace GameKit.Navigation.Screens.Core.Internal
{
    internal class ScreenLauncher<TScreen, TLayer, TProps> : IScreenLauncher<TScreen, TProps>
        where TScreen : IScreen, IScreenProps<TProps>
        where TLayer : IScreen
    {
        private readonly Router _router;
        private readonly string _pageId;
        private readonly ScreenRegistry<TLayer> _registry;
        private readonly ScreenPresenter<TLayer> _presenter;
        private readonly BindingContext _context;

        public ScreenLauncher(
            Router router,
            string pageId,
            ScreenRegistry<TLayer> registry,
            ScreenPresenter<TLayer> presenter
        )
        {
            _router = router;
            _pageId = pageId;
            _registry = registry;
            _presenter = presenter;
            _context = new BindingContext(_router);
        }

        public async UniTask PublishAsync<TCommand>(
            TCommand command, TProps props,
            CancellationToken ct = default
        ) where TCommand : ICommand
        {
            var screenResult = await _registry.GetScreenAsync(_pageId);
            if (screenResult.IsError)
            {
                return;
            }

            var (_, screen, _) = screenResult.Value;
            if (screen is TScreen typedScreen)
            {
                typedScreen.Props = props;
            }
#if UNITY_EDITOR
            else
            {
                UnityEngine.Debug.LogWarning($"Screen is not of type {typeof(TScreen).Name}");
            }
#endif
            await _router.PublishAsync(command, ct);
        }

        public async UniTask PublishAsync<TCommand, TState>(
            TCommand command, TProps props,
            TState state,
            Action<TState, TScreen, IBindingContext> binding,
            CancellationToken ct = default
        ) where TCommand : ICommand
        {
            var screenResult = await _registry.GetScreenAsync(_pageId);
            if (screenResult.IsError)
            {
                return;
            }

            var (_, screen, _) = screenResult.Value;
            if (screen is not TScreen typedScreen)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"Screen is not of type {typeof(TScreen).Name}");
#endif
                return;
            }

            typedScreen.Props = props;

            _context.Initialize();
            using (_context)
            {
                binding.Invoke(state, typedScreen, _context);
                await _router.PublishAsync(command, ct);
                await UniTask.WaitWhile((_presenter, _pageId), static state =>
                {
                    var (presenter, pageId) = state;
                    return presenter.IsRendering(pageId);
                }, cancellationToken: ct);
            }
        }


        public async UniTask PublishAsync<TCommand>(
            TCommand command,
            TProps props,
            Action<TScreen, IBindingContext> binding,
            CancellationToken ct = default
        ) where TCommand : ICommand
        {
            var screenResult = await _registry.GetScreenAsync(_pageId);
            if (screenResult.IsError)
            {
                return;
            }

            var (_, screen, _) = screenResult.Value;
            if (screen is not TScreen typedScreen)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"Screen is not of type {typeof(TScreen).Name}");
#endif
                return;
            }

            typedScreen.Props = props;

            _context.Initialize();
            using (_context)
            {
                binding.Invoke(typedScreen, _context);
                await _router.PublishAsync(command, ct);
                await UniTask.WaitWhile((_presenter, _pageId), static state =>
                {
                    var (presenter, pageId) = state;
                    return presenter.IsRendering(pageId);
                }, cancellationToken: ct);
            }
        }
    }
}