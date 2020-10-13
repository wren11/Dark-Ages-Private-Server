using System;
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.formulas
{
    [Script("Base Damage", "Wren", "Formula used to calculate monsters Base Damage.")]
    public class Damage : DamageFormulaScript
    {
        private readonly Sprite _obj;
        private readonly Sprite _target;

        public Damage(Sprite obj, Sprite target)
        {
            _obj = obj;
            _target = target;
        }

        public override int Calculate(Sprite obj, Sprite target, MonsterDamageType type)
        {
            if (obj is Monster || obj is Mundane)
            {
                var mod = 0.0;
                var diff = 0;

                if (target is Aisling aisling)
                    diff = obj.Level + 1 - aisling.ExpLevel;

                if (target is Monster monster)
                    diff = obj.Level + 1 - monster.Template.Level;

                if (diff <= 0)
                    mod = obj.Level * (type == MonsterDamageType.Physical ? 0.1 : 2) * ServerContext.Config.BaseDamageMod;
                else
                    mod = obj.Level * (type == MonsterDamageType.Physical ? 0.1 : 2) * (ServerContext.Config.BaseDamageMod * diff);

                var dmg = Math.Abs((int)(mod + 1));

                if (dmg <= 0)
                    dmg = 1;

                return dmg;
            }

            return 1;
        }
    }
}
