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
using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Systems.Loot;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static Darkages.Common.Generator;
using static Darkages.ServerContext;

namespace Darkages.Types
{
    public class Monster : Sprite
    {

        public Monster()
        {
            BashEnabled = false;
            CastEnabled = false;
            WalkEnabled = false;
            WaypointIndex = 0;
        }

        [JsonIgnore] public MonsterScript Script { get; set; }

        public GameServerTimer BashTimer { get; set; }
        public GameServerTimer CastTimer { get; set; }
        public GameServerTimer WalkTimer { get; set; }
        public MonsterTemplate Template { get; set; }

        [JsonIgnore] public bool IsAlive => CurrentHp > 0;

        [Browsable(false)] public bool BashEnabled { get; set; }

        [Browsable(false)] public bool CastEnabled { get; set; }

        [Browsable(false)] public bool WalkEnabled { get; set; }

        public ushort Image { get; private set; }

        [JsonIgnore] public bool Rewarded { get; set; }

        public bool Aggressive { get; set; }

        [JsonIgnore]
        public int WaypointIndex = 0;

        [JsonIgnore]
        public Position CurrentWaypoint => Template.Waypoints[WaypointIndex] ?? null;

        [JsonIgnore]
        public LootTable LootTable { get; set; }

        [JsonIgnore]
        public ConcurrentDictionary<int, Sprite> TaggedAislings { get; set; }

        [JsonIgnore]
        public LootTable UpgradeTable { get; set; }


        [JsonIgnore]
        public LootDropper LootManager { get; set; }

        public bool NextTo(int x, int y)
        {
            var xDist = Math.Abs(x - X);
            var yDist = Math.Abs(y - Y);

            return xDist + yDist == 1;
        }

        public bool NextTo(Sprite target)
        {
            return NextTo(target.X, target.Y);
        }

        public void GenerateRewards(Aisling player)
        {
            if (Rewarded)
                return;

            if (player.Equals(null))
                return;

            if (player.Client.Aisling == null)
                return;

            UpdateCounters(player);

            GenerateExperience(player);
            GenerateGold();           
            GenerateDrops();

            Rewarded = true;
            player.UpdateStats();
        }

        private void UpdateCounters(Aisling player)
        {
            if (!player.MonsterKillCounters.ContainsKey(Template.Name))
                player.MonsterKillCounters[Template.Name] = 1;
            else
            {
                player.MonsterKillCounters[Template.Name]++;
            }
        }



        private void GenerateExperience(Aisling player)
        {
            var exp = 0;
            var seed = (Template.Level * 0.1) + 1.5;
            {
                exp = (int)(Template.Level * seed * 300);
            }

            var critical = Math.Abs(GenerateNumber() % 100);
            if (critical >= 30 && critical <= 32)
            {
                player.SendAnimation(341, player, this);
                exp *= 2;
            }

            DistributeExperience(player, exp);

            foreach (var party in player.PartyMembers.Where(i => i.Serial != player.Serial))
            {
                if (party.WithinRangeOf(player))
                    DistributeExperience(party, exp);
            }
        }

        public static void DistributeExperience(Aisling player, double exp)
        {
            if (player.ExpLevel >= ServerContext.Config.PlayerLevelCap)
            {
                player.ExpLevel = 1;
            }

            //Formula BoilerPlate: 1000 * E7 * 10 /100
            var bonus = exp * player.GroupParty.LengthExcludingSelf * ServerContext.Config.GroupExpBonus / 100;

            if (bonus > 0)
                exp += bonus;

            player.ExpTotal += (int)exp;
            player.ExpNext -= (int)exp;

            player.Client.SendMessage(0x02, string.Format("You received {0} Experience!.", (int)exp));

            var seed = (player.ExpLevel * 0.1) + 0.5;

            while (player.ExpNext <= 0 && player.ExpLevel < 99)
            {
                player.ExpNext = (int)(player.ExpLevel * seed * 5000);

                if (player.ExpLevel == 99)
                {
                    break;
                }

                if (player.ExpTotal <= 0)
                {
                    player.ExpTotal = int.MaxValue;
                }

                if (player.ExpTotal > int.MaxValue)
                {
                    player.ExpTotal = int.MaxValue;
                }

                if (player.ExpNext <= 0)
                {
                    player.ExpNext = 1;
                }

                if (player.ExpNext > int.MaxValue)
                {
                    player.ExpNext = int.MaxValue;
                }

                Levelup(player);
            }
        }

