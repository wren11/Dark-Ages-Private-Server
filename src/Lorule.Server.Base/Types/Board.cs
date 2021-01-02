#region

using Darkages.Network;
using Darkages.Network.Game;
using Darkages.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#endregion

namespace Darkages.Types
{
    public class Board : NetworkFormat
    {
        public static string StoragePath = $@"{ServerContext.StoragePath}\Community\Boards";

        public List<PostFormat> Posts = new List<PostFormat>();

        static Board()
        {
            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);
        }

        public Board()
        {
            Secured = true;
            Command = 0x31;
        }

        public Board(string name, ushort index, bool isMail = false)
        {
            Index = index;
            Subject = name;
            IsMail = isMail;
        }

        [JsonIgnore] public GameClient Client { get; set; }
        public ushort Index { get; set; }
        public bool IsMail { get; set; }
        public ushort LetterId { get; set; }
        public string Subject { get; set; }

        public static List<Board> CacheFromStorage(string dir)
        {
            var results = new List<Board>();
            var assetNames = Directory.GetFiles(
                Path.Combine(StoragePath, dir),
                "*.json",
                SearchOption.TopDirectoryOnly);

            if (assetNames.Length == 0)
                return null;

            foreach (var asset in assetNames)
            {
                var tmp = LoadFromFile(asset);

                if (tmp != null)
                    results.Add(tmp);
            }

            return results;
        }

        public static Board Load(string lookupKey)
        {
            var path = Path.Combine(StoragePath, $"{lookupKey}.json");

            if (!File.Exists(path))
                return null;

            using var s = File.OpenRead(path);
            using var f = new StreamReader(s);
            return JsonConvert.DeserializeObject<Board>(f.ReadToEnd(), StorageManager.Settings);
        }

        public static Board LoadFromFile(string path)
        {
            if (!File.Exists(path))
                return null;

            using var s = File.OpenRead(path);
            using var f = new StreamReader(s);
            return JsonConvert.DeserializeObject<Board>(f.ReadToEnd(), StorageManager.Settings);
        }

        public void Save(string key)
        {
            var path = Path.Combine(StoragePath, $"{key}\\{Subject}.json");
            var objString = JsonConvert.SerializeObject(this, StorageManager.Settings);
            File.WriteAllText(path, objString);
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte) (IsMail ? 0x04 : 0x02));
            writer.Write((byte) 0x01);
            writer.Write((ushort) (IsMail ? 0x00 : LetterId));
            writer.WriteStringA(IsMail ? "Mail" : Subject);

            if (IsMail && Client != null)
                Posts = Posts.Where(i => i.Recipient != null &&
                                         i.Recipient.Equals(Client.Aisling.Username,
                                             StringComparison.OrdinalIgnoreCase)).ToList();

            writer.Write((byte) Posts.Count);
            foreach (var post in Posts)
            {
                writer.Write((byte) (!post.Read ? 0 : 1));
                writer.Write(post.PostId);
                writer.WriteStringA(post.Sender);
                writer.Write((byte) post.DatePosted.Month);
                writer.Write((byte) post.DatePosted.Day);
                writer.WriteStringA(post.Subject);
            }
        }
    }
}