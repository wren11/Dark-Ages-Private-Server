using Darkages.Network;
using Darkages.Network.Game;
using Darkages.Storage;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Darkages.Types
{
    public class Board : NetworkFormat
    {
        public override bool Secured => true;
        public override byte Command => 0x31;
        public ushort Index { get; set; }
        public bool IsMail { get; set; }

        [JsonIgnore]
        public GameClient Client { get; set; }

        public Board(string name, ushort index, bool isMail = false)
        {
            Index = index;
            Subject = name;
            IsMail = isMail;
        }

        public string Subject { get; set; }

        public ushort LetterId { get; set; }

        public List<PostFormat> Posts = new List<PostFormat>();

        public override void Serialize(NetworkPacketReader reader) { }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte)(IsMail ? 0x04 : 0x02));
            writer.Write((byte)0x01);
            writer.Write((ushort)(IsMail ? 0x00 : LetterId));
            writer.WriteStringA((IsMail ? "Diary" : Subject));


            if (IsMail && Client != null)
            {
                Posts = Posts.Where(i => i.Recipient != null &&
                    i.Recipient.Equals(Client.Aisling.Username,
                    System.StringComparison.OrdinalIgnoreCase)).ToList();
            }

            writer.Write((byte)Posts.Count);
            foreach (var post in Posts)
            {
                writer.Write((byte)(post.Read ? 0 : 1));
                writer.Write(post.PostId);
                writer.WriteStringA(post.Sender);
                writer.Write((byte)post.DatePosted.Month);
                writer.Write((byte)post.DatePosted.Day);
                writer.WriteStringA(post.Subject);
            }
        }

        public static string StoragePath = $@"{ServerContext.StoragePath}\Community\Boards";

        static Board()
        {
            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);
        }


        public static List<Board> CacheFromStorage()
        {
            var results = new List<Board>();
            var asset_names = Directory.GetFiles(
                StoragePath,
                "*.json",
                SearchOption.TopDirectoryOnly);

            if (asset_names.Length == 0)
                return null;

            foreach (var asset in asset_names)
            {
                var tmp = Load(Path.GetFileNameWithoutExtension(asset));

                if (tmp != null)
                    results.Add(tmp);
            }

            return results;
        }

        public static Board Load(string LookupKey)
        {
            var path = Path.Combine(StoragePath, string.Format("{0}.json", LookupKey));

            if (!File.Exists(path))
                return null;

            using (var s = File.OpenRead(path))
            using (var f = new StreamReader(s))
            {
                return JsonConvert.DeserializeObject<Board>(f.ReadToEnd(), StorageManager.Settings);
            }
        }

        public void Save()
        {
            var path = Path.Combine(StoragePath, string.Format("{0}.json", Subject));
            var objString = JsonConvert.SerializeObject(this, StorageManager.Settings);
            File.WriteAllText(path, objString);
        }
    }
}