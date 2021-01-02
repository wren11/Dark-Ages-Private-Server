#region

using System;
using System.IO;

using Newtonsoft.Json;

#endregion

namespace Darkages.Storage
{
    public class AislingStorage : IStorage<Aisling>
    {
        public static string StoragePath = $@"{ServerContext.StoragePath}\aislings";

        static AislingStorage()
        {
            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);
        }

        public string[] Files => Directory.GetFiles(StoragePath, "*.json", SearchOption.TopDirectoryOnly);

        public Aisling Load(string name)
        {
            try
            {
                var path = Path.Combine(StoragePath, $"{name.ToLower()}.json");

                if (!File.Exists(path))
                    return null;

                using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                using var f = new StreamReader(stream);
                var content = f.ReadToEnd();
                f.Close();
                stream.Close();

                return JsonConvert.DeserializeObject<Aisling>(content, StorageManager.Settings);
            }
            catch (Exception e)
            {
                ServerContext.Logger($"Error : {e.Message}. Aisling could not be loaded.");
                return null;
            }
        }

        public void Save(Aisling obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (ServerContext.Config.DontSavePlayers)
                return;

            var path = Path.Combine(StoragePath, $"{obj.Username.ToLower()}.json");
            var objString = JsonConvert.SerializeObject(obj, StorageManager.Settings);

            using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            using var sw = new StreamWriter(stream); 
            sw.Write(objString);
            sw.Close();
            stream.Close();
        }
    }
}