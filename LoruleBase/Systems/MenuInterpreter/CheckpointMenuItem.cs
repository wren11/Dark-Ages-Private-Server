namespace MenuInterpreter
{
    public class CheckpointMenuItem : MenuItem
    {
        public CheckpointMenuItem(int id, string text, Answer[] answers)
            : base(id, MenuItemType.Checkpoint, text, answers)
        {
        }

        public string Value { get; set; }
        public int Amount { get; set; }
    }
}