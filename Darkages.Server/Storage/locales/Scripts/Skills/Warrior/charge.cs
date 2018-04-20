using Darkages.Network.ServerFormats;
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Charge", "Dean")]
    public class Charge : SkillScript
    {
        private readonly Random rand = new Random();
        public Skill _skill;

        public Sprite Target;

        public Charge(Skill skill) : base(skill)
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

                for (int i = 0; i < 5; i++)
                {
                    if (sprite.FacingDir == Direction.East)
                    {
                        sprite.X++;
                    }
                    else if (sprite.FacingDir == Direction.West)
                    {
                        sprite.X--;
                    }
                    else if (sprite.FacingDir == Direction.North)
                    {
                        sprite.Y--;
                    }
                    else if (sprite.FacingDir == Direction.South)
                    {
                        sprite.Y++;
                    }

                    var hits = GetObjects(n => n.Facing(sprite, out var dir), Get.Monsters | Get.Aislings | Get.Mundanes);
                    if (hits != null && hits.Length > 0)
                    {
                        break;
                    }
                }

                client.Refresh();
            }
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                if (Skill.Ready)
                {
                    if (client.Aisling.Invisible && Skill.Template.PostQualifers.HasFlag(PostQualifer.BreakInvisible))
                    {
                        client.Aisling.Flags = AislingFlags.Normal;
                        client.Refresh();
                    }

                    client.TrainSkill(Skill);

                    var success = Skill.RollDice(rand);

                    if (success)
                        OnSuccess(sprite);
                    else
                        OnFailed(sprite);
                }
            }
        }
    }
}