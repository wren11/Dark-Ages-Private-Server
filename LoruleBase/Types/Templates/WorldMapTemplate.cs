#region

using System.Collections.Generic;

#endregion

namespace Darkages.Types
{
    public class WorldMapTemplate : Template
    {
        public List<WorldPortal> Portals = new List<WorldPortal>();

        public Warp Transition { get; set; }

        public int FieldNumber { get; set; }

        public int WorldIndex { get; set; } = 1;
    }

    public class WorldPortal
    {
        public string DisplayName { get; set; }

        public short PointX { get; set; }

        public short PointY { get; set; }

        public Warp Destination { get; set; }
    }
}