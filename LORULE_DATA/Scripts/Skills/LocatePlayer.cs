using System.Linq;
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Skills
{
    [Script("Locate Player", "Test")]
    public class LocatePlayer : SkillScript
    {
        public LocatePlayer(Skill skill) : base(skill)
        {
        }

        public override void OnUse(Sprite sprite)
        {
            var nearest = GetObjects<Aisling>(i => i.Serial != sprite.Serial && (i.CurrentMapId == sprite.CurrentMapId))
                .OrderBy(i => i.Position.DistanceFrom(sprite.Position)).FirstOrDefault();

            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                if (nearest != null)
                {
                    var prev = client.Aisling.Position;
                    Position targetPosition = null;

                    var blocks = nearest.Position.SurroundingContent(client.Aisling.Map);

                    if (blocks.Length > 0)
                    {
                        var selections = blocks.Where(i => i.Content == TileContent.Item
                                                           || i.Content == TileContent.Money
                                                           || i.Content == TileContent.None).ToArray();

                        var selection = selections
                            .OrderByDescending(i => i.Position.DistanceFrom(client.Aisling.Position)).FirstOrDefault();
                        if (selections.Length == 0 || selection == null)
                        {
                            client.SendMessageBox(0x02, "you can't do that.");
                            return;
                        }

                        targetPosition = selection.Position;
                    }

                    if (targetPosition != null)
                    {
                        client.Aisling.X = targetPosition.X;
                        client.Aisling.Y = targetPosition.Y;
                        client.Aisling.Map.Update(prev.X, prev.Y, TileContent.None);


                        if (!client.Aisling.Facing(nearest.X, nearest.Y, out var direction))
                        {
                            client.Aisling.Direction = (byte) direction;

                            if (client.Aisling.Position.IsNextTo(nearest.Position))
                                client.Aisling.Turn();
                        }


                        client.Refresh();
                    }
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