using System;
using Newtonsoft.Json;

namespace Darkages.Types.Templates
{
    public class Politics
    {
        public int Clout { get; set; }
        public int Nation { get; set; }
        public int NextRank { get; set; }
        public int Rank { get; set; }
        [JsonIgnore] public bool TermEnded => DateTime.UtcNow - TermStarted > TermLength;
        public TimeSpan TermLength { get; set; }
        public DateTime TermStarted { get; set; }
        public string User { get; set; }
    }
}