#region

using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Double Punch", "Dean")]
    public class DoublePunch : SkillScript
    {
        public Skill _skill;

        public Sprite Target;

        public DoublePunch(Skill skill) : base(skill)
        {
            _skill = skill;
        }

        public override void OnFailed(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                if (Target != null)
                    client.Aisling.Show(Scope.NearbyAislings,
                        new ServerFormat29(Skill.Template.MissAnimation, (ushort) Target.XPos, (ushort) Target.YPos));
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
                    Number = (byte) (client.Aisling.Path == Class.Monk ? 0x84 : 0x01),
                    Speed = 30
                };

                var enemy = client.Aisling.GetInfront();

                if (enemy != null)
                    foreach (var i in enemy)
                    {
                        if (i == null)
                            continue;

                        if (client.Aisling.Serial == i.Serial)
                            continue;

                        if (i is Money)
                            continue;

                        Target = i;

                        var imp = 20 + Skill.Level;
                        var dmg = client.Aisling.Str * 4 + client.Aisling.Con * 2;

                        dmg += dmg * imp / 100;

                        if (sprite.EmpoweredAssail)
                            if (sprite is Aisling)
                                if ((sprite as Aisling).Weapon == 0)
                                    dmg *= 3;

                        i.ApplyDamage(sprite, dmg, Skill.Template.Sound);

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

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                if (Skill.Ready)
                {
                    client.TrainSkill(Skill);
                    if (client.Aisling.Invisible)
                    {
                        client.Aisling.Invisible = false;
                        client.Refresh();
                    }

                    OnSuccess(sprite);
                }
            }
        }
    }
}