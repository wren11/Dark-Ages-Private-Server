namespace Darkages.Types
{
    public class EquipmentSlot
    {
        public EquipmentSlot(int _slot, Item _item)
        {
            Slot = _slot;
            Item = _item;
        }

        public int Slot { get; set; }
        public Item Item { get; set; }
    }
}