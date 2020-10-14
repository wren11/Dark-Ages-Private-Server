#region

using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.Buffs;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("Hide")]
    public class Hide : SpellScript
    {
        public Hide(Spell spell) : base(spell)
        {
        }

        public override void OnFailed(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                client.SendMessage(0x02, "failed.");
            }
        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                var buff = new buff_hide();

                client.TrainSpell(Spell);

                if (!target.HasBuff(buff.Name))
                {
                    buff.OnApplied(target, buff);

                    var action = new ServerFormat1A
                    {
                        Serial = sprite.Serial,
                        Number = 0x29,
                        Speed = 30
                    };

                    client.SendAnimation(Spell.Template.Animation, target, client.Aisling);
                    client.Aisling.Show(Scope.NearbyAislings, action);
                    client.SendMessage(0x02, "you cast " + Spell.Template.Name + ".");
                    client.SendStats(StatusFlags.All);
                }
                else
                {
                    client.SendMessage(0x02, "You are already hidden.");
                }
            }
        }

        public override void OnUse(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                if (client.Aisling.CurrentMp >= Spell.Template.ManaCost)
                {
                    client.Aisling.CurrentMp -= Spell.Template.ManaCost;
                    if (client.Aisling.CurrentMp < 0)
                        client.Aisling.CurrentMp = 0;

                    OnSuccess(sprite, target);
                }
                else
                {
                    if (sprite is Aisling)
                        (sprite as Aisling).Client.SendMessage(0x02, ServerContext.Config.NoManaMessage);
                    return;
                }

                client.SendStats(StatusFlags.StructB);
            }
            else
            {
                var buff = new buff_hide();

                if (!target.HasBuff(buff.Name))
                {
                    buff.OnApplied(target, buff);
                    sprite.SendAnimation(Spell.Template.Animation, target, sprite);
                }
            }
        }
    }
}