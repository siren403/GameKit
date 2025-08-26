using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using R3;

namespace GameKit.Navigation.Screens.Page
{
    public interface IQuickPage<out TPage, in TProps> where TPage : IPage, IPageProps<TProps>
    {
        UniTask PushAsync(
            TProps props,
            CancellationToken ct = default
        );

        UniTask PushAsync<TState>(
            TProps props,
            TState state,
            Action<TState, TPage, IBindingContext> binding,
            CancellationToken ct = default
        );
    }

    public interface IBindingContext
    {
        void OnBack<T>(Observable<T> source);

        void OnClick<T, TState>(
            Observable<T> source,
            TState state,
            Func<T, TState, CancellationToken, ValueTask> action
        );

        void OnClick<T>(
            Observable<T> source,
            Func<T, CancellationToken, ValueTask> action
        );
    }
}