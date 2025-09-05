using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Navigation.Screens.Dialog;

namespace GameKit.Navigation.Screens.Sessions.Dialog
{
    public interface IQuestionSession<out TDialog, TResult> where TDialog : IDialog
    {
        UniTask<TResult> ExecuteAsync(Action<TDialog, AnswerBinder<TResult>> binding, CancellationToken ct = default);
    }
}