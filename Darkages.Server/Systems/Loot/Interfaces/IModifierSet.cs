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

using System.Collections.Generic;

namespace Darkages.Systems.Loot.Interfaces
{
    public interface IModifierSet : ILootDefinition
    {
        /// <summary>
        ///     Gets the modifiers contained in this set.
        /// </summary>
        ICollection<IModifier> Modifiers { get; }

        /// <summary>
        ///     Modify an item using <see cref="Modifiers" />.
        /// </summary>
        /// <param name="item">The item to modify.</param>
        void ModifyItem(object item);

        /// <summary>
        ///     Add a modifier to this set.
        /// </summary>
        /// <param name="modifier">The modifier to add.</param>
        IModifierSet Add(IModifier modifier);

        /// <summary>
        ///     Removes a modifier from this set.
        /// </summary>
        /// <param name="modifier">The modifier to remove.</param>
        IModifierSet Remove(IModifier modifier);
    }
}