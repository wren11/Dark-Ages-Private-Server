#region

using System.Collections.Generic;

#endregion

namespace Darkages.Systems.Loot.Interfaces
{
    public interface IModifierSet : ILootDefinition
    {
        IList<IModifier> Modifiers { get; }

        IModifierSet Add(IModifier modifier);

        void ModifyItem(object item);

        IModifierSet Remove(IModifier modifier);
    }
}