using System;

namespace Darkages.Systems.Loot.Modifiers
{
    [Serializable]
    public class NumericModifier : BaseModifier
    {
        public Operation Operation { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }

        public NumericModifier() { }
        public NumericModifier(string propertyName, double min, double max, Operation operation)
            : base(propertyName)
        {
            Min = min;
            Max = max;
            Operation = operation;
        }

        public override void Apply(object itemToModify)
        {
            var value = GetValue<double>(itemToModify);

            if (Max < Min)
                Max = Min;

            var number = Min;

            switch (Operation)
            {
                case Operation.Add:
                    value += number;
                    break;
                case Operation.Subtract:
                    value -= number;
                    break;
                case Operation.Divide:
                    value /= number;
                    break;
                case Operation.Multiply:
                    value *= number;
                    break;
                case Operation.Equal:
                    value = number;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Operation));
            }

            SetValue(itemToModify, Convert.ChangeType(value, value.GetType()));
        }
    }

    public enum Operation
    {
        /// <summary>
        /// Adds the selected value to the property.
        /// </summary>
        Add,
        /// <summary>
        /// Subtracts the selected value from the property.
        /// </summary>
        Subtract,
        /// <summary>
        /// Divides the property by the selected value.
        /// </summary>
        Divide,
        /// <summary>
        /// Multiplies the property by selected value.
        /// </summary>
        Multiply,
        /// <summary>
        /// Sets the property to the selected value.
        /// </summary>
        Equal
    }
}
