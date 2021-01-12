namespace Darkages.Storage
{
    interface ISerializer<T>
    { 
        string Serialize(T obj);
        T Deserialize(string content);
    }

}
