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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

        public bool Saving { get; set; }

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
            if (ServerContext.Paused)
                return;

            try
            {
                var path = Path.Combine(StoragePath, string.Format("{0}.json", obj.Username.ToLower()));


                var objString = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

                File.WriteAllText(path, objString);

            }
            catch
            {
                /* Ignore */
            }
        }
    }
}