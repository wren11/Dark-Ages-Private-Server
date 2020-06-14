#region

using System.Collections.Generic;

#endregion

namespace Darkages.Types
{
    public class WorldMapTemplate : Template
    {
        public List<WorldPortal> Portals = new List<WorldPortal>();

        public int FieldNumber { get; set; }
        public Warp Transition { get; set; }
        public int WorldIndex { get; set; } = 1;
    }

    public class WorldPortal
    {
        public Warp Destination { get; set; }
        public string DisplayName { get; set; }

        public short PointX { get; set; }

        public short PointY { get; set; }
    }
}