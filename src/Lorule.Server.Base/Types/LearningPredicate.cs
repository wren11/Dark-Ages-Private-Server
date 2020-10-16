#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Darkages.Network.Game;

#endregion

namespace Darkages.Types
{
    public class ItemPredicate
    {
        public int AmountRequired { get; set; }
        public bool IsMet { get; set; }
        public string Item { get; set; }

        public void Validate(GameClient client)
        {
            if (client == null)
                IsMet = false;

            if (client.Aisling == null)
                IsMet = false;

            if (string.IsNullOrEmpty(Item))
                IsMet = false;

            var itemInSideInventory = client.Aisling.HasInInventory(Item, AmountRequired, out var found);
            IsMet = AmountRequired == found;
        }
    }

    public class LearningPredicate
    {
        public List<ushort> Areas_Visited_Required = new List<ushort>();
        public List<ItemPredicate> Items_Required = new List<ItemPredicate>();
        public List<string> Quests_Completed_Required = new List<string>();
        private Template _template;

        public LearningPredicate(Template template)
        {
            _template = template;
        }

        public LearningPredicate()
        {
            _template = null;
        }

        public Class Class_Required { get; set; }
        public int Con_Required { get; set; }
        public int Dex_Required { get; set; }
        public int ExpLevel_Required { get; set; }
        public int Gold_Required { get; set; }
        public int Int_Required { get; set; }
        public int Skill_Level_Required { get; set; }
        public string Skill_Required { get; set; }
        public int Skill_Tier_Required { get; set; }
        public int Spell_Level_Required { get; set; }
        public string Spell_Required { get; set; }
        public int Spell_Tier_Required { get; set; }
        public ClassStage Stage_Required { get; set; }
        public int Str_Required { get; set; }
        public int Wis_Required { get; set; }

        internal string[] MetaData
            => new[]
            {
                $"{(ExpLevel_Required > 0 ? ExpLevel_Required : 0)}/0/0",
                $"{(_template is SkillTemplate template ? template.Icon : ((SpellTemplate) _template).Icon)}/0/0",
                $"{(Str_Required == 0 ? 3 : Str_Required)}/{(Int_Required == 0 ? 3 : Int_Required)}/{(Wis_Required == 0 ? 3 : Wis_Required)}/{(Con_Required == 0 ? 3 : Con_Required)}/{(Dex_Required == 0 ? 3 : Dex_Required)}",
                $"{(!string.IsNullOrEmpty(Skill_Required) ? Skill_Required : "0")}/{(Skill_Level_Required > 0 ? Skill_Level_Required : 0)}",
                $"{(!string.IsNullOrEmpty(Spell_Required) ? Spell_Required : "0")}/{(Spell_Level_Required > 0 ? Spell_Level_Required : 0)}",
                $"{(_template.Description != "" ? _template.Description : _template.Name)} \n\n$Items Required: {(Items_Required.Count > 0 ? string.Join(",", Items_Required.Select(i => i.AmountRequired + " " + i.Item)) : "None")} $gold: {(Gold_Required > 0 ? Gold_Required : 0)}\n{Script}\n\n"
            };

        private string Script =>
            _template is SkillTemplate
                ? AreaAndPosition((_template as SkillTemplate)?.NpcKey) ?? "Unknown.."
                : AreaAndPosition((_template as SpellTemplate)?.NpcKey) ?? "Unknown..";

        public void AssociatedWith<T>(T template) where T : Template
        {
            _template = template;
        }

