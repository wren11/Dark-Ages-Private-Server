using Darkages.Types;

namespace Darkages.Scripting
{
    public abstract class RewardScript : IScriptBase
    {
        public abstract void GenerateRewards(Monster monster, Aisling player);
    }
}
