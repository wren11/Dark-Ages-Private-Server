#region

using Darkages.Types;

#endregion

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat37 : NetworkFormat
    {
        public ServerFormat37(Item item, byte slot) : this()
        {
            Item = item;
            EquipmentSlot = slot;
        }

        public ServerFormat37()
        {
            Secured = true;
            Command = 0x37;
        }

        public Item Item { get; set; }
        public byte EquipmentSlot { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(EquipmentSlot);
            writer.Write(Item.DisplayImage);
            writer.Write((byte) 0x03);
            writer.WriteStringA(Item.Template.Name);
            writer.WriteStringA(Item.DisplayName);
            writer.Write(Item.Durability);
            writer.Write(Item.Template.MaxDurability);
        }
    }
}