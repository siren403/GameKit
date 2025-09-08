using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Navigation.Screens.Page;

namespace GameKit.Navigation.Screens.Sessions.Page
{
    public interface IPageSession<out TPage> where TPage : IPage
    {
        UniTask PushAsync(Action<TPage, PageBinder> binding, CancellationToken ct = default);

        UniTask PushAsync<TState>(TState state, Action<TPage, TState, PageBinder> binding,
            CancellationToken ct = default);
    }
}