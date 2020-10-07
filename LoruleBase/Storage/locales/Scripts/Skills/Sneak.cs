#region

using System;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Skills
{
    [Script("Sneak", "Dean")]
    public class Sneak : SkillScript
    {
        public Skill _skill;

        public Random rand = new Random();

        public Sneak(Skill skill) : base(skill)
        {
            _skill = skill;
        }

        public override void OnFailed(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                client.SendMessage(0x02, "failed.");
            }
        }

        public override void OnSuccess(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                if (client.Aisling != null && !client.Aisling.Dead)
                {
                    client.Aisling.Invisible = true;

                    if (client.Aisling.Invisible)
                    {
                        client.SendMessage(0x02, "You blend in to the shadows.");

                        client.Aisling.Show(Scope.NearbyAislings,
                            new ServerFormat29(Skill.Template.TargetAnimation,
                                (ushort) client.Aisling.XPos,
                                (ushort) client.Aisling.YPos
                            ));
                    }

                    client.Refresh(true);
                }
            }
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                client.TrainSkill(Skill);

                var success = true;
                if (success)
                    OnSuccess(sprite);
                else
                    OnFailed(sprite);
            }
        }
    }
}