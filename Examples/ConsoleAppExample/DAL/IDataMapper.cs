namespace ConsoleAppExample.DAL
{
    public interface IDataMapper<TItem, TKey>
    {
        void Save(TItem item);
    }
}