        public bool IsMet(Aisling player, Action<string, bool> callbackMsg = null)
        {
            if (ServerContext.Config.DevModeExemptions != null && player.GameMaster &&
                ServerContext.Config.DevModeExemptions.Contains("learning_predicates"))
                return true;

            var result = new Dictionary<int, Tuple<bool, object>>();
            var n = 0;
            try
            {
                n = CheckSpellandSkillPredicates(player, result, n);
                n = CHeckAttributePredicates(player, result, n);
                n = CheckItemPredicates(player, result, n);
                n = CheckQuestPredicates(player, result, n);
            }
            catch (Exception e)
            {
                player.Client.CloseDialog();
                player.Client.SendMessage(0x02, "You cannot learn that yet. Not even close!");
                player.SendAnimation(94, player, player);

                ServerContext.Error(e);

                return false;
            }

            var ready = CheckPredicates(callbackMsg, result);
            {
                if (ready) player.SendAnimation(92, player, player);
            }

            return ready;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(
                $"Stats Required: ({Str_Required} STR, {Int_Required} INT, {Wis_Required} WIS, {Con_Required} CON, {Dex_Required} DEX)");
            sb.Append("\nDo you wish to learn this new ability?");
            return sb.ToString();
        }

        private static bool CheckPredicates(Action<string, bool> callbackMsg,
            Dictionary<int, Tuple<bool, object>> result)
        {
            if (result == null || result.Count == 0)
                return false;

            var predicate_result = result.ToList().TrueForAll(i => i.Value.Item1);

            if (predicate_result)
            {
                callbackMsg?.Invoke("You have met all prerequisites, Do you wish to proceed?.", true);
                return true;
            }

            var sb = string.Empty;
            {
                var errorCaps = result.Select(i => i.Value).Distinct();

                sb += "{=sYou are not worthy., \n{=u";
                foreach (var predicate in errorCaps)
                    if (predicate != null && !predicate.Item1)
                        sb += (string) predicate.Item2 + "\n";
            }

            callbackMsg?.Invoke(sb, false);
            return false;
        }

        private string AreaAndPosition(string npcKey)
        {
            if (string.IsNullOrEmpty(npcKey))
                npcKey = "none";

            if (!ServerContext.GlobalMundaneTemplateCache.ContainsKey(npcKey)) return "Secret!";

            var npc = ServerContext.GlobalMundaneTemplateCache[npcKey];

            if (!ServerContext.GlobalMapCache.ContainsKey(npc.AreaID))
                return "Secret!";

            var map = ServerContext.GlobalMapCache[npc.AreaID];
            {
                return $"From Who: {npc.Name}\nFrom Where:{map.Name} - (Coordinates: {npc.X},{npc.Y})";
            }
        }

        private int CHeckAttributePredicates(Aisling player, Dictionary<int, Tuple<bool, object>> result, int n)
        {
            result[n++] = new Tuple<bool, object>(player.ExpLevel >= ExpLevel_Required,
                $"Go level more. (Level {ExpLevel_Required} Required)");
            result[n++] = new Tuple<bool, object>(player.Str >= Str_Required,
                $"You are not strong enough. ({Str_Required} Str Required)");
            result[n++] = new Tuple<bool, object>(player.Int >= Int_Required,
                $"You are not smart enough.  ({Int_Required} Int Required)");
            result[n++] = new Tuple<bool, object>(player.Wis >= Wis_Required,
                $"You are not wise enough. ({Wis_Required} Wis Required)");
            result[n++] = new Tuple<bool, object>(player.Con >= Con_Required,
                $"You lack stamina. ({Con_Required} Con Required)");
            result[n++] = new Tuple<bool, object>(player.Dex >= Dex_Required,
                $"You are not nimble enough. ({Dex_Required} Dex Required)");
            result[n++] = new Tuple<bool, object>(player.GoldPoints >= Gold_Required,
                $"You best come back when you got the cash. ({Gold_Required} Gold Required)");
            result[n++] = new Tuple<bool, object>(player.Stage == Stage_Required, "You must transcend further first");
            result[n++] =
                new Tuple<bool, object>(player.Path == Class_Required, "You should not be here, " + player.Path);

            return n;
        }