        private static void Levelup(Aisling player)
        {
            player._MaximumHp += (int)(ServerContext.Config.HpGainFactor * player.Con * 0.65);
            player._MaximumMp += (int)(ServerContext.Config.MpGainFactor * player.Wis * 0.45);
            player.StatPoints += ServerContext.Config.StatsPerLevel;

            player.ExpLevel++;

            if (player.ExpLevel > 99)
            {
                player.AbpLevel++;
                player.ExpLevel = 99;
            }

            if (player.AbpLevel > 99)
            {
                player.AbpLevel = 99;
                player.GamePoints++;
            }

            player.Client.SendMessage(0x02, string.Format(ServerContext.Config.LevelUpMessage, player.ExpLevel));
            player.Show(Scope.NearbyAislings,
                new ServerFormat29((uint)player.Serial, (uint)player.Serial, 0x004F, 0x004F, 64));
        }

        private void GenerateGold()
        {
            if (!Template.LootType.HasFlag(LootQualifer.Gold))
                return;

            int sum = 0;

            lock (rnd)
            {
                sum = rnd.Next(
                            Template.Level * 500,
                            Template.Level * 1000);
            }

            if (sum > 0)
                Money.Create(this, sum, new Position(X, Y));
        }

        private List<string> DetermineDrop()
            => LootManager.Drop(LootTable, rnd.Next(ServerContext.Config.LootTableStackSize))
                .Select(i => i?.Name).ToList();

        private ItemUpgrade DetermineQuality()
            => (ItemUpgrade)LootManager.Drop(UpgradeTable, 1).FirstOrDefault();

        private void DetermineRandomDrop()
        {
            int idx = 0;
            if (Template.Drops.Count > 0)
                lock (rnd)
                {
                    idx = rnd.Next(Template.Drops.Count);
                }

            var rndSelector = Template.Drops[idx];
            if (GlobalItemTemplateCache.ContainsKey(rndSelector))
            {
                var item = Item.Create(this, GlobalItemTemplateCache[rndSelector], true);
                var chance = 0.00;

                lock (rnd)
                {
                    chance = Math.Round(rnd.NextDouble(), 2);
                }

                if (chance <= item.Template.DropRate)
                    item.Release(this, Position);
            }
        }

