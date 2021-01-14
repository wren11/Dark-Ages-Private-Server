#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using Darkages.Network.Game;
using Darkages.Scripting;
using Darkages.Storage;
using Darkages.Types;
using MenuInterpreter;
using MenuInterpreter.Parser;

#endregion

namespace Darkages.Network.Object
{
    public interface IObjectManager
    {
        void AddObject<T>(T obj, Predicate<T> p = null) where T : Sprite;

        void DelObject<T>(T obj) where T : Sprite;

        void DelObjects<T>(T[] obj) where T : Sprite;

        T GetObject<T>(Area map, Predicate<T> p) where T : Sprite;

        Sprite GetObject(Area map, Predicate<Sprite> p, ObjectManager.Get selections);

        T GetObjectByName<T>(string name, Area map = null) where T : Sprite, new();

        IEnumerable<T> GetObjects<T>(Area map, Predicate<T> p) where T : Sprite;

        IEnumerable<Sprite> GetObjects(Area map, Predicate<Sprite> p, ObjectManager.Get selections);

        void LoadScriptInterpreter(GameClient client,
            string name,
            Interpreter.MovedToNextStepHandler callbackHandler);
    }

    public class ObjectManager : IObjectManager
    {
        public void LoadScriptInterpreter(GameClient client,
            string name,
            Interpreter.MovedToNextStepHandler callbackHandler)
        {
            var parser = new YamlMenuParser();
            var yamlPath = ServerContext.StoragePath + $@"\Scripts\Menus\{name}.yaml";

            if (!File.Exists(yamlPath))
                return;
            if (client.MenuInterpter != null)
                return;

            client.MenuInterpter = parser.CreateInterpreterFromFile(yamlPath);
            client.MenuInterpter.Client = client;
            client.MenuInterpter.OnMovedToNextStep += callbackHandler;
        }


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

        public static T Clone<T>(object source)
        {
            var serialized = StorageManager.Serialize(source);

            var obj = StorageManager.Deserialize<T>(serialized);

            CloneItem(source, obj);
            CloneSkill(source, obj);
            CloseSpell(source, obj);
            CloneMonster(source, obj);
            CloneMundane(source, obj);

            return obj;
        }

        public void AddObject<T>(T obj, Predicate<T> p = null) where T : Sprite
        {
            if (p != null && p(obj))
                ServerContext.Game.ObjectFactory.AddGameObject(obj);
            else if (p == null)
                ServerContext.Game.ObjectFactory.AddGameObject(obj);
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

        public Sprite GetObject(Area map, Predicate<Sprite> p, Get selections)
        {
            return GetObjects(map, p, selections).FirstOrDefault();
        }

        public T GetObjectByName<T>(string name, Area map = null)
            where T : Sprite, new()
        {
            var objType = new T();

            if (objType is Aisling)
                return GetObject<Aisling>(null, i => i != null &&
                                                     string.Equals(i.Username.ToLower(), name.ToLower(),
                                                         StringComparison.InvariantCulture)).Cast<T>();

            if (objType is Monster)
                return GetObject<Monster>(map, i => i != null &&
                                                    string.Equals(i.Template.Name.ToLower(), name.ToLower(),
                                                        StringComparison.InvariantCulture)).Cast<T>();

            if (objType is Mundane)
                return GetObject<Mundane>(map, i => i != null &&
                                                    string.Equals(i.Template.Name.ToLower(), name.ToLower(),
                                                        StringComparison.InvariantCulture)).Cast<T>();

            if (objType is Item)
                return GetObject<Item>(map, i => i != null &&
                                                 string.Equals(i.Template.Name.ToLower(), name.ToLower(),
                                                     StringComparison.InvariantCulture)).Cast<T>();

            return null;
        }

        public IEnumerable<T> GetObjects<T>(Area map, Predicate<T> p) where T : Sprite
        {
            return ServerContext.Game?.ObjectFactory.QueryAll(map, p);
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

        private static void CloneItem<T>(object source, T obj)
        {
            switch (source)
            {
                case Item item:
                    item.Template = item.Template;
                    item.Scripts = ScriptManager.Load<ItemScript>(item.Template.ScriptName, obj as Item);
                    break;
            }
        }

        private static void CloneMonster<T>(object source, T obj)
        {
            switch (source)
            {
                case Monster monster:
                    monster.Template = monster.Template;
                    monster.Scripts = ScriptManager.Load<MonsterScript>(monster.Template.ScriptName, obj as Monster);
                    break;
            }
        }

        private static void CloneMundane<T>(object source, T obj)
        {
            switch (source)
            {
                case Mundane mundane:
                    mundane.Template = mundane.Template;
                    mundane.Scripts = ScriptManager.Load<MundaneScript>(mundane.Template.ScriptKey, obj as Mundane);
                    break;
            }
        }

        private static void CloneSkill<T>(object source, T obj)
        {
            switch (source)
            {
                case Skill skill:
                    skill.Template = skill.Template;
                    skill.Scripts = ScriptManager.Load<SkillScript>(skill.Template.ScriptName, obj as Skill);
                    break;
            }
        }

        private static void CloseSpell<T>(object source, T obj)
        {
            switch (source)
            {
                case Spell spell:
                    spell.Template = spell.Template;
                    spell.Scripts = ScriptManager.Load<SpellScript>(spell.Template.ScriptKey, obj as Spell);
                    break;
            }
        }
    }
}