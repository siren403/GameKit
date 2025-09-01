using Cysharp.Threading.Tasks;
using GameKit.Navigation.Screens.Core.Internal;
using GameKit.Navigation.Screens.Dialog.Commands;
using Microsoft.Extensions.Logging;
using VitalRouter;

namespace GameKit.Navigation.Screens.Dialog.Internal
{
    [Routes(CommandOrdering.Drop)]
    internal sealed partial class DialogNavigator
    {
        private readonly ScreenPresenter<IDialog> _presenter;
        private readonly ScreenRegistry<IDialog> _registry;
        private readonly Router _router;
        private readonly ILogger<DialogNavigator> _logger;

        private readonly DialogStack _stack;

        public DialogNavigator(
            ScreenPresenter<IDialog> presenter,
            ScreenRegistry<IDialog> registry,
            Router router,
            ILogger<DialogNavigator> logger,
            DialogStack stack
        )
        {
            _presenter = presenter;
            _registry = registry;
            _router = router;
            _logger = logger;
            _stack = stack;
        }

        public bool IsCurrentDialog(string dialogId)
        {
            return _presenter.IsRendering(dialogId) && _stack.AlreadyCurrentScreen(dialogId, _router);
        }

        [Route]
        private async UniTask On(ShowDialogCommand command, PublishContext context)
        {
            var dialogId = command.DialogId;

            if (_stack.AlreadyCurrentScreen(dialogId, _router))
            {
                return;
            }

            var dialogResult = await _registry.GetScreenWithErrorAsync(dialogId, _router);
            if (dialogResult.IsError)
            {
                return;
            }

            var dialogEntry = dialogResult.Value;

            var showResult = await _presenter.ShowScreenAsync(dialogEntry, context.CancellationToken);
            if (showResult.IsError)
            {
                return;
            }

            _stack.Push(dialogId);
        }

        [Route]
        private async UniTask On(HideDialogCommand command, PublishContext context)
        {
            if (_stack.TryPop(out var dialogId))
            {
                var dialogResult = await _registry.GetScreenWithErrorAsync(dialogId, _router);
                if (dialogResult.IsError)
                {
                    return;
                }

                var dialogEntry = dialogResult.Value;
                await _presenter.HideScreenAsync(dialogEntry, context.CancellationToken);
            }
            else
            {
                _logger.LogDebug("No dialog to hide.");
            }
        }
    }
}