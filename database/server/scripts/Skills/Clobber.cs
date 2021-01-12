#region

using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Clobber", "Dean")]
    public class Clobber : SkillScript
    {
        public Skill _skill;
        public Sprite Target;

        public Clobber(Skill skill) : base(skill)
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
                    Number = (byte) (client.Aisling.Path == Class.Warrior
                        ? client.Aisling.UsingTwoHanded ? 0x81 : 0x01
                        : 0x01),
                    Speed = 20
                };

                var enemy = client.Aisling.GetInfront();

                if (enemy != null)
                {
                    client.Aisling.Show(Scope.VeryNearbyAislings, new ServerFormat13(0, 0, Skill.Template.Sound));

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

                        Target = i;

                        var imp = 20 + Skill.Level;
                        var dmg = client.Aisling.Str * 4 + client.Aisling.Dex * 2;

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
                    if (client.Aisling.Invisible)
                    {
                        client.Aisling.Invisible = false;
                        client.Refresh();
                    }

                    client.TrainSkill(Skill);
                    OnSuccess(sprite);
                }
            }
        }
    }
}