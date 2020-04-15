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

using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Scripting;
using Darkages.Types;
using Newtonsoft.Json;

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

        public void DelObject<T>(T obj) where T : Sprite
        {
            ServerContext.Game?.ObjectFactory.RemoveGameObject(obj);
        }

        public void DelObjects<T>(T[] obj) where T : Sprite
        {
            ServerContext.Game?.ObjectFactory.RemoveAllGameObjects(obj);
        }

        public T GetObject<T>(Area map, Predicate<T> p) where T : Sprite
        {
            return ServerContext.Game?.ObjectFactory.Query(map, p);
        }

        /// <summary>
        /// GetObjectByName
        /// </summary>
        /// <param name="name">Aisling Name or Template Name</param>
        /// <param name="map">optional,only get objects from your current map instance.</param>
        /// <returns></returns>
        public T GetObjectByName<T>(string name, Area map = null)
            where T: Sprite, new()
        {
            var objType = new T();

            if (objType is Aisling)
            {
                return GetObject<Aisling>(map, i => i.Username.ToLower() == name.ToLower()).Cast<T>();
            }

            if (objType is Monster)
            {
                return GetObject<Monster>(map, i => i.Template.Name.ToLower() == name.ToLower()).Cast<T>();
            }

            if (objType is Mundane)
            {
                return GetObject<Mundane>(map, i => i.Template.Name.ToLower() == name.ToLower()).Cast<T>();
            }

            if (objType is Item)
            {
                return GetObject<Item>(map, i => i.Template.Name.ToLower() == name.ToLower()).Cast<T>();
            }

            return null;
        }

        public IEnumerable<T> GetObjects<T>(Area map, Predicate<T> p) where T : Sprite
        {
            return ServerContext.Game?.ObjectFactory.QueryAll(map, p);
        }

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

            if (source is Item)
            {
                (obj as Item).Template = (source as Item).Template;
                (obj as Item).Scripts  = ScriptManager.Load<ItemScript>((source as Item).Template.ScriptName, (obj as Item));
            }
            if (source is Skill)
            {
                (obj as Skill).Template = (source as Skill).Template;
                (obj as Skill).Scripts  = ScriptManager.Load<SkillScript>((source as Skill).Template.ScriptName, (obj as Skill));
            }
            if (source is Spell)
            {
                (obj as Spell).Template = (source as Spell).Template;
                (obj as Spell).Scripts  = ScriptManager.Load<SpellScript>((source as Spell).Template.ScriptKey, (obj as Spell));
            }

            if (source is Monster)
            {
                (obj as Monster).Template = (source as Monster).Template;
                (obj as Monster).Scripts  = ScriptManager.Load<MonsterScript>((source as Monster).Template.ScriptName, (obj as Monster));
            }

            if (source is Mundane)
            {
                (obj as Mundane).Template = (source as Mundane).Template;
                (obj as Mundane).Scripts  = ScriptManager.Load<MundaneScript>((source as Mundane).Template.ScriptKey, (obj as Mundane));
            }


            return obj;
        }

        public void AddObject<T>(T obj, Predicate<T> p = null) where T : Sprite
        {
            if (p != null && p(obj))
                ServerContext.Game.ObjectFactory.AddGameObject(obj);
            else if (p == null)
                ServerContext.Game.ObjectFactory.AddGameObject(obj);
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