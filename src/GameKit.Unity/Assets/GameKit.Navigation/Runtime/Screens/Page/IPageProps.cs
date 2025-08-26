namespace GameKit.Navigation.Screens.Page
{
    public interface IPageProps<in T> : IPage
    {
        T Props { set; }
    }
}