#region

using System.Collections.Generic;
using Darkages.Types;

#endregion

namespace Darkages.Types
{
    public class WorldMapTemplate : Template
    {
        public List<WorldPortal> Portals = new List<WorldPortal>();

        public int FieldNumber { get; set; }

        public override string[] GetMetaData()
        {
            return new[]
            {
                ""
            };
        }
    }
}