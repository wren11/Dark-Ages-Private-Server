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
using Darkages.Systems.Loot.Interfaces;
using Darkages.Types;
using System.Collections.Generic;

namespace Darkages.Systems.Loot
{
    public class LootModifierSet : Template, IModifierSet
    {
        public double Weight { get; set; }
        public ICollection<IModifier> Modifiers { get; }

        public LootModifierSet(string name, int weight)
        {
            Name = name;
            Weight = weight;
            Modifiers = new List<IModifier>();
        }

        public void ModifyItem(object item)
        {
            if (Modifiers.Count == 0)
                return;

            foreach (var modifier in Modifiers)
                modifier.Apply(item);
        }

        public IModifierSet Add(IModifier modifier)
        {
            Modifiers.Add(modifier);
            return this;
        }

        public IModifierSet Remove(IModifier modifier)
        {
            Modifiers.Remove(modifier);
            return this;
        }

        public override string ToString()
        {
            return $"Name: {Name}, Weight: {Weight}, Modifier Count: {Modifiers.Count}";
        }
    }
}
