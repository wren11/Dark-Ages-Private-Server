#region

using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Scripting;
using Newtonsoft.Json;

#endregion

namespace Darkages.Types
{
    public delegate void QuestDelegate(Quest q);

    public enum PlayerAttr : byte
    {
        STR,
        INT,
        WIS,
        CON,
        DEX
    }

    public enum QuestType
    {
        ItemHandIn = 0,
        KillCount = 1,
        Gossip = 2,
        Boss = 3,
        Legend = 4,
        HasItem = 5,
        SingleItemHandIn = 6,
        Accept = 255
    }

    public class AttrReward
    {
        public PlayerAttr Attribute { get; set; }
        public StatusOperator Operator { get; set; }
    }

    public class Quest
    {
        [JsonIgnore] public readonly int Id;
        public List<uint> ExpRewards = new List<uint>();
        public List<string> ItemRewards = new List<string>();
        public List<Legend.LegendItem> LegendRewards = new List<Legend.LegendItem>();
        public List<QuestStep<Template>> QuestStages = new List<QuestStep<Template>>();
        public List<string> SkillRewards = new List<string>();
        public List<string> SpellRewards = new List<string>();
        public List<AttrReward> StatRewards = new List<AttrReward>();
        private static readonly object SyncLock = new object();

        public Quest()
        {
            lock (Generator.Random)
            {
                Id = Generator.GenerateNumber();
            }
        }

        public event QuestDelegate OnQuestCompleted;

        public bool Completed { get; set; }
        [JsonIgnore] public QuestStep<Template> Current => QuestStages.Count > 0 ? QuestStages[StageIndex] : null;
        public uint GoldReward { get; set; }
        public string Name { get; set; }
        public bool Rewarded { get; set; }
        public int StageIndex { get; set; }
        public bool Started { get; set; }
        public DateTime TimeCompleted { get; set; }
        public DateTime TimeStarted { get; set; }

        public void GiveRewards(Aisling user, bool equipLoot)
        {
            Rewarded = true;
            Completed = true;
            TimeCompleted = DateTime.UtcNow;

            user.SendAnimation(22, user, user);

            lock (SyncLock)
            {
                var completeStages = QuestStages.Where(i => i.StepComplete).SelectMany(i => i.Prerequisites).ToArray();

                foreach (var attrs in StatRewards)
                {
                    if (attrs.Attribute == PlayerAttr.STR)
                    {
                        if (attrs.Operator.Option == Operator.Add)
                            user._Str += (byte)attrs.Operator.Value;

                        if (attrs.Operator.Option == Operator.Remove)
                            user._Str -= (byte)attrs.Operator.Value;
                    }

                    if (attrs.Attribute == PlayerAttr.INT)
                    {
                        if (attrs.Operator.Option == Operator.Add)
                            user._Int += (byte)attrs.Operator.Value;

                        if (attrs.Operator.Option == Operator.Remove)
                            user._Int -= (byte)attrs.Operator.Value;
                    }

                    if (attrs.Attribute == PlayerAttr.WIS)
                    {
                        if (attrs.Operator.Option == Operator.Add)
                            user._Wis += (byte)attrs.Operator.Value;

                        if (attrs.Operator.Option == Operator.Remove)
                            user._Wis -= (byte)attrs.Operator.Value;
                    }

                    if (attrs.Attribute == PlayerAttr.CON)
                    {
                        if (attrs.Operator.Option == Operator.Add)
                            user._Con += (byte)attrs.Operator.Value;

                        if (attrs.Operator.Option == Operator.Remove)
                            user._Con -= (byte)attrs.Operator.Value;
                    }

                    if (attrs.Attribute == PlayerAttr.DEX)
                    {
                        if (attrs.Operator.Option == Operator.Add)
                            user._Dex += (byte)attrs.Operator.Value;

                        if (attrs.Operator.Option == Operator.Remove)
                            user._Dex -= (byte)attrs.Operator.Value;
                    }
                }

                foreach (var step in completeStages)
                {
                    if (step.Type == QuestType.ItemHandIn)
                    {
                        var objs = user.Inventory.Get(o => o.Template.Name == step.TemplateContext.Name);

                        foreach (var obj in objs)
                            if (obj != null && obj.Template.Flags.HasFlag(ItemFlags.QuestRelated))
                            {
                                if (step.IsMet(user, b => b(obj.Template)))
                                    user.Inventory.RemoveRange(user.Client, obj, step.Amount);
                            }
                            else if (obj != null)
                            {
                                if (step.IsMet(user, b => b(obj.Template))) user.Inventory.Remove(user.Client, obj);
                            }
                    }

                    if (step.Type == QuestType.SingleItemHandIn)
                    {
                        var obj = user.Inventory.Get(o => o.Template.Name == step.TemplateContext.Name)
                            .FirstOrDefault();

                        if (obj != null && obj.Template.Flags.HasFlag(ItemFlags.QuestRelated))
                            if (step.IsMet(user, b => b(obj.Template)))
                                user.EquipmentManager.RemoveFromInventory(obj, true);
                    }
                }

                Rewards(user, equipLoot);
            }
        }

