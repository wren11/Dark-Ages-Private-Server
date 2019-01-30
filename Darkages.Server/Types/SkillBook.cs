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
    public class SkillBook : ObjectManager
    {
        public static readonly int SKILLLENGTH = 35 * 3;

        public Dictionary<int, Skill> Skills = new Dictionary<int, Skill>();

        public SkillBook()
        {
            for (var i = 0; i < SKILLLENGTH; i++) Skills[i + 1] = null;

            //this makes sure no skills are placed in the void slot.
            Skills[36] = new Skill();
        }


        public int Length => Skills.Count;

        public Skill FindInSlot(int Slot)
        {
            return Skills[Slot];
        }

        public void Assign(Skill skill)
        {
            Set(skill);
        }

        public new Skill[] Get(Predicate<Skill> prediate)
        {
            return Skills.Values.Where(i => i != null && prediate(i)).ToArray();
        }

        public void Swap(Skill A, Skill B)
        {
            A = Interlocked.Exchange(ref B, A);
        }

        public void Set(Skill s)
        {
            Skills[s.Slot] = Clone<Skill>(s);
        }

        public void Set(Skill s, bool clone = false)
        {
            Skills[s.Slot] = clone ? Clone<Skill>(s) : s;
        }

        public void Clear(Skill s)
        {
            Skills[s.Slot] = null;
        }

        public Skill Remove(byte movingFrom)
        {
            var copy = Skills[movingFrom];
            Skills[movingFrom] = null;
            return copy;
        }

        public bool Has(Skill s)
        {
            return Skills.Where(i => i.Value != null && i.Value != null).Select(i => i.Value.Template)
                .FirstOrDefault(i => i.Name.Equals(s.Template.Name)) != null;
        }

        public bool Has(SkillTemplate s)
        {
            return Skills.Where(i => i.Value != null && i.Value.Template != null).Select(i => i.Value.Template)
                .FirstOrDefault(i => i.Name.Equals(s.Name)) != null;
        }


        public int FindEmpty()
        {
            for (var i = 0; i < Length; i++)
            {

                if (Skills[i + 1] == null)
                    return i + 1;
            }
            return -1;
        }
    }
}
