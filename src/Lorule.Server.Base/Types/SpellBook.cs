#region

using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Network.Game;
using Darkages.Network.Object;
using Darkages.Network.ServerFormats;

#endregion

namespace Darkages.Types
{
    public class SpellBook : ObjectManager
    {
        public static readonly int SPELLLENGTH = 35 * 3;

        public Dictionary<int, Spell> Spells = new Dictionary<int, Spell>();

        public SpellBook()
        {
            for (var i = 0; i < SPELLLENGTH; i++)
            {
                Spells[i + 1] = null;
            }

            Spells[36] = new Spell();
        }

        public void Clear(GameClient client)
        {
            Spells = new Dictionary<int, Spell>();
            for (byte i = 0; i < SPELLLENGTH; i++)
            {
                client.Send(new ServerFormat18((byte)(i + 1)));
            }
        }

        public int Length => Spells.Count;

        public void Assign(Spell spell)
        {
            Set(spell);
        }

        public void Clear(Spell s)
        {
            Spells[s.Slot] = null;
        }

        public int FindEmpty(int start = 0)
        {
            var slot = 0;

            for (var i = start; i < Length; i++)
                if (Spells[i + 1] == null)
                {
                    slot = i + 1;
                    break;
                }

            return slot > 0 ? slot : -1;
        }

        public Spell FindInSlot(int slot)
        {
            Spell ret = null;

            if (Spells.ContainsKey(slot))
                ret = Spells[slot];

            if (ret != null && ret.Template != null) return ret;

            return null;
        }

        public new Spell[] Get(Predicate<Spell> prediate)
        {
            return Spells.Values.Where(i => i != null && prediate(i)).ToArray();
        }

        public bool Has(Spell s)
        {
            return Spells.Where(i => i.Value != null && i.Value.Template != null).Select(i => i.Value.Template)
                .FirstOrDefault(i => i.Name.Equals(s.Template.Name)) != null;
        }

        public bool Has(string s)
        {
            return Spells.Where(i => i.Value != null && i.Value.Template != null).Select(i => i.Value.Template)
                .FirstOrDefault(i => s.Equals(i.Name)) != null;
        }

        public bool Has(SpellTemplate s)
        {
            var obj = Spells.Where(i => i.Value != null && i.Value.Template != null).Select(i => i.Value.Template)
                .FirstOrDefault(i => i.Name.Equals(s.Name));

            return obj != null;
        }

        public Spell Remove(byte movingFrom)
        {
            var copy = Spells[movingFrom];
            Spells[movingFrom] = null;
            return copy;
        }

        public void Set(Spell s)
        {
            Spells[s.Slot] = Clone<Spell>(s);
        }

        public void Set(Spell s, bool clone = false)
        {
            Spells[s.Slot] = clone ? Clone<Spell>(s) : s;
        }
    }
}