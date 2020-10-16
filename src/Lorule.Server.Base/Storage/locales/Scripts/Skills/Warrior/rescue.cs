#region

using System.Collections.Generic;
using System.Linq;
using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Rescue", "Dean")]
    public class Rescue : SkillScript
    {
        public Skill _skill;
        public Sprite Target;

        public Rescue(Skill skill) : base(skill)
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
            if (sprite is Aisling aisling)
            {
                var client = aisling.Client;

                var action = new ServerFormat1A
                {
                    Serial = client.Aisling.Serial,
                    Number = 0x01,
                    Speed = 20
                };

                var enemy = client.Aisling.GetInfront();

                if (enemy != null)
                    foreach (var i in from i in enemy
                        where i != null
                        where client.Aisling.Serial != i.Serial
                        where !(i is Money)
                        where i.Attackable
                        select i)
                    {
                        if (i is Monster monster)
                        {
                            var membersExcludingSelfMapWide =
                                GetObjects<Aisling>(aisling.Map,
                                        n => n.Serial != aisling.Serial && n.GroupId == aisling.GroupId)
                                    .ToList();

                            if (membersExcludingSelfMapWide.Count > 0)
                            {
                                var nearbyTags = new List<Sprite>(membersExcludingSelfMapWide);
                                var removed = nearbyTags.Sum(obj =>
                                    monster.TaggedAislings.RemoveWhere(n => n != obj.Serial));
                            }
                        }

                        Target = i;
                        i.ApplyDamage(aisling, 0, Skill.Template.Sound);

                        if (i is Aisling)
                        {
                            if ((i as Aisling).Skulled)
                            {
                                (i as Aisling).RemoveDebuff("skulled", true);
                                (i as Aisling).Client.Revive();
                            }

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
                    OnSuccess(sprite);
                }
            }
        }
    }
}