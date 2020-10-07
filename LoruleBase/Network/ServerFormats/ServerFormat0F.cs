#region

using Darkages.Types;

#endregion

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat0F : NetworkFormat
    {
        public ServerFormat0F(Item item) : this()
        {
            Item = item;
        }

        public ServerFormat0F()
        {
            Secured = true;
            Command = 0x0F;
        }

        public Item Item { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Item.Slot);
            writer.Write(Item.DisplayImage);
            writer.Write(Item.Color);
            writer.WriteStringA(Item.DisplayName);
            writer.Write((uint) Item.Stacks);
            writer.Write((byte) Item.Stacks > 1);
            writer.Write(Item.Template.MaxDurability);
            writer.Write(Item.Durability);
            writer.Write((uint) 0x00);
        }
    }
}