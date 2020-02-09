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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.Object;
using Darkages.Network.ServerFormats;
using Darkages.Storage;
using Darkages.Types;
using Newtonsoft.Json;

namespace Darkages
{
    public class Area : ObjectManager
    {
        [JsonIgnore] private static readonly byte[] sotp = File.ReadAllBytes("sotp.dat");

        public static Dictionary<int, Area> Instances = new Dictionary<int, Area>();

        [JsonIgnore]
        [Browsable(false)]
        private readonly GameServerTimer UpdateTimer =
            new GameServerTimer(TimeSpan.FromMilliseconds(30));

        [JsonIgnore]
        [Browsable(false)]
        private readonly GameServerTimer WarpTimer =
            new GameServerTimer(TimeSpan.FromSeconds(1.1));

        [JsonIgnore]
        public GameServerTimer _Reaper = new GameServerTimer(TimeSpan.FromSeconds(2));

        [JsonIgnore]
        public Cache<Sprite[]> AreaObjectCache = new Cache<Sprite[]>();


        [JsonIgnore]
        [Browsable(false)]
        public byte[] Data;

        [JsonIgnore]
        [Browsable(false)]
        public ushort Hash;

        [JsonIgnore]
        [Browsable(false)]
        public bool Ready;

        public int Music { get; set; }

        public ushort Rows { get; set; }

        public ushort Cols { get; set; }

        public int ID { get; set; }

        public int ContentOrder { get; set; }

        public string ContentDescription { get; set; }

        public string Name { get; set; }

        public string ContentName { get; set; }

        public MapFlags Flags { get; set; }

        [JsonIgnore]
        public Tile[,] MapNodes { get; set; }

        [JsonIgnore]
        private TileContent[,] BaseMap { get; set; }

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

            public bool CanSpawnMonster() => !HasMonsters && !HasMundanes;

            public void Add(TileContent content)
            {
                lock (Content)
                {
                    Content.Push(content);
                }
            }

