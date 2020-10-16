#region

using System;

#endregion

namespace Darkages.Types
{
    public class NationTemplate : Template
    {
        public int AreaId { get; set; }
        public Position MapPosition { get; set; }
        public byte NationId { get; set; }

        public override string[] GetMetaData()
        {
            return new[]
            {
                ""
            };
        }

        public bool PastCurfew(Aisling aisling)
        {
            return (DateTime.UtcNow - aisling.LastLogged).TotalHours > ServerContext.Config.NationReturnHours;
        }
    }
}