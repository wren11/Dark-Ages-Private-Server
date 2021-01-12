#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Darkages.Network.Object;

#endregion

namespace Darkages.Types
{
    public class SkillBook : ObjectManager
    {
        public static readonly int SKILLLENGTH = 35 * 3;

        public Dictionary<int, Skill> Skills = new Dictionary<int, Skill>();

        public SkillBook()
        {
            for (var i = 0; i < SKILLLENGTH; i++) 
                Skills[i + 1] = null;
        }

        public int Length => Skills.Count;

        public void Assign(Skill skill)
        {
            Set(skill);
        }

        public int FindEmpty(int start = 0)
        {
            for (var i = start; i < Length; i++)
                if (Skills[i + 1] == null)
                    return i + 1;

            return -1;
        }

        public new Skill[] Get(Predicate<Skill> prediate)
        {
            return Skills.Values.Where(i => i != null && prediate(i)).ToArray();
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

        public Skill Remove(byte movingFrom)
        {
            if (!Skills.ContainsKey(movingFrom))
                return null;

            var copy = Skills[movingFrom];
            Skills[movingFrom] = null;
            return copy;
        }

        public void Set(Skill s)
        {
            Skills[s.Slot] = Clone<Skill>(s);
        }

        public void Set(Skill s, bool clone = false)
        {
            Skills[s.Slot] = clone ? Clone<Skill>(s) : s;
        }
    }
}