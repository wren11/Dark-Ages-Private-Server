#region

using System.Linq;
using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Assail", "Test")]
    public class Assail : SkillScript
    {
        public Skill _skill;

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
                var success = false;

                if (enemy != null)
                {
                    var imp = 10 + Skill.Level;
                    var dmg = client.Aisling.Str * 4 + client.Aisling.Dex * 2;

                    dmg += dmg * imp / 100;

                    if (sprite.EmpoweredAssail)
                        if (((Aisling) sprite).Weapon == 0)
                            dmg *= 3;

                    foreach (var i in from i in enemy
                        where i != null
                        where client.Aisling.Serial != i.Serial
                        where !(i is Money)
                        where i.Attackable
                        select i)
                    {
                        Target = i;

                        i.ApplyDamage(sprite, dmg, Skill.Template.Sound);
                        success = true;

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

                if (!success)
                    client.Aisling.Show(Scope.VeryNearbyAislings, new ServerFormat13(0, 0, Skill.Template.Sound));

                client.Aisling.Show(Scope.NearbyAislings, action);
            }
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling aisling)
            {
                if (Skill.Level < Skill.Template.MaxLevel)
                    aisling.Client.TrainSkill(Skill);

                if (aisling.Invisible)
                {
                    aisling.Invisible = false;
                    aisling.Client.Refresh();
                }

                OnSuccess(sprite);
            }
            else
            {
                if (!Skill.Ready)
                    return;

                var enemy = sprite.GetInfront();

                var action = new ServerFormat1A
                {
                    Serial = sprite.Serial,
                    Number = 0x01,
                    Speed = 30
                };

                if (enemy != null)
                    foreach (var i in from i in enemy
                        where i != null
                        where sprite.Serial != i.Serial
                        where !(i is Money)
                        where i.Attackable
                        select i)
                    {
                        Target = i;

                        var dmg = sprite.GetBaseDamage(Target, MonsterDamageType.Physical);
                        {
                            i.ApplyDamage(sprite, dmg, sprite.OffenseElement);
                        }

                        if (Skill.Template.TargetAnimation > 0)
                            if (i is Monster || i is Mundane || i is Aisling)
                                sprite.Show(Scope.NearbyAislings,
                                    new ServerFormat29((uint) sprite.Serial, (uint) i.Serial,
                                        Skill.Template.TargetAnimation, 0, 100));

                        sprite.Show(Scope.NearbyAislings, action);
                    }
            }
        }
    }
}