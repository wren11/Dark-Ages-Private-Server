using Darkages.Types;

namespace Darkages.Scripting
{
    public interface IScriptBase
    {
    }

    public interface IUseable
    { 
        void OnUse(Sprite sprite);
    }

    public interface IUseableTarget
    {
        void OnUse(Sprite sprite, Sprite target);
    }
}
