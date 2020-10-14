#region

using System;
using System.IO;
using System.Linq;
using Darkages.IO;
using Newtonsoft.Json;

#endregion

namespace Darkages.Storage
{
    public class AreaStorage : IStorage<Area>
    {
        public static string StoragePath;

        static AreaStorage()
        {
            StoragePath = $@"{ServerContext.StoragePath}\areas";

            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);
        }

        public int Count => Directory.GetFiles(StoragePath, "*.json", SearchOption.TopDirectoryOnly).Length;

        public static bool LoadMap(Area mapObj, string mapFile, bool save = false)
        {
            mapObj.FilePath = mapFile;
            mapObj.Data = File.ReadAllBytes(mapFile);
            mapObj.Hash = Crc16Provider.ComputeChecksum(mapObj.Data);
            {
                if (save) StorageManager.AreaBucket.Save(mapObj);
            }

            return mapObj.OnLoaded();
        }

        public void CacheFromStorage()
        {
            var area_dir = StoragePath;
            if (!Directory.Exists(area_dir))
                return;

            var area_names = Directory.GetFiles(area_dir, "*.json", SearchOption.TopDirectoryOnly);

            foreach (var area in area_names)
            {
                var mapObj = StorageManager.AreaBucket.Load(Path.GetFileNameWithoutExtension(area));

                if (mapObj == null)
                    continue;

                var mapFile = Directory.GetFiles($@"{ServerContext.StoragePath}\maps", $"lod{mapObj.ID}.map",
                    SearchOption.TopDirectoryOnly).FirstOrDefault();

                if (mapFile != null && File.Exists(mapFile))
                {
                    if (!LoadMap(mapObj, mapFile, true))
                    {
                        if (File.Exists(area))
                            File.Delete(area);
                    }
                    ServerContext.GlobalMapCache[mapObj.ID] = mapObj;
                }
            }
        }

        public Area Load(string name)
        {
            var path = Path.Combine(StoragePath, $"{name.ToLower()}.json");

            if (!File.Exists(path))
                return null;

            using var s = File.OpenRead(path);
            using var f = new StreamReader(s);
            var content = f.ReadToEnd();
            var settings = StorageManager.Settings;
            settings.TypeNameHandling = TypeNameHandling.None;

            try
            {
                var obj = JsonConvert.DeserializeObject<Area>(content, settings);

                return obj;
            }
            catch (Exception e)
            {
                ServerContext.Error(e);
                return null;
            }
        }

        public void Save(Area obj)
        {
            if (ServerContext.Paused)
                return;

            var path = Path.Combine(StoragePath, $"{obj.ContentName.ToLower()}.json");
            var objString = JsonConvert.SerializeObject(obj, StorageManager.Settings);
            File.WriteAllText(path, objString);
        }
    }
}