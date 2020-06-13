#region

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#endregion

namespace Darkages.Types
{
    public class WarpTemplate : Template
    {
        public WarpTemplate()
        {
            Activations = new List<Warp>();
        }

        [JsonProperty] public byte LevelRequired { get; set; }

        [JsonProperty] public int WarpRadius { get; set; }

        public List<Warp> Activations { get; set; }
        public Warp To { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WarpType WarpType { get; set; }

        [JsonProperty] public int ActivationMapId { get; set; }

        public int WorldTransionWarpId { get; set; }
        public int WorldResetWarpId { get; set; }
    }

    public enum WarpType
    {
        Map,
        World
    }
}