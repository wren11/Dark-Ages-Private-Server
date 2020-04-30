using Darkages.Network.Game;
using Darkages.Network.Object;
using Darkages.Types;
using Newtonsoft.Json;
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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Darkages
{
    public class Area : ObjectManager
    {
        [JsonIgnore] private readonly object _syncLock = new object();

        [JsonIgnore] private static readonly byte[] Sotp = File.ReadAllBytes("sotp.dat");

        [JsonIgnore] [Browsable(false)]
        private readonly GameServerTimer _updateTimer = new GameServerTimer(TimeSpan.FromMilliseconds(10));

        [JsonIgnore] public GameServerTimer Reaper = new GameServerTimer(TimeSpan.FromSeconds(2));

        [JsonIgnore] public Cache<Sprite[]> AreaObjectCache = new Cache<Sprite[]>();

        [JsonIgnore] [Browsable(false)] public byte[] Data;

        [JsonIgnore] [Browsable(false)] public ushort Hash;

        [JsonIgnore] [Browsable(false)] public bool Ready;

        public int Music { get; set; }

        public ushort Rows { get; set; }

        public ushort Cols { get; set; }

        public int ID { get; set; }

        public string Name { get; set; }

        public string ContentName { get; set; }

        public MapFlags Flags { get; set; }

        [JsonIgnore] public Tile[,] MapNodes { get; set; }

        [JsonIgnore] private TileContent[,] BaseMap { get; set; }

        public class Tile
        {
            public Stack<TileContent> Content = new Stack<TileContent>();

            public bool IsWall => Content.Any(i => i == TileContent.Wall);

            public bool HasMonsters => Content.Any(i => i == TileContent.Monster);

            public bool HasPlayers => Content.Any(i => i == TileContent.Aisling);

            public bool HasMundanes => Content.Any(i => i == TileContent.Monster);

            public bool IsAvailable()
            {
                if (IsWall)
                    return false;

                if (HasPlayers)
                    return false;

                if (HasMundanes)
                    return false;

                if (HasMonsters)
                    return false;

                return true;
            }

            public bool CanSpawnMonster()
            {
                return !HasMonsters && !HasMundanes;
            }

            public void Add(TileContent content)
            {
                Content.Push(content);
            }

            public void Empty()
            {
                Content = new Stack<TileContent>();
            }
        }


        public bool ParseSotp(short lWall, short rWall)
        {
            if (lWall == 0 && rWall == 0)
                return false;

            if (lWall == 0)
                return Sotp[rWall - 1] == 0x0F;

            if (rWall == 0)
                return Sotp[lWall - 1] == 0x0F;

            return Sotp[lWall - 1] == 0x0F && Sotp[rWall - 1] == 0x0F;
        }

        internal void Update(int xPos, int yPos)
        {
            UpdateTileContents(xPos, yPos);
        }

        internal void UpdateTileContents(int xPos, int yPos)
        {
            if (xPos < 0)
                xPos = 0;

            if (yPos < 0)
                yPos = 0;

            if (yPos >= MapNodes.GetUpperBound(1)) return;

            if (xPos >= MapNodes.GetUpperBound(0)) return;

            MapNodes[xPos, yPos].Empty();
            {
                if (BaseMap[xPos, yPos] == TileContent.Wall)
                {
                    MapNodes[xPos, yPos].Add(TileContent.Wall);
                    return;
                }

                var objects = GetObjects(this, i => i.X == xPos && i.Y == yPos,
                    Get.Monsters | Get.Mundanes | Get.Aislings);

                foreach (var obj in objects)
                    if (MapNodes[xPos, yPos].IsAvailable())
                        MapNodes[xPos, yPos].Add(obj.EntityType);
            }
        }

        public bool IsWall(int x, int y)
        {
            if (x < 0 || x >= Cols)
                return true;

            if (y < 0 || y >= Rows)
                return true;

            if (BaseMap[x, y] == TileContent.Wall)
                return true;

            UpdateTileContents(x, y);

            var obj = MapNodes[x, y];

            if (obj == null)
                return false;

            return !obj.IsAvailable();
        }

        public bool IsWall(Sprite obj, int x, int y)
        {
            if (obj is Monster monster)
                if (monster.Template.IgnoreCollision)
                    return false;

            return IsWall(x, y);
        }

        public bool IsWall(Aisling obj, int x, int y)
        {
            if (obj.Flags.HasFlag(AislingFlags.Dead))
                return false;

            return IsWall(x, y);
        }

        //public byte[] GetRowData(int row)
        //{
        //    var buffer = new byte[Cols * 6];

        //    unsafe
        //    {
        //        fixed (byte* lpData = buffer, lpTile = &Data[row * Cols * 6])
        //        {
        //            var lpD = lpData;
        //            var lpT = lpTile;

        //            for (var i = 0; i < Cols; i++, lpD += 6, lpT += 6)
        //            {
        //                lpD[0] = lpT[1];
        //                lpD[1] = lpT[0];

        //                lpD[2] = lpT[3];
        //                lpD[3] = lpT[2];

        //                lpD[4] = lpT[5];
        //                lpD[5] = lpT[4];
        //            }
        //        }
        //    }

        //    var a = GewRowDataSafe(row);
        //    return buffer;
        //}

        public byte[] GetRowData(int row)
        {
            var buffer = new byte[Cols * 6];
            var bPos = 0;
            var dPos = row * Cols * 6;

            lock (_syncLock)
            {
                for (var i = 0; i < Cols; i++, bPos += 6, dPos += 6)
                {
                    buffer[bPos + 0] = Data[dPos + 1];

                    buffer[bPos + 1] = Data[dPos + 0];

                    buffer[bPos + 2] = Data[dPos + 3];

                    buffer[bPos + 3] = Data[dPos + 2];

                    buffer[bPos + 4] = Data[dPos + 5];

                    buffer[bPos + 5] = Data[dPos + 4];
                }
            }

            return buffer;
        }


        public Sprite[] GetAreaObjects()
        {
            return GetObjects(this, i => i != null, Get.All).ToArray();
        }

        public void UpdateAreaObjects(TimeSpan elapsedTime)
        {
            var users = ServerContextBase.Game.Clients.Where(i => i?.Aisling != null && i.Aisling.CurrentMapId == ID)
                .Select(i => i.Aisling).ToArray();

            Sprite[] objectCache;

            if (!AreaObjectCache.Exists(Name))
            {
                objectCache = GetAreaObjects();

                if (objectCache.Length > 0) AreaObjectCache.AddOrUpdate(Name, objectCache, 3);
            }
            else
            {
                objectCache = AreaObjectCache.Get(Name);
            }

            if (objectCache == null || objectCache.Length <= 0)
                return;

            if (users.Length <= 0)
                return;

            UpdateMonsterObjects(elapsedTime, objectCache.OfType<Monster>());
            UpdateMundaneObjects(elapsedTime, objectCache.OfType<Mundane>());
            UpdateItemObjects(elapsedTime, objectCache.OfType<Money>().Concat<Sprite>(objectCache.OfType<Item>()));
        }

        public void Update(TimeSpan elapsedTime)
        {
            _updateTimer.Update(elapsedTime);

            if (!_updateTimer.Elapsed) return;

            UpdateAreaObjects(elapsedTime);
            _updateTimer.Reset();
        }

        public void UpdateMonsterObjects(TimeSpan elapsedTime, IEnumerable<Monster> objects)
        {
            Reaper.Update(elapsedTime);

            var enumerable = objects as Monster[] ?? objects.ToArray();

            if (Reaper.Elapsed)
            {
                Reaper.Reset();

                foreach (var obj in enumerable)
                    if (obj != null && obj.Map != null && obj.Scripts != null)
                        if (obj.CurrentHp <= 0)
                            if (obj.Target != null)
                                foreach (var script in obj.Scripts.Values)
                                    script.OnDeath(obj.Target.Client);
            }


            foreach (var obj in enumerable)
                if (obj != null && obj.Map != null && obj.Scripts != null)
                {
                    if (obj.CurrentHp <= 0)
                        if (obj.Target != null)
                            if (!obj.Skulled)
                            {
                                foreach (var script in obj.Scripts.Values) script.OnSkulled(obj.Target.Client);
                                obj.Skulled = true;
                            }

                    foreach (var script in obj.Scripts.Values) script.Update(elapsedTime);

                    obj.UpdateBuffs(elapsedTime);
                    obj.UpdateDebuffs(elapsedTime);
                    obj.LastUpdated = DateTime.UtcNow;
                }
        }

        public void UpdateItemObjects(TimeSpan elapsedTime, IEnumerable<Sprite> objects)
        {
            foreach (var obj in objects)
                if (obj != null)
                {
                    obj.LastUpdated = DateTime.UtcNow;

                    if (!(obj is Item item)) continue;
                    if (!((DateTime.UtcNow - item.AbandonedDate).TotalMinutes > 3)) continue;
                    if (!item.Cursed) continue;

                    item.AuthenticatedAislings = null;
                    item.Cursed = false;
                }
        }

        public void UpdateMundaneObjects(TimeSpan elapsedTime, IEnumerable<Mundane> objects)
        {
            foreach (var obj in objects)
            {
                if (obj == null)
                    continue;

                if (obj.CurrentHp <= 0)
                    obj.Remove();

                obj.UpdateBuffs(elapsedTime);
                obj.UpdateDebuffs(elapsedTime);
                obj.Update(elapsedTime);

                obj.LastUpdated = DateTime.UtcNow;
            }
        }

        public void OnLoaded()
        {
            MapNodes = new Tile[Cols, Rows];
            BaseMap = new TileContent[Cols, Rows];

            lock (_syncLock)
            {
                using (var stream = new MemoryStream(Data))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        for (var y = 0; y < Rows; y++)
                        for (var x = 0; x < Cols; x++)
                        {
                            reader.BaseStream.Seek(2, SeekOrigin.Current);

                            MapNodes[x, y] = new Tile();
                            MapNodes[x, y].Empty();

                            if (ParseSotp(reader.ReadInt16(), reader.ReadInt16()))
                            {
                                BaseMap[x, y] = TileContent.Wall;
                                MapNodes[x, y].Add(TileContent.Wall);
                            }
                            else
                            {
                                BaseMap[x, y] = TileContent.None;
                                MapNodes[x, y].Add(TileContent.None);
                            }
                        }
                    }
                }
            }

            Ready = true;
        }
    }
}