///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Darkages.Network;
using Darkages.Network.Game;
using Darkages.Storage;
using Newtonsoft.Json;

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

        public ushort Index { get; set; }
        public bool IsMail { get; set; }

        [JsonIgnore] public GameClient Client { get; set; }

        public string Subject { get; set; }

        public ushort LetterId { get; set; }

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


        public static List<Board> CacheFromStorage(string dir)
        {
            var results = new List<Board>();
            var asset_names = Directory.GetFiles(
                Path.Combine(StoragePath, dir),
                "*.json",
                SearchOption.TopDirectoryOnly);

            if (asset_names.Length == 0)
                return null;

            foreach (var asset in asset_names)
            {
                var tmp = LoadFromFile(asset);

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

        public static Board LoadFromFile(string path)
        {
            if (!File.Exists(path))
                return null;

            using (var s = File.OpenRead(path))
            using (var f = new StreamReader(s))
            {
                return JsonConvert.DeserializeObject<Board>(f.ReadToEnd(), StorageManager.Settings);
            }
        }

        public void Save(string key)
        {
            if (ServerContext.Paused)
                return;


            var path = Path.Combine(StoragePath, string.Format("{0}\\{1}.json", key, Subject));
            var objString = JsonConvert.SerializeObject(this, StorageManager.Settings);
            File.WriteAllText(path, objString);
        }
    }
}