#region

using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Scripting;
using Darkages.Types;
using Newtonsoft.Json;

#endregion

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

        Sprite GetObject(Area Map, Predicate<Sprite> p, ObjectManager.Get selections);
    }

    public class ObjectManager : IObjectManager
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
            ServerContextBase.Game?.ObjectFactory.RemoveGameObject(obj);
        }

        public void DelObjects<T>(T[] obj) where T : Sprite
        {
            ServerContextBase.Game?.ObjectFactory.RemoveAllGameObjects(obj);
        }

        public T GetObject<T>(Area map, Predicate<T> p) where T : Sprite
        {
            return ServerContextBase.Game?.ObjectFactory.Query(map, p);
        }

        public T GetObjectByName<T>(string name, Area map = null)
            where T : Sprite, new()
        {
            var objType = new T();

            if (objType is Aisling)
                return GetObject<Aisling>(map, i => i.Username.ToLower() == name.ToLower()).Cast<T>();

            if (objType is Monster)
                return GetObject<Monster>(map, i => i.Template.Name.ToLower() == name.ToLower()).Cast<T>();

            if (objType is Mundane)
                return GetObject<Mundane>(map, i => i.Template.Name.ToLower() == name.ToLower()).Cast<T>();

            if (objType is Item)
                return GetObject<Item>(map, i => i.Template.Name.ToLower() == name.ToLower()).Cast<T>();

            return null;
        }

        public IEnumerable<T> GetObjects<T>(Area map, Predicate<T> p) where T : Sprite
        {
            return ServerContextBase.Game?.ObjectFactory.QueryAll(map, p);
        }

        public void AddObject<T>(T obj, Predicate<T> p = null) where T : Sprite
        {
            if (p != null && p(obj))
                ServerContextBase.Game.ObjectFactory.AddGameObject(obj);
            else if (p == null)
                ServerContextBase.Game.ObjectFactory.AddGameObject(obj);
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
                (obj as Item).Scripts =
                    ScriptManager.Load<ItemScript>((source as Item).Template.ScriptName, obj as Item);
            }

            if (source is Skill)
            {
                (obj as Skill).Template = (source as Skill).Template;
                (obj as Skill).Scripts =
                    ScriptManager.Load<SkillScript>((source as Skill).Template.ScriptName, obj as Skill);
            }

            if (source is Spell)
            {
                (obj as Spell).Template = (source as Spell).Template;
                (obj as Spell).Scripts =
                    ScriptManager.Load<SpellScript>((source as Spell).Template.ScriptKey, obj as Spell);
            }

            if (source is Monster)
            {
                (obj as Monster).Template = (source as Monster).Template;
                (obj as Monster).Scripts =
                    ScriptManager.Load<MonsterScript>((source as Monster).Template.ScriptName, obj as Monster);
            }

            if (source is Mundane)
            {
                (obj as Mundane).Template = (source as Mundane).Template;
                (obj as Mundane).Scripts =
                    ScriptManager.Load<MundaneScript>((source as Mundane).Template.ScriptKey, obj as Mundane);
            }


            return obj;
        }
    }
}