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

namespace Darkages.Types
{
    /// <summary>
    ///     This entire exchange routine was shamelessly copy pasted from Kojasou's Server Project.
    ///     (Yes I'm way to lazy to write this myself when it's already been done correctly.)
    ///     Credits: https://github.com/kojasou/wewladh
    /// </summary>
    public class ExchangeSession
    {
        public ExchangeSession(Aisling user)
        {
            Trader = user;
            Items = new List<Item>();
        }

        public Aisling Trader { get; set; }

        public List<Item> Items { get; set; }

        public int Gold { get; set; }

        public bool Confirmed { get; set; }

        public int Weight { get; set; }
    }
}