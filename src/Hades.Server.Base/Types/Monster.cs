#region

using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Systems.Loot;

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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

        [JsonIgnore] public Aisling Summoner => GetObject<Aisling>(Map, b => b.Serial == SummonerId);

        [JsonIgnore] public Position CurrentWaypoint => Template?.Waypoints?[WaypointIndex];
        [JsonIgnore] public Item GlobalLastItemRoll { get; set; }
        [JsonIgnore]  public int SummonerId { get; set; }

        public static Monster Create(MonsterTemplate template, Area map)
        {
            var monsterCreateScript = ScriptManager.Load<MonsterCreateScript>(ServerContext.Config.MonsterCreationScript,
                    template,
                    map)
                .FirstOrDefault();

            return monsterCreateScript.Value?.Create(template, map);
        }

        public static void InitScripting(MonsterTemplate template, Area map, Monster obj)
        {
            if (obj.Scripts == null || !obj.Scripts.Any())
                obj.Scripts = ScriptManager.Load<MonsterScript>(template.ScriptName, obj, map);
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

            var script = ScriptManager.Load<RewardScript>(ServerContext.Config.MonsterRewardScript, this, player).FirstOrDefault();
            script.Value?.GenerateRewards(this, player);

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

            player.ExpTotal += (uint) exp;
            player.ExpNext -= (uint) exp;

            if (player.ExpNext >= int.MaxValue) player.ExpNext = 0;

            var seed = player.ExpLevel * 0.1 + 0.5;
            {
                if (player.ExpLevel >= ServerContext.Config.PlayerLevelCap)
                    return;
            }

            while (player.ExpNext <= 0 && player.ExpLevel < 99)
            {
                player.ExpNext = (uint) (player.ExpLevel * seed * 5000);

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

            player._MaximumHp += (int) (ServerContext.Config.HpGainFactor * player.Con * 0.65);
            player._MaximumMp += (int) (ServerContext.Config.MpGainFactor * player.Wis * 0.45);
            player.StatPoints += ServerContext.Config.StatsPerLevel;

            player.ExpLevel++;

            player.Client.SendMessage(0x02,
                string.Format(ServerContext.Config.LevelUpMessage, player.ExpLevel));
            player.Show(Scope.NearbyAislings,
                new ServerFormat29((uint) player.Serial, (uint) player.Serial, 0x004F, 0x004F, 64));
        }
    }
}