#region

using System.Collections.Generic;

#endregion

namespace Darkages.Systems.Loot.Interfaces
{
    public interface IModifierSet : ILootDefinition
    {
        ICollection<IModifier> Modifiers { get; }

        void ModifyItem(object item);

        IModifierSet Add(IModifier modifier);

        IModifierSet Remove(IModifier modifier);
    }
}