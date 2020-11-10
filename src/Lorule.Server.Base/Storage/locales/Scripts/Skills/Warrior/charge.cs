#region

using System;
using System.Threading.Tasks;
using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Charge", "Dean")]
    public class Charge : SkillScript
    {
        public Skill _skill;
        public Sprite Target;
        private readonly Random rand = new Random();

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

        public override async void OnSuccess(Sprite sprite)
        {
            var collided = false;

            var action = new ServerFormat1A
            {
                Serial = sprite.Serial,
                Number = 0x82,
                Speed = 20
            };

            for (var i = 0; i < 7; i++)
            {
                var targets = sprite.GetInfront(i, true);

                var hits = 0;
                foreach (var target in targets)
                {
                    if (target.Serial == sprite.Serial)
                        continue;

                    if (target != null)
                    {
                        var imp = 200 * hits + Skill.Level;
                        var dmg = sprite.Str * 5 + sprite.Con * 5;

                        dmg += dmg * imp / 100;

                        if (sprite.EmpoweredAssail)
                            if (sprite is Aisling)
                                if ((sprite as Aisling).Weapon == 0)
                                    dmg *= 3;

                        target.ApplyDamage(sprite, dmg, Skill.Template.Sound);
                        {
                            Target = target;
                        }

                        if (sprite is Aisling aisling)
                        {
                            var position = target.Position;

                            if (sprite.Direction == 0)
                                position.Y++;
                            if (sprite.Direction == 1)
                                position.X--;
                            if (sprite.Direction == 2)
                                position.Y--;
                            if (sprite.Direction == 3)
                                position.X++;

                            aisling.Client.WarpTo(position);
                        }
                    }

                    hits++;
                }

                if (hits > 0)
                {
                    collided = true;
                    break;
                }
            }

            await Task.Delay(50).ContinueWith(dc =>
            {
                if (Target != null && collided)
                {
                    if (Target is Monster || Target is Mundane || Target is Aisling)
                        Target.Show(Scope.NearbyAislings,
                            new ServerFormat29((uint)sprite.Serial, (uint)Target.Serial,
                                Skill.Template.TargetAnimation, 0, 100));

                    sprite.Show(Scope.NearbyAislings, action);
                }
            }).ConfigureAwait(true);
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
                }
            }

            var success = Skill.RollDice(rand);

            if (success)
                OnSuccess(sprite);
            else
                OnFailed(sprite);
        }
    }
}