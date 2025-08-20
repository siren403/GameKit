using VitalRouter;

namespace GameKit.Navigation.Screens.Page.Commands
{
    public struct ReplacePageCommand : ICommand
    {
        public readonly string PageId;

        public ReplacePageCommand(string pageId)
        {
            PageId = pageId;
        }
    }
}