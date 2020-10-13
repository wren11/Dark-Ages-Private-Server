using System;
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.formulas
{
    [Script("AC Formula", "Wren", "Base Formula for all AC damage calculations.")]
    public class Ac : FormulaScript
    {
        private readonly Sprite _obj;

        public Ac(Sprite obj)
        {
            _obj = obj;
        }

        public override int Calculate(Sprite obj, int value)
        {
            var armor = obj.Ac;

            var calculatedDmg = value * Math.Abs(armor + 101) / 99;

            if (calculatedDmg < 0)
                calculatedDmg = 1;

            var diff = Math.Abs(value - calculatedDmg);

            return calculatedDmg + diff;
        }
    }
}
