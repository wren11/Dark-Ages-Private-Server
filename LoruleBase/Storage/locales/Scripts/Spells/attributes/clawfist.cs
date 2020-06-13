#region

using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.Buffs;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("Claw Fist", "Dean")]
    public class clawfist : SkillScript
    {
        public clawfist(Skill skill) : base(skill)
        {
        }

        public override void OnFailed(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                client.SendMessage(0x02, Skill.Template.FailMessage);
            }
        }

        public override void OnSuccess(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                var buff = new buff_clawfist();

                client.TrainSkill(Skill);

                if (!sprite.HasBuff("Claw Fist"))
                {
                    buff.OnApplied(sprite, buff);

                    var action = new ServerFormat1A
                    {
                        Serial = sprite.Serial,
                        Number = (byte) (client.Aisling.Path == Class.Priest ? 0x80 :
                            client.Aisling.Path == Class.Wizard ? 0x88 : 0x06),
                        Speed = 30
                    };

                    client.SendAnimation(54, client.Aisling, client.Aisling);
                    client.SendStats(StatusFlags.All);
                    client.Aisling.Show(Scope.NearbyAislings, action);
                }
                else
                {
                    client.SendMessage(0x02, "Your hands are already empowered.");
                }
            }
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                OnSuccess(sprite);
            }
            else
            {
                var buff = new buff_clawfist();

                if (!sprite.HasBuff(buff.Name))
                {
                    buff.OnApplied(sprite, buff);
                    sprite.SendAnimation(54, sprite, sprite);
                }
            }
        }
    }
}