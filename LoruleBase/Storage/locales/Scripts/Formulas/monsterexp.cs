using Darkages.Common;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Darkages.Storage.locales.Scripts.Formulas
{
    [Script("Monster Exp 1x", "Wren", "Base Script for handling monster rewards and exp.")]
    public class Monsterexp : RewardScript
    {
        private readonly Monster _monster;
        private readonly Aisling _player;

        public Monsterexp(Monster monster, Aisling player)
        {
            _monster = monster;
            _player = player;
        }

        public override void GenerateRewards(Monster monster, Aisling player)
        {
            UpdateCounters(player);
            GenerateExperience(player);
            GenerateGold();
            GenerateDrops();
        }

        private void HandleExp(Aisling player, double exp)
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
            return _monster.LootManager.Drop(_monster.LootTable, Generator.Random.Next(ServerContext.Config.LootTableStackSize))
                .Select(i => i?.Name).ToList();
        }

        private ItemUpgrade DetermineQuality()
        {
            return (ItemUpgrade)_monster.LootManager.Drop(_monster.UpgradeTable, 1).FirstOrDefault();
        }

        private void DetermineRandomDrop()
        {
            var idx = 0;
            if (_monster.Template.Drops.Count > 0)
                idx = Generator.Random.Next(_monster.Template.Drops.Count);

            var rndSelector = _monster.Template.Drops[idx];
            if (!ServerContext.GlobalItemTemplateCache.ContainsKey(rndSelector))
                return;

            var item = Item.Create(_monster, ServerContext.GlobalItemTemplateCache[rndSelector], true);
            var chance = Math.Round(Generator.Random.NextDouble(), 2);

            if (chance <= item.Template.DropRate)
                item.Release(_monster, _monster.Position);
        }

        private Item.Variance DetermineVariance()
        {
            return Generator.RandomEnumValue<Item.Variance>();
        }

        private void GenerateDrops()
        {
            if (_monster.Template.LootType.HasFlag(LootQualifer.Table))
            {
                if (_monster.LootTable == null || _monster.LootManager == null)
                    return;

                DetermineDrop().ForEach(i =>
                {
                    if (i == null)
                        return;
                    if (!ServerContext.GlobalItemTemplateCache.ContainsKey(i))
                        return;

                    var rolledItem = Item.Create(_monster, ServerContext.GlobalItemTemplateCache[i]);

                    if (rolledItem == _monster.GlobalLastItemRoll)
                        return;

                    _monster.GlobalLastItemRoll = rolledItem;

                    var upgrade = DetermineQuality();

                    if (rolledItem.Template.Enchantable)
                    {
                        var variance = DetermineVariance();

                        if (!ServerContext.Config.UseLoruleVariants)
                            variance = Item.Variance.None;

                        if (variance != Item.Variance.None)
                            rolledItem.ItemVariance = variance;
                    }

                    if (rolledItem.Template.Flags.HasFlag(ItemFlags.QuestRelated))
                        upgrade = null;

                    if (!ServerContext.Config.UseLoruleItemRarity)
                        upgrade = null;

                    rolledItem.Upgrades = upgrade?.Upgrade ?? 0;

                    if (rolledItem.Upgrades > 0)
                    {
                        Item.ApplyQuality(rolledItem);

                        if (rolledItem.Upgrades > 2)
                            if (_monster.Target != null)
                            {
                                var user = _monster.Target;

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

                    rolledItem.Cursed = true;
                    rolledItem.AuthenticatedAislings = _monster.GetTaggedAislings().Cast<Sprite>().ToArray();
                    rolledItem.Release(_monster, _monster.Position);
                });
            }
            else if (_monster.Template.LootType.HasFlag(LootQualifer.Random))
            {
                DetermineRandomDrop();
            }
        }

        public void DistributeExperience(Aisling player, double exp)
        {
            var chunks = exp / 1000;

            if (chunks <= 1)
                HandleExp(player, exp);
            else
                for (var i = 0; i < chunks; i++)
                    HandleExp(player, 1000);
        }

        private void GenerateExperience(Aisling player, bool canCrit = false)
        {
            int exp;

            var seed = _monster.Template.Level * 0.1 + 1.5;
            {
                exp = (int)(_monster.Template.Level * seed * 300);
            }

            if (canCrit)
                lock (Generator.Random)
                {
                    var critical = Math.Abs(Generator.GenerateNumber() % 100);
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
            if (!_monster.Template.LootType.HasFlag(LootQualifer.Gold))
                return;

            var sum = Generator.Random.Next(
                _monster.Template.Level * 500,
                _monster.Template.Level * 1000);

            if (sum > 0)
                Money.Create(_monster, sum, new Position(_monster.XPos, _monster.YPos));
        }

        private void UpdateCounters(Aisling player)
        {
            if (!player.MonsterKillCounters.ContainsKey(_monster.Template.Name))
                player.MonsterKillCounters[_monster.Template.Name] = 1;
            else
                player.MonsterKillCounters[_monster.Template.Name]++;
        }
    }
}
