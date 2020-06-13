#region

using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Skills
{
    [Script("Inspect Item", "Dean")]
    public class Inspect : SkillScript
    {
        public Inspect(Skill skill) : base(skill)
        {
        }

        public override void OnFailed(Sprite sprite)
        {
            if (sprite is Aisling aisling)
            {
                var client = aisling.Client;
                client.SystemMessage("Failed.");
            }
        }

        public override void OnSuccess(Sprite sprite)
        {
            if (sprite is Aisling aisling)
            {
                var client = aisling.Client;
                var itemFirstSlot = aisling.Inventory.Has(i => i.Slot == 1);

                if (itemFirstSlot != null)
                {
                    itemFirstSlot.Identifed = true;
                    {
                        client.SystemMessage($"Success! Item is {itemFirstSlot.DisplayName}");
                    }
                }
            }
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling aisling)
            {
                var client = aisling.Client;

                if (client != null && Skill.CanUse())
                {
                    client.TrainSkill(Skill);
                    OnSuccess(sprite);
                }
                else
                {
                    OnFailed(sprite);
                }
            }
        }
    }
}