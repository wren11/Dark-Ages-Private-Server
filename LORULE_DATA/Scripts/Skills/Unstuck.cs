using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Skills
{
    [Script("Unstuck", "Test")]
    public class Unstuck : SkillScript
    {
        public Unstuck(Skill skill) : base(skill)
        {
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                var prev = new Position(client.Aisling.X, client.Aisling.Y);
                var targetPosition = client.Aisling.Map.FindNearestEmpty(client.Aisling.Position);

                if (targetPosition != null)
                {
                    client.Aisling.X = targetPosition.X;
                    client.Aisling.Y = targetPosition.Y;
                    client.Aisling.Map.Update(prev.X, prev.Y, TileContent.None);
                    client.Refresh();
                }
            }
        }

        public override void OnFailed(Sprite sprite)
        {
        }

        public override void OnSuccess(Sprite sprite)
        {
        }
    }
}