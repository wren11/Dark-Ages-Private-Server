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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Darkages
{
    public class Map : ObjectManager
    {
        public int ID { get; set; }
        public int Music { get; set; }
        public ushort Rows { get; set; }
        public ushort Cols { get; set; }
        public string Name { get; set; }
        public string ContentName { get; set; }
        public MapFlags Flags { get; set; }
    }

    public class TileGrid
    {
        public ConcurrentDictionary<int, Sprite> Sprites = new ConcurrentDictionary<int, Sprite>();

        public bool IsPassable(Sprite sprite, bool isAisling)
        {
            var length  = 0;
            var objects = Sprites.Select(i => i.Value).ToArray();

            foreach (var obj in objects)
            {
                if (obj.Serial == sprite.Serial)
                    continue;

                if (obj.X == sprite.X && obj.Y == sprite.Y)
                    continue;

                if (obj is Monster || obj is Aisling || obj is Mundane)
                {
                    length++;
                }
            }

            if (isAisling)
            {

            }

            return length == 0;
        }

        public void AddObject(Sprite obj)
        {
            if (Sprites.ContainsKey(obj.Serial))
                RemoveObject(obj);

            if (Sprites.TryAdd(obj.Serial, obj))
            {

            }
        }

        public void RemoveObject(Sprite obj)
        {
            if (Sprites.TryRemove(obj.Serial, out var removedSprite))
            {

            }
        }
    }
    public class Area : Map
    {
        [JsonIgnore] private static readonly byte[] Sotp = File.ReadAllBytes("sotp.dat");

        [JsonIgnore] readonly GameServerTimer _updateTimer = new GameServerTimer(TimeSpan.FromMilliseconds(10));

        [JsonIgnore] public byte[] Data;

        [JsonIgnore] public ushort Hash;

        [JsonIgnore] public bool Ready;

        [JsonIgnore] public TileContent[,] Tile { get; set; }

        [JsonIgnore] public TileGrid[,] ObjectGrid { get; set; }

        public bool ParseMapWalls(short lWall, short rWall)
        {
            if (lWall == 0 && rWall == 0)
                return false;

            if (lWall == 0)
                return Sotp[rWall - 1] == 0x0F;

            if (rWall == 0)
                return Sotp[lWall - 1] == 0x0F;

            return Sotp[lWall - 1] == 0x0F && Sotp[rWall - 1] == 0x0F;
        }


        public bool IsWall(int x, int y, bool IsAisling = false)
        {

            if (IsAisling)
            {

            }

            if (x < 0 || x >= Cols)
            {
                return true;
            }

            if (y < 0 || y >= Rows)
            {
                return true;
            }


            return Tile[x, y] == TileContent.Wall;
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

        //public byte[] GetRowData(int row)
        //{
        //    var buffer = new byte[Cols * 6];
        //    var bPos = 0;
        //    var dPos = row * Cols * 6;

        //    lock (_syncLock)
        //    {
        //        for (var i = 0; i < Cols; i++, bPos += 6, dPos += 6)
        //        {
        //            buffer[bPos + 0] = Data[dPos + 1];

        //            buffer[bPos + 1] = Data[dPos + 0];

        //            buffer[bPos + 2] = Data[dPos + 3];

        //            buffer[bPos + 3] = Data[dPos + 2];

        //            buffer[bPos + 4] = Data[dPos + 5];

        //            buffer[bPos + 5] = Data[dPos + 4];
        //        }
        //    }

        //    return buffer;
        //}


        public Sprite[] GetAreaObjects()
        {
            return GetObjects(this, i => i != null, Get.All).ToArray();
        }

        public void UpdateAreaObjects(TimeSpan elapsedTime)
        {
            var objectCache = GetAreaObjects();

            if (objectCache == null || objectCache.Length <= 0)
                return;

            lock (ServerContext.syncLock)
            {
                UpdateMonsterObjects(elapsedTime, objectCache.OfType<Monster>());
                UpdateMundaneObjects(elapsedTime, objectCache.OfType<Mundane>());
                UpdateItemObjects(elapsedTime, objectCache.OfType<Money>().Concat<Sprite>(objectCache.OfType<Item>()));
            }
        }

        public void Update(TimeSpan elapsedTime)
        {
            _updateTimer.Update(elapsedTime);

            if (!_updateTimer.Elapsed)
                return;

            UpdateAreaObjects(elapsedTime);

            _updateTimer.Reset();
        }

        public void UpdateMonsterObjects(TimeSpan elapsedTime, IEnumerable<Monster> objects)
        {
            var enumerable = objects as Monster[] ?? objects.ToArray();

            foreach (var obj in enumerable)
            {
                if (obj?.Map == null || obj.Scripts == null)
                    continue;

                if (obj.CurrentHp <= 0x0 && obj.Target != null && !obj.Skulled)
                {
                    foreach (var script in obj.Scripts.Values)
                    {
                        if (obj.Target?.Client == null)
                            continue;

                        script?.OnDeath(obj.Target.Client);
                    }

                    obj.Skulled = true;
                }

                foreach (var script in obj.Scripts.Values)
                {
                    script?.Update(elapsedTime);
                }

                if (obj.WalkEnabled
                    || ((DateTime.UtcNow - obj.LastMovementChanged).TotalMilliseconds > obj.Template.MovementSpeed * 6)
                    && ((DateTime.UtcNow - obj.LastTurnUpdated).TotalMilliseconds > obj.Template.MovementSpeed * 8))
                {
                    if (obj.Target == null && obj.CurrentHp <= 0)
                    {
                        obj.Remove();
                        continue;
                    }
                }


                if (obj.TrapsAreNearby())
                {
                    var nextTrap = Trap.Traps.Select(i => i.Value)
                        .FirstOrDefault(i => i.Location.X == obj.X && i.Location.Y == obj.Y);

                    if (nextTrap != null)
                    {
                        Trap.Activate(nextTrap, obj);
                    }
                }


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
                        obj.CurrentHp = obj.Template.MaximumHp;

                    obj.UpdateBuffs(elapsedTime);
                    obj.UpdateDebuffs(elapsedTime);
                    obj.Update(elapsedTime);
                    obj.LastUpdated = DateTime.UtcNow;
                }
        }

        public void OnLoaded()
        {
            lock (ServerContext.syncLock)
            {
                Tile = new TileContent[Cols, Rows];
                ObjectGrid = new TileGrid[Cols, Rows];

                var stream = new MemoryStream(Data);
                var reader = new BinaryReader(stream);

                for (var y = 0; y < Rows; y++)
                {
                    for (var x = 0; x < Cols; x++)
                    {
                        ObjectGrid[x,y] = new TileGrid();

                        reader.BaseStream.Seek(2, SeekOrigin.Current);

                        if (ParseMapWalls(reader.ReadInt16(), reader.ReadInt16()))
                        {
                            Tile[x, y] = TileContent.Wall;
                        }
                        else
                        {
                            Tile[x, y] = TileContent.None;
                        }
                    }
                }

                reader.Close();
                stream.Close();

                Ready = true;
            }
        }
    }
}