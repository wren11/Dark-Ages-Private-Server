#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.debuffs;
using Newtonsoft.Json;

#endregion

namespace Darkages.Types
{
    public class Skill
    {
        public byte Icon { get; set; }
        public int ID { get; set; }
        public bool InUse { get; internal set; }
        public int Level { get; set; }
        [JsonIgnore] [Browsable(false)] public string Name => $"{Template.Name} (Lev:{Level}/{Template.MaxLevel})";
        public DateTime NextAvailableUse { get; set; }
        public bool Ready => DateTime.UtcNow > NextAvailableUse;
        [JsonIgnore] public Dictionary<string, SkillScript> Scripts { get; set; }

        public byte Slot { get; set; }
        public SkillTemplate Template { get; set; }
        public int Uses { get; set; }

        public static Skill Create(int slot, SkillTemplate skillTemplate)
        {
            var obj = new Skill
            {
                Template = skillTemplate,
                Level = 0
            };
            lock (Generator.Random)
            {
                obj.ID = Generator.GenerateNumber();
            }

            obj.Slot = (byte)slot;
            obj.Icon = skillTemplate.Icon;

            if (obj.Template.Debuff == null || obj.Template.Buff == null) AssignDebuffs(obj);

            return obj;
        }

        public static bool GiveTo(GameClient client, string args)
        {
            var skillTemplate = ServerContext.GlobalSkillTemplateCache[args];
            var slot = client.Aisling.SkillBook.FindEmpty();

            if (slot <= 0)
                return false;

            if (client.Aisling.SkillBook.Has(skillTemplate))
                return false;

            var skill = Create(slot, skillTemplate);
            skill.Template = skillTemplate;

            skill.Scripts = ScriptManager.Load<SkillScript>(skill.Template.ScriptName, skill);
            {
                skill.Level = 1;
                client.Aisling.SkillBook.Assign(skill);
                client.Aisling.SkillBook.Set(skill, false);
                client.Send(new ServerFormat2C(skill.Slot, skill.Icon, skill.Name));
                client.Aisling.SendAnimation(22, client.Aisling, client.Aisling);
            }
            return true;
        }

        public static bool GiveTo(Aisling aisling, string args, byte slot, int level = 1)
        {
            var skillTemplate = ServerContext.GlobalSkillTemplateCache[args];
            var skill = Create(slot, skillTemplate);
            {
                skill.Level = level;
                skill.Scripts = ScriptManager.Load<SkillScript>(skill.Template.ScriptName, skill);
                aisling.SkillBook.Assign(skill);
            }
            return true;
        }

        public static bool GiveTo(Aisling aisling, string args, int level = 100)
        {
            if (!ServerContext.GlobalSkillTemplateCache.ContainsKey(args))
                return false;

            var skillTemplate = ServerContext.GlobalSkillTemplateCache[args];
            var slot = aisling.SkillBook.FindEmpty();

            if (slot <= 0)
                return false;

            var skill = Create(slot, skillTemplate);
            skill.Level = level;
            skill.Scripts = ScriptManager.Load<SkillScript>(skill.Template.ScriptName, skill);
            aisling.SkillBook.Assign(skill);
            aisling.SkillBook.Set(skill, false);

            if (aisling.LoggedIn)
            {
                aisling.Show(Scope.Self, new ServerFormat2C(skill.Slot, skill.Icon, skill.Name));
                aisling.SendAnimation(22, aisling, aisling);
            }

            return true;
        }

        public bool CanUse()
        {
            return Ready;
        }

        public bool RollDice(Random rand)
        {
            if (Level < 50)
                return rand.Next(1, 101) < 50;

            return rand.Next(1, 101) < Level;
        }

        private static void AssignDebuffs(Skill obj)
        {
            if (obj.Template.Name == "Wolf Fang Fist")
                obj.Template.Debuff = new debuff_frozen();

            if (obj.Template.Name == "beag suain")
                obj.Template.Debuff = new debuff_beagsuain();

            if (obj.Template.Name == "beag suain ia gar")
                obj.Template.Debuff = new debuff_beagsuain();
        }
    }
}