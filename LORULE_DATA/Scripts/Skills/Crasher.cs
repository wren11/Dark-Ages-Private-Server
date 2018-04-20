using System;
using Darkages.Network.ServerFormats;
using Darkages.Types;

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Crasher", "Huy")]
    public class Crasher : SkillScript
    {
        private Random _rand = new Random();
        private Skill _skill;
        public Random rand = new Random();
        public Sprite Target;

        public Crasher(Skill skill) : base(skill)
        {
            _skill = skill;
        }

        public override void OnFailed(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                client.SendMessage(0x02,
                    !string.IsNullOrEmpty(Skill.Template.FailMessage) ? Skill.Template.FailMessage : "failed.");
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
                    Number = 0x82,
                    Speed = 20
                };

                var enemy = client.Aisling.GetInfront(1);

                if (enemy == null) return;
                foreach (var i in enemy)
                {
                    if (i == null)
                        continue;
                    if (client.Aisling.Serial == i.Serial)
                        continue;
                    if (i is Money)
                        continue;

                    Target = i;

                    var dmg = client.Aisling.CurrentHp * 300 / 100 + 99 * Skill.Level;
                    i.ApplyDamage(sprite, dmg, false, 44);


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

                client.Aisling.CurrentHp = 1;
                client.SendStats(StatusFlags.All);
                client.Aisling.Show(Scope.NearbyAislings, action);
            }
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                if (Skill.Ready)
                {
                    client.TrainSkill(Skill);
                    if (client.Aisling.Invisible && Skill.Template.PostQualifers == PostQualifer.BreakInvisible)
                    {
                        client.Aisling.Flags = AislingFlags.Normal;
                        client.Refresh();
                    }

                    client.Send(new ServerFormat3F((byte)Skill.Template.Pane, Skill.Slot, Skill.Template.Cooldown));

                    var success = Skill.RollDice(rand);

                    if (success)
                        OnSuccess(sprite);
                    else
                        OnFailed(sprite);
                }
            }
            else
            {
                var target = sprite.Target;
                if (target == null)
                    return;

                if (target is Aisling)
                {
                    (target as Aisling).Client.Aisling.Show(Scope.NearbyAislings,
                        new ServerFormat29((uint) target.Serial, (uint) sprite.Serial,
                            Skill.Template.TargetAnimation, 0, 100));

                    var dmg = sprite.CurrentHp * 300 / 100 + 99;
                    target.ApplyDamage(sprite, dmg, false, 44);


                    var action = new ServerFormat1A
                    {
                        Serial = sprite.Serial,
                        Number = 0x82,
                        Speed = 20
                    };

                    if (sprite is Monster)
                    {
                        sprite.CurrentHp = 1;
                        (target as Aisling).Client.SendStats(StatusFlags.All);
                        (target as Aisling).Client.Aisling.Show(Scope.NearbyAislings, action);
                    }
                }
            }
        }
    }
}