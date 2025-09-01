using VitalRouter;

namespace GameKit.Navigation.Screens.Core.Commands
{
    public readonly struct ScreenErrorCommand : ICommand
    {
        public readonly string ScreenId;
        public readonly ScreenOperation Operation;
        public readonly string ErrorCode;
        public readonly string Message;

        public ScreenErrorCommand(
            string screenId,
            ScreenOperation operation,
            string errorCode,
            string message
        )
        {
            ScreenId = screenId;
            Operation = operation;
            ErrorCode = errorCode;
            Message = message;
        }
    }
}