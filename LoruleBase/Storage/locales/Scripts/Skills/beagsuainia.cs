#region

using System;
using System.Linq;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Skills
{
    [Script("beag suain ia", "Wren")]
    public class beagsuainia : SkillScript
    {
        public Random rand = new Random();
        public Sprite Target;

        public beagsuainia(Skill skill) : base(skill)
        {
        }

        public override void OnFailed(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                client.Aisling.Show(Scope.NearbyAislings,
                    new ServerFormat29(Skill.Template.MissAnimation, (ushort) sprite.XPos, (ushort) sprite.YPos));

                client.SendMessage(0x02, "The enemy has made it through.");
            }
        }

        public override void OnSuccess(Sprite sprite)
        {
            var a = sprite.AislingsNearby().ToList();
            var b = sprite.MonstersNearby().ToList();

            var i = a.Concat<Sprite>(b);

            if (i == null || !i.Any())
                if (sprite is Aisling)
                {
                    var client = (sprite as Aisling).Client;
                    client.SendMessage(0x02, "The enemy has made it through.");
                    return;
                }

            foreach (var target in i)
            {
                if (target.Serial == sprite.Serial)
                    continue;

                var debuff = new debuff_beagsuain();

                if (!target.HasDebuff(debuff.Name))
                    if (sprite is Aisling)
                    {
                        var client = (sprite as Aisling).Client;
                        var action = new ServerFormat1A
                        {
                            Serial = client.Aisling.Serial,
                            Number = 0x81,
                            Speed = 20
                        };

                        client.Aisling.Show(Scope.NearbyAislings, action);
                        {
                            target.ApplyDamage(client.Aisling, 0, Skill.Template.Sound);
                            debuff.OnApplied(target, debuff);
                        }
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
                    client.TrainSkill(Skill);

                    if (client.Aisling.Invisible && Skill.Template.PostQualifers == PostQualifer.BreakInvisible)
                    {
                        client.Aisling.Invisible = false;
                        client.Refresh();
                    }

                    if (rand.Next(1, 101) >= 10)
                        OnSuccess(sprite);
                    else
                        OnFailed(sprite);
                }
            }
            else
            {
                OnSuccess(sprite);
            }
        }
    }
}