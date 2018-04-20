using Darkages.Types;

namespace Darkages.Network.Object
{
    public delegate void ObjectEvent<T>(T obj)
        where T : Sprite;
}