using VitalRouter;

namespace GameKit.Navigation.Screens.Core
{
    public interface IScreen
    {
        bool IsVisible { set; get; }
        Subscription MapTo(ICommandSubscribable subscribable);
    }
}