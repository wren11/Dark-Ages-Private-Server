namespace Darkages.Types
{
    public class StatusOperator
    {
        public StatusOperator(Operator option, int value)
        {
            Option = option;
            Value = value;
        }

        public StatusOperator()
        {
            Option = Operator.Add;
            Value = 0;
        }

        public Operator Option { get; set; }
        public int Value { get; set; }
    }
}