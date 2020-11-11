#region

using Darkages.Network.Object;
using Darkages.Types;

#endregion

namespace Darkages.Scripting
{
    public abstract class SkillScript : ObjectManager, IScriptBase, IUseable
    {
        protected SkillScript(Skill skill)
        {
            Skill = skill;
        }

        public bool IsScriptDefault { get; set; }

        public Skill Skill { get; set; }

        public abstract void OnFailed(Sprite sprite);

        public abstract void OnSuccess(Sprite sprite);

        public abstract void OnUse(Sprite sprite);
    }
}