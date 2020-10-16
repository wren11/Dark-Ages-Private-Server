#region

using System;
using System.Linq;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("ao beag cradh", "Dean")]
    public class ao_beag_cradh : SpellScript
    {
        private readonly debuff_beagcradh Debuff = new debuff_beagcradh();
        private readonly Random rand = new Random();

        public ao_beag_cradh(Spell spell) : base(spell)
        {
        }

        public override void OnFailed(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
                (sprite as Aisling)
                    .Client
                    .SendMessage(0x02, "failed.");
        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                client.TrainSpell(Spell);

                var debuff = Clone<debuff_beagcradh>(Debuff);
                var curses = target.Debuffs.Values.OfType<debuff_cursed>().ToList();

                client.SendMessage(0x02, $"you cast {Spell.Template.Name}");
                client.SendAnimation(Spell.Template.Animation, target, sprite);

                var action = new ServerFormat1A
                {
                    Serial = sprite.Serial,
                    Number = (byte) (client.Aisling.Path == Class.Priest ? 0x80 :
                        client.Aisling.Path == Class.Wizard ? 0x88 : 0x06),
                    Speed = 30
                };

                var hpbar = new ServerFormat13
                {
                    Serial = client.Aisling.Serial,
                    Health = 255,
                    Sound = Spell.Template.Sound
                };

                client.Aisling.Show(Scope.NearbyAislings, action);
                client.Aisling.Show(Scope.NearbyAislings, hpbar);

                if (curses.Count > 0)
                {
                    if (target.HasDebuff(debuff.Name))
                    {
                        if (target.RemoveDebuff(debuff.Name, true))
                            if (target is Aisling)
                                (target as Aisling).Client
                                    .SendMessage(0x02,
                                        $"{client.Aisling.Username} Removes {Spell.Template.Name} from you.");
                    }
                    else
                    {
                        var c = curses.FirstOrDefault();
                        if (c != null)
                            client.SendMessage(0x02, $"A greater cure is required [{c.Name}]");
                    }
                }
            }
            else
            {
                var debuff = Clone<debuff_beagcradh>(Debuff);
                var curses = target.Debuffs.Values.OfType<debuff_cursed>().ToList();

                if (curses.Count > 0)
                    if (target.HasDebuff(debuff.Name))
                        if (target.RemoveDebuff(debuff.Name, true))
                            if (target is Aisling)
                                (target as Aisling).Client
                                    .SendMessage(0x02,
                                        $"{(sprite is Monster ? (sprite as Monster).Template.Name : (sprite as Mundane).Template.Name) ?? "Monster"} Removes {Spell.Template.Name} from you.");

                target.SendAnimation(Spell.Template.Animation, target, sprite);

                var action = new ServerFormat1A
                {
                    Serial = sprite.Serial,
                    Number = 1,
                    Speed = 30
                };

                var hpbar = new ServerFormat13
                {
                    Serial = target.Serial,
                    Health = 255,
                    Sound = Spell.Template.Sound
                };

                sprite.Show(Scope.NearbyAislings, action);
                target.Show(Scope.NearbyAislings, hpbar);
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

            var success = Spell.RollDice(rand);

            if (success)
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