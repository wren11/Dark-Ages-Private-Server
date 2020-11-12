#region

using Darkages.Storage;
using Darkages.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Darkages.Scripting;

#endregion

namespace Darkages
{
    public class Area : Map
    {
        [JsonIgnore] private static readonly byte[] Sotp = File.ReadAllBytes("sotp.dat");
        [JsonIgnore] public byte[] Data;
        [JsonIgnore] public ushort Hash;
        [JsonIgnore] public bool Ready;
        [JsonIgnore] public TileGrid[,] ObjectGrid { get; set; }
        [JsonIgnore] public TileContent[,] Tile { get; set; }
        [JsonIgnore] public Dictionary<string, AreaScript> Scripts { get; set; } = new Dictionary<string, AreaScript>();

        public string FilePath { get; set; }

        public byte[] GetRowData(int row)
        {
            var buffer = new byte[Cols * 6];
            var bPos = 0;
            var dPos = row * Cols * 6;

            lock (ServerContext.SyncLock)
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

        public bool IsWall(int x, int y)
        {
            if (x < 0 || x >= Cols) return true;

            if (y < 0 || y >= Rows) return true;

            var isWall = Tile[x, y] == TileContent.Wall;
            return isWall;
        }

        public bool OnLoaded()
        {


            var delete = false;
            lock (ServerContext.SyncLock)
            {
                Tile = new TileContent[Cols, Rows];
                ObjectGrid = new TileGrid[Cols, Rows];

                var stream = new MemoryStream(Data);
                var reader = new BinaryReader(stream);

                try
                {
                    for (var y = 0; y < Rows; y++)
                    {
                        for (var x = 0; x < Cols; x++)
                        {
                            ObjectGrid[x, y] = new TileGrid(this, x, y);

                            reader.BaseStream.Seek(2, SeekOrigin.Current);

                            if (ParseMapWalls(reader.ReadInt16(), reader.ReadInt16()))
                                Tile[x, y] = TileContent.Wall;
                            else
                                Tile[x, y] = TileContent.None;
                        }
                    }

                    foreach (var block in Blocks)
                    {
                        Tile[block.X, block.Y] = TileContent.Wall;
                    }

                    Ready = true;
                }
                catch
                {
                    //Ignore
                    delete = true;
                }
                finally
                {
                    reader.Close();
                    stream.Close();
                }

                if (!delete)
                    return true;

            }

            return Ready;
        }

        public bool ParseMapWalls(short lWall, short rWall)
        {
            if (lWall == 0 && rWall == 0)
                return false;

            if (lWall == 0)
                return Sotp[rWall - 1] == 0x0F;

            if (rWall == 0)
                return Sotp[lWall - 1] == 0x0F;

            var left = Sotp[lWall - 1];
            var right = Sotp[rWall - 1];

            return left == 0x0F || right == 0x0F;
        }

        public void Update(in TimeSpan elapsedTime)
        {
            if (Scripts != null)
                foreach (var script in Scripts.Values)
                    script.Update(elapsedTime);

            UpdateAreaObjects(elapsedTime);
        }

        public void UpdateAreaObjects(TimeSpan elapsedTime)
        {
            var objectCache = GetObjects(this, sprite => sprite.AislingsNearby().Any(), Get.All);

            foreach (var obj in objectCache)
            {
                if (obj != null)
                {
                    switch (obj)
                    {
                        case Monster monster when monster.Map == null || monster.Scripts == null:
                            continue;
                        case Monster monster:
                        {
                            if (obj.CurrentHp <= 0x0 && obj.Target != null && !monster.Skulled)
                            {
                                foreach (var script in monster.Scripts.Values.Where(
                                    script => obj.Target?.Client != null))
                                    script?.OnDeath(obj.Target.Client);

                                monster.Skulled = true;
                            }

                            foreach (var script in monster.Scripts.Values) script?.Update(elapsedTime);

                            if (obj.TrapsAreNearby())
                            {
                                var nextTrap = Trap.Traps.Select(i => i.Value)
                                    .FirstOrDefault(i => i.Location.X == obj.X && i.Location.Y == obj.Y);

                                if (nextTrap != null)
                                    Trap.Activate(nextTrap, obj);
                            }

                            monster.UpdateBuffs(elapsedTime);
                            monster.UpdateDebuffs(elapsedTime);
                            break;
                        }
                        case Item item:
                        {
                            var stale = !((DateTime.UtcNow - item.AbandonedDate).TotalMinutes > 3);

                            if (item.Cursed && stale)
                            {
                                item.AuthenticatedAislings = null;
                                item.Cursed = false;
                            }

                            break;
                        }
                        case Mundane mundane:
                        {
                            if (mundane.CurrentHp <= 0)
                                mundane.CurrentHp = mundane.Template.MaximumHp;

                            mundane.UpdateBuffs(elapsedTime);
                            mundane.UpdateDebuffs(elapsedTime);
                            mundane.Update(elapsedTime);
                            break;
                        }
                    }

                    obj.LastUpdated = DateTime.UtcNow;
                }
            }
        }

        public void AddBlock(Position position)
        {
            Blocks?.Add(position);
            
            StorageManager.AreaBucket.Save(this);
        }
    }
}