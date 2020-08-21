#region

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

#endregion

namespace Darkages.Types
{
    public class WarpTemplate : Template
    {
        public WarpTemplate()
        {
            Activations = new List<Warp>();
        }

        [JsonProperty] public int ActivationMapId { get; set; }
        public List<Warp> Activations { get; set; }
        [JsonProperty] public byte LevelRequired { get; set; }

        public Warp To { get; set; }
        [JsonProperty] public int WarpRadius { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WarpType WarpType { get; set; }

        public int WorldResetWarpId { get; set; }
        public int WorldTransionWarpId { get; set; }

        public override string[] GetMetaData()
        {
            return new[]
            {
                ""
            };
        }
    }
}