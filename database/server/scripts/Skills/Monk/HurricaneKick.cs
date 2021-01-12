#region

using System.Collections.Generic;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Skills
{
    [Script("Hurricane Kick", "Dean")]
    public class HurricaneKick : SkillScript
    {
        public Skill _skill;
        public Sprite Target;

        public HurricaneKick(Skill skill) : base(skill)
        {
            _skill = skill;
        }

        public List<Sprite> GetInCone(Sprite sprite, int distance)
        {
            var result = new List<Sprite>();
            var objects = GetObjects(sprite.Map, i => i.WithinRangeOf(sprite, distance),
                Get.Aislings | Get.Monsters | Get.Mundanes);
            foreach (var obj in objects)
                if (sprite.Position.DistanceFrom(obj.Position) <= distance)
                {
                    if ((Direction) sprite.Direction == Direction.North)
                    {
                        if (obj.YPos <= sprite.YPos)
                            result.Add(obj);
                    }
                    else if ((Direction) sprite.Direction == Direction.South)
                    {
                        if (obj.YPos >= sprite.YPos)
                            result.Add(obj);
                    }
                    else if ((Direction) sprite.Direction == Direction.East)
                    {
                        if (obj.XPos >= sprite.XPos)
                            result.Add(obj);
                    }
                    else if ((Direction) sprite.Direction == Direction.West)
                    {
                        if (obj.XPos <= sprite.XPos)
                            result.Add(obj);
                    }
                }

            return result;
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
                    Number = 0x85,
                    Speed = 25
                };

                var enemy = GetInCone(sprite, 2);

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

                        var debuff = new debuff_hurricane();
                        if (!i.HasDebuff(debuff.Name)) debuff.OnApplied(i, debuff);

                        var dmg = (int) (client.Aisling.Invisible
                            ? 2
                            : 1 * (client.Aisling.Str + client.Aisling.Con) * 0.05 * Skill.Level);
                        i.ApplyDamage(sprite, dmg, Skill.Template.Sound);

                        if (i is Monster)
                            (i as Monster).Target = client.Aisling;

                        if (i is Aisling)
                        {
                            (i as Aisling).Client.Aisling.Show(Scope.NearbyAislings,
                                new ServerFormat29((uint) client.Aisling.Serial, (uint) i.Serial, byte.MinValue,
                                    Skill.Template.TargetAnimation, 100));

                            (i as Aisling).Client.Send(new ServerFormat08(i as Aisling, StatusFlags.All));
                        }

                        if (i is Monster || i is Mundane)
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
                var target = sprite.Target;
                if (target == null)
                    return;

                target.Show(Scope.NearbyAislings,
                    new ServerFormat29((uint) target.Serial, (uint) target.Serial,
                        Skill.Template.TargetAnimation, 0, 100));

                var dmg = 50 * sprite.Str / 100;
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