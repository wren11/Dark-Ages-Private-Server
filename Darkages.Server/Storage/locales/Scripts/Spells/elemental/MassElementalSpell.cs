///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
using Darkages.Common;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using System;
using System.Linq;

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("Generic Elemental Mass", "Dean")]
    public class Generic_Elemental_Mass : SpellScript
    {
        private readonly Random rand = new Random();

        public Generic_Elemental_Mass(Spell spell) : base(spell)
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
                    (sprite.Target as Aisling)
                        .Client
                        .SendAnimation(33, sprite, target);
            }
        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                var d = 15 + (sprite.Int * Spell.Level / 10);
                var dmg = (int)((sprite.Int * d) * Spell.Template.DamageExponent);

                target.ApplyDamage(sprite, dmg, Spell.Template.ElementalProperty, Spell.Template.Sound);
            }
            else
            {
                if (!(target is Aisling))
                    return;

                var client = (target as Aisling).Client;

                var dmg = sprite.GetBaseDamage(target, MonsterDamageType.Elemental);
                target.ApplyDamage(sprite, dmg, Spell.Template.ElementalProperty, Spell.Template.Sound);

                (target as Aisling).Client
                    .SendMessage(0x02, string.Format("{0} Attacks you with {1}.",
                        (sprite is Monster
                            ? (sprite as Monster).Template.Name
                            : (sprite as Mundane).Template.Name) ?? "Monster",
                        Spell.Template.Name));

                client.SendAnimation(Spell.Template.Animation, target, sprite);

                var action = new ServerFormat1A
                {
                    Serial = sprite.Serial,
                    Number = 0x80,
                    Speed = 30
                };

                client.Aisling.Show(Scope.NearbyAislings, action);
            }
        }

        public override void OnUse(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                if (sprite.CurrentMp - Spell.Template.ManaCost > 0)
                    sprite.CurrentMp -= Spell.Template.ManaCost;
                else
                {
                    if (sprite is Aisling)
                    {
                        (sprite as Aisling).Client.SendMessage(0x02, ServerContext.Config.NoManaMessage);
                    }
                    return;

                }

                if (sprite.CurrentMp < 0)
                {
                    sprite.CurrentMp = 0;
                }
                var targets = GetObjects(sprite.Map, i => i.WithinRangeOf(sprite), Get.Aislings | Get.Monsters | Get.Mundanes);
                var client = (sprite as Aisling).Client;
                client.TrainSpell(Spell);

                foreach (var t in targets)
                {
                    if (t.Serial == sprite.Serial)
                        continue;

                    if (t.CurrentHp == 0)
                        continue;

                    client.SendAnimation(Spell.Template.Animation, t, sprite);

                    lock (rand)
                    {
                        if (rand.Next(Extensions.Clamp(sprite.Hit * 5, 0, 100), 100) > t.Mr)
                        {
                            OnSuccess(sprite, t);

                            if (t is Aisling)
                                (t as Aisling).Client
                                    .SendMessage(0x02,
                                        string.Format("{0} Attacks you with {1}.", client.Aisling.Username,
                                            Spell.Template.Name));


                        }
                        else
                            OnFailed(sprite, t);
                    }


                }

                client.SendMessage(0x02, string.Format("you cast {0}", Spell.Template.Name));

                var action = new ServerFormat1A
                {
                    Serial = sprite.Serial,
                    Number = (byte)(client.Aisling.Path == Class.Priest ? 0x80 : client.Aisling.Path == Class.Wizard ? 0x88 : 0x06),
                    Speed = 30
                };

                client.Aisling.Show(Scope.NearbyAislings, action);

                if (sprite is Aisling)
                    (sprite as Aisling)
                        .Client
                        .SendStats(StatusFlags.StructB);
            }
            else
            {
                var targets = GetObjects(sprite.Map, i => i.WithinRangeOf(sprite), Get.Monsters);

                foreach (var t in targets)
                {
                    if (t.Serial == sprite.Serial)
                        continue;

                    if (t.CurrentHp == 0)
                        continue;


                    var dmg = sprite.GetBaseDamage(t, MonsterDamageType.Elemental);

                    t.ApplyDamage(sprite, dmg, Spell.Template.ElementalProperty, Spell.Template.Sound);
                    t.SendAnimation(Spell.Template.Animation, t, sprite);

                    var action = new ServerFormat1A
                    {
                        Serial = sprite.Serial,
                        Number = 0x80,
                        Speed = 30
                    };


                    var hpbar = new ServerFormat13
                    {
                        Serial = t.Serial,
                        Health = (ushort)(100 * t.CurrentHp / t.MaximumHp),
                        Sound = Spell.Template.Sound
                    };

                    t.Show(Scope.NearbyAislings, hpbar);
                    sprite.Show(Scope.NearbyAislings, action);
                }
            }
        }
    }
}
