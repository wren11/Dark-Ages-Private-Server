using System.Collections.Generic;

namespace Darkages.Types
{
    public class WorldMapTemplate : Template
    {
        public List<WorldPortal> Portals = new List<WorldPortal>();
        public int FieldNumber { get; set; }

        public Warp Transition { get; set; }
    }

    public class WorldPortal
    {
        public string DisplayName { get; set; }

        public short PointX { get; set; }

        public short PointY { get; set; }

        public Warp Destination { get; set; }
    }
}