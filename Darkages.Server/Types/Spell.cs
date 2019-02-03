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
using Darkages.Storage.locales.Buffs;
using Darkages.Storage.locales.debuffs;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace Darkages.Types
{
    public class Spell
    {
        public int Casts = 0;
        public int ID { get; set; }

        [JsonIgnore] public SpellScript Script { get; set; }

        public SpellTemplate Template { get; set; }

        public byte Level { get; set; }

        public byte Slot { get; set; }

        public DateTime NextAvailableUse { get; set; }

        [JsonIgnore] public bool Ready => DateTime.UtcNow > NextAvailableUse;

        public bool InUse { get; internal set; }

        [JsonIgnore]
        [Browsable(false)]
        public string Name => string.Format("{0} (Lev:{1}/{2})",
            Template.Name,
            Level, Template.MaxLevel);

        [JsonIgnore] [Browsable(false)] public int Lines { get; set; }

        public bool CanUse()
        {
            return Ready;
        }

        public static Spell Create(int slot, SpellTemplate spellTemplate)
        {
            var obj = new Spell();
            lock (Generator.Random)
            {
                obj.ID = Generator.GenerateNumber();
            }

            obj.Template = spellTemplate;
            obj.Level = 0;
            obj.Slot = (byte)slot;
            obj.Lines = obj.Template.BaseLines;

            if (obj.Template.Buff == null || obj.Template.Debuff == null)
                AssignDebuffsAndBuffs(obj);

            return obj;
        }

        private static void AssignDebuffsAndBuffs(Spell obj)
        {
            if (obj.Template.Name == "dion")
                obj.Template.Buff = new buff_dion();

            if (obj.Template.Name == "mor dion")
                obj.Template.Buff = new buff_mordion();

            if (obj.Template.Name == "armachd")
                obj.Template.Buff = new buff_armachd();

            if (obj.Template.Name == "pramh")
                obj.Template.Debuff = new debuff_sleep();

            if (obj.Template.Name == "beag cradh")
                obj.Template.Debuff = new debuff_beagcradh();

            if (obj.Template.Name == "cradh")
                obj.Template.Debuff = new debuff_cradh();

            if (obj.Template.Name == "mor cradh")
                obj.Template.Debuff = new debuff_morcradh();

            if (obj.Template.Name == "ard cradh")
                obj.Template.Debuff = new debuff_ardcradh();

            if (obj.Template.Name == "fas nadur")
                obj.Template.Debuff = new debuff_fasnadur();

            if (obj.Template.Name == "mor fas nadur")
                obj.Template.Debuff = new debuff_morfasnadur();
        }

        public static void AttachScript(Aisling Aisling, Spell spell)
        {
            spell.Script = ScriptManager.Load<SpellScript>(spell.Template.ScriptKey, spell);
        }
        public static bool GiveTo(Aisling Aisling, string spellname, byte slot)
        {
            var spellTemplate = ServerContext.GlobalSpellTemplateCache[spellname];

            if (slot <= 0)
                return false;

            var spell = Create(slot, spellTemplate);

            AttachScript(Aisling, spell);

            Aisling.SpellBook.Assign(spell);

            return true;
        }


        public bool RollDice(Random rand)
        {
            if (Level < 50)
                return rand.Next(1, 101) < 50;

            return rand.Next(1, 101) < Level;
        }

        public static bool GiveTo(GameClient client, string args)
        {
            if (!ServerContext.GlobalSpellTemplateCache.ContainsKey(args))
                return false;


            var spellTemplate = ServerContext.GlobalSpellTemplateCache[args];
            var slot = client.Aisling.SpellBook.FindEmpty();

            if (slot <= 0)
                return false;

            if (client.Aisling.SpellBook.Has(spellTemplate))
                return false;

            var spell = Create(slot, spellTemplate);
            spell.Template = spellTemplate;


            AttachScript(client.Aisling, spell);
            {
                spell.Level = 1;
                client.Aisling.SpellBook.Assign(spell);
                client.Aisling.SpellBook.Set(spell, false);
                client.Send(new ServerFormat17(spell));
                client.Aisling.SendAnimation(22, client.Aisling, client.Aisling);
            }
            return true;
        }

        public static bool GiveTo(Aisling Aisling, string spellname, int level = 100)
        {
            if (!ServerContext.GlobalSpellTemplateCache.ContainsKey(spellname))
                return false;

            var spellTemplate = ServerContext.GlobalSpellTemplateCache[spellname];

            if (Aisling.SpellBook.Has(spellTemplate))
                return false;

            var slot = Aisling.SpellBook.FindEmpty();

            if (slot <= 0)
                return false;

            var spell = Create(slot, spellTemplate);
            {
                spell.Level = (byte)level;
                AttachScript(Aisling, spell);
                {
                    Aisling.SpellBook.Assign(spell);
                    Aisling.SpellBook.Set(spell, false);

                    if (Aisling.LoggedIn)
                    {
                        Aisling.Show(Scope.Self, new ServerFormat17(spell));
                        Aisling.SendAnimation(22, Aisling, Aisling);
                    }
                }
            }
            return true;
        }
    }
}
