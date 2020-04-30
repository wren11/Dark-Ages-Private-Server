///************************************************************************
//Project Lorule: A Dark Ages Client (http://darkages.creatorlink.net/index/)
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
using System.Linq;
using Darkages.IO;
using Newtonsoft.Json;

namespace Darkages.Storage
{
    public class AreaStorage : IStorage<Area>
    {
        public static string StoragePath;

        static AreaStorage()
        {
            if (ServerContextBase.StoragePath == null)
                ServerContextBase.LoadConstants();

            StoragePath = $@"{ServerContextBase.StoragePath}\areas";

            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);
        }

        public int Count => Directory.GetFiles(StoragePath, "*.json", SearchOption.TopDirectoryOnly).Length;

        public Area Load(string Name)
        {
            var path = Path.Combine(StoragePath, $"{Name.ToLower()}.json");

            if (!File.Exists(path))
                return null;

            using (var s = File.OpenRead(path))
            using (var f = new StreamReader(s))
            {
                var content = f.ReadToEnd();
                var settings = StorageManager.Settings;
                settings.TypeNameHandling = TypeNameHandling.None;

                try
                {
                    var obj = JsonConvert.DeserializeObject<Area>(content, settings);

                    ServerContextBase.Report(obj);


                    return obj;
                }
                catch (Exception e)
                {
                    ServerContextBase.Report(e);
                    return null;
                }
            }
        }

        public void Save(Area obj)
        {
            if (ServerContextBase.Paused)
                return;


            var path = Path.Combine(StoragePath, $"{obj.ContentName.ToLower()}.json");
            var objString = JsonConvert.SerializeObject(obj, StorageManager.Settings);
            File.WriteAllText(path, objString);

            ServerContextBase.Report(obj);
        }

        public void CacheFromStorage()
        {
            var area_dir = StoragePath;
            if (!Directory.Exists(area_dir))
                return;
            var area_names = Directory.GetFiles(area_dir, "*.json", SearchOption.TopDirectoryOnly);

            foreach (var area in area_names)
            {
                var mapObj = StorageManager.AreaBucket.Load(Path.GetFileNameWithoutExtension(area));

                if (mapObj == null)
                    continue;

                var mapFile = Directory.GetFiles($@"{ServerContextBase.StoragePath}\maps", $"lod{mapObj.ID}.map",
                    SearchOption.TopDirectoryOnly).FirstOrDefault();

                if (mapFile != null && File.Exists(mapFile))
                {
                    LoadMap(mapObj, mapFile, true);
                    ServerContextBase.GlobalMapCache[mapObj.ID] = mapObj;
                }
            }
        }

        public static void LoadMap(Area mapObj, string mapFile, bool save = false)
        {
            mapObj.Data = File.ReadAllBytes(mapFile);
            mapObj.Hash = Crc16Provider.ComputeChecksum(mapObj.Data);
            {
                if (save) StorageManager.AreaBucket.Save(mapObj);
            }

            mapObj.OnLoaded();
        }
    }
}