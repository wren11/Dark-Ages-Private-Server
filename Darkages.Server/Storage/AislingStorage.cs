using Newtonsoft.Json;
using System;
using System.IO;

namespace Darkages.Storage
{
    public class AislingStorage : IStorage<Aisling>
    {
        public static string StoragePath = $@"{ServerContext.StoragePath}\aislings";
        public int Count => Directory.GetFiles(StoragePath, "*.json", SearchOption.TopDirectoryOnly).Length;

        public bool Saving { get; set; }

        static AislingStorage()
        {
            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);
        }

        public Aisling Load(string Name)
        {
            var path = Path.Combine(StoragePath, string.Format("{0}.json", Name.ToLower()));

            if (!File.Exists(path))
                return null;

            using (var s = File.OpenRead(path))
            using (var f = new StreamReader(s))
            {
                return JsonConvert.DeserializeObject<Aisling>(f.ReadToEnd(), new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });
            }
        }
        public void Save(Aisling obj)
        {

            try
            {
                var path = Path.Combine(StoragePath, string.Format("{0}.json", obj.Username.ToLower()));
                var objString = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

                Saving = true;

                File.WriteAllText(path, objString);
            }
            catch (Exception)
            {
                /* Ignore */
            }
            finally
            {
                Saving = false;
            }
        }
    }
}