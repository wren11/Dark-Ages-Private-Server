using System;
using System.Collections.Generic;

namespace Darkages.Types
{
    public class Legend
    {
        public List<LegendItem> LegendMarks = new List<LegendItem>();

        public void AddLegend(LegendItem legend)
        {
            if (!LegendMarks.Contains(legend))
            {
                LegendMarks.Add(legend);
            }
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