#region

using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Spells.elemental
{
    [Script("MSPG")]
    public class mspg : SpellScript
    {
        public mspg(Spell spell) : base(spell)
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
            sprite.CurrentMp = 0;

            var targets = GetObjects(sprite.Map, i => i.WithinRangeOf(sprite), Get.Aislings | Get.Monsters);

            foreach (var targetObj in targets)
            {
                if (targetObj.Serial == sprite.Serial)
                    continue;

                var dmg = sprite.MaximumMp * 0.01 * (sprite.Int * 0.01) * 200;

                targetObj.ApplyDamage(sprite, (int) dmg, sprite.OffenseElement, Spell.Template.Sound);

                targetObj.SendAnimation(Spell.Template.Animation, targetObj, sprite);

                ShowDamage(targetObj);
            }

            SendAction(sprite);
        }

        private void SendAction(Sprite sprite)
        {
            var action = new ServerFormat1A
            {
                Serial = sprite.Serial,
                Number = 0x80,
                Speed = 30
            };
            sprite.Show(Scope.NearbyAislings, action);
        }

        private void ShowDamage(Sprite target)
        {
            var hpbar = new ServerFormat13
            {
                Serial = target.Serial,
                Health = (ushort) (100 * target.CurrentHp / target.MaximumHp),
                Sound = Spell.Template.Sound
            };
            target.Show(Scope.NearbyAislings, hpbar);
        }
    }
}