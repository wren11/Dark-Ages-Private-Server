///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
using Darkages.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darkages.Network.Object
{

    public class ObjectManager
    {

        [Flags]
        public enum Get
        {
            Aislings = 1,
            Monsters = 2,
            Mundanes = 4,
            Items = 8,
            Money = 16,
            All = Aislings | Items | Money | Monsters | Mundanes
        }

        public void DelObject<T>(T obj) where T : Sprite => ServerContext.Game?.ObjectFactory.RemoveGameObject(obj);

        public void DelObjects<T>(T[] obj) where T : Sprite => ServerContext.Game?.ObjectFactory.RemoveAllGameObjects(obj);

        public T GetObject<T>(Area map, Predicate<T> p) where T : Sprite => ServerContext.Game?.ObjectFactory.Query(map, p);

        public IEnumerable<T> GetObjects<T>(Area map, Predicate<T> p) where T : Sprite => ServerContext.Game?.ObjectFactory.QueryAll(map, p);

        public static T Clone<T>(object source)
        {
            var serialized = JsonConvert.SerializeObject(source, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            var obj = JsonConvert.DeserializeObject<T>(serialized, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            if ((source as Item) != null)
                (obj as Item).Template = (source as Item).Template;

            if ((source as Skill) != null)
                (obj as Skill).Template = (source as Skill).Template;

            if ((source as Spell) != null)
                (obj as Spell).Template = (source as Spell).Template;

            if ((source as Monster) != null)
                (obj as Monster).Template = (source as Monster).Template;

            if ((source as Mundane) != null)
                (obj as Mundane).Template = (source as Mundane).Template;

            return obj;
        }

        public void AddObject<T>(T obj, Predicate<T> p = null) where T : Sprite
        {
            if (p != null && p(obj))
            {
                ServerContext.Game.ObjectFactory.AddGameObject(obj);
                return;
            }
            else if (p == null) 
            {
                ServerContext.Game.ObjectFactory.AddGameObject(obj);
            }
        }

        public IEnumerable<Sprite> GetObjects(Area map, Predicate<Sprite> p, Get selections)
        {
            var bucket = new List<Sprite>();

            if ((selections & Get.All) == Get.All)
                selections = Get.Items | Get.Money | Get.Monsters | Get.Mundanes | Get.Aislings;

            if ((selections & Get.Aislings) == Get.Aislings)
                bucket.AddRange(GetObjects<Aisling>(map, p));
            if ((selections & Get.Monsters) == Get.Monsters)
                bucket.AddRange(GetObjects<Monster>(map, p));
            if ((selections & Get.Mundanes) == Get.Mundanes)
                bucket.AddRange(GetObjects<Mundane>(map, p));
            if ((selections & Get.Money) == Get.Money)
                bucket.AddRange(GetObjects<Money>(map, p));
            if ((selections & Get.Items) == Get.Items)
                bucket.AddRange(GetObjects<Item>(map, p));

            return bucket;
        }

        public Sprite GetObject(Area Map, Predicate<Sprite> p, Get selections)
        {
            return GetObjects(Map, p, selections).FirstOrDefault();
        }
    }
}