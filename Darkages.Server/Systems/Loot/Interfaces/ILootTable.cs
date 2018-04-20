using System.Collections.Generic;

namespace Darkages.Systems.Loot.Interfaces
{
    /// <summary>
    /// An interface for loot tables.
    /// </summary>
    public interface ILootTable : ILootDefinition
    {
        /// <summary>
        /// Gets the children contained in this loot table
        /// </summary>
        ICollection<ILootDefinition> Children { get; }

        /// <summary>
        /// Add an item to this table.
        /// </summary>
        /// <param name="item">The item to add.</param>
        ILootTable Add(ILootDefinition item);

        /// <summary>
        /// Gets a <see cref="ILootDefinition"/> from the table with the specified name.
        /// </summary>
        /// <param name="name">The name of the item</param>
        /// <returns>The item or null if the item could not be found.</returns>
        ILootDefinition Get(string name);

        /// <summary>
        /// Removes an item from this table.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        ILootTable Remove(ILootDefinition item);
    }
}
