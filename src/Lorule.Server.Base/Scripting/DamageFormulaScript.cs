using Darkages.Types;

namespace Darkages.Scripting
{
    public abstract class DamageFormulaScript
    {
        public abstract int Calculate(Sprite obj, Sprite target, MonsterDamageType type);
    }
}
