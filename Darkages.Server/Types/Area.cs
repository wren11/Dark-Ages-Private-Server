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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Darkages
{
    public class Area : ObjectManager
    {
        [JsonIgnore] private static readonly byte[] sotp = File.ReadAllBytes("sotp.dat");

        [JsonIgnore] [Browsable(false)] public ushort Hash;

        [JsonIgnore] [Browsable(false)] private TileContent[,] Tile;
         
        [JsonIgnore]
        [Browsable(false)]
        private readonly GameServerTimer WarpTimer =
            new GameServerTimer(TimeSpan.FromSeconds(ServerContext.Config.WarpUpdateTimer));

        [JsonIgnore]
        [Browsable(false)]
        private readonly GameServerTimer ScriptTimer =
            new GameServerTimer(TimeSpan.FromMilliseconds(100));


        [JsonIgnore] [Browsable(false)] public byte[] Data { get; set; }

        public int Music { get; set; }

        [JsonIgnore] [Browsable(false)] public bool Ready { get; set; }

        [JsonRequired] public ushort Rows { get; set; }

        [JsonRequired] public ushort Cols { get; set; }

        [JsonRequired] public int Number { get; set; }

        public int ID { get; set; }
        public string Name { get; set; }
        public MapFlags Flags { get; set; }

        public TileContent this[int x, int y]
        {
            get => Tile[x, y];
            set => Update(x, y, value);
        }


        public void Update(int x, int y, TileContent value)
        {
            lock (Tile.SyncRoot)
            {
                Tile[x, y] = value;
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

            return (this.Tile[x, y] != TileContent.None);
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

        public void Update(TimeSpan elapsedTime)
        {
            UpdateScripts(elapsedTime);

            if (!Has<Aisling>())
                return;

            if (Has<Mundane>())
                UpdateMundanes(elapsedTime);

            if (Has<Item>() || Has<Money>())
                UpdateItems(elapsedTime);

            WarpTimer.Update(elapsedTime);
            if (WarpTimer.Elapsed)
            {
                UpdateWarps();
                WarpTimer.Reset();
            }
        }

        private void UpdateScripts(TimeSpan elapsedTime)
        {
            ScriptTimer.Update(elapsedTime);
            if (ScriptTimer.Elapsed)
            {
                if (Has<Monster>())
                    UpdateMonsters(elapsedTime);

                ScriptTimer.Reset();
            }
        }

        private void UpdateWarps()
        {
            var warpsOnMap = ServerContext.GlobalWarpTemplateCache.Where(i => i.ActivationMapId == ID);

            foreach (var warp in warpsOnMap)
            {
                if (!Has<Aisling>())
                    continue;

                if (!warp.Activations.Any())
                    continue;

                var nearby = GetObjects<Aisling>(i =>
                    i.LoggedIn && i.CurrentMapId == warp.ActivationMapId);

                if (nearby.Length == 0)
                    continue;

                foreach (var warpObj in warp.Activations)
                    foreach (var obj in nearby)
                        if (obj.Position.WithinSquare(warpObj.Location, 10))
                            obj.Show(Scope.Self, new ServerFormat29(
                                ServerContext.Config.WarpAnimationNumber,
                                warpObj.Location.X, warpObj.Location.Y));

            }
        }

        private void UpdateMonsters(TimeSpan elapsedTime)
        {
            foreach (var obj in GetObjects<Monster>(i => i.CurrentMapId == ID))
            {
                var nearby = GetObjects<Aisling>(i => i.WithinRangeOf(obj) && i.CurrentMapId == ID).Length;

                if (nearby == 0 && !obj.Template.UpdateMapWide)
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
                }
            }
        }

        private void UpdateItems(TimeSpan elapsedTime)
        {
            foreach (var obj in GetObjects(i => i.CurrentMapId == ID, Get.Items | Get.Money))
            {
                var nearby = GetObjects<Aisling>(i => i.WithinRangeOf(obj) && i.CurrentMapId == ID);

                if (nearby.Length == 0)
                    continue;

                if (obj != null)
                {

                    foreach (var nb in nearby)
                    {
                        if (!nb.InsideView(obj))
                            obj.ShowTo(nb);
                    }

                    obj.LastUpdated = DateTime.UtcNow;

                    if (obj is Item)
                    {
                        if ((DateTime.UtcNow - obj.CreationDate).TotalMinutes > 3)
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

        private void UpdateMundanes(TimeSpan elapsedTime)
        {
            var objects = GetObjects<Mundane>(i => i != null && i.CurrentMapId == ID);
            if (objects == null)
                return;

            foreach (var obj in objects)
            {
                if (obj == null)
                    continue;

                var nearby = GetObjects<Aisling>(i => i.WithinRangeOf(obj) && i.CurrentMapId == ID);

                if (nearby.Length == 0)
                    continue;

                foreach (var nb in nearby)
                {
                    if (!nb.InsideView(obj))
                        obj.ShowTo(nb);
                }

                obj.UpdateBuffs(elapsedTime);
                obj.UpdateDebuffs(elapsedTime);
                obj.Update(elapsedTime);

                obj.LastUpdated = DateTime.UtcNow;
            }
        }

        public bool Has<T>()
            where T : Sprite, new()
        {
            var objs = GetObjects<T>(i => i != null && i.CurrentMapId == ID);

            return objs.Length > 0;
        }

        public void OnLoaded(Aisling obj = null)
        {
            Tile = new TileContent[Cols, Rows];

            using (var stream = new MemoryStream(Data))
            {
                using (var reader = new BinaryReader(stream))
                {
                    for (var y = 0; y < Rows; y++)
                    {
                        for (var x = 0; x < Cols; x++)
                        {
                            reader.BaseStream.Seek(2, SeekOrigin.Current);

                            if (ParseSotp(reader.ReadInt16(), reader.ReadInt16()))
                                this[x, y] = TileContent.Wall;
                            else
                                this[x, y] = TileContent.None;
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
                        this[o.Location.X, o.Location.Y] = TileContent.None;
        }

        public Position FindNearestEmpty(Position aislingPosition)
        {
            var positions = new List<Position>();

            for (var y = 0; y < Rows; y++)
                for (var x = 0; x < Cols; x++)
                    if (this[x, y] == TileContent.None
                        || this[x, y] == TileContent.Money
                        || this[x, y] == TileContent.Item)
                        positions.Add(new Position(x, y));

            return positions.OrderBy(i => i.DistanceFrom(aislingPosition))
                .FirstOrDefault();
        }

        internal void UpdateCollisions(Aisling obj)
        {
            SetWarps();

            if (obj == null)
                return;

            lock (obj.ViewFrustrum)
            {
                foreach (var sprite in obj.ViewFrustrum.Select(i => i.Value))
                    this[sprite.X, sprite.Y] = sprite.Content;
            }
        }
    }
}
