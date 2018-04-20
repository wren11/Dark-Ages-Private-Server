using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Items
{
    [Script("Scroll", "Dean")]
    public class Scroll : ItemScript
    {
        public Scroll(Item item) : base(item)
        {
        }

        public override void Equipped(Sprite sprite, byte displayslot)
        {
        }

        public override void UnEquipped(Sprite sprite, byte displayslot)
        {
        }

        public override void OnUse(Sprite sprite, byte slot)
        {
            if (sprite is Aisling)
            {
            }
        }
    }
}