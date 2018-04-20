namespace Darkages.Types
{
    public class StatusOperator
    {
        public enum Operator
        {
            Add = 0,
            Remove = 1
        }

        public StatusOperator(Operator option, int value)
        {
            Option = option;
            Value = value;
        }

        public Operator Option { get; set; }
        public int Value { get; set; }

        public void Apply(object item) { }
    }
}