        private void GenerateDrops()
        {
            if (Template.LootType.HasFlag(LootQualifer.Table))
            {
                if (LootTable == null || LootManager == null)
                    return;

                lock (rnd)
                {
                    DetermineDrop().ForEach(i =>
                    {
                        if (i != null)
                        {
                            if (GlobalItemTemplateCache.ContainsKey(i))
                            {

                                var rolled_item = Item.Create(this, GlobalItemTemplateCache[i]);
                                if (rolled_item != GlobalLastItemRoll)
                                {
                                    GlobalLastItemRoll = rolled_item;
                                    var upgrade = DetermineQuality();

                                    if (rolled_item == null)
                                        return;

                                    if (rolled_item.Template.Flags.HasFlag(ItemFlags.QuestRelated))
                                        upgrade = null;

                                    rolled_item.Upgrades = upgrade?.Upgrade ?? 0;
                                    Item.ApplyQuality(rolled_item);

                                    rolled_item.Cursed = true;
                                    rolled_item.AuthenticatedAislings = GetTaggedAislings();
                                    rolled_item.Release(this, Position);

                                    if (rolled_item.Upgrades > 3)
                                    {
                                        var users = GetTaggedAislings();
                                        foreach (var user in users)
                                        {
                                            var msg = string.Format("{0} Drop!!! ({1})", upgrade?.Name, rolled_item.DisplayName);
                                            user.Client.SendMessage(3, msg);

                                            //TODO: implement more rarity animations to display.
                                            user.Client.SendAnimation(Config.RareDropAnimation, rolled_item, rolled_item, 100, true);
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
                return;
            }
            else if (Template.LootType.HasFlag(LootQualifer.Random))
            {
                DetermineRandomDrop();
                return;
            }
        }

        private Sprite[] GetTaggedAislings()
        {
            var tagged = TaggedAislings.Select(i => i.Value).ToArray();
            return tagged;
        }

        public void AppendTags(Sprite target)
        {
            if (TaggedAislings == null)
                TaggedAislings = new ConcurrentDictionary<int, Sprite>();

            if (target is Aisling)
            {
                var aisling = target as Aisling;

                if (!TaggedAislings.ContainsKey(aisling.Serial))
                {
                    TaggedAislings.TryAdd(aisling.Serial, aisling);
                }

                if (aisling.GroupParty.LengthExcludingSelf > 0)
                {
                    foreach (var member in aisling.GroupParty.MembersExcludingSelf)
                    {
                        if (!TaggedAislings.ContainsKey(member.Serial))
                        {
                            TaggedAislings.TryAdd(member.Serial, member);
                        }
                    }
                }
            }
        }


        public static Monster Create(MonsterTemplate template, Area map)
        {

            if (template.CastSpeed == 0)
                template.CastSpeed = 2000;

            if (template.AttackSpeed == 0)
                template.AttackSpeed = 1000;

            if (template.MovementSpeed == 0)
                template.MovementSpeed = 2000;

            if (template.Level <= 0)
                template.Level = 1;

            var obj = new Monster();
            obj.Template = template;
            obj.CastTimer = new GameServerTimer(TimeSpan.FromMilliseconds(1 + template.CastSpeed / 10));
            obj.BashTimer = new GameServerTimer(TimeSpan.FromMilliseconds(1 + template.AttackSpeed / 10));
            obj.WalkTimer = new GameServerTimer(TimeSpan.FromMilliseconds(1 + template.MovementSpeed / 10));
            obj.CastEnabled = template.MaximumMP > 0;
            obj.TaggedAislings = new ConcurrentDictionary<int, Sprite>();

            if (obj.Template.Grow)
                obj.Template.Level++;


            obj.Template.MaximumHP = Config.MONSTER_HP_TABLE[obj.Template.Level];
            obj.Template.MaximumMP = Config.MONSTER_HP_TABLE[obj.Template.Level] / 3;


            var stat = RandomEnumValue<PrimaryStat>();

            obj._Str = 3;
            obj._Int = 3;
            obj._Wis = 3;
            obj._Con = 3;
            obj._Dex = 3;

            switch (stat)
            {
                case PrimaryStat.STR:
                    obj._Str += (byte)(obj.Template.Level * 0.5 * 2);
                    break;
                case PrimaryStat.INT:
                    obj._Int += (byte)(obj.Template.Level * 0.5 * 2);
                    break;
                case PrimaryStat.WIS:
                    obj._Wis += (byte)(obj.Template.Level * 0.5 * 2);
                    break;
                case PrimaryStat.CON:
                    obj._Con += (byte)(obj.Template.Level * 0.5 * 2);
                    break;
                case PrimaryStat.DEX:
                    obj._Dex += (byte)(obj.Template.Level * 0.5 * 2);
                    break;

            }

            obj.MajorAttribute = stat;

            //calculate what ac to give depending on level.
            obj.BonusAc = (sbyte)(70 - 101 / 70 * template.Level);

            if (obj.Template.Level > 99)
            {
                var remaining = Math.Abs(ServerContext.Config.MaxAC - obj.Template.Level) + obj.Template.Level / 10;

                obj.BonusAc = (sbyte)-remaining;

            }

            obj.DefenseElement = ElementManager.Element.None;
            obj.OffenseElement = ElementManager.Element.None;

            if (obj.Template.ElementType == ElementQualifer.Random)
            {
                obj.DefenseElement = RandomEnumValue<ElementManager.Element>();
                obj.OffenseElement = RandomEnumValue<ElementManager.Element>();
            }
            else if (obj.Template.ElementType == ElementQualifer.Defined)
            {
                obj.DefenseElement = template?.DefenseElement == ElementManager.Element.None
                    ? RandomEnumValue<ElementManager.Element>()
                    : template.DefenseElement;
                obj.OffenseElement = template?.OffenseElement == ElementManager.Element.None
                    ? RandomEnumValue<ElementManager.Element>()
                    : template.OffenseElement;
            }

            obj.BonusMr = (byte)(10 * (template.Level / 10 * 100 / 100));

            if (obj.BonusMr > Config.BaseMR)
                obj.BonusMr = Config.BaseMR;

            if ((template.PathQualifer & PathQualifer.Wander) == PathQualifer.Wander)
                obj.WalkEnabled = true;
            else if ((template.PathQualifer & PathQualifer.Fixed) == PathQualifer.Fixed)
                obj.WalkEnabled = false;
            else if ((template.PathQualifer & PathQualifer.Patrol) == PathQualifer.Patrol)
                obj.WalkEnabled = true;

            if (template.MoodType.HasFlag(MoodQualifer.Aggressive))
                obj.Aggressive = true;
            else if (template.MoodType.HasFlag(MoodQualifer.Unpredicable))
                lock (Generator.Random)
                {
                    //this monster has a 50% chance of being aggressive.
                    obj.Aggressive = Generator.Random.Next(1, 101) > 50;
                }
            else
            {
                obj.Aggressive = false;
            }

            if (template.SpawnType == SpawnQualifer.Random)
            {
                var x = Generator.Random.Next(1, map.Cols);
                var y = Generator.Random.Next(1, map.Rows);

                obj.X = x;
                obj.Y = y;

                if (map.IsWall(x, y))
                    return null;
            }
            else if (template.SpawnType == SpawnQualifer.Defined)
            {
                obj.X = template.DefinedX;
                obj.Y = template.DefinedY;
            }

            lock (Generator.Random)
            {
                obj.Serial = Generator.GenerateNumber();
            }

            obj.CurrentMapId = map.ID;
            obj.CurrentHp = template.MaximumHP;
            obj.CurrentMp = template.MaximumMP;
            obj._MaximumHp = template.MaximumHP;
            obj._MaximumMp = template.MaximumMP;
            obj.AbandonedDate = DateTime.UtcNow;

            lock (Generator.Random)
            {
                obj.Image = template.ImageVarience
                            > 0
                    ? (ushort)Generator.Random.Next(template.Image, template.Image + template.ImageVarience)
                    : template.Image;
            }

            obj.Script = ScriptManager.Load<MonsterScript>(template.ScriptName, obj, map);

            if (obj.Template.LootType.HasFlag(LootQualifer.Table))
            {
                obj.LootManager = new LootDropper();
                obj.LootTable = new LootTable(template.Name);
                obj.UpgradeTable = new LootTable("Probabilities");

                lock (ServerContext.SyncObj)
                {
                    foreach (var drop in obj.Template.Drops)
                    {
                        if (drop.Equals("random", StringComparison.OrdinalIgnoreCase))
                        {
                            lock (Generator.Random)
                            {
                                var available = GlobalItemTemplateCache.Select(i => i.Value)
                                    .Where(i => Math.Abs(i.LevelRequired - obj.Template.Level) <= 10).ToList();
                                if (available.Count > 0)
                                {
                                    obj.LootTable.Add(available[GenerateNumber() % available.Count]);
                                }
                            }
                        }
                        else
                        {
                            if (GlobalItemTemplateCache.ContainsKey(drop))
                            {
                                obj.LootTable.Add(GlobalItemTemplateCache[drop]);
                            }
                        }
                    }
                }

                obj.UpgradeTable.Add(new Common());
                obj.UpgradeTable.Add(new Uncommon());
                obj.UpgradeTable.Add(new Rare());
                obj.UpgradeTable.Add(new Epic());
                obj.UpgradeTable.Add(new Legendary());
                obj.UpgradeTable.Add(new Mythical());
                obj.UpgradeTable.Add(new Godly());
                obj.UpgradeTable.Add(new Forsaken());
            }

            return obj;
        }

        public void Patrol(bool ignoreWalls = false)
        {
            if (CurrentWaypoint != null)
            {
                WalkTo(CurrentWaypoint.X, CurrentWaypoint.Y, ignoreWalls);
            }

            if (Position.DistanceFrom(CurrentWaypoint) <= 1 || CurrentWaypoint == null)
            {
                if (WaypointIndex + 1 < Template.Waypoints.Count)
                    WaypointIndex++;
                else
                    WaypointIndex = 0;
            }
        }
    }
}
