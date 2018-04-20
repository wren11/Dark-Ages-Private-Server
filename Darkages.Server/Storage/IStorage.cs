namespace Darkages.Storage
{
    public interface IStorage<T>
    {
        T Load(string name);
        void Save(T obj);
    }
}