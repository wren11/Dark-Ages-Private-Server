///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/

using System;

namespace Darkages.Systems.Loot.Modifiers
{
    [Serializable]
    public class NumericModifier : BaseModifier
    {
        public NumericModifier()
        {
        }

        public NumericModifier(string propertyName, double min, double max, Operation operation)
            : base(propertyName)
        {
            Min = min;
            Max = max;
            Operation = operation;
        }

        public Operation Operation { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }

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
        ///     Adds the selected value to the property.
        /// </summary>
        Add,

        /// <summary>
        ///     Subtracts the selected value from the property.
        /// </summary>
        Subtract,

        /// <summary>
        ///     Divides the property by the selected value.
        /// </summary>
        Divide,

        /// <summary>
        ///     Multiplies the property by selected value.
        /// </summary>
        Multiply,

        /// <summary>
        ///     Sets the property to the selected value.
        /// </summary>
        Equal
    }
}