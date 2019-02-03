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
using Darkages.Storage.locales.debuffs;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace Darkages.Types
{
    public class Skill
    {
        [JsonIgnore] public SkillScript Script { get; set; }

        public SkillTemplate Template { get; set; }

        public byte Slot { get; set; }
        public byte Icon { get; set; }

        [JsonIgnore]
        [Browsable(false)]
        public string Name => string.Format("{0} (Lev:{1}/{2})",
            Template.Name,
            Level, Template.MaxLevel);

        public int Level { get; set; }
        public int ID { get; set; }

        public DateTime NextAvailableUse { get; set; }

        public bool Ready => DateTime.UtcNow > NextAvailableUse;

        public bool InUse { get; internal set; }
        public int Uses { get; set; }

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

        public static Skill Create(int slot, SkillTemplate skillTemplate)
        {
            var obj = new Skill();
            obj.Template = skillTemplate;
            obj.Level = 0;
            lock (Generator.Random)
            {
                obj.ID = Generator.GenerateNumber();
            }

            obj.Slot = (byte)slot;
            obj.Icon = skillTemplate.Icon;

            if (obj.Template.Debuff == null || obj.Template.Buff == null)
            {
                AssignDebuffs(obj);
            }

            return obj;
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

            skill.Script = ScriptManager.Load<SkillScript>(skill.Template.ScriptName, skill);
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
                skill.Script = ScriptManager.Load<SkillScript>(skill.Template.ScriptName, skill);
                aisling.SkillBook.Assign(skill);
            }
            return true;
        }

        public static bool GiveTo(Aisling aisling, string args, int level = 100)
        {
            var skillTemplate = ServerContext.GlobalSkillTemplateCache[args];
            var slot = aisling.SkillBook.FindEmpty();

            if (slot <= 0)
                return false;

            var skill = Create(slot, skillTemplate);
            skill.Level = level;
            skill.Script = ScriptManager.Load<SkillScript>(skill.Template.ScriptName, skill);
            aisling.SkillBook.Assign(skill);
            aisling.SkillBook.Set(skill, false);

            if (aisling.LoggedIn)
            {
                aisling.Show(Scope.Self, new ServerFormat2C(skill.Slot, skill.Icon, skill.Name));
                aisling.SendAnimation(22, aisling, aisling);
            }

            return true;
        }
    }
}
