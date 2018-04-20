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
