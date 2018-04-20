using Darkages.Network.Game;
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Skills
{
    [Script("Charge", "Huy")]
    public class Charge : SkillScript
    {
        public Skill _skill;
        
        public Charge(Skill skill) : base(skill)
        {
            _skill = skill;
        }
        
        public override void OnFailed(Sprite sprite)
        {

        }

        public override void OnSuccess(Sprite sprite)
        {
            
        }

        public override void OnUse(Sprite sprite)
        {
            
        }
    }
}