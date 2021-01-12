#region

using Darkages.Scripting;
using Darkages.Types;
using System;
using System.Linq;

#endregion

namespace Darkages.Storage.locales.Scripts.Skills
{
    [Script("Ambush", "Dean")]
    public class Ambush : SkillScript
    {
        public Skill _skill;

        public Random rand = new Random();

        public Ambush(Skill skill) : base(skill)
        {
            _skill = skill;
        }

        public override void OnFailed(Sprite sprite)
        {
            if (sprite is Aisling aisling)
            {
                var client = aisling.Client;

                client.SendMessage(0x02,
                    string.IsNullOrEmpty(Skill.Template.FailMessage) ? Skill.Template.FailMessage : "failed.");
            }
        }

        public override void OnSuccess(Sprite sprite)
        {
            foreach (var target in sprite.GetInfront(3)
                .Where(target => !(target is Item))
                .Where(target => !(target is Money))
                .Where(target => target.Serial != sprite.Serial))
            {
                var directions = new[]
                {
                    new[] {-1, +0},
                    new[] {+0, -1},
                    new[] {+1, +0},
                    new[] {+0, +1}
                };
                for (var i = 4 - 1; i >= 0; i--)
                {
                    var newX = target.X + directions[1][0];
                    var newY = target.Y + directions[1][1];
                    if (newX == target.X && newY == target.Y || sprite.Map.IsWall(newX, newY) ||
                        HasObject(newX, newY, target))
                        continue;

                    sprite.X = newX;
                    sprite.Y = newY;
                    sprite.Facing(target.X, target.Y, out var dir);
                    sprite.Direction = (byte) dir;

                    if (sprite is Aisling aisling)
                        aisling.Client.Refresh();

                    return;
                }
            }

            bool HasObject(int newX, int newY, Sprite target) =>
                sprite.Map.IsWall(newX, newY) && !GetObjects(sprite.Map,
                    n => n.Serial == target.Serial && n.X == newX && n.Y == newY,
                    Get.Monsters | Get.Aislings | Get.Mundanes).Any();
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                if (client.Aisling != null && !client.Aisling.Dead && Skill.Ready)
                    client.TrainSkill(Skill);
            }

            if (Skill.Ready)
            {
                var success = Skill.Level >= 100 || rand.Next(1, 3) == 1;

                if (success)
                    OnSuccess(sprite);
                else
                    OnFailed(sprite);
            }
        }
    }
}