#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Systems.Loot;
using Newtonsoft.Json;
using static Darkages.Common.Generator;
using static Darkages.Types.Item;

#endregion

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
            TaggedAislings = new HashSet<int>();

            EntityType = TileContent.Monster;
        }

        public bool Aggressive { get; set; } 
        public bool BashEnabled { get; set; }
        public bool CastEnabled { get; set; }
        public ushort Image { get; set; }
        public bool WalkEnabled { get; set; }
        public GameServerTimer BashTimer { get; set; } 
        public GameServerTimer CastTimer { get; set; }
        public MonsterTemplate Template { get; set; } 
        public GameServerTimer WalkTimer { get; set; }
        [JsonIgnore] public LootTable UpgradeTable { get; set; }
        [JsonIgnore] public bool IsAlive => CurrentHp > 0;
        [JsonIgnore] public LootDropper LootManager { get; set; }
        [JsonIgnore] public LootTable LootTable { get; set; }
        [JsonIgnore] public bool Rewarded { get; set; }
        [JsonIgnore] public Dictionary<string, MonsterScript> Scripts { get; set; }
        [JsonIgnore] public bool Skulled { get; set; }
        [JsonIgnore] public HashSet<int> TaggedAislings { get; set; }
        [JsonIgnore] public int WaypointIndex;
        [JsonIgnore] public Position CurrentWaypoint => Template?.Waypoints?[WaypointIndex];
        [JsonIgnore] public Item GlobalLastItemRoll { get; set; }

        public static Monster Create(MonsterTemplate template, Area map)
        {
            bool FindBestMonsterMapSlot(Monster monster)
            {
                switch (template.SpawnType)
                {
                    case SpawnQualifer.Random:
                    {
                        var x = Generator.Random.Next(1, map.Cols);
                        var y = Generator.Random.Next(1, map.Rows);

                        monster.XPos = x;
                        monster.YPos = y;

                        if (map.IsWall(x, y))
                            return true;
                        break;
                    }
                    case SpawnQualifer.Defined:
                        monster.XPos = template.DefinedX;
                        monster.YPos = template.DefinedY;
                        break;
                }

                return false;
            }

            //validate values in template.
            TemplateCreationSanityChecks();

            var obj = new Monster
            {
                Template = template,
                CastTimer = new GameServerTimer(TimeSpan.FromMilliseconds(1 + template.CastSpeed)),
                BashTimer = new GameServerTimer(TimeSpan.FromMilliseconds(1 + template.AttackSpeed)),
                WalkTimer = new GameServerTimer(TimeSpan.FromMilliseconds(1 + template.MovementSpeed)),
                CastEnabled = template.MaximumMP > 0,
                TaggedAislings = new HashSet<int>()
            };

            if (obj.Template.Grow)
                obj.Template.Level++;

            var mod = (obj.Template.Level + 1) * 0.01;
            var hp  = mod + 50 + obj.Template.Level * (obj.Template.Level + 40);
            var mp  = hp / 3;

            obj.Template.MaximumHP = (int)hp;
            obj.Template.MaximumMP = (int)mp;

            var stat = RandomEnumValue<PrimaryStat>();

            obj._Str = 1;
            obj._Int = 1;
            obj._Wis = 1;
            obj._Con = 1;
            obj._Dex = 1;

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

            obj.BonusAc = (int)(70 - obj.Template.Level * 0.5 / 1.0);

            if (obj.BonusAc < -70) obj.BonusAc = -70;

            obj.DefenseElement = ElementManager.Element.None;
            obj.OffenseElement = ElementManager.Element.None;

            if (obj.Template.ElementType == ElementQualifer.Random)
            {
                obj.DefenseElement = RandomEnumValue<ElementManager.Element>();
                obj.OffenseElement = RandomEnumValue<ElementManager.Element>();
            }
            else if (obj.Template.ElementType == ElementQualifer.Defined)
            {
                obj.DefenseElement = template.DefenseElement == ElementManager.Element.None
                    ? RandomEnumValue<ElementManager.Element>()
                    : template.DefenseElement;
                obj.OffenseElement = template.OffenseElement == ElementManager.Element.None
                    ? RandomEnumValue<ElementManager.Element>()
                    : template.OffenseElement;
            }

            obj.BonusMr = (byte)(10 * (template.Level / 20));

            if (obj.BonusMr > ServerContext.Config.BaseMR)
                obj.BonusMr = ServerContext.Config.BaseMR;

            if ((template.PathQualifer & PathQualifer.Wander) == PathQualifer.Wander)
                obj.WalkEnabled = true;
            else if ((template.PathQualifer & PathQualifer.Fixed) == PathQualifer.Fixed)
                obj.WalkEnabled = false;
            else if ((template.PathQualifer & PathQualifer.Patrol) == PathQualifer.Patrol)
                obj.WalkEnabled = true;

            if (template.MoodType.HasFlag(MoodQualifer.Aggressive))
                obj.Aggressive = true;
            else if (template.MoodType.HasFlag(MoodQualifer.Unpredicable))
            {
                lock (Generator.Random)
                {
                    obj.Aggressive = Generator.Random.Next(1, 101) > 50;
                }
            }
            else
                obj.Aggressive = false;

            //Based on Spawn type, Work out where i need to spawn.
            if (FindBestMonsterMapSlot(obj))
                return null;

            lock (Generator.Random)
            {
                obj.Serial = GenerateNumber();
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

            //Don't load scripts on creation. Instead on Approach.
            //InitScripting(template, map, obj);

            if (!obj.Template.LootType.HasFlag(LootQualifer.Table)) return obj;
            obj.LootManager = new LootDropper();
            obj.LootTable = new LootTable(template.Name);
            obj.UpgradeTable = new LootTable("Probabilities");

            foreach (var drop in obj.Template.Drops)
                if (drop.Equals("random", StringComparison.OrdinalIgnoreCase))
                {
                    lock (Generator.Random)
                    {
                        var available = ServerContext.GlobalItemTemplateCache.Select(i => i.Value)
                            .Where(i => Math.Abs(i.LevelRequired - obj.Template.Level) <= 10).ToList();
                        if (available.Count > 0) obj.LootTable.Add(available[GenerateNumber() % available.Count]);
                    }
                }
                else
                {
                    if (ServerContext.GlobalItemTemplateCache.ContainsKey(drop))
                        obj.LootTable.Add(ServerContext.GlobalItemTemplateCache[drop]);
                }

            obj.UpgradeTable.Add(new Common());
            obj.UpgradeTable.Add(new Uncommon());
            obj.UpgradeTable.Add(new Rare());
            obj.UpgradeTable.Add(new Epic());
            obj.UpgradeTable.Add(new Legendary());
            obj.UpgradeTable.Add(new Mythical());
            obj.UpgradeTable.Add(new Godly());
            obj.UpgradeTable.Add(new Forsaken());

            return obj;

            void TemplateCreationSanityChecks()
            {
                if (template.CastSpeed == 0)
                    template.CastSpeed = 2000;

                if (template.AttackSpeed == 0)
                    template.AttackSpeed = 1000;

                if (template.MovementSpeed == 0)
                    template.MovementSpeed = 2000;

                if (template.Level <= 0)
                    template.Level = 1;
            }
        }

        public static void InitScripting(MonsterTemplate template, Area map, Monster obj)
        {
            if (obj.Scripts == null || !obj.Scripts.Any())
            {
                obj.Scripts = ScriptManager.Load<MonsterScript>(template.ScriptName, obj, map);
            }
        }

        public static void DistributeExperience(Aisling player, double exp)
        {
            var chunks = exp / 1000;

            if (chunks <= 1)
                HandleExp(player, exp);
            else
                for (var i = 0; i < chunks; i++)
                    HandleExp(player, 1000);
        }

        public void AppendTags(Sprite target)
        {
            if (TaggedAislings == null)
                TaggedAislings = new HashSet<int>();

            if (!(target is Aisling aisling))
                return;

            if (!TaggedAislings.Contains(aisling.Serial))
                TaggedAislings.Add(aisling.Serial);

            if (aisling.GroupParty != null && aisling.GroupParty.PartyMembers.Count - 1 <= 0)
                return;

            if (aisling.GroupParty == null) return;

            foreach (var member in aisling.GroupParty.PartyMembers.Where(member =>
                !TaggedAislings.Contains(member.Serial)))
                TaggedAislings.Add(member.Serial);
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

        public List<Aisling> GetTaggedAislings()
        {
            if (TaggedAislings.Any())
                return TaggedAislings.Select(b => GetObject<Aisling>(Map, n => n.Serial == b)).Where(i => i != null)
                    .ToList();

            return new List<Aisling>();
        }

        public void Patrol(bool ignoreWalls = false)
        {
            if (CurrentWaypoint != null) WalkTo(CurrentWaypoint.X, CurrentWaypoint.Y, ignoreWalls);

            if (Position.DistanceFrom(CurrentWaypoint) <= 1 || CurrentWaypoint == null)
            {
                if (WaypointIndex + 1 < Template.Waypoints.Count)
                    WaypointIndex++;
                else
                    WaypointIndex = 0;
            }
        }

        private static void HandleExp(Aisling player, double exp)
        {
            if (exp <= 0)
                exp = 1;

            if (player.GroupParty != null)
            {
                var bonus = exp * (1 + player.GroupParty.PartyMembers.Count - 1) *
                            ServerContext.Config.GroupExpBonus /
                            100;

                if (bonus > 0)
                    exp += bonus;
            }

            player.ExpTotal += (uint)exp;
            player.ExpNext -= (uint)exp;

            if (player.ExpNext >= int.MaxValue) player.ExpNext = 0;

            var seed = player.ExpLevel * 0.1 + 0.5;
            {
                if (player.ExpLevel >= ServerContext.Config.PlayerLevelCap)
                    return;
            }

            while (player.ExpNext <= 0 && player.ExpLevel < 99)
            {
                player.ExpNext = (uint)(player.ExpLevel * seed * 5000);

                if (player.ExpLevel == 99)
                    break;

                if (player.ExpTotal <= 0)
                    player.ExpTotal = uint.MaxValue;

                if (player.ExpTotal >= uint.MaxValue)
                    player.ExpTotal = uint.MaxValue;

                if (player.ExpNext <= 0)
                    player.ExpNext = 1;

                if (player.ExpNext >= uint.MaxValue)
                    player.ExpNext = uint.MaxValue;

                Levelup(player);
            }
        }

        public static void Levelup(Aisling player)
        {
            if (player.ExpLevel >= ServerContext.Config.PlayerLevelCap)
                return;

            player._MaximumHp += (int)(ServerContext.Config.HpGainFactor * player.Con * 0.65);
            player._MaximumMp += (int)(ServerContext.Config.MpGainFactor * player.Wis * 0.45);
            player.StatPoints += ServerContext.Config.StatsPerLevel;

            player.ExpLevel++;

            player.Client.SendMessage(0x02,
                string.Format(ServerContext.Config.LevelUpMessage, player.ExpLevel));
            player.Show(Scope.NearbyAislings,
                new ServerFormat29((uint)player.Serial, (uint)player.Serial, 0x004F, 0x004F, 64));
        }

        private List<string> DetermineDrop()
        {
            return LootManager.Drop(LootTable, Generator.Random.Next(ServerContext.Config.LootTableStackSize))
                .Select(i => i?.Name).ToList();
        }

        private ItemUpgrade DetermineQuality()
        {
            return (ItemUpgrade)LootManager.Drop(UpgradeTable, 1).FirstOrDefault();
        }

        private void DetermineRandomDrop()
        {
            var idx = 0;
            if (Template.Drops.Count > 0)
                idx = Generator.Random.Next(Template.Drops.Count);

            var rndSelector = Template.Drops[idx];
            if (!ServerContext.GlobalItemTemplateCache.ContainsKey(rndSelector))
                return;

            var item = Item.Create(this, ServerContext.GlobalItemTemplateCache[rndSelector], true);
            var chance = Math.Round(Generator.Random.NextDouble(), 2);

            if (chance <= item.Template.DropRate)
                item.Release(this, Position);
        }

        private Variance DetermineVariance()
        {
            return RandomEnumValue<Variance>();
        }

        private void GenerateDrops()
        {
            if (Template.LootType.HasFlag(LootQualifer.Table))
            {
                if (LootTable == null || LootManager == null)
                    return;

                DetermineDrop().ForEach(i =>
                {
                    if (i == null) return;
                    if (!ServerContext.GlobalItemTemplateCache.ContainsKey(i)) return;

                    var rolledItem = Item.Create(this, ServerContext.GlobalItemTemplateCache[i]);
                    if (rolledItem == GlobalLastItemRoll)
                        return;

                    GlobalLastItemRoll = rolledItem;

                    var upgrade = DetermineQuality();

                    if (rolledItem.Template.Enchantable)
                    {
                        var variance = DetermineVariance();

                        if (!ServerContext.Config.UseLoruleVariants)
                            variance = Variance.None;

                        if (variance != Variance.None)
                            rolledItem.ItemVariance = variance;
                    }

                    if (rolledItem.Template.Flags.HasFlag(ItemFlags.QuestRelated))
                        upgrade = null;

                    if (!ServerContext.Config.UseLoruleItemRarity)
                        upgrade = null;

                    rolledItem.Upgrades = upgrade?.Upgrade ?? 0;

                    if (rolledItem.Upgrades > 0)
                    {
                        ApplyQuality(rolledItem);

                        if (rolledItem.Upgrades > 2)
                        {
                            if (Target != null)
                            {
                                var user = Target;

                                if (user is Aisling aisling)
                                {
                                    var party = aisling.GroupParty.PartyMembers;

                                    foreach (var player in party)
                                        player.Client.SendMessage(0x03,
                                            $"Special Drop: {rolledItem.DisplayName}");

                                    Task.Delay(1000).ContinueWith(ct => { rolledItem.Animate(160, 200); });
                                }
                            }
                        }
                    }

                    rolledItem.Cursed = true;
                    rolledItem.AuthenticatedAislings = GetTaggedAislings().Cast<Sprite>().ToArray();
                    rolledItem.Release(this, Position);
                });
            }
            else if (Template.LootType.HasFlag(LootQualifer.Random))
            {
                DetermineRandomDrop();
            }
        }

        private void GenerateExperience(Aisling player, bool canCrit = false)
        {
            int exp;

            var seed = Template.Level * 0.1 + 1.5;
            {
                exp = (int)(Template.Level * seed * 300);
            }

            if (canCrit)
                lock (Generator.Random)
                {
                    var critical = Math.Abs(GenerateNumber() % 100);
                    if (critical >= 85) exp *= 2;
                }

            DistributeExperience(player, exp);

            if (player.PartyMembers != null)
                foreach (var party in player.PartyMembers
                    .Where(party => party.Serial != player.Serial)
                    .Where(party => party.WithinRangeOf(player)))
                {
                    DistributeExperience(party, exp);

                    party.Client.SendStats(StatusFlags.StructC);
                    party.Client.SendMessage(0x02, $"You received {exp} Experience!.");
                }

            player.Client.SendStats(StatusFlags.StructC);
            player.Client.SendMessage(0x02, $"You received {exp} Experience!.");
        }

        private void GenerateGold()
        {
            if (!Template.LootType.HasFlag(LootQualifer.Gold))
                return;

            var sum = Generator.Random.Next(
                Template.Level * 500,
                Template.Level * 1000);

            if (sum > 0)
                Money.Create(this, sum, new Position(XPos, YPos));
        }

        private void UpdateCounters(Aisling player)
        {
            if (!player.MonsterKillCounters.ContainsKey(Template.Name))
                player.MonsterKillCounters[Template.Name] = 1;
            else
                player.MonsterKillCounters[Template.Name]++;
        }
    }
}