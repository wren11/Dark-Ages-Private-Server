using Darkages.Types;

namespace Darkages.Scripting
{
    public abstract class FormulaScript : IScriptBase
    {
        public abstract int Calculate(Sprite obj, int value);
    }
}
