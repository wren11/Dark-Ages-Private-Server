using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Assets.locales.Scripts.Traps
{
    [Script("Needle Trap")]
    public class NeedleTrap : SpellScript
    {
        public NeedleTrap(Spell spell) : base(spell)
        {

        }

        public override void OnFailed(Sprite sprite, Sprite target)
        {

        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {

        }

        public override void OnUse(Sprite sprite, Sprite target)
        {
            var trap = new Trap()
            {
                Duration = 5,
                EffectRadius = 1,
            };

            //setup script callbacks
            Trap.CreateTrap(sprite, trap);
            {
                trap.Activated = OnActivated;
                trap.Toggle    = OnSelectionToggle;
                trap.Tripped   = OnTriggeredBy;
            };
        }


        public override void OnActivated(Sprite sprite)
        {

        }

        public override void OnSelectionToggle(Sprite sprite)
        {

        }

        public override void OnTriggeredBy(Sprite sprite, Sprite target)
        {

            target.ApplyDamage(sprite, 1000, true, 1);
            if (target is Aisling)
            {
                target.Show(Scope.Self, new ServerFormat0A(0x01, $"Uh oh. [{Spell.Name}]"));
            }

            if (target is Monster || target is Mundane || target is Aisling)
                target.Show(Scope.NearbyAislings,
                    new ServerFormat29((uint)target.Serial, (uint)target.Serial,
                        Spell.Template.TargetAnimation, 0, 100));


        }
    }
}
