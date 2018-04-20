using Darkages.Systems.Loot.Extensions;
using Darkages.Systems.Loot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darkages.Systems.Loot
{
    public class LootDropper : ILootDropper
    {
        public event EventHandler<EventArgs> OnDropStarted;
        public event EventHandler<EventArgs> OnDropCompleted;

        public ILootDefinition Drop(ILootTable lootTable, string name)
        {
            var item = lootTable.Get(name);

            if (item is ILootTable childTable)
                return Drop(childTable);

            return item;
        }

        public IEnumerable<ILootDefinition> Drop(ILootTable lootTable, int amount)
        {
            if (amount <= 0)
                return new List<ILootDefinition>();

            OnDropStarted?.Invoke(this, EventArgs.Empty);

            var drops = new List<ILootDefinition>();

            for (var i = 0; i < amount; i++)
            {
                drops.Add(Drop(lootTable));
            }

            OnDropCompleted?.Invoke(this, EventArgs.Empty);

            return drops;
        }

        public IEnumerable<ILootDefinition> Drop(ILootTable lootTable, int amount, string name)
        {
            if (amount <= 0)
                return new List<ILootDefinition>();

            var drops = new List<ILootDefinition>();

            for (var i = 0; i < amount; i++)
            {
                drops.Add(Drop(lootTable, name));
            }

            return drops;
        }

        public static long GlobalRolls = 0;
        public ILootDefinition Drop(ILootTable lootTable)
        {
            if (lootTable == null || lootTable.Children.Count == 0)
                return null;

            var item = Pick(lootTable.Children);
            if (item == null)
                return null;

            var bonus = (Math.Round(GlobalRolls * 0.01, 3));

            lock (Common.Generator.Random)
            {
                var roll = Math.Abs((Common.Generator.Random.NextDouble() * 2.0) - 1.0);
                if (roll - bonus <= item.Weight || bonus > 0.05)
                {
                    GlobalRolls = 0;

                    if (item is ILootTable childTable)
                        return Drop(childTable);

                    return item;
                }
            }

            GlobalRolls++;
            return null;
        }

        public static T Pick<T>(IEnumerable<T> items) where T : class, IWeighable
        {
            var itemList = items as IList<T> ?? items.ToList();
            if (itemList == null || !itemList.Any())
                throw new ArgumentException("Items cannot be null or empty", nameof(items));

            var selectedItem = itemList.WeightedChoice(
                itemList.Sum(item => item.Weight));

            return selectedItem;
        }
    }
}