            public void Empty()
            {
                Content = new Stack<TileContent>();
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

        internal void Update(int xPos, int yPos) => UpdateTileContents(xPos, yPos);

        internal void UpdateTileContents(int xPos, int yPos)
        {
            if (xPos < 0)
                xPos = 0;

            if (yPos < 0)
                yPos = 0;



            if (yPos >= MapNodes.GetUpperBound(1))
            {
                return;
            }

            if (xPos >= MapNodes.GetUpperBound(0))
            {
                return;
            }

            MapNodes[xPos, yPos].Empty();
            {
                if (BaseMap[xPos, yPos] == TileContent.Wall)
                {
                    MapNodes[xPos, yPos].Add(TileContent.Wall);
                    return;
                }

                var objects = GetObjects(this,
                    i => i.X == xPos && i.Y == yPos, Get.Monsters | Get.Mundanes | Get.Aislings);

                foreach (var obj in objects)
                {
                    if (MapNodes[xPos, yPos].IsAvailable())
                        MapNodes[xPos, yPos].Add(obj.EntityType);
                }
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

            int b_pos = 0;

            int d_pos = (row * Cols * 6);

            for (var i = 0; i < Cols; i++, b_pos += 6, d_pos += 6)
            {
                buffer[b_pos + 0] = Data[d_pos + 1];

                buffer[b_pos + 1] = Data[d_pos + 0];

                buffer[b_pos + 2] = Data[d_pos + 3];

                buffer[b_pos + 3] = Data[d_pos + 2];

                buffer[b_pos + 4] = Data[d_pos + 5];

                buffer[b_pos + 5] = Data[d_pos + 4];
            }

            return buffer;
        }


        public Sprite[] GetAreaObjects()
        {
            return GetObjects(this, i => i != null, Get.All).ToArray();
        }

        public void ObjectUpdate(TimeSpan elapsedTime)
        {
            var users = ServerContext.Game.Clients.Where(i => i != null &&
                                                              i.Aisling != null && i.Aisling.CurrentMapId == ID)
                .Select(i => i.Aisling).ToArray();

            Sprite[] ObjectCache = null;

            if (!AreaObjectCache.Exists(Name))
            {
                ObjectCache = GetAreaObjects();

                if (ObjectCache.Length > 0)
                {
                    AreaObjectCache.AddOrUpdate(Name, ObjectCache, 3);
                }
            }
            else
            {
                ObjectCache = AreaObjectCache.Get(Name);
            }

            if (ObjectCache != null && ObjectCache.Length > 0)
            {
                if (users.Length > 0)
                {
                    UpdateMonsterObjects (elapsedTime, ObjectCache.OfType<Monster>());
                    UpdateMundaneObjects (elapsedTime, ObjectCache.OfType<Mundane>());
                    UpdateItemObjects    (elapsedTime, ObjectCache.OfType<Money>().Concat<Sprite>(ObjectCache.OfType<Item>()));
                }
            }
        }

        public void Update(TimeSpan elapsedTime)
        {
            UpdateTimer.Update(elapsedTime);

            if (UpdateTimer.Elapsed)
            {
                ObjectUpdate(elapsedTime);
                UpdateTimer.Reset();
            }

            if (ServerContext.Config.ShowWarpAnimation)
            {
                WarpTimer.Update(elapsedTime);

                if (WarpTimer.Elapsed)
                {
                    UpdateWarps();
                    WarpTimer.Reset();
                }
            }
        }

        private void UpdateWarps()
        {
            var warpsOnMap = ServerContext.GlobalWarpTemplateCache.Where(i => i.ActivationMapId == ID);

            foreach (var warp in warpsOnMap)
            {
                if (!warp.Activations.Any())
                    continue;

                var nearby = GetObjects<Aisling>(this, i =>
                    i.LoggedIn && i.CurrentMapId == warp.ActivationMapId);

                if (!nearby.Any())
                    continue;

                foreach (var warpObj in warp.Activations)
                {
                    foreach (var obj in nearby)
                        if (obj.WithinRangeOf(warpObj.Location.X, warpObj.Location.Y, 16))
                            DisplayWarpTo(warpObj, obj);
                }
            }
        }

        public void DisplayWarpTo(Warp warpObj, Aisling obj)
        {
            if (obj.WithinRangeOf(warpObj.Location.X, warpObj.Location.Y, 10))
            {
                obj.Show(Scope.Self, new ServerFormat29(
                    ServerContext.Config.WarpNumber,
                    warpObj.Location.X, warpObj.Location.Y));
            }
        }

        public void UpdateMonsterObjects(TimeSpan elapsedTime, IEnumerable<Monster> objects)
        {
            _Reaper.Update(elapsedTime);

            if (_Reaper.Elapsed)
            {
                _Reaper.Reset();

                foreach (var obj in objects)
                    if (obj != null && obj.Map != null && obj.Script != null)
                        if (obj.CurrentHp <= 0)
                            if (obj.Target != null)
                                obj.Script?.OnDeath(obj.Target.Client);
            }


            foreach (var obj in objects)
                if (obj != null && obj.Map != null && obj.Script != null)
                {
                    if (obj.CurrentHp <= 0)
                        if (obj.Target != null)
                            if (!obj.Skulled)
                            {
                                obj.Script?.OnSkulled(obj.Target.Client);
                                obj.Skulled = true;
                            }

                    obj.Script.Update(elapsedTime);
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

                    if (obj is Item)
                        if ((DateTime.UtcNow - obj.AbandonedDate).TotalMinutes > 3)
                            if ((obj as Item).Cursed)
                            {
                                (obj as Item).AuthenticatedAislings = null;
                                (obj as Item).Cursed = false;
                            }
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

            lock (MapNodes)
            {
                using (var stream = new MemoryStream(Data))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        for (var y = 0; y < Rows; y++)
                        {
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
            }

            Ready = true;
        }
    }
}


