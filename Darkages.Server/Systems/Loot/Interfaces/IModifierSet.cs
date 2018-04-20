using System.Collections.Generic;

namespace Darkages.Systems.Loot.Interfaces
{
    public interface IModifierSet : ILootDefinition
    {
        /// <summary>
        /// Gets the modifiers contained in this set.
        /// </summary>
        ICollection<IModifier> Modifiers { get; }

        /// <summary>
        /// Modify an item using <see cref="Modifiers"/>.
        /// </summary>
        /// <param name="item">The item to modify.</param>
        void ModifyItem(object item);

        /// <summary>
        /// Add a modifier to this set.
        /// </summary>
        /// <param name="modifier">The modifier to add.</param>
        IModifierSet Add(IModifier modifier);

        /// <summary>
        /// Removes a modifier from this set.
        /// </summary>
        /// <param name="modifier">The modifier to remove.</param>
        IModifierSet Remove(IModifier modifier);
    }
}
