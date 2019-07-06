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
    /// <summary>
    ///     An interface for loot tables.
    /// </summary>
    public interface ILootTable : ILootDefinition
    {
        /// <summary>
        ///     Gets the children contained in this loot table
        /// </summary>
        ICollection<ILootDefinition> Children { get; }

        /// <summary>
        ///     Add an item to this table.
        /// </summary>
        /// <param name="item">The item to add.</param>
        ILootTable Add(ILootDefinition item);

        /// <summary>
        ///     Gets a <see cref="ILootDefinition" /> from the table with the specified name.
        /// </summary>
        /// <param name="name">The name of the item</param>
        /// <returns>The item or null if the item could not be found.</returns>
        ILootDefinition Get(string name);

        /// <summary>
        ///     Removes an item from this table.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        ILootTable Remove(ILootDefinition item);
    }
}