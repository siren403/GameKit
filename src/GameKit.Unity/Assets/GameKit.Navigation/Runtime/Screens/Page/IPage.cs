using VitalRouter;

namespace GameKit.Navigation.Screens.Page
{
    public interface IPage
    {
        bool IsVisible { set; get; }
        Subscription MapTo(ICommandSubscribable subscribable);
    }
}