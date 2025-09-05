using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Navigation.Screens.Core.Internal;
using GameKit.Navigation.Screens.Dialog;
using R3;
using VitalRouter;

namespace GameKit.Navigation.Screens.Sessions.Dialog.Internal
{
    internal sealed class ReviewSession<TDialog, TState> : IReviewSession<TDialog, TState>, IDisposable
        where TDialog : IDialog
    {
        private readonly Router _router;
        private readonly string _id;
        private readonly ScreenRegistry<IDialog> _registry;
        private readonly ScreenPresenter<IDialog> _presenter;
        private readonly ReactiveProperty<TState> _state = new();
        private readonly Subject<(bool approved, TState state)> _stream = new();


        public ReviewSession(
            Router router,
            string id,
            ScreenRegistry<IDialog> registry,
            ScreenPresenter<IDialog> presenter
        )
        {
            _router = router;
            _id = id;
            _registry = registry;
            _presenter = presenter;
        }

        public async UniTask<(bool approved, TState state)> ExecuteAsync(
            TState initialState,
            Action<TDialog, ReviewBinder<TState>> binding,
            CancellationToken ct = default
        )
        {
            var screenResult = await _registry.GetScreenAsync(_id);
            if (screenResult.IsError || screenResult.Value.Screen is not TDialog dialog)
            {
                return default;
            }

            _state.Value = initialState;
            var task = _stream.FirstAsync(cancellationToken: ct);
            using var binder = new ReviewBinder<TState>(initialState, _state, _stream);
            binding.Invoke(dialog, binder);
            return await task;
        }

        public void Dispose()
        {
            _state.Dispose();
            _stream.Dispose();
        }
    }
}