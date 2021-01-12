#region

using System;
using System.IO;
using System.Text;
using System.Text.Json;

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

                var content = File.ReadAllBytes(path);

                // ReSharper disable UseIndexFromEndExpression
                if (content[content.Length - 1] == 0x7D && content[content.Length - 3] == 0x7D)
                {
                    content[content.Length - 3] = 0x7D;
                    content[content.Length - 2] = 0x20;
                    content[content.Length - 1] = 0x20;
                }

                var jsoncontent = Encoding.ASCII.GetString(content);
                var aisling = StorageManager.Deserialize<Aisling>(jsoncontent);

                return aisling;
            }
            catch (Exception ex)
            {
                ServerContext.Logger(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error);
                ServerContext.Logger(ex.StackTrace, Microsoft.Extensions.Logging.LogLevel.Error);
                ServerContext.Logger($"Error : {ex.Message}. Aisling could not be loaded.", Microsoft.Extensions.Logging.LogLevel.Error);
            }

            return null;
        }

        public void Save(Aisling obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (ServerContext.Config.DontSavePlayers) return;

            try
            {
                var path = Path.Combine(StoragePath, $"{obj.Username.ToLower()}.json");
                var objString = StorageManager.Serialize(obj);

                File.WriteAllText(path, objString);
            }
            catch (Exception ex)
            {
                ServerContext.Logger(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error);
                ServerContext.Logger(ex.StackTrace, Microsoft.Extensions.Logging.LogLevel.Error);
            }
        }
    }
}