using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Navigation.Screens.Dialog;

namespace GameKit.Navigation.Screens.Sessions.Dialog
{
    public interface IReviewSession<out TDialog, TState> where TDialog : IDialog
    {
        UniTask<(bool approved, TState state)> ExecuteAsync(
            TState initialState,
            Action<TDialog, DecisionBinder<TState>> binding,
            CancellationToken ct = default
        );
    }
}