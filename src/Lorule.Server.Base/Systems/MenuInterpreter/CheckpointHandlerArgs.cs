namespace MenuInterpreter
{
    public class CheckpointHandlerArgs
    {
        public int Amount { get; set; }
        public bool Result { get; set; }
        public string Value { get; set; }
    }

    public class QuestHanderArgs
    {
        public bool IsMet { get; set; }
        public string QuestName { get; set; }
    }
}