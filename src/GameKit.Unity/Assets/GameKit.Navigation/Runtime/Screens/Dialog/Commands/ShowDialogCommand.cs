using VitalRouter;

namespace GameKit.Navigation.Screens.Dialog.Commands
{
    public readonly struct ShowDialogCommand : ICommand
    {
        public readonly string DialogId;

        public ShowDialogCommand(string dialogId)
        {
            DialogId = dialogId;
        }
    }
}