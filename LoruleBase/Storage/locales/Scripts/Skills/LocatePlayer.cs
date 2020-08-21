#region

using System.Linq;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Skills
{
    [Script("Locate Player", "Test")]
    public class LocatePlayer : SkillScript
    {
        public LocatePlayer(Skill skill) : base(skill)
        {
        }

        public override void OnFailed(Sprite sprite)
        {
        }

        public override void OnSuccess(Sprite sprite)
        {
            if (sprite is Aisling aisling)
            {
                var msg = " Current Items\n";

                foreach (var iter in aisling.Inventory.Items)
                {
                    var item = iter.Value;
                    if (item == null)
                        continue;

                    msg += item.DisplayName + "\n";
                }

                aisling.Client.SendMessage(0x08, msg);
            }
        }

        public override void OnUse(Sprite sprite)
        {
            var nearest = GetObjects<Aisling>(sprite.Map,
                    i => i.Serial != sprite.Serial && i.CurrentMapId == sprite.CurrentMapId)
                .OrderBy(i => i.Position.DistanceFrom(sprite.Position)).FirstOrDefault();

            OnSuccess(sprite);
        }
    }
}