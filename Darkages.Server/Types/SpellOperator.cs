namespace Darkages.Types
{
    public class SpellOperator
    {
        public enum SpellOperatorPolicy
        {
            Set = 0,
            Increase = 1,
            Decrease = 2
        }

        public enum SpellOperatorScope
        {
            ioc = 0,
            cradh = 1,
            nadur = 2,
            all = 3
        }

        public SpellOperator(SpellOperatorPolicy option, SpellOperatorScope scope, int value, int min, int max = 9)
        {
            Option = option;
            Scope = scope;
            Value = value;
            MinValue = min;
            MaxValue = max;
        }

        public SpellOperatorScope Scope { get; set; }
        public SpellOperatorPolicy Option { get; set; }
        public int Value { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
    }
}