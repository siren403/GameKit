using System;
using System.Threading;
using System.Threading.Tasks;
using GameKit.Navigation.Screens.Page.Commands;
using R3;
using VitalRouter;

namespace GameKit.Navigation.Screens.Sessions.Page
{
    public struct PageBinder : IDisposable
    {
        private readonly Router _router;
        private DisposableBag _disposable;

        public PageBinder(Router router)
        {
            _router = router;
            _disposable = new DisposableBag();
        }

        public void Bind<T>(Observable<T> source, Action<T> action)
        {
            source.Subscribe(action,
                static (value, action) => action(value)
            ).AddTo(ref _disposable);
        }

        public void Bind<T>(Observable<T> source, Func<T, CancellationToken, ValueTask> action)
        {
            source.SubscribeAwait(action, async static (value, action, ct) =>
            {
                await action.Invoke(value, ct); //
            }, AwaitOperation.Drop).AddTo(ref _disposable);
        }

        public void Back<T>(Observable<T> source)
        {
            source.SubscribeAwait(_router, async static (_, router, ct) =>
            {
                await router.PublishAsync(new BackPageCommand(), ct); //
            }, AwaitOperation.Drop).AddTo(ref _disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}