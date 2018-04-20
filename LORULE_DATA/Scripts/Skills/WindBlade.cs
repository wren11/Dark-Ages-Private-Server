using System;
using System.Linq;
using Darkages.Network.ServerFormats;
using Darkages.Types;

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Wind Blade", "Dean")]
    public class WindBlade : SkillScript
    {
        private readonly Random rand = new Random();
        public Skill _skill;

        public Sprite Target;

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
                    Number = 0x81,
                    Speed = 20
                };

                var enemy = client.Aisling.GetInfront(4);

                if (enemy != null)
                {
                    foreach (var i in enemy.Cast<Sprite>())
                    {
                        if (i == null)
                            continue;


                        if (client.Aisling.Serial == i.Serial)
                            continue;

                        if (i is Money)
                            continue;

                        var dmg = client.Aisling.Invisible ? 2 : 1 * client.Aisling.Str * 20 * Skill.Level;
                        i.ApplyDamage(sprite, dmg, false, Skill.Template.Sound);

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
                client.TrainSkill(Skill);

                if (Skill.Ready)
                {
                    if (client.Aisling.Invisible && Skill.Template.PostQualifers.HasFlag(PostQualifer.BreakInvisible))
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
                        new ServerFormat29((uint) target.Serial, (uint) target.Serial,
                            Skill.Template.TargetAnimation, 0, 100));

                    var dmg = 1 * sprite.Str * 20 * Skill.Level;
                    target.ApplyDamage(sprite, dmg, true, Skill.Template.Sound);

                    var action = new ServerFormat1A
                    {
                        Serial = sprite.Serial,
                        Number = 0x82,
                        Speed = 20
                    };

                    if (sprite is Monster)
                    {
                        (target as Aisling).Client.SendStats(StatusFlags.All);
                        (target as Aisling).Client.Aisling.Show(Scope.NearbyAislings, action);
                    }
                }
            }
        }
    }
}