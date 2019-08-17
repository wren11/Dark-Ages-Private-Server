using System;
using System.Collections.Generic;
using Darkages.Types;

namespace Darkages.Network.Object
{
    public interface IObjectManager
    {
        void AddObject<T>(T obj, Predicate<T> p = null) where T : Sprite;
        void DelObject<T>(T obj) where T : Sprite;
        void DelObjects<T>(T[] obj) where T : Sprite;

        Sprite GetObject(Area Map, Predicate<Sprite> p, ObjectManager.Get selections);
        T GetObject<T>(Area map, Predicate<T> p) where T : Sprite;
        IEnumerable<Sprite> GetObjects(Area map, Predicate<Sprite> p, ObjectManager.Get selections);
        IEnumerable<T> GetObjects<T>(Area map, Predicate<T> p) where T : Sprite;
    }
}