#region

using System.IO;
using System.Text.Json;
using Darkages.Types;


#endregion

namespace Darkages.Storage
{
    public class WarpStorage : IStorage<WarpTemplate>
    {
        public static string StoragePath;

        static WarpStorage()
        {
            StoragePath = $@"{ServerContext.StoragePath}\templates\warps";

            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);
        }

        public void CacheFromStorage()
        {
            var areaDir = StoragePath;
            if (!Directory.Exists(areaDir))
                return;

            var areaNames = Directory.GetFiles(areaDir, "*.json", SearchOption.TopDirectoryOnly);

            foreach (var area in areaNames)
            {
                var obj = StorageManager.WarpBucket.Load(Path.GetFileNameWithoutExtension(area));
                ServerContext.GlobalWarpTemplateCache.Add(obj);
            }
        }

        public WarpTemplate Load(string name)
        {
            var path = Path.Combine(StoragePath, $"{name.ToLower()}.json");

            if (!File.Exists(path))
                return null;

            using var s = File.OpenRead(path);
            using var f = new StreamReader(s);
            return StorageManager.Deserialize<WarpTemplate>(f.ReadToEnd());
        }

        public void Save(WarpTemplate obj)
        {
            var path = Path.Combine(StoragePath, $"{obj.Name.ToLower()}.json");
            var objString = StorageManager.Serialize(obj);
            File.WriteAllText(path, objString);
        }
    }
}