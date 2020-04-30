using System;
using System.Collections.Generic;
using Darkages.Types;

namespace Darkages.Network.Object
{
    public interface IObjectManager
    {
        void DelObject<T>(T obj) where T : Sprite;
        void DelObjects<T>(T[] obj) where T : Sprite;
        T GetObject<T>(Area map, Predicate<T> p) where T : Sprite;
        T GetObjectByName<T>(string name, Area map = null) where T : Sprite, new();
        IEnumerable<T> GetObjects<T>(Area map, Predicate<T> p) where T : Sprite;
        void AddObject<T>(T obj, Predicate<T> p = null) where T : Sprite;
        IEnumerable<Sprite> GetObjects(Area map, Predicate<Sprite> p, ObjectManager.Get selections);
        Sprite GetObject(Area map, Predicate<Sprite> p, ObjectManager.Get selections);
    }
}