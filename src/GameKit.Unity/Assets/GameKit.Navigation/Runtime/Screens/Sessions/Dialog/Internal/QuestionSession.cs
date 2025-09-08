using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Navigation.Screens.Core.Internal;
using GameKit.Navigation.Screens.Dialog;
using GameKit.Navigation.Screens.Dialog.Commands;
using R3;
using VitalRouter;

namespace GameKit.Navigation.Screens.Sessions.Dialog.Internal
{
    internal sealed class QuestionSession<TDialog, TResult> : IQuestionSession<TDialog, TResult>, IDisposable
        where TDialog : IDialog
    {
        private readonly string _id;
        private readonly ScreenRegistry<IDialog> _registry;
        private readonly Subject<TResult> _stream = new();

        private readonly Router _router;
        public QuestionSession(
            Router router,
            string id,
            ScreenRegistry<IDialog> registry
        )
        {
            _router = router;
            _id = id;
            _registry = registry;
        }

        public async UniTask<TResult> ExecuteAsync(
            Action<TDialog, AnswerBinder<TResult>> binding,
            CancellationToken ct = default
        )
        {
            var screenResult = await _registry.GetScreenAsync(_id);
            if (screenResult.IsError || screenResult.Value.Screen is not TDialog dialog)
            {
                return default!;
            }

            var task = _stream.FirstAsync(cancellationToken: ct);
            using var binder = new AnswerBinder<TResult>(_stream);
            binding.Invoke(dialog, binder);
            return await task;
        }

        public async UniTask<TResult> ShowAsync(
            Action<TDialog, AnswerBinder<TResult>> binding,
            CancellationToken ct = default
        )
        {
            await _router.PublishAsync(new ShowDialogCommand(_id), ct);
            var result = await ExecuteAsync(binding, ct);
            await _router.PublishAsync(new HideDialogCommand(), ct);
            return result;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}