namespace GameKit.Assets.Tables
{
    public interface IEntityFactory<out T>
    {
        T ToEntity();
    }
}