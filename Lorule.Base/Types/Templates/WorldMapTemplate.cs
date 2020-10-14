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

        public override string[] GetMetaData()
        {
            return new[]
            {
                ""
            };
        }
    }
}