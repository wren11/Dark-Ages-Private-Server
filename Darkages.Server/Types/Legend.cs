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
using System.Linq;

namespace Darkages.Types
{
    public class Legend
    {
        public List<LegendItem> LegendMarks = new List<LegendItem>();

        public bool Has(string lpVal)
        {
            return LegendMarks.Any(i => i.Value.Equals(lpVal));
        }

        public void AddLegend(LegendItem legend)
        {
            if (!LegendMarks.Contains(legend))
                LegendMarks.Add(legend);
        }

        public void Remove(LegendItem legend)
        {
            LegendMarks.Remove(legend);
        }

        public class LegendItem
        {
            public byte Icon { get; set; }
            public byte Color { get; set; }
            public string Category { get; set; }
            public string Value { get; set; }
        }
    }
}