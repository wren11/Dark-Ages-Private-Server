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
using System;

namespace Darkages.Types
{
    [Flags]
    public enum ItemFlags
    {
        Equipable = 1,
        Perishable = 1 << 1,
        Tradeable = 1 << 2,
        Dropable = 1 << 3,
        Bankable = 1 << 4,
        Sellable = 1 << 5,
        Repairable = 1 << 6,
        Stackable = 1 << 7,
        Consumable = 1 << 8,
        Elemental = 1 << 10,
        QuestRelated = 1 << 11,
        Upgradeable = 1 << 12,
        TwoHanded = 1 << 13,
        LongRanged = 1 << 14,
    }
}
