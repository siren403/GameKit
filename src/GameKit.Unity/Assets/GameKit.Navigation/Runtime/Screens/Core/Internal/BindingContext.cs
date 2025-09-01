using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using R3;
using VitalRouter;

namespace GameKit.Navigation.Screens.Core.Internal
{
    internal class BindingContext : IBindingContext, IDisposable
    {
        private readonly Router _router;
        private DisposableBag _disposable;
        private readonly HashSet<object> _binds = new();

        private bool _isDisposed = true;

        public BindingContext(Router router)
        {
            _router = router;
        }

        public void Initialize()
        {
            if (!_isDisposed)
            {
                throw new InvalidOperationException("This context is already initialized.");
            }

            _isDisposed = false;
            _disposable = new DisposableBag();
        }

        public void Subscribe<T, TState>(
            Observable<T> source,
            TState state,
            Func<T, TState, CancellationToken, ValueTask> action
        )
        {
            if (_binds.Contains(source))
            {
                throw new InvalidOperationException("This source is already bound.");
            }

            source.SubscribeAwait(
                state,
                action,
                AwaitOperation.Drop
            ).AddTo(ref _disposable);
            _binds.Add(source);
        }

        public void Subscribe<T>(Observable<T> source, Func<T, CancellationToken, ValueTask> action)
        {
            if (_binds.Contains(source))
            {
                throw new InvalidOperationException("This source is already bound.");
            }

            source.SubscribeAwait(
                action,
                AwaitOperation.Drop
            ).AddTo(ref _disposable);
            _binds.Add(source);
        }

        public void Subscribe<T, TCommand>(Observable<T> source, TCommand command) where TCommand : struct, ICommand
        {
            if (_binds.Contains(source))
            {
                throw new InvalidOperationException("This source is already bound.");
            }

            source.SubscribeAwait((_router, command), static (_, state, ct) =>
            {
                var (router, cmd) = state;
                return router.PublishAsync(cmd, ct);
            }, AwaitOperation.Drop).AddTo(ref _disposable);
            _binds.Add(source);
        }

        public void Dispose()
        {
            _isDisposed = true;
            _binds.Clear();
            _disposable.Dispose();
        }
    }
}