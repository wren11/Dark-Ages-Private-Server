#region

using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Skills
{
    [Script("Unstuck", "Test")]
    public class Unstuck : SkillScript
    {
        public Unstuck(Skill skill) : base(skill)
        {
        }

        public override void OnFailed(Sprite sprite)
        {
        }

        public override void OnSuccess(Sprite sprite)
        {
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling)
            {
            }
        }
    }
}