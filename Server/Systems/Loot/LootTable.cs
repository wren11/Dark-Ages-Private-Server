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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darkages.Systems.Loot
{
    public class LootTable : ILootTable
    {
        public string Name { get; set; }
        public double Weight { get; set; }

        public ICollection<ILootDefinition> Children { get; }

        public LootTable(string name)
        {
            Name = name;
            Children = new List<ILootDefinition>();
        }

        public ILootTable Add(ILootDefinition item)
        {
            Children.Add(item);
            return this;
        }

        public ILootTable Remove(ILootDefinition item)
        {
            Children.Remove(item);
            return this;
        }

        public ILootDefinition Get(string name)
        {
            if (string.IsNullOrEmpty(name))
                return this;

            var names = name.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            return Find(names);
        }

        private ILootDefinition Find(IReadOnlyList<string> names)
        {
            if (names == null || names.Count == 0)
                return this;

            var item = Children.SingleOrDefault(x => x.Name.Equals(names[0], StringComparison.InvariantCultureIgnoreCase));

            if (item is LootTable table)
                return table.Find(names.Skip(1).ToArray());

            return item;
        }
    }
}
