#region

using System.IO;
using Darkages.Types;
using Newtonsoft.Json;

#endregion

namespace Darkages.Storage
{
    public class WarpStorage : IStorage<WarpTemplate>
    {
        public static string StoragePath;

        static WarpStorage()
        {
            StoragePath = $@"{ServerContextBase.StoragePath}\templates\warps";

            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);
        }

        public WarpTemplate Load(string Name)
        {
            var path = Path.Combine(StoragePath, $"{Name.ToLower()}.json");

            if (!File.Exists(path))
                return null;

            using (var s = File.OpenRead(path))
            using (var f = new StreamReader(s))
            {
                return JsonConvert.DeserializeObject<WarpTemplate>(f.ReadToEnd(), StorageManager.Settings);
            }
        }

        public void Save(WarpTemplate obj)
        {
            if (ServerContextBase.Paused)
                return;


            var path = Path.Combine(StoragePath, $"{obj.Name.ToLower()}.json");
            var objString = JsonConvert.SerializeObject(obj, StorageManager.Settings);
            File.WriteAllText(path, objString);
        }

        public void CacheFromStorage()
        {
            var area_dir = StoragePath;
            if (!Directory.Exists(area_dir))
                return;
            var area_names = Directory.GetFiles(area_dir, "*.json", SearchOption.TopDirectoryOnly);

            foreach (var area in area_names)
            {
                var obj = StorageManager.WarpBucket.Load(Path.GetFileNameWithoutExtension(area));
                ServerContextBase.GlobalWarpTemplateCache.Add(obj);
            }
        }
    }
}