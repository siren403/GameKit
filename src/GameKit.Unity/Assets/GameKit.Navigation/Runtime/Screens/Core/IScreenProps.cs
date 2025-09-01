namespace GameKit.Navigation.Screens.Core
{
    public interface IScreenProps<in TProps> : IScreen
    {
        TProps Props { set; }
    }
}