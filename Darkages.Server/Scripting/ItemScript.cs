using Darkages.Network.Object;
using Darkages.Types;

namespace Darkages.Scripting
{
    public abstract class ItemScript : ObjectManager
    {
        public ItemScript(Item item)
        {
            Item = item;
        }

        public Item Item { get; set; }

        public abstract void OnUse(Sprite sprite, byte slot);
        public abstract void Equipped(Sprite sprite, byte displayslot);
        public abstract void UnEquipped(Sprite sprite, byte displayslot);
    }
}