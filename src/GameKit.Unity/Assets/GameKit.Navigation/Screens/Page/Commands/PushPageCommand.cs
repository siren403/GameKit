using VitalRouter;

namespace GameKit.Navigation.Screens.Page.Commands
{
    public struct PushPageCommand : ICommand
    {
        public readonly string PageId;

        public PushPageCommand(string pageId)
        {
            PageId = pageId;
        }
    }
}