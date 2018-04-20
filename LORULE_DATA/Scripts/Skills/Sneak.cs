using System;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

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

                client.SendMessage(0x02,
                    string.IsNullOrEmpty(Skill.Template.FailMessage) ? Skill.Template.FailMessage : "failed.");
            }
        }

        public override void OnSuccess(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                if (client.Aisling != null && !client.Aisling.Dead)
                {
                    client.Aisling.Flags = client.Aisling.Flags == AislingFlags.Invisible
                        ? AislingFlags.Normal
                        : AislingFlags.Invisible;
                    client.Send(new ServerFormat3F((byte)Skill.Template.Pane, Skill.Slot, Skill.Template.Cooldown));
                    client.Refresh();
                }
            }
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
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