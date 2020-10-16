#region

using System.Collections.Generic;
using System.Linq;
using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Cone Attack", "Dean")]
    public class ConeAttack : SkillScript
    {
        public Skill _skill;

        public ConeAttack(Skill skill) : base(skill)
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
                    Serial = sprite.Serial,
                    Number = (byte) (client.Aisling.Path == Class.Warrior
                        ? client.Aisling.UsingTwoHanded ? 0x81 : 0x01
                        : 0x01),
                    Speed = 20
                };

                var enemy = GetInCone(sprite, 3);

                if (enemy != null)
                {
                    foreach (var i in enemy.OfType<Monster>())
                    {
                        if (i == null)
                            continue;

                        if (client.Aisling.Serial == i.Serial)
                            continue;

                        var imp = 300 + Skill.Level;
                        var dmg = client.Aisling.Str * 4 + client.Aisling.Dex * 2;

                        dmg += dmg * imp / 100;

                        if (sprite.EmpoweredAssail)
                            if (sprite is Aisling)
                                if ((sprite as Aisling).Weapon == 0)
                                    dmg *= 3;

                        i.ApplyDamage(sprite, dmg, Skill.Template.Sound);
                        i.Target = client.Aisling;

                        client.Aisling.Show(Scope.NearbyAislings,
                            new ServerFormat29(Skill.Template.TargetAnimation, (ushort) i.XPos, (ushort) i.YPos));
                    }

                    client.Aisling.Show(Scope.NearbyAislings, action);
                }
            }
        }

        public override void OnUse(Sprite sprite)
        {
            if (!Skill.Ready)
                return;

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

                    var success = true;

                    if (success)
                        OnSuccess(sprite);
                    else
                        OnFailed(sprite);
                }
            }
            else
            {
                var enemy = GetInCone(sprite, 3);

                if (enemy != null)
                    foreach (var i in enemy)
                    {
                        if (i == null)
                            continue;

                        if (sprite.Serial == i.Serial)
                            continue;

                        if (i is Aisling || i is Monster)
                        {
                            (i as Aisling).Client.Aisling.Show(Scope.NearbyAislings,
                                new ServerFormat29(Skill.Template.TargetAnimation, (ushort) i.XPos, (ushort) i.YPos));

                            var dmg = 50 * (sprite.Str + Skill.Level) / 100;
                            i.ApplyDamage(sprite, dmg, Skill.Template.Sound);
                        }
                    }
            }
        }

        private Sprite[] GetInCone(Sprite sprite, int v)
        {
            var objs = new List<Sprite>();
            var front = sprite.GetInfront(v);

            if (front.Any())
            {
                objs.AddRange(objs);

                foreach (var obj in front)
                {
                    var valid_target = sprite.GetObject<Monster>(sprite.Map,
                        i => i != null && i.Alive && i.Position.IsNextTo(obj.Position) && i.Serial != obj.Serial &&
                             i.Serial != sprite.Serial);

                    if (valid_target != null) objs.Add(valid_target);
                }
            }

            return objs.ToArray();
        }
    }
}