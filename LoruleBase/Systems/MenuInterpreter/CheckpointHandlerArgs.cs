namespace MenuInterpreter
{
    public class QuestHanderArgs
    {
        public string QuestName { get; set; }
        public bool IsMet { get; set; }
    }

    public class CheckpointHandlerArgs
    {
        public string Value { get; set; }

        public int Amount { get; set; }

        public bool Result { get; set; }
    }
}