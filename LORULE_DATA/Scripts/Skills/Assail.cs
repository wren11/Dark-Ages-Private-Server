using System;
using System.Linq;
using Darkages.Network.ServerFormats;
using Darkages.Types;

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Assail", "Test")]
    public class Assail : SkillScript
    {
        public Skill _skill;

        private Random rand = new Random();

        public Sprite Target;

        public Assail(Skill skill) : base(skill)
        {
            _skill = skill;
        }

        public override void OnFailed(Sprite sprite)
        {
            if (Target != null)
                if (sprite is Aisling)
                {
                    var client = (sprite as Aisling).Client;
                    client.Aisling.Show(Scope.NearbyAislings,
                        new ServerFormat29(Skill.Template.MissAnimation, (ushort)Target.X, (ushort)Target.Y));
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
                    Number = 0x01,
                    Speed = 20
                };


                var enemy = client.Aisling.GetInfront();

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

                        if (!i.Attackable)
                            continue;

                        if (!sprite.CanHitTarget(i))
                            continue;

                        Target = i;


                        var imp = (Skill.Level * 10 / 100);
                        var dmg = (client.Aisling.Str + client.Aisling.Dex * imp);

                        dmg *= 8;

                        i.ApplyDamage(sprite, dmg);


                        if (i is Aisling)
                        {
                            (i as Aisling).Client.Aisling.Show(Scope.NearbyAislings,
                                new ServerFormat29((uint)client.Aisling.Serial, (uint)i.Serial, byte.MinValue,
                                    Skill.Template.TargetAnimation, 100));
                            (i as Aisling).Client.Send(new ServerFormat08(i as Aisling, StatusFlags.All));
                        }

                        if (i is Monster || i is Mundane || i is Aisling)
                            client.Aisling.Show(Scope.NearbyAislings,
                                new ServerFormat29((uint)client.Aisling.Serial, (uint)i.Serial,
                                    Skill.Template.TargetAnimation, 0, 100));
                    }
                }

                client.Aisling.Show(Scope.NearbyAislings, action);
            }
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                client.TrainSkill(Skill);
                if (Skill.Ready)
                {
                    if (client.Aisling.Invisible)
                    {
                        client.Aisling.Flags = AislingFlags.Normal;
                        client.Refresh();
                    }

                    client.Send(new ServerFormat3F((byte)Skill.Template.Pane, Skill.Slot, Skill.Template.Cooldown));

                    OnSuccess(sprite);
                }
            }
            else
            {
                var enemy = sprite.GetInfront();


                var action = new ServerFormat1A
                {
                    Serial = sprite.Serial,
                    Number = 0x01,
                    Speed = 30
                };

                if (enemy != null)
                {
                    foreach (var i in enemy)
                    {
                        if (i == null)
                            continue;

                        if (sprite.Serial == i.Serial)
                            continue;

                        if (i is Money)
                            continue;

                        if (!i.Attackable)
                            continue;

                        Target = i;

                        var dmg = sprite.GetBaseDamage(Target);
                        {
                            i.ApplyDamage(sprite, dmg);
                        }

                        if (Skill.Template.TargetAnimation > 0)
                        {
                            if (i is Monster || i is Mundane || i is Aisling)
                                sprite.Show(Scope.NearbyAislings,
                                    new ServerFormat29((uint)sprite.Serial, (uint)i.Serial,
                                        Skill.Template.TargetAnimation, 0, 100));
                        }

                        sprite.Show(Scope.NearbyAislings, action);
                    }
                }
            }
        }
    }
}