        private int CheckItemPredicates(Aisling player, Dictionary<int, Tuple<bool, object>> result, int n)
        {
            if (Items_Required != null && Items_Required.Count > 0)
            {
                var msg = new StringBuilder(ServerContext.Config.ItemNotRequiredMsg);

                var items = Items_Required.Select(i => $"{i.Item} ({i.AmountRequired}) ");

                foreach (var itemstrs in items) msg.Append(itemstrs);

                var errorMsg = msg.ToString();

                var formatted = errorMsg.Replace(") ", "), ").TrimEnd(',', ' ');

                foreach (var ir in Items_Required)
                {
                    if (!ServerContext.GlobalItemTemplateCache.ContainsKey(ir.Item))
                    {
                        result[n] = new Tuple<bool, object>(false, formatted);

                        break;
                    }

                    var item = ServerContext.GlobalItemTemplateCache[ir.Item];

                    if (item == null)
                    {
                        result[n] = new Tuple<bool, object>(false, formatted);
                        break;
                    }

                    var item_obtained = player.Inventory.Get(i => i.Template.Name.Equals(item.Name));

                    var item_total = 0;

                    foreach (var itemObj in item_obtained)
                    {
                        var itemcount = 0;
                        if (itemObj.Template.CanStack)
                            itemcount += itemObj.Stacks;
                        else
                            itemcount++;

                        item_total += itemcount;
                    }

                    if (item_total >= ir.AmountRequired)
                        result[n] = new Tuple<bool, object>(true, string.Empty);
                    else
                        result[n] = new Tuple<bool, object>(false, formatted);

                    n++;
                }
            }

            return n;
        }

        private int CheckQuestPredicates(Aisling player, Dictionary<int, Tuple<bool, object>> result, int n)
        {
            if (Quests_Completed_Required != null && Quests_Completed_Required.Count > 0)
                foreach (var qr in Quests_Completed_Required)
                {
                    if (player.Quests.Where(i => i.Name.Equals(qr) && i.Completed) != null)
                        result[n] = new Tuple<bool, object>(true, "Thank you. Please proceed.");
                    else
                        result[n] = new Tuple<bool, object>(false, "Come back when you complete the quests required.");

                    n++;
                }

            return n;
        }

        private int CheckSpellandSkillPredicates(Aisling player, Dictionary<int, Tuple<bool, object>> result, int n)
        {
            if (Skill_Required != null)
            {
                var skill = ServerContext.GlobalSkillTemplateCache[Skill_Required];
                var skill_retainer = player.SkillBook.Get(i => i.Template?.Name.Equals(skill.Name) ?? false)
                    .FirstOrDefault();

                if (skill_retainer == null)
                {
                    result[n++] = new Tuple<bool, object>(false,
                        $"You don't have the skill required. ({Skill_Required})");
                }
                else
                {
                    if (skill_retainer != null && skill_retainer.Level >= Skill_Level_Required)
                        result[n++] = new Tuple<bool, object>(true,
                            "Skills Required.");
                    else
                        result[n++] = new Tuple<bool, object>(false,
                            $"{skill.Name} Must be level {Skill_Level_Required} - Go get {Math.Abs(skill_retainer.Level - Skill_Level_Required)} more levels.");
                }
            }

            if (Spell_Required != null)
            {
                var spell = ServerContext.GlobalSpellTemplateCache[Spell_Required];
                var spell_retainer = player.SpellBook.Get(i => i
                                                               != null && i.Template != null &&
                                                               i.Template.Name.Equals(spell.Name)).FirstOrDefault();

                if (spell_retainer == null)
                    result[n++] = new Tuple<bool, object>(false,
                        $"You don't have the spell required. ({Spell_Required})");
                else if (spell_retainer.Level >= Spell_Level_Required)
                    result[n++] = new Tuple<bool, object>(true,
                        "Spells Required.");
                else
                    result[n++] = new Tuple<bool, object>(false,
                        $"{spell.Name} Must be level {Skill_Level_Required} - Go get {Math.Abs(spell_retainer.Level - Spell_Level_Required)} more levels.");
            }

            return n;
        }
    }
}