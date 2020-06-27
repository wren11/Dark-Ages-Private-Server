using System.Collections.Generic;
using Newtonsoft.Json;

namespace Darkages.Types
{
    public class QuestStep<T>
    {
        [JsonIgnore] public List<QuestRequirement> Prerequisites = new List<QuestRequirement>();

        public bool StepComplete { get; set; }
        public QuestType Type { get; set; }
    }
}