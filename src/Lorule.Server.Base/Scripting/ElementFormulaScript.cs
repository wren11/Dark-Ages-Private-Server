using Darkages.Types;

namespace Darkages.Scripting
{
    public abstract class ElementFormulaScript
    {
        public abstract double Calculate(Sprite obj, ElementManager.Element element);
    }
}
