namespace ConsoleAppExample.DAL
{
    public interface IDataMapper<in TItem>
    {
        void Save(TItem item);
    }
}
