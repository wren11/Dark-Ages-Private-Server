#region

using Darkages.Network.Object;
using Darkages.Types;

#endregion

namespace Darkages.Scripting
{
    public abstract class ItemScript : ObjectManager, IScriptBase
    {
        protected ItemScript(Item item)
        {
            Item = item;
        }

        public Item Item { get; set; }

        public abstract void Equipped(Sprite sprite, byte displayslot);

        public virtual void OnDropped(Sprite sprite, Position droppedPosition, Area map)
        {
        }

        public virtual void OnPickedUp(Sprite sprite, Position pickedPosition, Area map)
        {
        }

        public abstract void OnUse(Sprite sprite, byte slot);

        public abstract void UnEquipped(Sprite sprite, byte displayslot);
    }
}