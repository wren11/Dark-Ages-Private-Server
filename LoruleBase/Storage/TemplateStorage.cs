#region

using Darkages.Types;
using Newtonsoft.Json;
using System;
using System.IO;

#endregion

namespace Darkages.Storage
{
    public class TemplateStorage<T> where T : Template, new()
    {
        public static string StoragePath;

        static TemplateStorage()
        {
            StoragePath = $@"{ServerContextBase.StoragePath}\templates";

            var tmp = new T();

            StoragePath = Path.Combine(StoragePath, "%");

            if (tmp is SkillTemplate)
                StoragePath = StoragePath.Replace("%", "Skills");

            if (tmp is SpellTemplate)
                StoragePath = StoragePath.Replace("%", "Spells");

            if (tmp is MonsterTemplate)
                StoragePath = StoragePath.Replace("%", "Monsters");

            if (tmp is ItemTemplate)
                StoragePath = StoragePath.Replace("%", "Items");

            if (tmp is MundaneTemplate)
                StoragePath = StoragePath.Replace("%", "Mundanes");

            if (tmp is WorldMapTemplate)
                StoragePath = StoragePath.Replace("%", "WorldMaps");

            if (tmp is Reactor)
                StoragePath = StoragePath.Replace("%", "Reactors");

            if (tmp is PopupTemplate)
                StoragePath = StoragePath.Replace("%", "Popups");

            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);
        }

        public void CacheFromStorage()
        {
            var tmp = new T();

            var assetNames = Directory.GetFiles(
                StoragePath,
                "*.json",
                tmp is MonsterTemplate ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            if (assetNames.Length == 0)
                return;

            foreach (var obj in assetNames)
            {
                var asset = obj;

                switch (tmp)
                {
                    case SkillTemplate _:
                        {
                            var template =
                                StorageManager.SkillBucket.Load<SkillTemplate>(Path.GetFileNameWithoutExtension(asset));
                            if (template != null)
                                ServerContextBase.GlobalSkillTemplateCache[template.Name] = template;

                            break;
                        }

                    case SpellTemplate _:
                        {
                            var template =
                                StorageManager.SpellBucket.Load<SpellTemplate>(Path.GetFileNameWithoutExtension(asset));
                            if (template != null)
                                ServerContextBase.GlobalSpellTemplateCache[template.Name] = template;
                            break;
                        }

                    case Reactor _:
                        {
                            var template =
                                StorageManager.ReactorBucket.Load<Reactor>(Path.GetFileNameWithoutExtension(asset));
                            if (template != null)
                                ServerContextBase.GlobalReactorCache[template.Name] = template;
                            break;
                        }

                    case MonsterTemplate _:
                        {
                            var template =
                                StorageManager.MonsterBucket.Load<MonsterTemplate>(Path.GetFileNameWithoutExtension(asset),
                                    asset);

                            if (template != null)
                            {
                                ServerContextBase.GlobalMonsterTemplateCache.Add(template);
                                template.NextAvailableSpawn = DateTime.UtcNow;
                            }

                            break;
                        }

                    case MundaneTemplate _:
                        {
                            var template =
                                StorageManager.MundaneBucket.Load<MundaneTemplate>(Path.GetFileNameWithoutExtension(asset));
                            if (template != null)
                                ServerContextBase.GlobalMundaneTemplateCache[template.Name] = template;
                            break;
                        }

                    case ItemTemplate _:
                        {
                            var template =
                                StorageManager.ItemBucket.Load<ItemTemplate>(Path.GetFileNameWithoutExtension(asset));
                            if (template != null)
                                ServerContextBase.GlobalItemTemplateCache[template.Name] = template;
                            break;
                        }

                    case WorldMapTemplate _:
                        {
                            var template =
                                StorageManager.WorldMapBucket.Load<WorldMapTemplate>(
                                    Path.GetFileNameWithoutExtension(asset));
                            if (template != null)
                                ServerContextBase.GlobalWorldMapTemplateCache[template.WorldIndex] = template;
                            break;
                        }

                    case PopupTemplate _:
                        {
                            var template =
                                StorageManager.PopupBucket.Load<PopupTemplate>(Path.GetFileNameWithoutExtension(asset));

                            switch (template.TypeOfTrigger)
                            {
                                case TriggerType.UserClick:
                                    template = StorageManager.PopupBucket.Load<UserClickPopup>(
                                        Path.GetFileNameWithoutExtension(asset));
                                    ServerContextBase.GlobalPopupCache.Add(template);
                                    break;

                                case TriggerType.ItemDrop:
                                    template = StorageManager.PopupBucket.Load<ItemDropPopup>(
                                        Path.GetFileNameWithoutExtension(asset));
                                    ServerContextBase.GlobalPopupCache.Add(template);
                                    break;

                                case TriggerType.ItemPickup:
                                    template = StorageManager.PopupBucket.Load<ItemPickupPopup>(
                                        Path.GetFileNameWithoutExtension(asset));
                                    ServerContextBase.GlobalPopupCache.Add(template);
                                    break;

                                case TriggerType.MapLocation:
                                    template = StorageManager.PopupBucket.Load<UserWalkPopup>(
                                        Path.GetFileNameWithoutExtension(asset));
                                    ServerContextBase.GlobalPopupCache.Add(template);
                                    break;
                            }

                            break;
                        }
                }
            }
        }

        public TD Load<TD>(string name, string fixedPath = null) where TD : class, new()
        {
            var path = fixedPath ?? Path.Combine(StoragePath, $"{name.ToLower()}.json");

            if (!File.Exists(path))
                return null;

            using var s = File.OpenRead(path);
            using var f = new StreamReader(s);
            var obj = JsonConvert.DeserializeObject<TD>(f.ReadToEnd(), StorageManager.Settings);
            return obj;
        }

        public Template LoadFromStorage(Template existing)
        {
            var template = StorageManager.ItemBucket.Load<ItemTemplate>(existing.Name);
            if (template == null)
                return null;

            ServerContextBase.GlobalItemTemplateCache[template.Name] = template;
            return template;
        }

        public FileInfo MakeUnique(string path)
        {
            var dir = Path.GetDirectoryName(path);
            var fileName = Path.GetFileNameWithoutExtension(path);
            var fileExt = Path.GetExtension(path);

            for (var i = 1; ; ++i)
            {
                if (!File.Exists(path))
                    return new FileInfo(path);

                if (dir != null)
                    path = Path.Combine(dir, fileName + " " + i + fileExt);
            }
        }

        public void Save(T obj, bool replace = false)
        {
            if (replace)
            {
                var path = Path.Combine(StoragePath, $"{obj.Name.ToLower()}.json");

                if (File.Exists(path))
                    File.Delete(path);

                var objString = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });
                File.WriteAllText(path, objString);
            }
            else
            {
                var path = MakeUnique(Path.Combine(StoragePath, $"{obj.Name.ToLower()}.json"))
                    .FullName;

                var objString = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });
                File.WriteAllText(path, objString);
            }
        }
    }
}