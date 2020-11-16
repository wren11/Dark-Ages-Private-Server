using Darkages.Types;

namespace Darkages.Scripting
{
    public abstract class DamageFormulaScript : IScriptBase
    {
        public abstract int Calculate(Sprite obj, Sprite target, MonsterDamageType type);
    }
}
