#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.Buffs;
using Darkages.Storage.locales.debuffs;


#endregion

namespace Darkages.Types
{
    public class Spell
    {
        public int Casts = 0;
        public int ID { get; set; }

        public bool InUse { get; set; }
        public byte Level { get; set; }
        [JsonIgnore] [Browsable(false)] public int Lines { get; set; }
        [JsonIgnore] [Browsable(false)] public string Name => $"{Template.Name} (Lev:{Level}/{Template.MaxLevel})";
        public DateTime NextAvailableUse { get; set; }
        [JsonIgnore] public bool Ready => DateTime.UtcNow > NextAvailableUse;
        [JsonIgnore] public Dictionary<string, SpellScript> Scripts { get; set; }

        public byte Slot { get; set; }
        public SpellTemplate Template { get; set; }

        public static void AttachScript(Spell spell)
        {
            spell.Scripts = ScriptManager.Load<SpellScript>(spell.Template.ScriptKey, spell);
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
            obj.Slot = (byte) slot;
            obj.Lines = obj.Template.BaseLines;

            if (obj.Template.Buff == null || obj.Template.Debuff == null)
                AssignDebuffsAndBuffs(obj);

            return obj;
        }

        public static bool GiveTo(GameClient client, string args, byte level = 1, byte index = 0)
        {
            if (!ServerContext.GlobalSpellTemplateCache.ContainsKey(args))
                return false;

            var spellTemplate = ServerContext.GlobalSpellTemplateCache[args];

            if (spellTemplate != null)
            {
                var slot = client.Aisling.SpellBook.FindEmpty(spellTemplate.Pane == Pane.Spells ? 0 : 72);

                if (slot <= 0)
                    return false;

                if (slot <= 0)
                    return false;

                if (client.Aisling.SpellBook.Has(spellTemplate))
                    return false;

                var spell = Create(slot, spellTemplate);
                spell.Template = spellTemplate;

                AttachScript(spell);
                {
                    spell.Level = client.Aisling.GameMaster ? spellTemplate.MaxLevel : level;
                    client.Aisling.SpellBook.Assign(spell);
                    client.Aisling.SpellBook.Set(spell, false);
                    client.Send(new ServerFormat17(spell));
                    client.Aisling.SendAnimation(22, client.Aisling, client.Aisling);
                }

                return true;
            }

            return false;
        }

        public static bool GiveTo(Aisling aisling, string spellname, int level = 100, byte index = 0)
        {
            if (!ServerContext.GlobalSpellTemplateCache.ContainsKey(spellname))
                return false;

            var spellTemplate = ServerContext.GlobalSpellTemplateCache[spellname];

            if (spellTemplate != null)
            {
                if (aisling.SpellBook.Has(spellTemplate))
                    return false;

                var slot = aisling.SpellBook.FindEmpty(spellTemplate.Pane == Pane.Spells ? 0 : 72);

                if (slot <= 0)
                    return false;

                var spell = Create(slot, spellTemplate);
                {
                    spell.Level = (byte) level;
                    AttachScript(spell);
                    {
                        aisling.SpellBook.Assign(spell);
                        aisling.SpellBook.Set(spell, false);

                        if (aisling.LoggedIn)
                        {
                            aisling.Show(Scope.Self, new ServerFormat17(spell));
                            aisling.SendAnimation(22, aisling, aisling);
                        }
                    }
                }

                return true;
            }

            return false;
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

            if (obj.Template.Name == "blind")
                obj.Template.Debuff = new debuff_blind();
        }
    }
}