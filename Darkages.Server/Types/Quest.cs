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
using Darkages.Scripting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darkages.Types
{
    public enum QuestType
    {
        ItemHandIn = 0,
        KillCount = 1,
        Gossip = 2,
        Boss = 3,
        Legend = 4,
        HasItem = 5,
        SingleItemHandIn = 6,
        Accept = 255,
    }

    public enum PlayerAttr : byte
    {
        STR,
        INT,
        WIS,
        CON,
        DEX
    }

    public class AttrReward
    {
        public PlayerAttr Attribute { get; set; }
        public StatusOperator Operator { get; set; }
    }

    public class Quest
    {
        [JsonIgnore]
        public readonly int Id;

        public Quest()
        {
            lock (Generator.Random)
            {
                Id = Generator.GenerateNumber();
            }
        }

        public List<string> ItemRewards = new List<string>();
        public List<Legend.LegendItem> LegendRewards = new List<Legend.LegendItem>();

        public List<QuestStep<Template>> QuestStages = new List<QuestStep<Template>>();

        public List<string> SkillRewards = new List<string>();
        public List<string> SpellRewards = new List<string>();

        public List<AttrReward> StatRewards = new List<AttrReward>();

        public string Name { get; set; }
        public bool Started { get; set; }
        public bool Completed { get; set; }

        public DateTime TimeStarted { get; set; }
        public DateTime TimeCompleted { get; set; }

        public List<uint> ExpRewards = new List<uint>();
        public uint GoldReward { get; set; }
        public int StageIndex { get; set; }

        public bool Rewarded { get; set; }

        [JsonIgnore]
        public QuestStep<Template> Current => QuestStages.Count > 0 ? QuestStages[StageIndex] : null;


        public void OnCompleted(Aisling user, bool equipLoot = false)
        {
            GiveRewards(user, equipLoot);
        }

        public void GiveRewards(Aisling user, bool equipLoot)
        {
            Rewarded = true;
            Completed = true;
            TimeCompleted = DateTime.Now;

            user.SendAnimation(22, user, user);

            lock (QuestStages)
            {
                var completeStages = QuestStages.Where(i => i.StepComplete).SelectMany(i => i.Prerequisites).ToArray();


                foreach (var attrs in StatRewards)
                {
                    if (attrs.Attribute == PlayerAttr.STR)
                    {
                        if (attrs.Operator.Option == StatusOperator.Operator.Add)
                            user._Str += (byte)attrs.Operator.Value;

                        if (attrs.Operator.Option == StatusOperator.Operator.Remove)
                            user._Str -= (byte)attrs.Operator.Value;
                    }

                    if (attrs.Attribute == PlayerAttr.INT)
                    {
                        if (attrs.Operator.Option == StatusOperator.Operator.Add)
                            user._Int += (byte)attrs.Operator.Value;

                        if (attrs.Operator.Option == StatusOperator.Operator.Remove)
                            user._Int -= (byte)attrs.Operator.Value;
                    }

                    if (attrs.Attribute == PlayerAttr.WIS)
                    {
                        if (attrs.Operator.Option == StatusOperator.Operator.Add)
                            user._Wis += (byte)attrs.Operator.Value;

                        if (attrs.Operator.Option == StatusOperator.Operator.Remove)
                            user._Wis -= (byte)attrs.Operator.Value;
                    }

                    if (attrs.Attribute == PlayerAttr.CON)
                    {
                        if (attrs.Operator.Option == StatusOperator.Operator.Add)
                            user._Con += (byte)attrs.Operator.Value;

                        if (attrs.Operator.Option == StatusOperator.Operator.Remove)
                            user._Con -= (byte)attrs.Operator.Value;
                    }

                    if (attrs.Attribute == PlayerAttr.DEX)
                    {
                        if (attrs.Operator.Option == StatusOperator.Operator.Add)
                            user._Dex += (byte)attrs.Operator.Value;

                        if (attrs.Operator.Option == StatusOperator.Operator.Remove)
                            user._Dex -= (byte)attrs.Operator.Value;
                    }


                }

                foreach (var step in completeStages)
                {
                    if (step.Type == QuestType.ItemHandIn)
                    {
                        var objs = user.Inventory.Get(o => o.Template.Name == step.TemplateContext.Name);

                        foreach (var obj in objs)
                        {
                            if (obj != null && obj.Template.Flags.HasFlag(ItemFlags.QuestRelated))
                            {
                                if (step.IsMet(user, b => b(obj.Template)))
                                    user.Inventory.RemoveRange(user.Client, obj, step.Amount);
                            }
                            else if (obj != null)
                            {
                                if (step.IsMet(user, b => b(obj.Template)))
                                {
                                    user.Inventory.Remove(user.Client, obj);
                                }
                            }
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

        public void Rewards(Aisling user, bool equipLoot)
        {
            foreach (var items in SkillRewards)
                if (!Skill.GiveTo(user.Client, items))
                {
                }



            foreach (var items in SpellRewards)
                if (!Spell.GiveTo(user, items))
                {
                }

            foreach (var items in ItemRewards)
            {
                if (ServerContext.GlobalItemTemplateCache.ContainsKey(items))
                {
                    var template = ServerContext.GlobalItemTemplateCache[items];

                    var obj = Item.Create(user, template);
                    if (!obj.GiveTo(user, true))
                    {
                        obj.Release(user, user.Position);
                    }
                }
            }

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
                user.Client.SendMessage(0x02, string.Format("You are awarded {0} gold.", GoldReward));
            }

            if (equipLoot)
            {
                EquipRewards(user);
                user.Client.Refresh();
            }

            user.Client.SendStats(StatusFlags.All);
        }

        private static void EquipRewards(Aisling user)
        {
            var items = new List<Item>();

            lock (user.Inventory)
            {
                items = new List<Item>(user
                    .Inventory
                    .Items
                    .Select(i => i.Value).ToArray()
                    );
            }

            foreach (var obj in items)
            {
                if (obj != null)
                {
                    user.EquipmentManager.Add
                        (
                            obj.Template.EquipmentSlot,
                            obj
                        );

                    obj.Script = ScriptManager.Load<ItemScript>(obj.Template.ScriptName, obj);
                    if (!string.IsNullOrEmpty(obj.Template.WeaponScript))
                    {
                        obj.WeaponScript = ScriptManager.Load<WeaponScript>(obj.Template.WeaponScript, obj);
                    }
                    obj.Script?.Equipped(user, (byte)obj.Template.EquipmentSlot);
                }
            }
        }

        public void UpdateQuest(Aisling user)
        {
            if (StageIndex + 1 < QuestStages.Count)
                StageIndex++;
            else
                OnCompleted(user);
        }


        public void HandleQuest(GameClient client, Dialog menu = null, Action<bool> cb = null)
        {
            var valid = false;

            foreach (var stage in QuestStages)
            {
                var results = new List<bool>();
                foreach (var reqs in stage.Prerequisites)
                {
                    results.Add(reqs.IsMet(client.Aisling, i => i(reqs.TemplateContext)));
                }

                valid = results.TrueForAll(i => i != false);
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

            if (menu != null && valid)
            {
                if (menu.CanMoveNext)
                {
                    menu.MoveNext(client);
                    menu.Invoke(client);
                }
            }
        }
    }


    public class QuestRequirement
    {
        public int Amount { get; set; }
        public Template TemplateContext { get; set; }
        public QuestType Type { get; set; }
        public string Value { get; set; }

        public bool IsMet(Aisling user, Func<Predicate<Template>, bool> predicate)
        {
            if (Type == QuestType.ItemHandIn)
                return predicate(i => user.Inventory.Has(TemplateContext) >= Amount);
            if (Type == QuestType.KillCount)
                return predicate(i => user.HasKilled(Value, Amount));
            if (Type == QuestType.HasItem)
                return predicate(i => user.Inventory.HasCount(TemplateContext) >= Amount);
            if (Type == QuestType.SingleItemHandIn)
                return predicate(i => user.Inventory.HasCount(TemplateContext) >= Amount);
            if (Type == QuestType.Gossip)
                return true;

            return false;
        }
    }

    public class QuestStep<T>
    {
        [JsonIgnore]
        public List<QuestRequirement> Prerequisites = new List<QuestRequirement>();

        public QuestType Type { get; set; }

        public bool StepComplete { get; set; }

        public string AcceptedMessage { get; set; }

        public string RejectedMessage { get; set; }
    }
}
