using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Spells.elemental
{
    [Script(name: "MSPG")]
    public class mspg : SpellScript
    {
        public mspg(Spell spell) : base(spell) { }

        //Don't need to use these if you don't need too.
        public override void OnFailed(Sprite sprite, Sprite target)  { }
        public override void OnSuccess(Sprite sprite, Sprite target) { }

        public override void OnUse(Sprite sprite, Sprite target)
        {
            //MSPG MP = 0;
            sprite.CurrentMp = 0;

            var targets = GetObjects(sprite.Map, i => i.WithinRangeOf(sprite), Get.Aislings | Get.Monsters);

            foreach (var targetObj in targets)
            {
                //don't mpsg yourself.
                if (targetObj.Serial == sprite.Serial)
                    continue;

                //apply your damage formula.
                var dmg = sprite.MaximumMp * 0.01 * (sprite.Int * 0.01) * 200;

                //deal dmg
                targetObj.ApplyDamage(sprite, (int)dmg, sprite.OffenseElement, Spell.Template.Sound);

                //send animation
                targetObj.SendAnimation(Spell.Template.Animation, targetObj, sprite);

                //display to clients
                ShowDamage(targetObj);
            }

            SendAction(sprite);
        }

        private void ShowDamage(Sprite target)
        {
            var hpbar = new ServerFormat13
            {
                Serial = target.Serial,
                Health = (ushort)(100 * target.CurrentHp / target.MaximumHp),
                Sound  = Spell.Template.Sound
            };
            target.Show(Scope.NearbyAislings, hpbar);
        }

        private void SendAction(Sprite sprite)
        {
            var action = new ServerFormat1A
            {
                Serial = sprite.Serial,
                Number = 0x80,
                Speed  = 30
            };            
            sprite.Show(Scope.NearbyAislings, action);
        }
    }
}
