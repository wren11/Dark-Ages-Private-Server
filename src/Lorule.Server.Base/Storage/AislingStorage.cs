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

                var s = File.OpenRead(path);
                var f = new StreamReader(s);
                return JsonConvert.DeserializeObject<Aisling>(f.ReadToEnd(), new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

            }
            catch (Exception e)
            {
                ServerContext.Logger($"Error : {e.Message}. Aisling could not be loaded.");
                return null;
            }
        }

        public void Save(Aisling obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (ServerContext.Paused)
                return;

            if (ServerContext.Config.DontSavePlayers) return;

            try
            {
                var path = Path.Combine(StoragePath, $"{obj.Username.ToLower()}.json");

                var objString = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

                File.WriteAllText(path, objString);
            }
            catch (Exception ex)
            {
                Console.Write("Another process was using player's json file: " + ex);
            }
        }
    }
}