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
using Darkages.Network.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Darkages.Types
{
    public class SpellBook : ObjectManager
    {
        public static readonly int SPELLLENGTH = 35 * 3;

        public Dictionary<int, Spell> Spells = new Dictionary<int, Spell>();

        public SpellBook()
        {
            for (var i = 0; i < SPELLLENGTH; i++) Spells[i + 1] = null;

            //this makes sure no spell is placed in the void slot.
            Spells[36] = new Spell();
        }


        public int Length => Spells.Count;

        public Spell FindInSlot(int Slot)
        {
            Spell ret = null;

            if (Spells.ContainsKey(Slot))
                ret = Spells[Slot];

            if (ret != null && ret.Template != null)
            {
                return ret;
            }

            return null;
        }


        public void Assign(Spell spell)
        {
            Set(spell);
        }

        public new Spell[] Get(Predicate<Spell> prediate)
        {
            return Spells.Values.Where(i => i != null && prediate(i)).ToArray();
        }

        public void Swap(Spell A, Spell B)
        {
            A = Interlocked.Exchange(ref B, A);
        }

        public void Set(Spell s)
        {
            Spells[s.Slot] = Clone<Spell>(s);
        }

        public void Set(Spell s, bool clone = false)
        {
            Spells[s.Slot] = clone ? Clone<Spell>(s) : s;
        }

        public void Clear(Spell s)
        {
            Spells[s.Slot] = null;
        }

        public Spell Remove(byte movingFrom)
        {
            var copy = Spells[movingFrom];
            Spells[movingFrom] = null;
            return copy;
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


        public int FindEmpty()
        {
            for (var i = 0; i < Length; i++)
                if (Spells[i + 1] == null)
                    return i + 1;

            return -1;
        }
    }
}
