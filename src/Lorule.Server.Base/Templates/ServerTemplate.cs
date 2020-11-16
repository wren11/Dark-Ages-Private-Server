#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace Darkages.Types.Templates
{
    public class ServerTemplate : Template
    {
        [JsonProperty] public ICollection<Politics> Politics = new List<Politics>();

        [JsonProperty] public Dictionary<string, int> Variables = new Dictionary<string, int>();

        public override string[] GetMetaData()
        {
            return new[]
            {
                ""
            };
        }
    }
}