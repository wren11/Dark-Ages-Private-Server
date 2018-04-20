using Newtonsoft.Json;
using System.Collections.Generic;

namespace Darkages.Types
{
    public class WarpTemplate : Template
    {
        [JsonProperty] public byte LevelRequired { get; set; }

        [JsonProperty] public int WarpRadius { get; set; }

        public List<Warp> Activations { get; set; }
        public Warp To { get; set; }
        public WarpType WarpType { get; set; }

        [JsonProperty] public int ActivationMapId { get; set; }

        public WarpTemplate()
        {
            Activations = new List<Warp>();
        }

    }

    public enum WarpType
    {
        Map,
        World
    }
}