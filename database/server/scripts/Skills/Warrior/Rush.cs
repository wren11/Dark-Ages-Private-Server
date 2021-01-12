#region

using System;
using System.Threading.Tasks;
using Darkages.Network.ServerFormats;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;

#endregion

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Rush", "Dean")]
    public class Rush : SkillScript
    {
        public Skill _skill;
        public Sprite Target;
        private readonly Random rand = new Random();

        public Rush(Skill skill) : base(skill)
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

            var debuff = new debuff_frozen();
            Sprite t = null;

            for (var i = 0; i < 5; i++)
            {
                var targets = sprite.GetInfront(i, true);
                var hits = 0;
                foreach (var target in targets)
                {
                    if (target.Serial == sprite.Serial)
                        continue;

                    if (target != null)
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

                    t = target;
                    hits++;
                }

                if (hits > 0)
                {
                    if (t != null)
                    {
                        t.RemoveDebuff("sleep");
                        t.RemoveDebuff("frozen");

                        if (!t.HasDebuff(debuff.Name))
                        {
                            sprite.Show(Scope.NearbyAislings, action);
                            t.ApplyDamage(sprite, 0, Skill.Template.Sound);
                            debuff.OnApplied(t, debuff);
                        }
                    }

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