        public void HandleQuest(GameClient client, Dialog menu = null, Action<bool> cb = null)
        {
            var valid = !QuestStages.Any();

            foreach (var stage in QuestStages)
            {
                var results = stage.Prerequisites
                    .Select(reqs => reqs.IsMet(client.Aisling, i => i(reqs.TemplateContext))).ToList();

                valid = results.TrueForAll(i => i);
                stage.StepComplete = valid;
            }

            if (menu == null)
            {
                if (cb != null)
                {
                    cb.Invoke(valid);
                    return;
                }

                if (valid && !Rewarded)
                {
                    OnCompleted(client.Aisling);
                    return;
                }
            }

            if (menu == null || !valid)
                return;

            if (!menu.CanMoveNext)
                return;

            menu.MoveNext(client);
            menu.Invoke(client);
        }

        public void OnCompleted(Aisling user, bool equipLoot = false)
        {
            OnQuestCompleted?.Invoke(this);

            GiveRewards(user, equipLoot);
        }

        public void Rewards(Aisling user, bool equipLoot)
        {
            foreach (var items in SkillRewards.Where(items => !Skill.GiveTo(user.Client, items)))
            {
            }

            foreach (var items in SpellRewards.Where(items => !Spell.GiveTo(user, items)))
            {
            }

            foreach (var obj in from items in ItemRewards
                                where ServerContext.GlobalItemTemplateCache.ContainsKey(items)
                                select ServerContext.GlobalItemTemplateCache[items]
                into template
                                select Item.Create(user, template)
                into obj
                                where !obj.GiveTo(user)
                                select obj) obj.Release(user, user.Position);

            foreach (var legends in LegendRewards)
                user.LegendBook.AddLegend(new Legend.LegendItem
                {
                    Category = "Quest Reward",
                    Color = legends.Color,
                    Icon = legends.Icon,
                    Value = legends.Value
                });

            if (ExpRewards.Count > 0)
                ExpRewards.ForEach(i => Monster.DistributeExperience(user, i));

            if (GoldReward > 0)
            {
                user.GoldPoints += (int)GoldReward;
                user.Client.SendMessage(0x02, $"You are awarded {GoldReward} gold.");
            }

            if (equipLoot)
            {
                EquipRewards(user);
                user.Client.Refresh();
            }

            user.Client.SendStats(StatusFlags.All);
        }

        public void UpdateQuest(Aisling user)
        {
            if (StageIndex + 1 < QuestStages.Count)
                StageIndex++;
            else
                OnCompleted(user);
        }

        private static void EquipRewards(Aisling user)
        {
            List<Item> items;

            lock (SyncLock)
            {
                items = new List<Item>(user
                    .Inventory
                    .Items
                    .Select(i => i.Value).ToArray()
                );
            }

            foreach (var obj in items.Where(obj => obj != null))
            {
                user.EquipmentManager.Add
                (
                    obj.Template.EquipmentSlot,
                    obj
                );

                obj.Scripts = ScriptManager.Load<ItemScript>(obj.Template.ScriptName, obj);
                if (string.IsNullOrEmpty(obj.Template.WeaponScript))
                    continue;

                obj.WeaponScripts = ScriptManager.Load<WeaponScript>(obj.Template.WeaponScript, obj);

                if (obj.Scripts?.Values == null)
                    continue;

                foreach (var script in obj.Scripts?.Values) script.Equipped(user, (byte)obj.Template.EquipmentSlot);
            }
        }
    }
}