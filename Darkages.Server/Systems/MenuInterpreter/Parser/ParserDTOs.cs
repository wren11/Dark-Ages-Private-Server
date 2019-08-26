using System.Collections.Generic;

namespace MenuInterpreter.Parser
{
    public class Start : Link
    {
    }

    public class Link
    {
        public int? step { get; set; }
        public int? sequence { get; set; }
        public int? menu { get; set; }
        public int? checkpoint { get; set; }
        public int? quest { get; set; }
    }

    public class Answer : Link
    {
        public int id { get; set; }
        public string text { get; set; }
    }

    public class Step
    {
        public int id { get; set; }
        public string text { get; set; }
        public IList<Answer> answers { get; set; }
    }

    public class Sequence
    {
        public int id { get; set; }
        public string name { get; set; }
        public IList<Step> steps { get; set; }
    }

    public class Option : Link
    {
        public int id { get; set; }
        public string text { get; set; }
    }

    public class Menu
    {
        public int id { get; set; }
        public string text { get; set; }
        public IList<Option> options { get; set; }
    }

    public class QuestEvent
    {
        public int id { get; set; }
        public string name { get; set; }
        public Link accepted { get; set; }
    }


    public class Checkpoint
    {
        public int id { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public int amount { get; set; }
        public Link success { get; set; }
        public Link fail { get; set; }

    }

    public class ParseResult
    {
        public Start start { get; set; }
        public IList<Sequence> sequences { get; set; }
        public IList<Menu> menus { get; set; }
        public IList<Checkpoint> checkpoints { get; set; }

        public IList<QuestEvent> quests { get; set; }
    }
}