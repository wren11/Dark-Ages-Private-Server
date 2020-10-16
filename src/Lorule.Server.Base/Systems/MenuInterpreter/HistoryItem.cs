namespace MenuInterpreter
{
    public class HistoryItem
    {
        public HistoryItem()
        {
        }

        public HistoryItem(int itemId, int answerId)
        {
            ItemId = itemId;
            AnswerId = answerId;
        }

        public int AnswerId { get; set; }
        public int ItemId { get; set; }
    }
}