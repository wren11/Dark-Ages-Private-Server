#region

using Darkages.IO;
using Darkages.Scripting;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

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
            var areaDir = StoragePath;
            if (!Directory.Exists(areaDir))
                return;

            var areaNames = Directory.GetFiles(areaDir, "*.json", SearchOption.TopDirectoryOnly);

            foreach (var area in areaNames)
            {
                var mapObj = StorageManager.AreaBucket.Load(Path.GetFileNameWithoutExtension(area));

                if (mapObj == null)
                    continue;

                var mapFile = Directory.GetFiles($@"{ServerContext.StoragePath}\maps", $"lod{mapObj.Id}.map",
                    SearchOption.TopDirectoryOnly).FirstOrDefault();

                if (mapFile != null && File.Exists(mapFile))
                {
                    if (!LoadMap(mapObj, mapFile, true))
                    {
                    }

                    if (!string.IsNullOrEmpty(mapObj.ScriptKey))
                    {
                        mapObj.Scripts = ScriptManager.Load<AreaScript>(mapObj.ScriptKey, mapObj);
                    }

                    ServerContext.GlobalMapCache[mapObj.Id] = mapObj;
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


            try
            {
                var obj = StorageManager.Deserialize<Area>(content);

                return obj;
            }
            catch (Exception ex)
            {
                ServerContext.Logger(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error);
                ServerContext.Logger(ex.StackTrace, Microsoft.Extensions.Logging.LogLevel.Error);
                return null;
            }
        }

        public void Save(Area obj)
        {
            var path = Path.Combine(StoragePath, $"{obj.Name.ToLower()}.json");

            obj.FilePath = PathNetCore.GetRelativePath(".", ServerContext.StoragePath + "\\maps\\lod" + obj.Id + ".map");

            var objString = StorageManager.Serialize(obj);
            File.WriteAllText(path, objString);
        }
    }
}