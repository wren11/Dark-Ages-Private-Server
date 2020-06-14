#region

using System.Collections.Generic;

#endregion

namespace MenuInterpreter.Parser
{
    public class Answer : Link
    {
        public int id { get; set; }
        public string text { get; set; }
    }

    public class Checkpoint
    {
        public int amount { get; set; }
        public Link fail { get; set; }
        public int id { get; set; }
        public Link success { get; set; }
        public string type { get; set; }
        public string value { get; set; }
    }

    public class Link
    {
        public int? checkpoint { get; set; }
        public int? menu { get; set; }
        public int? quest { get; set; }
        public int? sequence { get; set; }
        public int? step { get; set; }
    }

    public class Menu
    {
        public int id { get; set; }
        public IList<Option> options { get; set; }
        public string text { get; set; }
    }

    public class Option : Link
    {
        public int id { get; set; }
        public string text { get; set; }
    }

    public class ParseResult
    {
        public IList<Checkpoint> checkpoints { get; set; }
        public IList<Menu> menus { get; set; }
        public IList<QuestEvent> quests { get; set; }
        public IList<Sequence> sequences { get; set; }
        public Start start { get; set; }
    }

    public class QuestEvent
    {
        public Link accepted { get; set; }
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Sequence
    {
        public int id { get; set; }
        public string name { get; set; }
        public IList<Step> steps { get; set; }
    }

    public class Start : Link
    {
    }

    public class Step
    {
        public IList<Answer> answers { get; set; }
        public int id { get; set; }
        public string text { get; set; }
    }
}