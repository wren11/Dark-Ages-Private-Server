#region

using System;
using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Wind Blade", "Dean")]
    public class WindBlade : SkillScript
    {
        public Skill _skill;
        public Sprite Target;
        private readonly Random rand = new Random();

        public WindBlade(Skill skill) : base(skill)
        {
            _skill = skill;
        }

        public override void OnFailed(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                client.SendMessage(0x02,
                    string.IsNullOrEmpty(Skill.Template.FailMessage) ? Skill.Template.FailMessage : "failed.");
            }
        }

        public override void OnSuccess(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                var action = new ServerFormat1A
                {
                    Serial = client.Aisling.Serial,
                    Number = (byte) (client.Aisling.Path == Class.Warrior ? 0x81 : 0x84),
                    Speed = 25
                };

                var enemy = client.Aisling.GetInfront(4);

                if (enemy != null)
                {
                    foreach (var i in enemy)
                    {
                        if (i == null)
                            continue;

                        if (client.Aisling.Serial == i.Serial)
                            continue;

                        if (i is Money)
                            continue;

                        var imp = 50 + Skill.Level;
                        var dmg = client.Aisling.Str * 10 + client.Aisling.Dex * 5;

                        dmg += dmg * imp / 100;

                        i.ApplyDamage(sprite, dmg, Skill.Template.Sound);

                        if (i is Monster) (i as Monster).Target = client.Aisling;
                        if (i is Aisling)
                        {
                            (i as Aisling).Client.Aisling.Show(Scope.NearbyAislings,
                                new ServerFormat29((uint) client.Aisling.Serial, (uint) i.Serial, byte.MinValue,
                                    Skill.Template.TargetAnimation, 100));
                            (i as Aisling).Client.Send(new ServerFormat08(i as Aisling, StatusFlags.All));
                        }

                        if (i is Monster || i is Mundane || i is Aisling)
                            client.Aisling.Show(Scope.NearbyAislings,
                                new ServerFormat29((uint) client.Aisling.Serial, (uint) i.Serial,
                                    Skill.Template.TargetAnimation, 0, 100));
                    }

                    client.Aisling.Show(Scope.NearbyAislings, action);
                }
            }
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                if (Skill.Ready)
                {
                    if (client.Aisling.Invisible && Skill.Template.PostQualifers.HasFlag(PostQualifer.BreakInvisible))
                    {
                        client.Aisling.Invisible = false;
                        client.Refresh();
                    }

                    client.TrainSkill(Skill);

                    var success = Skill.RollDice(rand);

                    if (success)
                        OnSuccess(sprite);
                    else
                        OnFailed(sprite);
                }
            }
            else
            {
                if (Skill.Ready)
                {
                    var target = sprite.Target;
                    if (target == null)
                        return;

                    target.Show(Scope.NearbyAislings,
                        new ServerFormat29((uint) target.Serial, (uint) target.Serial,
                            Skill.Template.TargetAnimation, 0, 100));

                    var dmg = 1 * sprite.Str * 20 * Skill.Level;
                    target.ApplyDamage(sprite, dmg, Skill.Template.Sound);

                    var action = new ServerFormat1A
                    {
                        Serial = sprite.Serial,
                        Number = 0x82,
                        Speed = 20
                    };

                    target.Show(Scope.NearbyAislings, action);
                }
            }
        }
    }
}