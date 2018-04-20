using Darkages.Network.Object;
using Darkages.Types;

namespace Darkages.Scripting
{
    public abstract class SkillScript : ObjectManager
    {

        public SkillScript(Skill skill)
        {
            Skill = skill;
        }

        public Skill Skill { get; set; }

        public bool IsScriptDefault { get; set; }

        public abstract void OnUse(Sprite sprite);
        public abstract void OnFailed(Sprite sprite);
        public abstract void OnSuccess(Sprite sprite);
    }
}