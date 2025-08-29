using GameKit.Navigation.Screens.Dialog.Errors;
using VitalRouter;

namespace GameKit.Navigation.Screens.Dialog.Commands
{
    public readonly struct DialogErrorCommand : ICommand
    {
        public readonly string DialogId;
        public readonly DialogOperation Operation;
        public readonly string ErrorCode;
        public readonly string Message;

        public DialogErrorCommand(
            string dialogId,
            DialogOperation operation,
            string errorCode,
            string message
        )
        {
            DialogId = dialogId;
            Operation = operation;
            ErrorCode = errorCode;
            Message = message;
        }
    }
}