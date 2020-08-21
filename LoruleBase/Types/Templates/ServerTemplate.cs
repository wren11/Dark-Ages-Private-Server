using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Darkages.Types.Templates
{
    public class Politics
    {
        public int Clout { get; set; }
        public int Nation { get; set; }
        public int NextRank { get; set; }
        public int Rank { get; set; }
        [JsonIgnore] public bool TermEnded => (DateTime.UtcNow - TermStarted) > TermLength;
        public TimeSpan TermLength { get; set; }
        public DateTime TermStarted { get; set; }
        public string User { get; set; }
    }

    public class ServerTemplate : Template
    {
        [JsonProperty]
        public ICollection<Politics> Politics = new List<Politics>();

        [JsonProperty]
        public Dictionary<string, int> Variables = new Dictionary<string, int>();

        public override string[] GetMetaData()
        {
            return new[]
            {
                ""
            };
        }
    }
}