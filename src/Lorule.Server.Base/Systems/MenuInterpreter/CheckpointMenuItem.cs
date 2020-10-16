namespace MenuInterpreter
{
    public class CheckpointMenuItem : MenuItem
    {
        public CheckpointMenuItem(int id, string text, Answer[] answers)
            : base(id, MenuItemType.Checkpoint, text, answers)
        {
        }

        public int Amount { get; set; }
        public string Value { get; set; }
    }
}