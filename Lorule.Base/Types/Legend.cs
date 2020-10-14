#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Darkages.Types
{
    public class Legend
    {
        public List<LegendItem> LegendMarks = new List<LegendItem>();

        public void AddLegend(LegendItem legend)
        {
            if (!LegendMarks.Contains(legend))
                LegendMarks.Add(legend);
        }

        public bool Has(string lpVal)
        {
            return LegendMarks.Any(i => i.Value.Equals(lpVal));
        }

        public void Remove(LegendItem legend)
        {
            LegendMarks.Remove(legend);
        }

        public class LegendItem
        {
            public string Category { get; set; }
            public byte Color { get; set; }
            public byte Icon { get; set; }
            public string Value { get; set; }
        }
    }
}