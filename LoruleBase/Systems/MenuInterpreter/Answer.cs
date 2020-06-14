namespace MenuInterpreter
{
    public class Answer
    {
        public Answer(int id, string text, int linkedId = Constants.NoLink)
        {
            Id = id;
            Text = text;
            LinkedId = linkedId;
        }

        public int Id { get; }
        public int LinkedId { get; private set; }
        public string Text { get; set; }

        public void SetLink(int linkedId)
        {
            LinkedId = linkedId;
        }
    }
}