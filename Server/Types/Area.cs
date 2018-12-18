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
using Darkages.Network.Game;
using Darkages.Network.Object;
using Darkages.Network.ServerFormats;
using Darkages.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Darkages
{
    public class Area : ObjectManager
    {
        [JsonIgnore] private static readonly byte[] sotp = File.ReadAllBytes(Path.Combine(Environment.CurrentDirectory, "Static", "sotp.lor"));

        [JsonIgnore] [Browsable(false)] public ushort Hash;

        [JsonIgnore]
        [Browsable(false)]
        private readonly GameServerTimer WarpTimer =
            new GameServerTimer(TimeSpan.FromSeconds(ServerContext.Config.WarpUpdateTimer));

        [JsonIgnore]
        [Browsable(false)]
        private readonly GameServerTimer UpdateTimer =
            new GameServerTimer(TimeSpan.FromMilliseconds(250));


        [JsonIgnore] [Browsable(false)] public byte[] Data { get; set; }

        public int Music { get; set; }

        [JsonIgnore] [Browsable(false)] public bool Ready { get; set; }

        public ushort Rows { get; set; }

        public ushort Cols { get; set; }

        public int Number { get; set; }

        public int ID { get; set; }

        public string Name { get; set; }

        public MapFlags Flags { get; set; }

        public MapTile[,] MapNodes;


        public class MapTile
        {
            public ushort X, Y;

            public TileContent BaseObject { get; set; }

            public bool HasWall => BaseObject == TileContent.Wall;
          
            private ConcurrentDictionary<int, Sprite> Objects = new ConcurrentDictionary<int, Sprite>();

            public List<Sprite> Sprites
            {
                get => Objects.Select(i => i.Value).ToList(); 
            }

            public void Empty()
            {
                Objects = new ConcurrentDictionary<int, Sprite>();
            }

            public bool SpotVacant()
            {
                var result = true;

                if (BaseObject == TileContent.Warp)
                    return true;

                if (BaseObject == TileContent.Wall)
                    return false;


                for (int i = 0; i < Sprites.Count; i++)
                {
                    if (Sprites[i] is Monster)
                    {
                        if ((Sprites[i] as Monster).Template.IgnoreCollision)
                        {
                            return false;
                        }
                    }

                    if (Sprites[i] is Mundane)
                    {
                        return false;
                    }

                    if (Sprites[i].CurrentHp > 0)
                    {
                        return false;
                    }
                }

                return result;
            }

            public Sprite Find(Func<Sprite, bool> subject)
            {
                return Sprites.Find(i => subject(i));
            }

            public Sprite[] FindAll(Func<Sprite, bool> subject)
            {
                return Sprites.FindAll(i => subject(i)).ToArray();
            }


            public bool Add(Sprite obj)
            {
                return Objects.TryAdd(obj.Serial, obj);
            }

            public bool Remove(Sprite obj)
            {
                Sprite removedObj;
                return Objects.TryRemove(obj.Serial, out removedObj);
            }
        }

        public void Update(int x, int y, Sprite obj)
        {
            if (x < 0 ||
                x >= this.Cols)
                return;

            if (y < 0 ||
                y >= this.Rows)
                return;


            if (MapNodes[x, y]?.Add(obj) ?? false)
            {

            }
        }

        public void Update(int x, int y, Sprite obj, bool remove)
        {
            if (x < 0 ||
                x >= this.Cols)
                return;

            if (y < 0 ||
                y >= this.Rows)
                return;


            if (remove)
            {
                if (MapNodes[x, y]?.Remove(obj) ?? false)
                {

                }
            }
        }


        public static bool ParseSotp(short lWall, short rWall)
        {
            if (lWall == 0 &&
                rWall == 0)
                return false;
            if (lWall == 0)
                return sotp[rWall - 1] == 0x0F;
            if (rWall == 0)
                return sotp[lWall - 1] == 0x0F;
            return
                sotp[lWall - 1] == 0x0F &&
                sotp[rWall - 1] == 0x0F;
        }

        public bool IsWall(int x, int y)
        {
            if (x < 0 ||
                x >= this.Cols)
                return true;

            if (y < 0 ||
                y >= this.Rows)
                return true;

            var obj = MapNodes[x, y];

            if (obj == null)
                return false;

            var isEmpty = obj.SpotVacant();
            return !isEmpty;
        }

        public bool IsWall(Sprite obj, int x, int y)
        {
            if (obj is Monster)
            {
                if ((obj as Monster).Template.IgnoreCollision)
                    return false;
            }

            return IsWall(x, y);
        }

        public bool IsWall(Aisling obj, int x, int y)
        {
            if (obj.Flags.HasFlag(AislingFlags.GM))
                return false;

            if (obj.Flags.HasFlag(AislingFlags.Dead))
                return false;

            return IsWall(x, y);
        }

        public byte[] GetRowData(int row)
        {
            var buffer = new byte[Cols * 6];

            unsafe
            {
                fixed (byte* lpData = buffer, lpTile = &Data[row * Cols * 6])
                {
                    var lpD = lpData;
                    var lpT = lpTile;

                    for (var i = 0; i < Cols; i++, lpD += 6, lpT += 6)
                    {
                        lpD[0] = lpT[1];
                        lpD[1] = lpT[0];

                        lpD[2] = lpT[3];
                        lpD[3] = lpT[2];

                        lpD[4] = lpT[5];
                        lpD[5] = lpT[4];
                    }
                }
            }

            return buffer;
        }

        private List<Sprite> ObjectCache { get; set; }

        public void Update(TimeSpan elapsedTime)
        {
            UpdateTimer.Update(elapsedTime);

            if (UpdateTimer.Elapsed)
            {
                try
                {
                    var objects = GetObjects(i => i.CurrentMapId == ID, Get.All).ToArray();

                    lock (ServerContext.SyncObj)
                    {
                        if (objects.Any())
                        {
                            ObjectCache = new List<Sprite>(objects).ToList();
                        }
                    }
                }
                catch (Exception)
                {
                    //ignore
                }
                finally
                {
                    UpdateTimer.Reset();
                }
            }

            if (ObjectCache != null && ObjectCache.Count > 0)
            {
                UpdateMonsters(elapsedTime, ObjectCache.OfType<Monster>());
                UpdateMundanes(elapsedTime, ObjectCache.OfType<Mundane>());
                UpdateItems(elapsedTime, ObjectCache.OfType<Money>().Concat<Sprite>(ObjectCache.OfType<Item>()));
            }


            WarpTimer.Update(elapsedTime);

            if (WarpTimer.Elapsed)
            {
                UpdateWarps();
                WarpTimer.Reset();
            }
        }

        private void UpdateWarps()
        {
            var warpsOnMap = ServerContext.GlobalWarpTemplateCache.Where(i => i.ActivationMapId == ID);

            foreach (var warp in warpsOnMap)
            {
                if (!warp.Activations.Any())
                    continue;

                var nearby = GetObjects<Aisling>(i =>
                    i.LoggedIn && i.CurrentMapId == warp.ActivationMapId);

                if (!nearby.Any())
                    continue;

                foreach (var warpObj in warp.Activations)
                    foreach (var obj in nearby)
                        if (obj.Position.WithinSquare(warpObj.Location, 10))
                            obj.Show(Scope.Self, new ServerFormat29(
                                 ServerContext.Config.WarpAnimationNumber,
                                warpObj.Location.X, warpObj.Location.Y));

            }
        }

        private void UpdateMonsters(TimeSpan elapsedTime, IEnumerable<Monster> objects)
        {
            var updates = 0;

            foreach (var obj in objects)
            {
                var nearby = obj.AislingsNearby();

                if (nearby.Length == 0 && !obj.Template.UpdateMapWide)
                {
                    if (obj.TaggedAislings != null && obj.TaggedAislings.Count > 0)
                    {
                        obj.TaggedAislings.Clear();
                    }

                    continue;
                }

                if (obj != null && obj.Map != null && obj.Script != null)
                {

                    obj.Script.Update(elapsedTime);
                    obj.UpdateBuffs(elapsedTime);
                    obj.UpdateDebuffs(elapsedTime);

                    obj.LastUpdated = DateTime.UtcNow;
                    updates++;
                }
            }
        }

        private void UpdateItems(TimeSpan elapsedTime, IEnumerable<Sprite> objects)
        {
            var updates = 0;

            foreach (var obj in objects)
            {
                var nearby = GetObjects<Aisling>(i => i.WithinRangeOf(obj) && i.CurrentMapId == ID);

                if (!nearby.Any())
                    continue;

                if (obj != null)
                {
                    obj.LastUpdated = DateTime.UtcNow;
                    updates++;

                    if (obj is Item)
                    {
                        if ((DateTime.UtcNow - obj.AbandonedDate).TotalMinutes > 3)
                        {
                            if ((obj as Item).Cursed)
                            {
                                (obj as Item).AuthenticatedAislings = null;
                                (obj as Item).Cursed = false;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateMundanes(TimeSpan elapsedTime, IEnumerable<Mundane> objects)
        {
            var updates = 0;

            foreach (var obj in objects)
            {
                if (obj == null)
                    continue;

                obj.UpdateBuffs(elapsedTime);
                obj.UpdateDebuffs(elapsedTime);
                obj.Update(elapsedTime);
                obj.LastUpdated = DateTime.UtcNow;
                updates++;
            }
        }


        public void OnLoaded(Aisling obj = null)
        {
            MapNodes = new MapTile[Cols, Rows];

            using (var stream = new MemoryStream(Data))
            {
                using (var reader = new BinaryReader(stream))
                {
                    for (var y = 0; y < Rows; y++)
                    {
                        for (var x = 0; x < Cols; x++)
                        {
                            reader.BaseStream.Seek(2, SeekOrigin.Current);

                            MapNodes[x, y] = new MapTile();

                            if (ParseSotp(reader.ReadInt16(), reader.ReadInt16()))
                            {
                                MapNodes[x, y].BaseObject = TileContent.Wall;
                            }
                            else
                            {
                                MapNodes[x, y].BaseObject = TileContent.None;
                            }
                        }
                    }
                }
            }

            UpdateCollisions(obj);
            Ready = true;
        }

        private void SetWarps()
        {
            var warps = ServerContext.GlobalWarpTemplateCache
                .Where(i => i.ActivationMapId == ID).ToArray();

            if (warps.Length == 0)
                return;

            foreach (var warp in warps)
                foreach (var o in warp.Activations)
                    if (warp.WarpType == WarpType.Map)
                        MapNodes[o.Location.X, o.Location.Y].BaseObject = TileContent.Warp;
        }

        public Position FindNearestEmpty(Position aislingPosition)
        {
            var positions = new List<Position>();

            for (var y = 0; y < Rows; y++)
                for (var x = 0; x < Cols; x++)
                    if (MapNodes[x, y].SpotVacant())
                        positions.Add(new Position(x, y));

            return positions.OrderBy(i => i.DistanceFrom(aislingPosition))
                .FirstOrDefault();
        }

        internal void UpdateCollisions(Aisling obj)
        {
            SetWarps();
        }
    }
}
