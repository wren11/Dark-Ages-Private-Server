using Darkages.Types;
using Newtonsoft.Json;
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
using System.IO;

namespace Darkages.Storage
{
    public class TemplateStorage<T> where T : Template, new()
    {
        public static string StoragePath;

        static TemplateStorage()
        {
            if (ServerContext.StoragePath == null)
                ServerContext.LoadConstants();

            StoragePath = $@"{ServerContext.StoragePath}\templates";

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


        public bool IsStored(T obj)
        {
            if (Load<T>(obj.Name) == null)
                return false;

            return true;
        }

        public void SaveOrReplace(T template)
        {
            var path = Path.Combine(StoragePath, string.Format("{0}.json", template.Name.ToLower()));

            if (IsStored(template))
                File.Delete(path);

            if (template is ItemTemplate)
                StorageManager.ItemBucket.Save(template as ItemTemplate);

            if (template is SpellTemplate)
                StorageManager.SpellBucket.Save(template as SpellTemplate);

            if (template is SkillTemplate)
                StorageManager.SkillBucket.Save(template as SkillTemplate);

            if (template is MonsterTemplate)
                StorageManager.MonsterBucket.Save(template as MonsterTemplate);

            if (template is MundaneTemplate)
                StorageManager.MundaneBucket.Save(template as MundaneTemplate);

            if (template is Reactor)
                StorageManager.ReactorBucket.Save(template as Reactor);

            if (template is WarpTemplate)
                StorageManager.WarpBucket.Save(template as WarpTemplate);

            if (template is PopupTemplate)
            {
                StorageManager.PopupBucket.Save(template as PopupTemplate);
            }
        }

        public Template LoadFromStorage(Template existing)
        {
            var template = StorageManager.ItemBucket.Load<ItemTemplate>(existing.Name);
            if (template == null)
                return null;

            ServerContext.GlobalItemTemplateCache[template.Name] = template;
            return template;
        }

        public void CacheFromStorage()
        {
            var tmp = new T();

            var results = new List<T>();
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
                            ServerContext.GlobalSkillTemplateCache[template.Name] = template;

                        break;
                    }

                    case SpellTemplate _:
                    {
                        var template =
                            StorageManager.SpellBucket.Load<SpellTemplate>(Path.GetFileNameWithoutExtension(asset));
                        if (template != null)
                            ServerContext.GlobalSpellTemplateCache[template.Name] = template;
                        break;
                    }

                    case Reactor _:
                    {
                        var template =
                            StorageManager.ReactorBucket.Load<Reactor>(Path.GetFileNameWithoutExtension(asset));
                        if (template != null)
                            ServerContext.GlobalReactorCache[template.Name] = template;
                        break;
                    }

                    case MonsterTemplate _:
                    {
                        var template =
                            StorageManager.MonsterBucket.Load<MonsterTemplate>(Path.GetFileNameWithoutExtension(asset),
                                asset);

                        if (template != null)
                        {
                            ServerContext.GlobalMonsterTemplateCache.Add(template);
                            template.NextAvailableSpawn = DateTime.UtcNow;
                        }

                        break;
                    }

                    case MundaneTemplate _:
                    {
                        var template =
                            StorageManager.MundaneBucket.Load<MundaneTemplate>(Path.GetFileNameWithoutExtension(asset));
                        if (template != null)
                            ServerContext.GlobalMundaneTemplateCache[template.Name] = template;
                        break;
                    }

                    case ItemTemplate _:
                    {
                        var template =
                            StorageManager.ItemBucket.Load<ItemTemplate>(Path.GetFileNameWithoutExtension(asset));
                        if (template != null)
                            ServerContext.GlobalItemTemplateCache[template.Name] = template;
                        break;
                    }

                    case WorldMapTemplate _:
                    {
                        var template =
                            StorageManager.WorldMapBucket.Load<WorldMapTemplate>(Path.GetFileNameWithoutExtension(asset));
                        if (template != null)
                            ServerContext.GlobalWorldMapTemplateCache[template.WorldIndex] = template;
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
                                ServerContext.GlobalPopupCache.Add(template);
                                break;
                            case TriggerType.ItemDrop:
                                template = StorageManager.PopupBucket.Load<ItemDropPopup>(
                                    Path.GetFileNameWithoutExtension(asset));
                                ServerContext.GlobalPopupCache.Add(template);
                                break;
                            case TriggerType.ItemPickup:
                                template = StorageManager.PopupBucket.Load<ItemPickupPopup>(
                                    Path.GetFileNameWithoutExtension(asset));
                                ServerContext.GlobalPopupCache.Add(template);
                                break;
                            case TriggerType.MapLocation:
                                template = StorageManager.PopupBucket.Load<UserWalkPopup>(
                                    Path.GetFileNameWithoutExtension(asset));
                                ServerContext.GlobalPopupCache.Add(template);
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

            using (var s = File.OpenRead(path))
            {
                using (var f = new StreamReader(s))
                {
                    var obj = JsonConvert.DeserializeObject<TD>(f.ReadToEnd(), StorageManager.Settings);
                    ServerContext.Report<TD>(obj);
                    return obj;
                }
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

        public FileInfo MakeUnique(string path)
        {
            var dir = Path.GetDirectoryName(path);
            var fileName = Path.GetFileNameWithoutExtension(path);
            var fileExt = Path.GetExtension(path);

            for (var i = 1;; ++i)
            {
                if (!File.Exists(path))
                    return new FileInfo(path);

                if (dir != null)
                    path = Path.Combine(dir, fileName + " " + i + fileExt);
            }
        }
    }
}