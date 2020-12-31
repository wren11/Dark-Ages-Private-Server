#region

using System;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("Generic Elemental Single", "Dean")]
    public class Generic_Elemental_Single : SpellScript
    {
        private readonly Random rand = new Random();

        public Generic_Elemental_Single(Spell spell) : base(spell)
        {
        }

        public override void OnFailed(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                (sprite as Aisling)
                    .Client
                    .SendMessage(0x02, "Your spell has been deflected.");
                (sprite as Aisling)
                    .Client
                    .SendAnimation(33, target, sprite);
            }
            else
            {
                if (sprite is Monster)
                    (sprite.Target as Aisling)?.Client
                        .SendAnimation(33, sprite, target);
            }
        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                client.TrainSpell(Spell);

                if (target is Aisling)
                    (target as Aisling).Client
                        .SendMessage(0x02,
                            $"{client.Aisling.Username} Attacks you with {Spell.Template.Name}.");

                var imp = Spell.Level * 2 / 100;
                var dmg = (int) (client.Aisling.Int / 2 * Spell.Template.DamageExponent);

                dmg *= imp;

                target.ApplyDamage(sprite, dmg, Spell.Template.ElementalProperty, Spell.Template.Sound);

                client.SendMessage(0x02, $"you cast {Spell.Template.Name}");
                client.SendAnimation(Spell.Template.Animation, target, sprite);

                var action = new ServerFormat1A
                {
                    Serial = sprite.Serial,
                    Number = (byte) (client.Aisling.Path == Class.Priest ? 0x80 :
                        client.Aisling.Path == Class.Wizard ? 0x88 : 0x06),
                    Speed = 30
                };

                client.Aisling.Show(Scope.NearbyAislings, action);
            }
            else
            {
                var dmg = sprite.GetBaseDamage(sprite.Target, MonsterDamageType.Elemental);

                target.ApplyDamage(sprite, dmg, Spell.Template.ElementalProperty, Spell.Template.Sound);

                if (target is Aisling)
                    (target as Aisling).Client
                        .SendMessage(0x02,
                            $"{(sprite is Monster ? (sprite as Monster).Template.Name : (sprite as Mundane).Template.Name) ?? "Monster"} Attacks you with {Spell.Template.Name}.");

                target.SendAnimation(Spell.Template.Animation, target, sprite);

                var action = new ServerFormat1A
                {
                    Serial = sprite.Serial,
                    Number = 1,
                    Speed = 30
                };

                sprite.Show(Scope.NearbyAislings, action);
            }
        }

        public override void OnUse(Sprite sprite, Sprite target)
        {
            if (sprite.CurrentMp - Spell.Template.ManaCost > 0)
            {
                sprite.CurrentMp -= Spell.Template.ManaCost;
            }
            else
            {
                if (sprite is Aisling)
                    (sprite as Aisling).Client.SendMessage(0x02, ServerContext.Config.NoManaMessage);
                return;
            }

            if (sprite.CurrentMp < 0)
                sprite.CurrentMp = 0;

            if (rand.Next(0, 100) > target.Mr)
                OnSuccess(sprite, target);
            else
                OnFailed(sprite, target);

            if (sprite is Aisling)
                (sprite as Aisling)
                    .Client
                    .SendStats(StatusFlags.StructB);
        }
    }
}